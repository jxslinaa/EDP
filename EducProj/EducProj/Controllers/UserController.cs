using System;
using System.Collections.Generic;
using System.Linq;
using EducProj.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducProj.Controllers
{
    public class UserController : Controller
    {
        public readonly EducProjContext _contxt;
        public UserController(EducProjContext context)
        {
            _contxt = context;
        }
        //Listing all available courses
        public JsonResult getWorkshopList(int pageNo = 1)
        {
            IList<Workshopitems> lists = _contxt.Workshopitems.ToList();
            return Json(new { status = true, data = lists });
        }

        public IActionResult index()
        {
            string userType = HttpContext.Session.GetString("userType");
            int? userId   = HttpContext.Session.GetInt32("userId");

            ViewData["userId"] = (userId == null || userId == 0) ? "0" : userId.ToString();

            string CartSessionId = HttpContext.Session.Id;

            IList<ShoppingCartItem> cartItems = _contxt.ShoppingCartItem
                .Where(x => x.CartSessionId == CartSessionId)
                .ToList();

            decimal orderTotal = cartItems.Sum(x => x.OrderPrice);
            ViewData["cartCount"] = cartItems.Count;
            ViewData["orderTotal"] = orderTotal;
            return View();
        }
        //check processing viewing page 
        public IActionResult checkout()
        {
            string CartSessionId = HttpContext.Session.Id;
            int? userId        = HttpContext.Session.GetInt32("userId");
            if (userId == null || userId == 0)
                return RedirectToAction("Signup", "User");

            return View();
        }
        //login processing 
        public IActionResult login() { 
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId != null && userId != 0)
                return RedirectToAction("MyProfile", "Dashboard");
            ViewData["errorMsg"] = "";
            return View();
        }
        //login processing - steps mentioned inside
        [HttpPost]
        public IActionResult login(Users customer)
        {
            ViewData["errorMsg"] = "";
            string errMsg = "-NA-";
            if(customer != null) {
                //fetching user details from email - logged username
                Users loggedCustomer = _contxt.Users.Where(x => x.Email.Equals(customer.Email)).FirstOrDefault();
                //if the user details are empty then we raise error
                if (loggedCustomer == null)
                    errMsg = "Invalid username";
                else {
                    //if the user fetched through username then we start password check, 
                    //Steps: 1. Get the encryption key & hash the requested password, 
                    //2. Compare the hashed password, if ok then go for next , else stay login
                    //3. Check the user activation status,
                    //4. Set the session username & userid,
                    //5. Assign user id for temporarily created cart items 
                    //6. Go for dashboard based successful login
                    //For failed login update the failed login entries
                    string userPassword = Helper.EncodePassword(customer.Password, loggedCustomer.PasswordSalt.TrimStart().TrimEnd());
                    if (userPassword.Equals(loggedCustomer.Password))
                    {
                        if (loggedCustomer.isActive == 1) {
                            ViewData["errorMsg"] = "";
                            //step 4
                            HttpContext.Session.SetString("userName",loggedCustomer.FirstName);
                            HttpContext.Session.SetInt32("userId",loggedCustomer.UserId);
                            HttpContext.Session.SetString("userType", loggedCustomer.UserType.ToString());

                            var sessId = HttpContext.Session.Id;
                            //step 5
                            IList<ShoppingCartItem> cartList = _contxt.ShoppingCartItem.Where(x => x.CartSessionId.Equals(sessId)).ToList();
                            //if the user trying logged with cart items check out then go order preview details
                            //Else redirect to profile details
                            if (cartList != null && cartList.Count >= 1)
                            {
                                string updateQry = String.Format("UPDATE ShoppingCartItem SET CustomerId = {0} WHERE CartSessionId = '{1}'", loggedCustomer.UserId.ToString(), sessId);
                                var rst = _contxt.executeSql(updateQry);
                                return RedirectToAction("OrderPreview", "CartManager");
                            }
                            else {
                                return RedirectToAction("MyProfile", "Dashboard");
                            }
                        }
                    }
                    else
                    {
                        errMsg = "Invalid Password";
                        ViewData["errorMsg"] = errMsg;
                        loggedCustomer.FailedLogins = loggedCustomer.FailedLogins + 1;
                        _contxt.Users.Update(loggedCustomer);
                        _contxt.SaveChanges();
                        return View();
                    }
                }
            }
            ViewData["errorMsg"] = errMsg;

            return View();
        }



        //Steps: 1. Check the current request already having session,
        //2.if not show the page,
        //3. else stay in login page
        public IActionResult Signup()
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId != 0)
                return RedirectToAction("MyProfile", "Dashboard");

            ViewData["errorMsg"] = "";
            Users customer = new Users();
            return View(customer);
        }
            
        //step1: Create encryption key.
        //step2: validate all fields, Check for duplicate email entry, Check the password length is 6 letters
        //step3: Hash the password with created encryption key
        //step4: save all entries in database.
        [HttpPost]
        public IActionResult Signup(Users customer) 
        {
            string errorMsg = string.Empty;
            string saltString = "Abcfjlk12!@#$";// Helper.GeneratePassword(10);
           
            if (customer != null) {
                if (String.IsNullOrEmpty(customer.Email))
                {
                    errorMsg = "Email Required";
                }
                else if (String.IsNullOrEmpty(customer.FirstName))
                {
                    errorMsg = "Firstname Required";
                }
                else if (String.IsNullOrEmpty(customer.LastName))
                {
                    errorMsg = "Lastname Required";
                }
                else if (String.IsNullOrEmpty(customer.Password))
                {
                    errorMsg = "Password Required";
                }
                else if (_contxt.Users.Any(x => x.Email.Equals(customer.Email)))
                {
                    errorMsg = "Email exists";
                }
                else if (customer.Password.Length < 6)
                {
                    errorMsg = "Password must contains 6 character length";
                }
                else if (String.IsNullOrEmpty(customer.UserType))
                {
                    errorMsg = "Usertype is required";
                }
                if (String.IsNullOrEmpty(errorMsg))
                {
                    customer.CreatedDate = DateTime.Now;
                    customer.UpdatedDate = DateTime.Now;
                    customer.isActive = 1;
                    customer.EmailValidated = 1;
                    customer.PasswordSalt = saltString;
                    customer.Password = Helper.EncodePassword(customer.Password,saltString.TrimStart().TrimEnd());
                    _contxt.Users.Add(customer);
                    _contxt.SaveChanges();
                    ViewData["errorMsg"] = "-NA-";
                    return View(customer);
                }
                else {
                    ViewData["errorMsg"] = errorMsg;
                    return View(customer);
                }
            }
            ViewData["errorMsg"] = "Invalid Operation";
            return View(customer);
        }
        //step1:Get the user id
        //step2: Clear all temporary cart items ie, In Basket items
        //step3: Clear the session
        //step4: Redirect to login page
        public IActionResult logout() {
            try
            {
                int? userId = HttpContext.Session.GetInt32("userId");
                if (userId != 0)
                {
                    string updateQry = String.Format("DELETE FROM ShoppingCartItem Where CustomerId = {0}", userId);
                    var rst = _contxt.executeSql(updateQry);
                }

                HttpContext.Session.Clear();
            }
            catch (Exception e) {
                return RedirectToAction("login", "user");
            }
            return RedirectToAction("login", "user");
        }
    }
}

     