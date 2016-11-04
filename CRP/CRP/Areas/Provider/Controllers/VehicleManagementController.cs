﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRP.Models.ViewModels;
using CRP.Models.Entities.Services;
using CRP.Models.Entities;
using CRP.Models.JsonModels;
using CRP.Controllers;
using CRP.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using CloudinaryDotNet.Actions;
using Microsoft.Ajax.Utilities;
using System.IO;
using Constants = CRP.Models.Constants;

namespace CRP.Areas.Provider.Controllers
{
	[Authorize(Roles = "Provider")]
	public class VehicleManagementController : BaseController
	{

		// Route to vehicleManagement page
		[Route("management/vehicleManagement")]
		public ViewResult VehicleManagement()
		{
			var brandService = this.Service<IBrandService>();
			var brandList = brandService.Get(
				b => b.VehicleModels.Count != 0 // Only get brand w/ model
			).OrderBy(b => b.Name).ToList();

			var garageService = this.Service<IGarageService>();
			var providerID = User.Identity.GetUserId();
			var listGarage = garageService.Get(q => q.OwnerID == providerID)
				.Select(q => new SelectListItem()
				{
					Text = q.Name,
					Value = q.ID.ToString(),
					Selected = true,
				});

			var groupService = this.Service<IVehicleGroupService>();
			var groupList = groupService.Get(q => q.OwnerID == providerID)
				.Select(q => new SelectListItem()
				{
					Text = q.Name,
					Value = q.ID.ToString()
				});

			var viewModel = new FilterByGarageView()
			{
				listGarage = listGarage,
				GroupList = groupList,
				BrandList = brandList
			};

			return View("~/Areas/Provider/Views/VehicleManagement/VehicleManagement.cshtml", viewModel);
		}

		// Load listOtherGarage
		[Route("api/listOtherGarage/{garageID:int}")]
		[HttpGet]
		public JsonResult LoadOtherGarage(int garageID)
		{
			var service = this.Service<IGarageService>();
			FilterByGarageView garageView = new FilterByGarageView();
			var providerID = User.Identity.GetUserId();
			garageView.listGarage = service.Get()
				.Where(q => q.OwnerID == providerID && q.IsActive && q.ID != garageID)
				.Select(q => new SelectListItem()
				{
					Text = q.Name,
					Value = q.ID.ToString(),
					Selected = true,
				});

			return Json(new {list = garageView.listGarage}, JsonRequestBehavior.AllowGet);
		}


		[Route("api/listGroup")]
		[HttpGet]
		public JsonResult LoadGroupList()
		{
			var service = this.Service<IVehicleGroupService>();
			// just use it to return a list not for keeping data purpose
			FilterByGarageView garageView = new FilterByGarageView();
			var providerID = User.Identity.GetUserId();
			garageView.listGarage = service.Get()
				.Where(q => q.OwnerID == providerID)
				.Select(q => new SelectListItem()
				{
					Text = q.Name + " [" + (q.IsActive ? "đang hoạt động" : "ngưng hoạt động") + "]",
					Value = q.ID.ToString(),
					Selected = true,
				});

			return Json(new {list = garageView.listGarage}, JsonRequestBehavior.AllowGet);
		}

		// Route to vehicle's detailed info page
		[Route("management/vehicleManagement/{id:int}")]
		public ActionResult VehihicleDetail(int id)
		{
			var providerID = User.Identity.GetUserId();

			var vehicleService = this.Service<IVehicleService>();
			var garageService = this.Service<IGarageService>();
			var groupService = this.Service<IVehicleGroupService>();
			var brandService = this.Service<IBrandService>();

			Vehicle vehicle = vehicleService.Get(v => v.ID == id && v.Garage.OwnerID == providerID).FirstOrDefault();
			if (vehicle == null)
			{
				return new HttpStatusCodeResult(404, "Not found");
			}

			var viewModel = new VehicleDetailInfoViewModel(vehicle)
			{
				listGarage = garageService.Get()
					.Where(q => q.OwnerID == providerID)
					.Select(q => new SelectListItem()
					{
						Text = q.Name,
						Value = q.ID.ToString(),
						Selected = true,
					}),
				listGroup = groupService.Get()
					.Where(q => q.OwnerID == providerID)
					.Select(q => new SelectListItem()
					{
						Text = q.Name,
						Value = q.ID.ToString(),
						Selected = true,
					}),
				brandList = brandService.Get(b => b.VehicleModels.Count != 0)
					.OrderBy(b => b.Name)
					.ToList()
			};

			return View("~/Areas/Provider/Views/VehicleManagement/VehicleDetail.cshtml", viewModel);
		}


		// API Route to get a list of vehicle to populate vehicleTable
		// Sort needed
		// Pagination needed
		[Route("api/vehicles", Name = "GetVehicleListAPI")]
		[HttpGet]
		public ActionResult GetVehicleListAPI(VehicleManagementFilterConditionModel filterConditions)
		{
			if (filterConditions.Draw == 0)
				return new HttpStatusCodeResult(400, "Unqualified request");
			if (filterConditions.OrderBy != null
			    && typeof(VehicleManagementItemJsonModel).GetProperty(filterConditions.OrderBy) == null)
				return new HttpStatusCodeResult(400, "Invalid sorting property");

			filterConditions.ProviderID = User.Identity.GetUserId();

			var service = this.Service<IVehicleService>();
			var vehicles = service.FilterVehicle(filterConditions);

			return Json(vehicles, JsonRequestBehavior.AllowGet);
		}


		// API Route for getting vehicle's detailed infomations (for example, to duplicate vehicle)
		[Route("api/vehicles/{id}")]
		[HttpGet]
		public JsonResult GetVehicleDetailAPI(int id)
		{
			var service = this.Service<IVehicleService>();
			Vehicle vehicle = service.Get(id);

			return Json(new
			{
				Name = vehicle.Name,
				ModelID = vehicle.ModelID,
				Year = vehicle.Year,
				GarageID = vehicle.GarageID,
				VehicleGroupID = vehicle.VehicleGroupID,
				TransmissionType = vehicle.TransmissionType,
				TransmissionDetail = vehicle.TransmissionDetail,
				FuelType = vehicle.FuelType,
				Engine = vehicle.Engine,
				Color = vehicle.Color,
				Description = vehicle.Description
			}, JsonRequestBehavior.AllowGet);
		}


		// API Route to create single new vehicles
		[Route("api/vehicles")]
		[HttpPost]
		public async Task<ActionResult> CreateVehicleAPI(ManagingVehicleModel newVehicle)
		{
			var errorMessage = CheckVehicleValidity(newVehicle);
			if (errorMessage != null)
				return new HttpStatusCodeResult(400, errorMessage);

			var newVehicleEntity = this.Mapper.Map<Vehicle>(newVehicle);

			if(Request.Files.Count < 1 || Request.Files.Count > 10)
				return new HttpStatusCodeResult(400, "Chỉ được phép upload từ 1 đến 10 hình.");

			// Upload images to cloudinary
			var cloudinary = new CloudinaryDotNet.Cloudinary(Models.Constants.CLOUDINARY_ACC);
			var imageList = new List<VehicleImage>();
			try
			{
				foreach (string fileName in Request.Files)
				{
					var file = Request.Files[fileName];
					if (file?.ContentLength <= 0) continue;

					// Upload to cloud
					var uploadResult = cloudinary.Upload(new ImageUploadParams()
					{
						File = new FileDescription(file.FileName, file.InputStream)
					});

					// Get the image's id and url
					imageList.Add(new VehicleImage() { ID = uploadResult.PublicId, URL = uploadResult.Uri.ToString() });
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, "Upload ảnh thất bại. Vui lòng thử lại sau.");
			}

			var vehicleService = this.Service<IVehicleService>();
			await vehicleService.CreateAsync(newVehicleEntity);

			foreach (var image in imageList)
			{
				image.VehicleID = newVehicleEntity.ID;
				image.Vehicle = newVehicleEntity;
			}

			newVehicleEntity.VehicleImages = imageList;
			await vehicleService.UpdateAsync(newVehicleEntity);

			return new HttpStatusCodeResult(200, "Created successfully.");
		}


		// API Route to edit single vehicle
		[Route("api/vehicles/{id:int}")]
		[HttpPatch]
		public async Task<ActionResult> EditVehicleAPI(int id, ManagingVehicleModel updateModel)
		{
			var errorMessage = CheckVehicleValidity(updateModel);
			if (errorMessage != null)
				return new HttpStatusCodeResult(400, errorMessage);

			var vehicleService = this.Service<IVehicleService>();

			var currentUserID = User.Identity.GetUserId();
			var vehicleEntity = vehicleService.Get(v => v.ID == id && v.Garage.OwnerID == currentUserID).FirstOrDefault();

			if(vehicleEntity == null)
				return new HttpStatusCodeResult(404, "Không tìm thấy xe.");

			Mapper.Map<ManagingVehicleModel, Vehicle>(updateModel, vehicleEntity);
			await vehicleService.UpdateAsync(vehicleEntity);

			return new HttpStatusCodeResult(200, "Updated successfully.");
		}


		// API Route to delete single vehicle
		[Route("api/vehicles/{id:int}")]
		[HttpDelete]
		public async Task<ActionResult> DeleteVehiclesAPI(int id)
		{
			var currentUserID = User.Identity.GetUserId();

			var service = this.Service<IVehicleService>();
			var entity = service.Get(v => v.ID == id && v.Garage.OwnerID == currentUserID).FirstOrDefault();
			if (entity == null)
				return new HttpStatusCodeResult(404, "Không tìm thấy xe.");

			// Remove vehicle ref in booking receipt
			var vehicleReceiptService = this.Service<IBookingReceiptService>();
			var receiptEntities = vehicleReceiptService.Get(br => br.VehicleID == id);
			if (receiptEntities.Any())
            {
                foreach (var item in receiptEntities)
                {
                    item.VehicleID = null;
                }
            }

			// Remove all vehicle's images
			var vehicleImageService = this.Service<IVehicleImageService>();
			var vehicleImageEntities = vehicleImageService.Get(q => q.VehicleID == id);
			if (vehicleImageEntities.Any())
			{
				foreach (var item in vehicleImageEntities)
				{
					vehicleImageService.DeleteAsync(item);
				}
			}

			await service.DeleteAsync(entity);
			return new HttpStatusCodeResult(200, "Deleted successfully.");
		}


		// API Route to change garage of multiple vehicles
		[Route("api/vehicles/changeGarage/{garageID:int}")]
		[HttpPatch]
		public ActionResult ChangeGarageAPI(int garageID, List<int> listVehicleId)
		{
			var service = this.Service<IVehicleService>();
			List<Vehicle> lstVehicle = service.Get().ToList();
			List<Vehicle> listVehicleNeedChange = new List<Vehicle>();

			foreach (var item in listVehicleId)
			{
				Vehicle v = lstVehicle.FirstOrDefault(a => a.ID == item);
				v.GarageID = garageID;
				service.Update(v);
			}

			return new HttpStatusCodeResult(200, "Garage changed successfully.");
		}


		// API Route to change group of multiple vehicles
		[Route("api/vehicles/changeGroup/{groupID:int}")]
		[HttpPatch]
		public ActionResult ChangeGroupAPI(int groupID, List<int> listVehicleId)
		{
			var service = this.Service<IVehicleService>();
			List<Vehicle> lstVehicle = service.Get().ToList();
			List<Vehicle> listVehicleNeedChange = new List<Vehicle>();
			foreach (var item in listVehicleId)
			{
				Vehicle v = lstVehicle.FirstOrDefault(a => a.ID == item);
				v.VehicleGroupID = groupID;
				service.Update(v);
			}
			return new HttpStatusCodeResult(200, "Group changed successfully.");
		}


		// API route for getting booking receipts of a vehicle
		// Pagination needed
		// Order by booking's startTime, newer to older
		[Route("api/vehicles/bookings/{vehiceID:int}/{page:int?}")]
		[HttpGet]
		public JsonResult GetVehicleBookingAPI(int vehiceID, int page = 1)
		{
			var service = this.Service<IBookingReceiptService>();
			List<BookingReceipt> br = service.Get(q => q.VehicleID == vehiceID).ToList();
			br.Sort((x, y) => DateTime.Compare(x.StartTime, y.StartTime));
			return Json(br, JsonRequestBehavior.AllowGet);
		}


		// API route for creating an own booking
		[Route("api/vehicles/bookings")]
		[HttpPost]
		public async Task<HttpStatusCodeResult> CreateOwnBookingAPI(DateTime startTime, DateTime endTime, int vehicleID)
		{
			var bookingService = this.Service<IBookingReceiptService>();
			var vehicleService = this.Service<IVehicleService>();

			var currentUserID = User.Identity.GetUserId();

			var vehicle = vehicleService.Get(v => v.ID == vehicleID && v.Garage.OwnerID == currentUserID).FirstOrDefault();

			if(vehicle == null)
				return new HttpStatusCodeResult(400, "Not found.");

			var newBooking = new BookingReceipt()
			{
				CustomerID = currentUserID,
				ProviderID = currentUserID,
				StartTime = startTime,
				EndTime = endTime,
				GarageID = vehicle.GarageID,
				VehicleID = vehicleID,
			};

			await bookingService.CreateAsync(newBooking);

			return new HttpStatusCodeResult(200, "Created successfully.");
		}


		// API route for canceling an own booking
		[Route("api/vehicles/bookings/{receiptID:int}")]
		[HttpDelete]
		public async Task<ActionResult> CancelBookingAPI(int receiptID)
		{
			var currentUserID = User.Identity.GetUserId();
			var bookingService = this.Service<IBookingReceiptService>();
			var receipt = bookingService.Get(br => br.ID == receiptID
											&& br.ProviderID == currentUserID
											&& br.CustomerID == currentUserID)
										.FirstOrDefault();
			if(receipt == null)
				return new HttpStatusCodeResult(400, "Not found.");

			receipt.IsCanceled = true;
			await bookingService.UpdateAsync(receipt);

			return new HttpStatusCodeResult(200, "Deleted successfully");
		}


		[Route("api/vehicles/images/{vehicleID:int}")]
		[HttpPost]
		public async Task<ActionResult> SavePictureAPI(int vehicleID)
		{
			string userID = User.Identity.GetUserId();

			var vehicleService = this.Service<IVehicleService>();
			var vehicle = vehicleService.Get(v => v.ID == vehicleID && v.Garage.OwnerID == userID).FirstOrDefault();

			if(vehicle == null)
				return new HttpStatusCodeResult(400, "Not found.");

			if (vehicle.VehicleImages.Count > 10)
			{
				return new HttpStatusCodeResult(400, "Không thể lưu trữ hơn 10 ảnh.");
			}

			// Upload images to cloudinary
			var cloudinary = new CloudinaryDotNet.Cloudinary(Constants.CLOUDINARY_ACC);
			try
			{
				foreach (string fileName in Request.Files)
				{
					var file = Request.Files[fileName];
					if (file?.ContentLength <= 0) continue;

					// Upload to cloud
					var uploadResult = cloudinary.Upload(new ImageUploadParams()
					{
						File = new FileDescription(file.FileName, file.InputStream)
					});

					// Get the image's id and url
					vehicle.VehicleImages.Add(new VehicleImage() { ID = uploadResult.PublicId, URL = uploadResult.Uri.ToString() });
				}
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, "Upload ảnh thất bại. Vui lòng thử lại sau.");
			}

			return new HttpStatusCodeResult(200, "OK");
		}


		[Route("api/vehicles/images/{imageID}")]
		[HttpDelete]
		public async Task<ActionResult> DeletePictureAPI(string imageID)
		{
			var currentUserID = User.Identity.GetUserId();

			var vehicleImageService = this.Service<IVehicleImageService>();
			var entityImage = vehicleImageService.Get(img => img.ID == imageID
															&& img.Vehicle.Garage.OwnerID == currentUserID)
												.FirstOrDefault();

			await vehicleImageService.DeleteAsync(entityImage);

			return new HttpStatusCodeResult(200, "Deleted successfully");
		}


		// Check entity on create/update
		public string CheckVehicleValidity(ManagingVehicleModel vehicle)
		{
			var vehicleService = this.Service<IVehicleService>();
			var modelService = this.Service<IModelService>();

			var userService = this.Service<IUserService>();
			var userID = this.User.Identity.GetUserId();
			var currentUser = userService.Get(userID);

			//License number's uniquity
			if (vehicleService.Get().Any(v => v.LicenseNumber == vehicle.LicenseNumber))
				return "Xe với biển số xe này đã tồn tại.";

			if (vehicle.LicenseNumber.Length > 50)
				return "Biển số xe phải dưới 50 ký tự.";

			if (vehicle.Name.Length > 100)
				return "Tên xe phải dưới 100 ký tự.'";

			if (!modelService.Get().Any(m => m.ID == vehicle.ModelID))
				return "Dòng xe không tồn tại.";

			if (vehicle.Year < Models.Constants.MIN_YEAR || vehicle.Year > DateTime.Now.Year)
				return "Năm sản xuất không hợp lệ";

			if (currentUser.Garages.All(g => g.ID != vehicle.GarageID))
				return "Garage không tồn tại.";

			if(vehicle.VehicleGroupID != null && currentUser.Garages.All(g => g.ID != vehicle.VehicleGroupID))
				return "Nhóm xe không tồn tại.";

			if(!Models.Constants.TRANSMISSION_TYPE.ContainsKey(vehicle.TransmissionType))
				return "Loại hộp số không hợp lệ.";

			if (vehicle.FuelType != null && !Models.Constants.FUEL_TYPE.ContainsKey(vehicle.FuelType.Value))
				return "Loại nhiên liệu không hợp lệ";

			if (vehicle.TransmissionDetail != null && vehicle.TransmissionDetail.Length > 100)
				return "Chi tiết hộp số phải dưới 100 ký tự.";

			if (vehicle.Engine != null && vehicle.Engine.Length > 100)
				return "Chi tiết động cơ phải dưới 100 ký tự.";

			if (vehicle.Description != null && vehicle.Description.Length > 1000)
				return "Mô tả xe phải dưới 1000 ký tự.";

			if (!Models.Constants.COLOR.ContainsKey(vehicle.Color))
				return "Mã màu không hợp lệ.";

			return null;
		}
	}
}