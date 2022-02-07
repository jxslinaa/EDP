using System;
using System.Collections.Generic;
using System.Linq;
using EducProj.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducProj.Controllers
{
    public class DashboardController : Controller
    {
        public readonly EducProjContext _contxt;
        public DashboardController(EducProjContext context)
        {
            _contxt = context;
        }
        //GET LIST IF WORKSHOP ITEMS AJAX REQUEST
        public JsonResult getWorkshopList(int pageNo = 1)
        {
            IList<Workshopitems> lists = _contxt.Workshopitems.Where(x=>x.Status==1).ToList();
            return Json(new { status = true, data = lists });

        }
        //Intializing the session used to session if regeneration conflict, we enabeld the guest checkout
        private void initSession() {
            HttpContext.Session.SetInt32("userId",0);
            HttpContext.Session.SetString("userName", string.Empty);
            HttpContext.Session.SetString("userType", "Guest");
            //userName
        }

        public IActionResult index()
        {
            //SESSION CHECK
            int? userId = (HttpContext.Session.GetInt32("userId"));
            string CartSessionId = HttpContext.Session.Id;

            if (userId == null || userId == 0)
                this.initSession();
            ViewData["userId"] = (userId == null || userId == 0) ? 0 : userId;
            
            IList<ShoppingCartItem> cartItems = _contxt.ShoppingCartItem.Where(x => x.CartSessionId == CartSessionId).ToList();
            decimal orderTotal = cartItems.Sum(x => x.OrderPrice);//BUCKET LIST ORDER TOTAL 
           
            ViewData["cartCount"] = cartItems.Count;
            ViewData["orderTotal"] = orderTotal;
            var model = (from items in _contxt.Workshopitems
                         where items.Status == 1
                         select new Workshopitems
                         {
                             SessionId = items.SessionId,
                            AvailableSlots = items.AvailableSlots,
                            CreatedBy = items.CreatedBy,
                            CreatedDate = items.CreatedDate,
                            EndLesDate = items.EndLesDate,
                            FirstLesDate = items.FirstLesDate,
                            SessionTitle = items.SessionTitle,
                            TotalLessons = items.TotalLessons,
                            SessionDetails = items.SessionDetails,
                            LocationCity = items.LocationCity,
                             LocationDetails = items.LocationDetails,
                             RegStartDate = items.RegStartDate,
                             RegEndDate = items.RegEndDate,
                             SessionCategory = items.SessionCategory,
                             SessionCost = items.SessionCost,
                             SessionSchedule = items.SessionSchedule,
                             TotalSlots = items.TotalSlots,
                             WorkshopImages = _contxt.WorkShopItemImages.Where(x=>x.SessionId == items.SessionId).ToList(),
                             CreatedByName = items.CreatedByName,
                         }).ToList();


            _contxt.Workshopitems.Where(x=>x.Status==1).ToList();
            return View(model);
        }

        //step checkout - Cart summary page
        //if the session is empty then redirect to signup / login page 
        [SessionCheck]
        public IActionResult checkout()
        {
            return RedirectToAction("OrderPreview","CartManager");
        }
        [SessionCheck]
        //CURRENT USER PPROFILE ie,LOGGED USER details
        public IActionResult MyProfile() {
            ViewData["errorMsg"] = "";
            try
            {
                int? userId = (int)(HttpContext.Session.GetInt32("userId"));
                Users customer = _contxt.Users.Where(x => x.UserId == userId).SingleOrDefault();
                return View(customer);
            }
            catch (Exception e) {
                return RedirectToAction("errorPage", "Dashboard");
            }
        }

        //user details update
        [HttpPost]
        [SessionCheck]
        public IActionResult MyProfile(Users updateCustomer)
        {
            ViewData["errorMsg"] = "";
            try
            {
                int userId = Int32.Parse(HttpContext.Session.GetString("userId"));
                Users customer = _contxt.Users.Where(x => x.UserId == userId).SingleOrDefault();
                customer.FirstName = updateCustomer.FirstName;
                customer.LastName = updateCustomer.LastName;
                customer.MobileNumber = updateCustomer.MobileNumber;
                customer.Email = updateCustomer.Email;
                _contxt.Users.Update(customer);
                _contxt.SaveChanges();
                ViewData["errorMsg"] = "Your profile updated successfully";
                return View(customer);
            }
            catch (Exception e)
            {
                ViewData["errorMsg"] = "";
                return RedirectToAction("errorPage", "Dashboard");
            }
        }
        public IActionResult WorkshopDetails(int id) {
            if (id != 0)
            {
                Workshopitems pageDetails = _contxt.Workshopitems.Where(x => x.SessionId == id).FirstOrDefault();
                pageDetails.WorkshopImages = _contxt.WorkShopItemImages.Where(x => x.SessionId == id).ToList();
                return View(pageDetails);
            }
            else {
                return NotFound();
            }
        }


    }
}