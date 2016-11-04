﻿using CloudinaryDotNet.Actions;
using CRP.Controllers;
using CRP.Models;
using CRP.Models.Entities;
using CRP.Models.Entities.Services;
using CRP.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRP.Areas.Provider.Controllers
{
    [Authorize(Roles = "Provider")]
    public class DashboardController : BaseController
    {
        // GET: Provider/DashBoard
        [Authorize(Roles = "Provider")]
        [Route("dashboard/provider")]
        public ActionResult Dashboard()
        {
            DateTime day = DateTime.Now;
            DateTime fromDay = new DateTime();
            fromDay = day.AddMonths(-(day.Month));
            var providerID = User.Identity.GetUserId();
            var garageService = this.Service<IGarageService>();
            var bookingService = this.Service<IBookingReceiptService>();
            var userService = this.Service<IUserService>();

            var listGarage = garageService.Get(q => q.OwnerID == providerID).ToList();

            // Current month's report
            var now = DateTime.Now;
            var thisMonth = new DateTime(now.Year, now.Month, 1);

            var receipts = bookingService.Get(r => !r.IsPending && r.CustomerID != r.ProviderID
                                        && r.StartTime < now
                                        && r.StartTime.Month == thisMonth.Month
                                        && r.StartTime.Year == thisMonth.Year).ToList();

            ProviderReportViewModel model = new ProviderReportViewModel();
            model.NumOfGarage = listGarage.Count;
            model.NumOfVehicle = 0;
            foreach(var garage in listGarage)
            {
                model.NumOfVehicle += garage.Vehicles.Count;
            }
            model.NumOfBookingInThisMonth = receipts.Count;
            model.NumOfBookingSuccessfulInThisMonth = receipts.Where(r => r.IsCanceled == false).Count();

            if(receipts.Any(r => r.IsCanceled == false)) {
                model.Profit = receipts.Where(r => r.IsCanceled == false).Sum(r => r.RentalPrice);
            }

            model.ProviderUtil = userService.Get(providerID).IsProviderUntil;

            var receiptDes = bookingService.Get(r => !r.IsPending && r.CustomerID != r.ProviderID && !r.IsCanceled).OrderByDescending(r => r.StartTime).ToList();

            // last 10 comment
            for(int i = 0; i < receiptDes.Count; i++)
            {
                model.GetCommentData(receiptDes.ElementAt(i));
                if(model.Comment.Count == 10)
                {
                    break;
                }
            }

            // Calculate monthly reports for last half year
            for (var i = 1; i < 7; i++)
            {
                var reportTime = thisMonth.AddMonths(-i);
                receipts = bookingService.Get(r => r.StartTime < now
                                        && r.StartTime.Month == reportTime.Month
                                        && r.StartTime.Year == reportTime.Year).ToList();

                model.GetDataForReport(receipts, reportTime);
            }

            return View("~/Areas/Provider/Views/DashBoard/Index.cshtml", model);
        }
    }
}