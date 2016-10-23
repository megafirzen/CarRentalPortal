﻿using AutoMapper.QueryableExtensions;
using CRP.Controllers;
using CRP.Models;
using CRP.Models.Entities;
using CRP.Models.Entities.Services;
using CRP.Models.JsonModels;
using CRP.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CRP.Areas.Provider.Controllers
{

	public class VehicleGroupManagementController : BaseController
	{
        //VehicleGroupService service = new VehicleGroupService();
		// Route to vehicleGroupManagement page
		[Route("management/vehicleGroupManagement")]
		public ViewResult VehicleGroupManagement()
		{
			return View("~/Areas/Provider/Views/VehicleGroupManagement/VehicleGroupManagement.cshtml");
		}

		//// Route to group's detailed info page
        [Authorize(Roles = "Provider")]
		[Route("management/vehicleGroupManagement/{id:int}")]
		public ViewResult VehicleGroupDetail(int id)
		{
            var service = this.Service<IVehicleGroupService>();
            var vehicleService = this.Service<IVehicleService>();
            VehicleGroupViewModel viewModel = this.Mapper.Map<VehicleGroupViewModel> (service.Get(id));
            var providerID = User.Identity.GetUserId();

            viewModel.listVehicle = vehicleService.Get()
                .Where(q => q.Garage.AspNetUser.Id == providerID && q.VehicleGroupID != id)
                .Select(q => new SelectListItem()
                {
                    Text = q.Name + " [Biển số: "+ q.LicenseNumber +"]" + "[Nhóm: "+ (q.VehicleGroupID != null ? q.VehicleGroup.Name : "-chưa có nhóm-" )+"]",
                    Value = q.ID.ToString(),
                    Selected = false,
                });

            viewModel.listGroup = service.Get()
                .Where(q => q.OwnerID == providerID && q.ID != id)
                .Select(q => new SelectListItem()
                {
                    Text = q.Name,
                    Value = q.ID.ToString(),
                    Selected = false,
                });

            return View("~/Areas/Provider/Views/VehicleGroupManagement/VehicleGroupDetail.cshtml", viewModel);
		}

		// API Route to get list of group
        [Authorize(Roles = "Provider")]
		[Route("api/vehicleGroups")]
		[HttpGet]
		public JsonResult GetVehicleGroupListAPI()
		{
            var userID = User.Identity.GetUserId();
            var service = this.Service<IVehicleGroupService>();
            var list = service.Get(q => q.OwnerID == userID).ToList();
            var result = list.Select(q => new IConvertible[] {
                q.ID,
                q.Name,
                q.PriceGroup1 != null ? (q.PriceGroup1.MaxRentalPeriod != null ? q.PriceGroup1.MaxRentalPeriod : null) : null,
                q.PriceGroup1 != null ? (q.PriceGroup1.DepositPercentage * 100m).ToString("#") + "%" : null,
                q.PriceGroup1 != null ? q.PriceGroup1.PerDayPrice.ToString() : null,
                q.PriceGroup != null ? (q.PriceGroup.MaxRentalPeriod != null ? q.PriceGroup.MaxRentalPeriod : null) : null,
                q.PriceGroup != null ? (q.PriceGroup.DepositPercentage * 100m).ToString("#") + "%" : null,
                q.PriceGroup != null ? q.PriceGroup.PerDayPrice.ToString() : null,
                q.Vehicles.Count,
                q.IsActive
            });
            return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
		}

        // Add a car to this group
        [Authorize(Roles = "Provider")]
        [Route("api/vehicleGroup/updateVehicle/{vehicleID:int}/{groupID:int}")]
        [HttpPatch]
        public async Task<JsonResult> UpdateVehicleInGroup(int vehicleID, int groupID)
        {
            if (!this.ModelState.IsValid)
            {
                return Json(new { result = false, message = "Invalid!" });
            }
            var service = this.Service<IVehicleService>();
            var vehicle = service.Get(vehicleID);
            if (groupID == 0)
            {
                vehicle.VehicleGroupID = null;
            } else
            {
                vehicle.VehicleGroupID = groupID;
            }
            
            await service.UpdateAsync(vehicle);
            return Json(new { result = true, message = "Update done!" });
        }

        // Reload dropdownlist after update vehicle in group
        [Authorize(Roles = "Provider")]
        [Route("api/vehicleList/{groupID:int}")]
        [HttpGet]
        public JsonResult VehiclesInGroup(int groupID)
        {
            var service = this.Service<IVehicleService>();
            var providerID = User.Identity.GetUserId();

            var listVehicle = service.Get()
                .Where(q => q.Garage.AspNetUser.Id == providerID && q.VehicleGroupID != groupID)
                .Select(q => new SelectListItem()
                {
                    Text = q.Name + " [Biển số: " + q.LicenseNumber + "]" + "[Nhóm: " + (q.VehicleGroupID != null ? q.VehicleGroup.Name : "-chưa có nhóm-") + "]",
                    Value = q.ID.ToString(),
                    Selected = false,
                }).ToList();
            return Json(new { list = listVehicle}, JsonRequestBehavior.AllowGet);
        }

        // Get all car in this group
        [Authorize(Roles = "Provider")]
        [Route("api/vehiclesInGroup/{id:int}")]
        [HttpGet]
        public JsonResult GetVehiclesInGroup(int id)
        {
            var service = this.Service<IVehicleService>();
            var list = service.Get(q => q.VehicleGroupID == id).ToList();
            var result = list.Select(q => new IConvertible[] {
                q.ID,
                q.Name,
                q.LicenseNumber,
                q.Color,
                q.Star
            });
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        // Show create popup
        [Route("management/vehicleGroupManagement/create")]
        [HttpGet]
        public ViewResult CreateVehicleGroup()
        {
            VehicleGroupViewModel viewModel = new VehicleGroupViewModel();
            return View("~/Areas/Provider/Views/VehicleGroupManagement/CreatePopup.cshtml", viewModel);
        }

		// API Route to create single new group
        [Authorize(Roles = "Provider")]
		[Route("api/vehicleGroups")]
		[HttpPost]
		public async Task<JsonResult> CreateVehicleGroupAPI(VehicleGroup model)
		{
            if (!this.ModelState.IsValid)
            {
                return Json(new { result = false, message = "Invalid!" });
            }
            var service = this.Service<IVehicleGroupService>();
            var priceGroupService = this.Service<IPriceGroupService>();
            var priceGroupItemService = this.Service<IPriceGroupItemService>();
            model.OwnerID = User.Identity.GetUserId();
            model.IsActive = true;
            
            var entity = this.Mapper.Map<VehicleGroup>(model);
            var priceGroupEntity = this.Mapper.Map<PriceGroup>(model.PriceGroup);
            var priceGroupItemsEntity = model.PriceGroup.PriceGroupItems;

            if(entity == null || priceGroupEntity == null || priceGroupItemsEntity == null || priceGroupItemsEntity.Count == 0)
            {
                return Json(new { result = false, message = "Create failed!" });
            }

            // create follow this step
            // 1
            await priceGroupService.CreateAsync(priceGroupEntity);
            //2
            entity.WithDriverPriceGroupID = priceGroupEntity.ID;
            await service.CreateAsync(entity);

            return Json(new { result = true, message = "Create successful!" });
        }

		// API Route to edit single group
		[Route("api/vehicleGroups")]
		[HttpPatch]
		public async Task<JsonResult> EditVehicleGroupAPI(VehicleGroupViewModel model)
		{
            if (!this.ModelState.IsValid)
            {
                return Json(new { result = false, message = "Invalid!" });
            }
            var service = this.Service<IVehicleGroupService>();
            var priceGroupService = this.Service<IPriceGroupService>();
            var priceGroupItemService = this.Service<IPriceGroupItemService>();

            var entity = this.Mapper.Map<VehicleGroup>(model);
            var priceGroupEntity = this.Mapper.Map<PriceGroup>(model.PriceGroup);
            var priceGroupItemEntity = this.Mapper.Map<PriceGroupItem>(model.PriceGroup.PriceGroupItems);

            if (entity == null || priceGroupEntity == null || priceGroupItemEntity == null)
            {
                return Json(new { result = false, message = "Update failed!" });
            }

            // update follow this step
            // 1
            await priceGroupItemService.UpdateAsync(priceGroupItemEntity);
            // 2
            await priceGroupService.UpdateAsync(priceGroupEntity);
            // 3
            await service.UpdateAsync(entity);

            return Json(new { result = true, message = "Update success!" });
        }

		// API Route to delete single group
		[Route("api/vehicleGroups/{id:int}")]
		[HttpDelete]
		public async Task<JsonResult> DeleteVehicleGroupAPI(int id)
		{
            var service = this.Service<IVehicleGroupService>();
            var priceGroupService = this.Service<IPriceGroupService>();
            var priceGroupItemService = this.Service<IPriceGroupItemService>();

            var entity = await service.GetAsync(id);
            if(entity != null)
            {
                var priceGroupEntity = await priceGroupService.GetAsync(entity.PriceGroup.ID);
                var priceGroupItemsEntity = priceGroupItemService.Get(q => q.PriceGroupID == priceGroupEntity.ID);
                if (priceGroupEntity != null)
                {
                    await service.DeleteAsync(entity);
                    foreach(var item in priceGroupItemsEntity)
                    {
                        await priceGroupItemService.DeleteAsync(item);
                    }
                    await priceGroupService.DeleteAsync(priceGroupEntity);
                    

                    return Json(new { result = true, message = "Delete success!" });
                }
            }

            return Json(new { result = false, message = "Delete failed!" });
        }

        // API Route to deactive/active vehicle group
        [Route("api/vehicleGroups/status/{id:int}")]
        [HttpPatch]
        public async Task<JsonResult> ChangeStatus(int id)
        {
            var service = this.Service<IVehicleGroupService>();
            var entity = await service.GetAsync(id);
            if(entity != null)
            {
                entity.IsActive = !entity.IsActive;
                await service.UpdateAsync(entity);
                return Json(new { result = true, message = "Change status success!" });
            }

            return Json(new { result = false, message = "Change status failed!" });
        }
    }
}