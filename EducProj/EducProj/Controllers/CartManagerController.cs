using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using EducProj.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
/* FileName: Manage Cart Actions */
namespace EducProj.Controllers
{
    public class CartManagerController : Controller
    {

        private string[] imgExtensions = { ".jpg", ".png", ".jpeg", ".bmp" };
        /* DB CONTEXT INTIALIZE */
        public readonly EducProjContext _contxt;
        public IActionResult Index()
        {
            return View();
        }
     
        public CartManagerController(EducProjContext context)
        {
            _contxt = context;
        }
        /* AJAX REQUEST SHOPPING CART CART ITEMS LIST */
        public JsonResult getShoppingCartItems() {
            try
            {
                /* GETTING THE RECENTLY CREATED SESSION ID  */
                string CartSessionId = HttpContext.Session.Id;
                /* GETTING THE LIST OF BUCKET ITEMS   */
                IList<ShoppingCartItem> cartItems = _contxt.ShoppingCartItem.Where(x => x.CartSessionId == CartSessionId).ToList();
                /* ORDER TOTAL CALCULATION   */
                double orderTotal = (double)cartItems.Sum(x => x.OrderPrice);
                /* CHECKING THE CART ITEMS LIST   */
                if (cartItems != null)
                    return Json(new { status = true, cartCount = cartItems.Count, cartTotal = orderTotal });
                else
                    return Json(new { status = false, cartCount = 0, cartTotal = 0 });
            }
            catch (Exception e) {
                return Json(new { status = false, cartCount = 0, cartTotal = 0 });
            }
        }


        /* BUCKET ORDER PREVIEW   */
        [SessionCheck]
        public IActionResult OrderPreview()
        {
            string cartSummary = string.Empty;
            /* GET THE CURRENT SESSION ID AGAINST THE GUEST USER   */
            string CartSessionId = HttpContext.Session.Id;//cart session id stored in browser cookie - session key
            //checking the logged user details
            int? loggedUserId = HttpContext.Session.GetInt32("userId");

            if (loggedUserId == null && loggedUserId == 0)//checking the user session details if not show the sign up/login form 
                return RedirectToAction("Signup", "User");
            //invalid parsing will raise exception for this created the second if condition
           
            if (loggedUserId == 0)//checking the user session details if not show the sign up/login form 
                return RedirectToAction("Signup", "User");

            /* LISTING THE BUCKET ITEMS AGAINST THE  GUEST USER SESSION ID  */
            //IList<ShoppingCartItem> cartItems = _contxt.ShoppingCartItem.Where(x => x.CustomerId == loggedUserId).ToList();
            IList<ShoppingCartItem> basketItems = (from buckets in _contxt.ShoppingCartItem
                                                   where buckets.CustomerId == loggedUserId 
                                      select new ShoppingCartItem
                                      {
                                          CartItemId = buckets.CartItemId,
                                          OrderPrice = buckets.OrderPrice,
                                          Quantity = buckets.Quantity,
                                          ProductId = buckets.ProductId,//acting as linked key with bucket item product id & workshop item id
                                          CustomerId = buckets.CustomerId,//current customer product id
                                          CartSessionId = buckets.CartSessionId,
                                          ShoppingCartType = buckets.ShoppingCartType,
                                          CreatedDate = buckets.CreatedDate,
                                          UpdatedDate = buckets.UpdatedDate,
                                          workshopitems = (from items in _contxt.Workshopitems
                                                           where items.SessionId == buckets.ProductId
                                                           select new Workshopitems
                                                           {
                                                               
                                                               SessionId = items.SessionId,
                                                               AvailableSlots = items.AvailableSlots,
                                                               CreatedBy = items.CreatedBy,
                                                               CreatedByName = items.CreatedByName,
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
                                                               WorkshopImages = _contxt.WorkShopItemImages.Where(x => x.SessionId == items.SessionId).ToList(),
                                                              
                                                           }).FirstOrDefault(),
                                      }).ToList();
            //The above query simplified in sql like below,
            /** select a.* from ShoppingCartItem as 
             * left join Workshopitems as b On a.ProductId = b.session id where a.customerid = loggedUserId
            /* GENERATING THE CART SUMMARY  */
            if (basketItems.Count >= 1)
            {

                /* GENERATING CHECKOUT ALONG WITH THIS CART SUMMARY , WE DONT WANT TO ASSIGN AS HIDDEN FORM IT WILL MAKE SECURITY ISSUE AGAINST BROWSER*/

            }
            else {
            }
            ViewData["cartSummary"] = cartSummary;
            /* IT WILL RENDER THE VIEW FILE ALONG WITH THIS TEMPORARY CART LIST ie, Basket List*/

            return View(basketItems);
        }

        [HttpPost]
        public JsonResult OrderBooking(BookingDetails bookingdetails) {
            string errMsg = "";
            string errKey = "";
            string cartSummary = string.Empty;
            /* GET THE CURRENT SESSION ID AGAINST THE GUEST USER   */
            string CartSessionId = HttpContext.Session.Id;//cart session id stored in browser cookie - session key
            //checking the logged user details
            int? _userId = HttpContext.Session.GetInt32("userId");

            if (_userId == null || _userId ==0)//checking the user session details if not show the sign up/login form 
                return Json(new { status=false,message="Please login to continue"});
            int loggedUserId = (int)(_userId);
            if(bookingdetails.CustomerId != loggedUserId)
                return Json(new { status = false, message = "Refresh the page and continue with login" });

            if (String.IsNullOrEmpty(bookingdetails.SessionId))
            {
                errMsg = "Internal Error occured, try again later";
                errKey = "CommonErr";
            }
            else if (String.IsNullOrEmpty(bookingdetails.FirstName))
            {
                errMsg = "Firstname is required";
                errKey = "FirstNameErr";
            }
            else if (!bookingdetails.FirstName.All(Char.IsLetter))
            {
                errMsg = "Alphabets only allowed";
                errKey = "FirstNameErr";
            }
            else if (String.IsNullOrEmpty(bookingdetails.LastName))
            {
                errMsg = "Lastname is required"; errKey = "LastNameErr";
            }
            else if (!bookingdetails.LastName.All(Char.IsLetter))
            {
                errMsg = "Alphabets only allowed "; errKey = "LastNameErr";
            }
            else if (String.IsNullOrEmpty(bookingdetails.Email))
            {
                errMsg = "Email is required"; errKey = "EmailErr";
            }
            else if (String.IsNullOrEmpty(bookingdetails.PhoneNumber))
            {
                errMsg = "PhoneNumber is required"; errKey = "PhoneNumberErr";
            }
            else if (!bookingdetails.PhoneNumber.All(Char.IsNumber))
            {
                errMsg = "Phonenumber invalid";
                errKey = "PhoneNumberErr";
            }
            else if (!validateEmail(bookingdetails.Email))
            {
                
                errMsg = "Invalid email"; errKey = "EmailErr";
            }
            else if (bookingdetails.CustomerId == 0)
            {
                errMsg = "Invalid email"; errKey = "EmailErr";
                errMsg = "Invalid email"; errKey = "EmailErr";
            }

            if (string.IsNullOrEmpty(errMsg)) {
                bookingdetails.PaymentStatus = 0;
                bookingdetails.CreatedDate = DateTime.Now;

                _contxt.BookingDetails.Add(bookingdetails);
                _contxt.SaveChanges();
                HttpContext.Session.SetInt32("paymentStep",0);
                HttpContext.Session.SetInt32("bookingId", bookingdetails.BookingId);

                return Json(new { status = true, Message = "Booking started", errMsgs = errMsg, errKey= errKey });
            }
            else { 
                return Json(new { status = false, Message = "", errMsgs = errMsg, errKey = errKey });
            }
        }
        [SessionCheck]
        public IActionResult ConfirmOrderPayment() {
            try
            {
                int? _userId = HttpContext.Session.GetInt32("userId");

                if (_userId == null || _userId == 0)//checking the user session details if not show the sign up/login form 
                    throw new Exception();

                int loggedUserId = (int)_userId;
                ConfirmOrderPayment paymentDetails = new ConfirmOrderPayment();
                int paymentStep = (int)HttpContext.Session.GetInt32("paymentStep");
                int bookingId = (int)HttpContext.Session.GetInt32("bookingId");
                ViewData["errKey"] = "";
                ViewData["errMsg"] = "";
                return View(paymentDetails);
            }
            catch (Exception e)
            {
                return RedirectToAction("login", "user");
            }

            ViewData["errKey"] = "";
            ViewData["errMsg"] = "";
            return View(null);
        }

        [HttpPost]
        public JsonResult ConfirmOrderPayment(ConfirmOrderPayment paymentDetails)
        {
            try
            {
                int? _userId = HttpContext.Session.GetInt32("userId");
                string cartSessionId = "";
            if (_userId == null || _userId == 0)//checking the user session details if not show the sign up/login form 
                throw new Exception();
            int loggedUserId = (int)(_userId);
            int paymentStep = (int)HttpContext.Session.GetInt32("paymentStep");
            int bookingId = (int)HttpContext.Session.GetInt32("bookingId");
           
                     if (ModelState.IsValid)
                      {

                    /*  IF ALL PASSES THEN WILL GET THE CURRENT SESSION ID AND LIST THE BUCKET LIST FROM SHOPPINGCARTITEMS TABLE */
                    IList<ShoppingCartItem> cartItems = _contxt.ShoppingCartItem.Where(x => x.CustomerId == loggedUserId).ToList();
                    if (cartItems.Count >= 1)
                    {
                        decimal orderTotal = cartItems.Sum(x => x.OrderPrice);//ORDER TOTAL 
                                                                              //PROCESSIGN PAYMENT IF THE PAYMENT SUCCEEDS MOVE THE BUCKET LIST INTO ORDER TABLE
                        Orders newOrder = new Orders();
                        newOrder.CartTotal = orderTotal;
                        newOrder.CustomerId = loggedUserId;
                        newOrder.CreatedDate = DateTime.Now;
                        _contxt.Orders.Add(newOrder);
                        _contxt.SaveChanges();

                        foreach (ShoppingCartItem item in cartItems)
                        {
                            Workshopitems prod_detaisl = _contxt.Workshopitems.Where(x => x.SessionId == item.ProductId).FirstOrDefault();
                            OrderItems orderItem = new OrderItems();
                            //MOVING THE BUCKET LIST INTO ORDER ITEMS TABLE
                            orderItem.CartId = newOrder.CartId;
                            orderItem.ProductId = item.ProductId;
                            orderItem.Quantity = item.Quantity;
                            orderItem.ProductPrice = item.OrderPrice;
                            orderItem.CustomerId = item.CustomerId;
                            orderItem.CreatedDate = DateTime.Now;
                            orderItem.productTitle = prod_detaisl.SessionTitle;
                            orderItem.AssignedTeacher = prod_detaisl.CreatedBy;
                            orderItem.BookingStatus = "Inprogress";
                            _contxt.OrderItems.Add(orderItem);
                            //REMOVING THE TEMPORARY BUCKET LIST
                            _contxt.ShoppingCartItem.Remove(item);
                            _contxt.SaveChanges();
                            cartSessionId = item.CartSessionId;
                        }
                        string err = "UPDATE BookingDetails SET CartId = " + newOrder.CartId + " WHERE SessionId = '" + cartSessionId + "'";
                        try
                        {
                            // _contxt.BookingDetails.Where(x => x.SessionId.Equals(item.CartSessionId)).
                            _contxt.executeSql("UPDATE BookingDetails SET CartId = " + newOrder.CartId + " WHERE SessionId = '" + cartSessionId + "'");
                        }
                        catch (Exception e)
                        {
                            err = e.Message;
                        }


                    }
                    

                    BookingDetails bookingDetails = new BookingDetails();

                    string previewHtml = "";
                    previewHtml += "<div class='container-fluid '><div class='card'><div class='card-body bg-white p-1 m-1'><div class='jumbotron text-center'>";

                    previewHtml += "<h1 class='display-3'>Thank You! "+ bookingDetails.FirstName + " " + bookingDetails.LastName + "</h1>";

                    previewHtml += " <p class='lead'><strong>Payment Successful</strong> for this transaction. A receipt for this purchase has been sent to your email.</p>You have been successfully charged for this transaction.<hr><p><img src='http://localhost:56634/images/tickicon.png' style='width: 50vh '/></p><p>Having trouble? <a href=''>Contact us</a></p><p class='lead'><a class='btn btn-primary btn-sm' href='../' role='button'>Continue to homepage</a></p></div></div></div></div>";


                    ViewData["name"] = bookingDetails.FirstName + " " + bookingDetails.LastName;
                    return Json(new { status=true,message= "showSuccessMessage", previewHtml = previewHtml });
                }
                return Json(new { status = false, message = "showErrorMessage",errMsg = ModelState.Values.ToList() });
            }
            catch (Exception e)
            {
                return Json(new { status = false, message = "showLoginMessage" });

                
            }
            return Json(new { status = false, message = "showNullMessage" });
        }

        [SessionCheck]
        public IActionResult previewOrder(int orderId) {
            string responseHtml = "";
            ViewData["orderHtml"] = getPreviewString(orderId);// responseHtml;
           // MailMessage msg = new MailMessage();
          //  msg.From = new MailAddress("@gmail.com");
          //  msg.To.Add("@gmail.com");
         //   msg.Subject = "test";
          //  msg.Body = "Test Content";
           // msg.Priority = MailPriority.High;

           // SmtpClient client = new SmtpClient();

           // client.Credentials = new NetworkCredential("@gmail.com", "", "smtp.gmail.com");
           // client.Host = "smtp.gmail.com";
           // client.Port = 587;
           // client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.EnableSsl = true;
            //client.UseDefaultCredentials = true;

            ////client.Send(msg);
           

            return View();
        }
        private string getPreviewString(int orderId) {
            string responseHtml = "";
            try
            {
                int? _userId = HttpContext.Session.GetInt32("userId");

                if (_userId == null || _userId == 0)//checking the user session details if not show the sign up/login form 
                    throw new Exception();
                int loggedUserId = (int)(_userId);
                Orders orderDetails = _contxt.Orders.Where(x => x.CartId == orderId).FirstOrDefault();
                IList<OrderItems> orderItems = _contxt.OrderItems.Where(x => x.CartId == orderId).ToList();
                responseHtml = "<table class='table table-bordered'>";
                double orderTotal = 0.0;
                foreach (OrderItems items in orderItems)
                {
                    Workshopitems product = _contxt.Workshopitems.Where(x => x.SessionId == items.ProductId).FirstOrDefault();
                    IList<WorkShopItemImages> wrkshopImgs = _contxt.WorkShopItemImages.Where(x => x.SessionId == items.ProductId).ToList();
                    responseHtml += "<tr><td colspan='2'>" + product.SessionTitle + "</td></tr>";

                    responseHtml += "<tr><td colspan='2'><img src='../images/" + wrkshopImgs.FirstOrDefault().ImageDetails + "'/></td></tr>";
                    responseHtml += "<tr><td colspan='2'>" + product.SessionDetails + "</td></tr>";
                    responseHtml += "<tr><td>" + product.LocationCity + "</td><td>" + String.Format("{0:C}", items.ProductPrice) + "</td></tr>";
                    orderTotal += (double)items.ProductPrice;
                    responseHtml += "<tr><td>" + product.LocationDetails + "</td><td>" + product.RegEndDate + "</td></tr>";
                    responseHtml += "<tr><td>" + (product.AvailableSlots - product.TotalSlots) + "/" + product.TotalSlots + " have signed up!</td><td>" + product.RegEndDate + "</td></tr>";


                    responseHtml += "<tr><td>" + items.CreatedDate + "</td></tr>";

                }
                BookingDetails bookings = _contxt.BookingDetails.Where(x => x.CartId == orderId).FirstOrDefault();
                responseHtml += "<tr><td colspan='2'>Booking Details</td></tr>";

                responseHtml += "<tr><td>" + bookings.FirstName + " " + bookings.LastName + "</td><td>" + bookings.Email + "</td></tr>";
                responseHtml += "<tr><td>" + bookings.PhoneNumber + "</td><td>" + bookings.NumberOfAttendees + "</td></tr>";
                responseHtml += "<tr><td>Order Total</td><td>" + String.Format("{0:C}", orderTotal) + "</td></tr></table>";

                responseHtml += "</table>";
            }
            catch (Exception e)
            {
                return "";
            }
            return responseHtml;
        }



        private bool validateEmail(string email) {
            Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.CultureInvariant | RegexOptions.Singleline);

            return regex.IsMatch(email)?true:false;
        }

        //Remove Cart
        //We should allow the user to remove the created basket items only , we dont want to allow the user to remove the cart with cart id.- This is get request only so we should restrict the id mapping . Security Feature!.
        // Remove the cart items with cart id & session id
        //STEPS:
        //1. GET THE CURRENT USER ID 
        //2. The user id is empty then get current session id
        //
        [SessionCheck]
        [HttpDelete]
        public JsonResult removeCart(int cartItemId) {
            try
            {
                var sessId = HttpContext.Session.Id;
                var userId = HttpContext.Session.GetInt32("userId");
                if (userId == null || userId == 0)//for guest user basket items removal
                {
                    ShoppingCartItem cartItem = _contxt.ShoppingCartItem
                        .Where(x => x.CartSessionId == sessId && x.CartItemId == cartItemId)
                        .FirstOrDefault();
                    _contxt.ShoppingCartItem.Remove(cartItem);
                    _contxt.SaveChanges();
                    return Json(new { status = true, message = "Item Removed from your basket" });
                }
                else
                {//for registered user basket items removal
                    int customerId = (int)(userId);
                    ShoppingCartItem cartItem = _contxt.ShoppingCartItem
                            .Where(x => x.CustomerId == customerId && x.CartItemId == cartItemId)
                            .FirstOrDefault();
                    _contxt.ShoppingCartItem.Remove(cartItem);
                    _contxt.SaveChanges();
                    return Json(new { status = true, message = "Item Removed from your basket" });
                }
            }
            catch (Exception e) {
                return Json(new { status = false, message = "Error removing cartitem" });
            }
        }
        /* CONFIRM SUMMARY OF THE CUSTOMER ORDER */
        //Steps:
        //1. List all basket items and move the basket items to orders entry, order items used to list all product here one-to-many relationship system implemented
        [HttpPost] /* THIS FUNCTION EXECUTE ONLY HTTP POST REQUESTS*/
        [SessionCheck]
        public IActionResult confirmOrder(string cardDetails) {
            /* USER SESSION CHECKING */
            int? _userId = HttpContext.Session.GetInt32("userId");
            if (_userId == null || _userId ==0)
            {
                return RedirectToAction("Signup", "User");
                /* EOF USER SESSION CHECKING */
            }
            else if (String.IsNullOrEmpty(cardDetails))/*  CARD NUMBER IS EMPTY CHECK ITS EMPTY THEN WE REDIRECT TO PREVIOUS PAGE WITH ERROR*/
            {
                return RedirectToAction("confirmOrder", "CartManager");
            }
            else if (cardDetails.Length != 16)//HERE WE CAN IMPLEMENT PAYMENT PROCESSING STAGE
            {
                /*  CARD NUMBER LENGTH CHECK */
                
                return RedirectToAction("confirmOrder", "CartManager");
            }
            else
            {
                /*  IF ALL PASSES THEN WILL GET THE CURRENT SESSION ID AND LIST THE BUCKET LIST FROM SHOPPINGCARTITEMS TABLE */
                int loggedUserId = (int)(_userId);
                IList<ShoppingCartItem> cartItems = _contxt.ShoppingCartItem.Where(x => x.CustomerId == loggedUserId).ToList();
                if (cartItems.Count >= 1)
                {
                    decimal orderTotal = cartItems.Sum(x => x.OrderPrice);//ORDER TOTAL 
                    //PROCESSIGN PAYMENT IF THE PAYMENT SUCCEEDS MOVE THE BUCKET LIST INTO ORDER TABLE
                    Orders newOrder = new Orders();
                    newOrder.CartTotal = orderTotal;
                    newOrder.CustomerId = loggedUserId;
                    newOrder.CreatedDate = DateTime.Now;
                    _contxt.Orders.Add(newOrder);
                    _contxt.SaveChanges();

                    foreach (ShoppingCartItem item in cartItems)
                    {
                        OrderItems orderItem = new OrderItems();
                        //MOVING THE BUCKET LIST INTO ORDER ITEMS TABLE
                        orderItem.CartId = newOrder.CartId;
                        orderItem.ProductId = item.ProductId;
                        orderItem.Quantity = item.Quantity;
                        orderItem.ProductPrice = item.OrderPrice;
                        orderItem.CustomerId = item.CustomerId;
                        orderItem.CreatedDate = DateTime.Now;
                        orderItem.productTitle = _contxt.Workshopitems.Where(x => x.SessionId == item.ProductId).FirstOrDefault().SessionTitle;


                        _contxt.OrderItems.Add(orderItem);
                        //REMOVING THE TEMPORARY BUCKET LIST
                        _contxt.ShoppingCartItem.Remove(item);
                        _contxt.SaveChanges();


                    }
                }
                //REDIRECTING TO MY ORDERS PAGE WITH SUCCESS MESSAGE, NEED TO SET THE SUCCESS MESSAGE
                return RedirectToAction("MyOrders", "CartManager",new { msg="Product Successfully added in your subscription"});
            }
        }
        //MY ORDERS VIEW PAGE
        //Recent 10 orders only it will took 
        //we can change the max limit with pagination with skip option 

        public IActionResult MyOrders() {
            //SESSION CHECKING
            int? _userId = HttpContext.Session.GetInt32("userId");
            if (_userId == null || _userId == 0)
                return RedirectToAction("Signup", "User");
            //EOF SESSION CHECKING
            int loggedUserId = (int)_userId;//CONVERTING STRING TYPE USER ID INTO INTEGER FOR FETCING FROM THE TABLE
            //LISTING THE SUCCESS ORDERS
            //IList<Orders> myorders = _contxt.Orders.Where(x => x.CustomerId == loggedUserId).ToList();
            IList<Orders> myorders = (from ord in _contxt.Orders
                       select new Orders { 
                            CartId = ord.CartId,
                            CartTotal = ord.CartTotal,
                            CreatedDate = ord.CreatedDate,
                            orderItems = _contxt.OrderItems.Where(x=>x.CartId.Equals(ord.CartId)).ToList()
                       }).OrderByDescending(c=>c.CartId).Take(10).ToList();
            //The above is the order and sub queries the order items list 
            //we dont want the below approach
                // foreach (Orders items in myorders) {
                //    items.orderItems = _contxt.OrderItems.Where(x => x.CartId == items.CartId).ToList();
                //}
            return View(myorders);
        }

        /** MAIN ACTION - ADD TO CART PAGE **/
        //step1: Session checking &  Get the product id & check for available slots, 
        //step2: Insert the records into basket
        //Here the Guest cart adding functionality enabled after successfull login the temporary cart items got updated automatically using their sessin id
      
        public IActionResult addToCart(int productId,int userId=0)
            {
            if(productId == 0)
                return Json(new { status = false,message="Invalid Product Id" });
            try
            {
                /** GETTING THE PRODUCTS AND SET INTO BUCKET LIST HERE THE SESSION ID NOT CHECKED ANY ONE CAN ADD INTO BUCKET LIST, WE CAN TRACK LATER USIGN SESSION ID **/
                // x.SessionId = Workshopitems ITEMS SESSION ID
                Workshopitems product = _contxt.Workshopitems.Where(x => x.SessionId == productId).FirstOrDefault();
                if(product == null)
                    return Json(new { status = false, message = "Invalid Product Id" });

                //checking the slots availability
                if (product.AvailableSlots != 0) {
                    //WE NEED TO UPDATE SLOT AVAILABILITY ON CONFIRM CHECKOUT
                    int? loggedUserId = HttpContext.Session.GetInt32("userId");
                    if (loggedUserId != null && loggedUserId != 0)
                        userId = (int)(loggedUserId);

                    string CartSessionId = HttpContext.Session.Id;
                    ShoppingCartItem cartItem = new ShoppingCartItem();
                    cartItem.ProductId = product.SessionId;
                    cartItem.Quantity = 1;
                    cartItem.CustomerId = userId;

                    cartItem.OrderPrice = (decimal)product.SessionCost;
                    cartItem.ShoppingCartType = userId == 0 ? 0 : 1;//0 - guest check out , 1 - registered user checkout
                    cartItem.CartSessionId = CartSessionId;

                    cartItem.CreatedDate = DateTime.Now;
                    cartItem.UpdatedDate = DateTime.Now;
                    _contxt.ShoppingCartItem.Add(cartItem);
                    _contxt.SaveChanges();
                    return RedirectToAction("Index", "Dashboard");
                }
                
            }
            catch (Exception e) {
                return RedirectToAction("Index", "Dashboard");
            }
            return RedirectToAction("Index", "Dashboard");
        }
        [SessionCheck]
        public IActionResult MyWorkshops() {
            int? loggedUserId = (int)(HttpContext.Session.GetInt32("userId"));

            IList<Workshopitems> itemLists = (from items in _contxt.Workshopitems
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
                                                 WorkshopImages = _contxt.WorkShopItemImages.Where(x => x.SessionId == items.SessionId).ToList(),
                                                 CreatedByName = items.CreatedByName,
                                             }).ToList();

            /*(from Workshopitems in _contxt.Workshopitems
             where Workshopitems.CreatedBy == loggedUserId
             select new Workshopitems
             {
                 SessionId = Workshopitems.SessionId,
                 RegStartDate = Workshopitems.RegStartDate,
                 RegEndDate = Workshopitems.RegEndDate,
                 FirstLesDate = Workshopitems.FirstLesDate,

                 EndLesDate = Workshopitems.EndLesDate,
                 SessionTitle = Workshopitems.SessionTitle,
                 SessionCategory = Workshopitems.SessionCategory,
                 SessionCost = Workshopitems.SessionCost,
                 CreatedDate = Workshopitems.CreatedDate,
                 Status = Workshopitems.Status
                 
             }).ToList();*/

            return View(itemLists);
        }
        public IActionResult MyBookings()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            IList<OrderItems> list = _contxt.OrderItems.Where(x => x.AssignedTeacher == loggedUserId).ToList();
            return View(list);
        }
        public IActionResult CreateWorkshop() {
            ViewData["errorMsg"] = "";
            Workshopitems newWorkshopitem = new Workshopitems();
            return View(newWorkshopitem);
        }

        [HttpGet]
        [SessionCheck]
        [Route("ViewMySessionDetails/{id}/{message?}")]
        public IActionResult ViewMySessionDetails(int id,string message = "") {
            if (id != 0) {
                Workshopitems mySession = _contxt.Workshopitems.Where(x => x.SessionId == id).FirstOrDefault();
                mySession.WorkshopImages = _contxt.WorkShopItemImages.Where(x => x.SessionId == id).ToList();
                ViewData["errorMsg"] = message;
                return View(mySession);
            }
            else {
                return NotFound();
            }
        }
        [HttpPost]
        [SessionCheck]
        public IActionResult updateMySessionDetails(Workshopitems updatedItem) {
            if (updatedItem != null)
            {
                // Workshopitems mySession = _contxt.Workshopitems.Where(x => x.SessionId == /updatedItem.SessionId).FirstOrDefault();
                updatedItem.CreatedDate = DateTime.Now;
               _contxt.Workshopitems.Update(updatedItem);
                _contxt.SaveChanges();
                ViewData["errorMsg"] = "Updated successfully";
                return Redirect("~/ViewMySessionDetails/"+updatedItem.SessionId+ "/Updated%20successfully");
            }
            else
            {
                return NotFound();
            }
        }
        [SessionCheck]
        [HttpDelete]
        public JsonResult RemoveSessionImage(int imgId,int sessionId)
        {
            if (imgId != 0)
            {
                var allImgs = _contxt.WorkShopItemImages.Where(x => x.SessionId == sessionId).ToList();
                if (allImgs.Count > 1)
                {
                    var imgDets = allImgs.Where(x => x.SessionId == sessionId && x.SessionImageId == imgId).FirstOrDefault();
                    _contxt.WorkShopItemImages.Remove(imgDets);
                    _contxt.SaveChanges();

                    return Json(new { status = true, message = "Image removed" });
                }
                else {

                    return Json(new { status = false, message = "You cant delete all images atleast one image required" });
                }
            }
            else
            {
                return Json(new { status = false, message = "Error removing" });
            }
        }
        [HttpDelete]
        [SessionCheck]
        public JsonResult RemoveSession(int sessionId) {
            if (sessionId != 0) {
                var sessDets = _contxt.Workshopitems.Where(x => x.SessionId == sessionId).FirstOrDefault();
                sessDets.Status = 0;
                _contxt.Workshopitems.Update(sessDets);
                _contxt.SaveChanges();

                return Json(new { status = true, message = "Item removed" });
            }
            else
            {
                return Json(new { status = false, message = "Error removing" });
            }
        }
        [HttpPost]
        [SessionCheck]
        public IActionResult addMoreSessionImageDetails(Workshopitems imgItems) {
            IList<WorkShopItemImages> ImgList = new List<WorkShopItemImages>();

            foreach (IFormFile image in imgItems.Fileinps)
            {
                var fpaths = Path.GetExtension(image.FileName).ToLower();
                if (!this.imgExtensions.Any(x => x.Equals(Path.GetExtension(image.FileName).ToLower())))
                {
                    ViewData["errorMsg"] = "Images only allowed, Allowable formats - jpg, png, jpeg, bmp";
                    return Redirect("~/ViewMySessionDetails/" + imgItems.SessionId+ "/imgupdateerror");
                }
            }

            foreach (IFormFile image in imgItems.Fileinps)
            {
                string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                //Get url To Save
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", ImageName);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                    WorkShopItemImages imgDets = new WorkShopItemImages();
                    imgDets.SessionId = imgItems.SessionId;
                    imgDets.ImageDetails = ImageName;
                    imgDets.CreatedDate = DateTime.Now;
                    _contxt.WorkShopItemImages.Add(imgDets);
                    _contxt.SaveChanges();
                }
            }
            return Redirect("~/ViewMySessionDetails/"+imgItems.SessionId+"/imgupdated");
        }



        [SessionCheck]
        [HttpPost]
        public IActionResult CreateWorkshop(Workshopitems newItem)
        {
            ViewData["errorMsg"] = "";
            IList<WorkShopItemImages> ImgList = new List<WorkShopItemImages>();
            if (newItem.Fileinps != null)
            {
                foreach (IFormFile image in newItem.Fileinps)
                {
                    var fpaths = Path.GetExtension(image.FileName).ToLower();
                    if (!this.imgExtensions.Any(x => x.Equals(Path.GetExtension(image.FileName).ToLower())))
                    {
                        ViewData["errorMsg"] = "Images only allowed, Allowable formats - jpg, png, jpeg, bmp";
                        return View(newItem);
                    }
                }
            }
            else {
                ViewData["errorMsg"] = "Image Required";
                return View(newItem);
            }

            int? loggedUserId = HttpContext.Session.GetInt32("userId");
            Users userdets = _contxt.Users.Where(x => x.UserId == loggedUserId).FirstOrDefault();

            Workshopitems newWorkshopitem = new Workshopitems();
            newWorkshopitem.RegStartDate = newItem.RegStartDate;
            newWorkshopitem.RegEndDate = newItem.RegEndDate;

            newWorkshopitem.FirstLesDate = newItem.FirstLesDate;
            newWorkshopitem.EndLesDate = newItem.EndLesDate;
            newWorkshopitem.SessionDetails = newItem.SessionDetails;
            newWorkshopitem.SessionTitle = newItem.SessionTitle;
            newWorkshopitem.SessionCategory = newItem.SessionCategory;
            newWorkshopitem.SessionSchedule = newItem.SessionSchedule;
            newWorkshopitem.SessionCost = newItem.SessionCost;
            newWorkshopitem.TotalLessons = newItem.TotalLessons;
            newWorkshopitem.AvailableSlots = newItem.TotalSlots;
            newWorkshopitem.TotalSlots = newItem.TotalSlots;
            newWorkshopitem.LocationCity = newItem.LocationCity;
            newWorkshopitem.LocationDetails = newItem.LocationDetails;
            newWorkshopitem.CreatedBy = (int)loggedUserId;
            newWorkshopitem.CreatedByName = (userdets.FirstName+" "+ userdets.LastName);
            newWorkshopitem.CreatedDate = DateTime.Now;
            newWorkshopitem.Status = 1;
            _contxt.Workshopitems.Add(newWorkshopitem);
            _contxt.SaveChanges();
           
            foreach (IFormFile image in newItem.Fileinps)
            {
                string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                //Get url To Save
                string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", ImageName);
                using (var stream = new FileStream(SavePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                    WorkShopItemImages imgDets = new WorkShopItemImages();
                    imgDets.SessionId = newWorkshopitem.SessionId;
                    imgDets.ImageDetails = ImageName;
                    imgDets.CreatedDate = DateTime.Now;
                    _contxt.WorkShopItemImages.Add(imgDets);
                    _contxt.SaveChanges();
                }
            }
            ViewData["errorMsg"] = "-NA-";
            return View(newWorkshopitem);
        }
        [HttpPost]
        public JsonResult ConfirmBooking(int cartId) {
            var orderdetails = _contxt.OrderItems.Where(x => x.OrderItemId == cartId).FirstOrDefault();
            if (orderdetails.BookingStatus == "Inprogress")
            {
                var bookingdetails = _contxt.BookingDetails.Where(x => x.CartId == orderdetails.CartId).FirstOrDefault();
                int noOfSeats = bookingdetails.NumberOfAttendees;

                Workshopitems pdctDetails = _contxt.Workshopitems.Where(x => x.SessionId == orderdetails.ProductId).FirstOrDefault();
                pdctDetails.AvailableSlots = pdctDetails.AvailableSlots - 1;// bookingdetails.NumberOfAttendees;
                _contxt.Workshopitems.Update(pdctDetails);
                _contxt.SaveChanges();

                orderdetails.BookingStatus = "Confirmed";
                _contxt.OrderItems.Update(orderdetails);
                _contxt.SaveChanges();
                return Json(new { status = true, message = "Booking update and slot created for the user" });
            }
            else {
                return Json(new { status = false, message = "Could not create slot" });
            }
        }
        [HttpPost]
        public JsonResult CancelBooking(int cartId)
        {
            var orderdetails = _contxt.OrderItems.Where(x => x.OrderItemId == cartId).FirstOrDefault();
            if (orderdetails.BookingStatus == "Inprogress")
            {
                orderdetails.BookingStatus = "Cancelled";
                _contxt.OrderItems.Update(orderdetails);
                _contxt.SaveChanges();
                return Json(new { status = true, message = "Booking update and slot created for the user" });
            }
            else
            {
                return Json(new { status = false, message = "Could not create slot" });
            }


        }

        [Route("ViewBookingDetails/{orderId}")]
        public IActionResult viewBookingDetails(int orderId) {
            string responseHtml = "";
            ViewData["orderHtml"] = getPreviewString(orderId);// responseHtml;
            return View();                                            
        }

    }
}




    