﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRP.Models.Entities.Repositories;
using CRP.Models.JsonModels;
using CRP.Models.ViewModels;

namespace CRP.Models.Entities.Services
{
	public interface IVehicleService : IService<Vehicle>
	{
		SearchResultJsonModel SearchVehicle(SearchConditionModel filterConditions);
		VehicleDataTablesJsonModel FilterVehicle(VehicleManagementFilterConditionModel filterConditions);
	}

	public class VehicleService : BaseService<Vehicle>, IVehicleService
	{
		public VehicleService(IUnitOfWork unitOfWork, IVehicleRepository repository) : base(unitOfWork, repository)
		{

		}

		public SearchResultJsonModel SearchVehicle(SearchConditionModel filterConditions)
		{
			var vehicles = repository.Get(v => v.VehicleGroupID != null && v.VehicleGroup.IsActive && v.Garage.IsActive);

			// Run basic common filters
			vehicles = BasicFilter(vehicles, filterConditions);

			// NumOfSeatList condition
			if (filterConditions.NumberOfSeatList != null)
				vehicles = vehicles.Where(v => filterConditions.NumberOfSeatList.Contains(v.Model.NumOfSeat));

			// Get the rental time in hour
			var rentalTimeSpan = (DateTime)filterConditions.EndTime - (DateTime)filterConditions.StartTime;
			var rentalTime = (int)Math.Ceiling(rentalTimeSpan.TotalHours);

			// vehicleGroup's max rental time constraint
			vehicles = vehicles.Where(v => v.VehicleGroup.MaxRentalPeriod == null
										|| v.VehicleGroup.MaxRentalPeriod * 24 > rentalTime);

			// get only vehicles that are free in the booking period condition
			vehicles = vehicles.Where(v =>
				!(v.BookingReceipts.Any(br => !br.IsCanceled
					&& (
						   (filterConditions.StartTime > br.StartTime && filterConditions.StartTime < br.EndTime)
						|| (filterConditions.EndTime > br.StartTime && filterConditions.EndTime < br.EndTime)
						|| (filterConditions.StartTime <= br.StartTime && filterConditions.EndTime >= br.EndTime)
					)))
			);

			// Check StartTime/EndTime to be within garage's OpenTime ~ CloseTime
			// Compare by convert time to the number of minute from 00:00
			// Max margin of error: 60 secs w/ CloseTime (Because we do not validate to second)

			// Booking StartTime
			var startDayInDoW = (int)filterConditions.StartTime.Value.DayOfWeek;
			var startTimeInMunute = filterConditions.StartTime.Value.Minute + filterConditions.StartTime.Value.Hour * 60;
			vehicles = vehicles.Where(v =>
				v.Garage.GarageWorkingTimes.Any(gwt => gwt.DayOfWeek == startDayInDoW
							  && startTimeInMunute >= gwt.OpenTimeInMinute
							  && startTimeInMunute <= gwt.CloseTimeInMinute)
			);

			// Booking EndTime
			var endDayInDoW = (int) filterConditions.EndTime.Value.DayOfWeek;
			var endTimeInMunute = filterConditions.EndTime.Value.Minute + filterConditions.EndTime.Value.Hour * 60;
			vehicles = vehicles.Where(v =>
				v.Garage.GarageWorkingTimes.Any(gwt => gwt.DayOfWeek == endDayInDoW
							  && endTimeInMunute >= gwt.OpenTimeInMinute
							  && endTimeInMunute <= gwt.CloseTimeInMinute)
			);

			// Parse into model suitable to send back to browser
			var results = vehicles.ToList().Select(vehicle => new SearchResultItemJsonModel(vehicle, rentalTime));

			double? averagePrice = null;
			if (results.Any())
				averagePrice = results.Average(r => r.BestPossibleRentalPrice);

			// Max/Min Price conditions
			// Do not validate MaxPrice > MinPrice here. Do it before this in the controller
			if (filterConditions.MaxPrice != null
			    && filterConditions.MinPrice != null)
			{
				results = results.Where(
					r => filterConditions.MaxPrice >= r.BestPossibleRentalPrice
						&& filterConditions.MinPrice <= r.BestPossibleRentalPrice
				);
			}

			// Sort
			// Validate OrderBy in the controller
			if (filterConditions.OrderBy == null)
			{
				results = results.OrderBy(r => r.BestPossibleRentalPeriod)
								.ThenByDescending(r => r.Star)
								.ThenByDescending(r => r.NumOfComment);
			}
			else
			{
				var sortingProp = typeof(SearchResultItemJsonModel).GetProperty(filterConditions.OrderBy);

				// Keep the order descending for star and comment if those are not the main sorting prop
				// Ensure that the magical string represent attribute name exist
				if (filterConditions.IsDescendingOrder)
				{
					if (nameof(SearchResultItemJsonModel.Star) == filterConditions.OrderBy)
						results = results.OrderByDescending(r => r.Star)
										.ThenByDescending(r => r.NumOfComment);
					else if (nameof(SearchResultItemJsonModel.NumOfComment) == filterConditions.OrderBy)
						results = results.OrderByDescending(r => r.NumOfComment)
										.ThenByDescending(r => r.Star);
					else
						results = results.OrderByDescending(r => sortingProp.GetValue(r))
										.ThenByDescending(r => r.Star)
										.ThenByDescending(r => r.NumOfComment);
				}
				else
				{
					if (nameof(SearchResultItemJsonModel.Star) == filterConditions.OrderBy)
						results = results.OrderBy(r => r.Star)
										.ThenBy(r => r.NumOfComment);
					else if (nameof(SearchResultItemJsonModel.NumOfComment) == filterConditions.OrderBy)
						results = results.OrderBy(r => r.NumOfComment)
										.ThenBy(r => r.Star);
					else
						results = results.OrderBy(r => sortingProp.GetValue(r))
										.ThenByDescending(r => r.Star)
										.ThenByDescending(r => r.NumOfComment);
				}
			}
			
			// Paginate
			var filteredRecords = results.Count();
			if (filterConditions.Page < 1 || (filterConditions.Page - 1) * filterConditions.RecordPerPage > filteredRecords)
				filterConditions.Page = 1;

			results = results.Skip((filterConditions.Page - 1) * filterConditions.RecordPerPage)
							 .Take(filterConditions.RecordPerPage);

			// Nest into result object
			return new SearchResultJsonModel(results.ToList(), averagePrice, filteredRecords, filterConditions.Page);
		}

		public VehicleDataTablesJsonModel FilterVehicle(VehicleManagementFilterConditionModel filterConditions)
		{
			// Get only vehicles belonged to this user
			var vehicles = repository.Get(
				v => v.Garage.OwnerID == filterConditions.ProviderID
			);

			var recordsTotal = vehicles.Count();

			// Filters, GO!!
			// Filters that can take out the most records with the least work go first

			// LicenseNumber condition
			if (filterConditions.LicenseNumber != null)
			{
				vehicles = vehicles.Where(v => v.LicenseNumber.Contains(filterConditions.LicenseNumber));
			}

			// Name condtion
			if (filterConditions.Name != null)
			{
				vehicles = vehicles.Where(v => v.Name.Contains(filterConditions.Name));
			}

			// GarageIDList condition
			if (filterConditions.GarageIDList != null)
			{
				vehicles = vehicles.Where(v => filterConditions.VehicleGroupIDList.Contains(v.GarageID));
			}

			// VehicleGroupIDList condtion
			if (filterConditions.VehicleGroupIDList != null)
			{
				vehicles = vehicles.Where(v => filterConditions.VehicleGroupIDList.Contains(v.VehicleGroupID));
			}

			// Max/Min Rating condition
			// Do not validate Max > Min here. Do it before this in the controller
			if (filterConditions.MaxRating != null &&
				filterConditions.MinRating != null)
			{
				vehicles = vehicles.Where(v => v.Star <= filterConditions.MaxRating
											&& v.Star >= filterConditions.MinRating);
			}

			// Run basic common filters
			var results = BasicFilter(vehicles, filterConditions).ToList().Select(v => new VehicleManagementItemJsonModel(v));

			// Sort
			// Validate OrderBy in controller
			if (filterConditions.OrderBy == null || nameof(VehicleManagementItemJsonModel.ID ) == filterConditions.OrderBy)
			{
				results = results.OrderBy(r => r.Name);
			}
			else
			{
				// Always sort by name after selected sorting prop
				if (nameof(VehicleManagementItemJsonModel.Name) == filterConditions.OrderBy)
				{
					results = filterConditions.IsDescendingOrder
						? results.OrderByDescending(r => r.Name)
						: results.OrderBy(r => r.Name);
				}
				else
				{
					var sortingProp = typeof(VehicleManagementItemJsonModel).GetProperty(filterConditions.OrderBy);
					results = filterConditions.IsDescendingOrder
						? results.OrderByDescending(r => sortingProp.GetValue(r))
						: results.OrderBy(r => sortingProp.GetValue(r));
				}
			}

			// Paginate
			var filteredRecords = results.Count();
			if ((filterConditions.Page - 1) * Constants.NUM_OF_SEARCH_RESULT_PER_PAGE > filteredRecords)
				filterConditions.Page = 1;

			results = results.Skip((filterConditions.Page - 1) * Constants.NUM_OF_SEARCH_RESULT_PER_PAGE)
					.Take(Constants.NUM_OF_SEARCH_RESULT_PER_PAGE);

			return new VehicleDataTablesJsonModel(results.ToList(), filterConditions.Draw, recordsTotal, filteredRecords);
		}

		// Run common filters on a vehicle list
		protected IQueryable<Vehicle> BasicFilter(IQueryable<Vehicle> vehicles , VehicelFilterConditionModel filterConditions)
		{
			// Transmission condition
			if (filterConditions.TransmissionTypeIDList != null)
				vehicles = vehicles.Where(v => filterConditions.TransmissionTypeIDList.Contains(v.TransmissionType));

			// Color condition
			if (filterConditions.ColorIDList != null)
				vehicles = vehicles.Where(v => filterConditions.ColorIDList.Contains(v.Color));

			// FuelType condition
			if (filterConditions.FuelTypeIDList != null)
				vehicles = vehicles.Where(v => filterConditions.FuelTypeIDList.Contains(v.FuelType));

			// Location condition
			if (filterConditions.LocationIDList != null)
				vehicles = vehicles.Where(v => filterConditions.LocationIDList.Contains(v.Garage.LocationID));

			// Category condition
			if (filterConditions.CategoryIDList != null)
				vehicles = vehicles.Where(v => v.Model.Categories.Any(r => filterConditions.CategoryIDList.Contains(r.ID)));


			// Max/Min ProductionYear condition
			// Do not validate Max > Min here. Do it before this in the controller
			if (filterConditions.MaxProductionYear != null && filterConditions.MinProductionYear != null)
				vehicles = vehicles.Where(v => v.Year <= filterConditions.MaxProductionYear
											&& v.Year >= filterConditions.MinProductionYear);

			// Brand and Model condition
			if (filterConditions.BrandIDList.Any() || filterConditions.ModelIDList.Any())
				vehicles = vehicles.Where(v => filterConditions.BrandIDList.Contains(v.Model.BrandID)
											|| filterConditions.ModelIDList.Contains(v.ModelID));

			return vehicles;
		}

		// Check to see if the vehicle is available
		public bool CheckVehicleAvailability(int vehicleId, DateTime startTime, DateTime endTime)
		{
			// Check if startTime is after SoonestPossibleBookingStartTimeFromNow
			// Check if startTime is before LatestPossibleBookingStartTimeFromNow
			// Check if endTime is after SoonestPossibleBookingEndTimeFromNow
			if (startTime < DateTime.Now.AddHours(Constants.SOONEST_POSSIBLE_BOOKING_START_TIME_FROM_NOW_IN_HOUR)
					|| startTime > DateTime.Now.AddDays(Constants.LATEST_POSSIBLE_BOOKING_START_TIME_FROM_NOW_IN_DAY)
					|| endTime < DateTime.Now.AddHours(Constants.SOONEST_POSSIBLE_BOOKING_END_TIME_FROM_NOW_IN_HOUR))
				return false;

			var vehicle = this.repository.Get(v => v.ID == vehicleId);

			if (vehicle.Any())
			{
				// Check StartTime/EndTime to be within garage's OpenTime ~ CloseTime
				// Compare by convert time to the number of minute from 00:00
				// Max margin of error: 60 secs vs CloseTime (Because we do not validate to second)

				// Booking StartTime
				var startTimeDoW = (int) startTime.DayOfWeek;
				var startTimeInMinute = startTime.Minute + startTime.Hour*60;
				vehicle = vehicle.Where(v =>
					v.Garage.GarageWorkingTimes.Any(gwt => gwt.DayOfWeek == startTimeDoW
								  && startTimeInMinute >= gwt.OpenTimeInMinute
								  && startTimeInMinute <= gwt.CloseTimeInMinute));

				// Booking EndTime
				var endTimeDoW = (int) endTime.DayOfWeek;
				var endTimeInMunute = endTime.Minute + endTime.Hour * 60;
				vehicle = vehicle.Where(v =>
					v.Garage.GarageWorkingTimes.Any(gwt => gwt.DayOfWeek == endTimeDoW
								  && endTimeInMunute >= gwt.OpenTimeInMinute
								  && endTimeInMunute <= gwt.CloseTimeInMinute));

				if (vehicle.Any())
				{
					// Check if this vehicle has any other bookings in the timespan of this booking
					vehicle = vehicle.Where(v =>
						v.BookingReceipts.Any(br => !br.IsCanceled
								&& (
										(startTime > br.StartTime && startTime < br.EndTime)
									 || (endTime > br.StartTime && endTime < br.EndTime)
									 || (startTime <= br.StartTime && endTime >= br.EndTime)
								)));
				}
			}

			return vehicle.Any();
		}
	}
}