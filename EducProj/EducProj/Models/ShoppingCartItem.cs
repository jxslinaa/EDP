using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducProj.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int CartItemId { get; set; }
        public decimal OrderPrice { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }//For guest user mapped with 0, logged user mapped with non zero value
        public string CartSessionId { get; set; }//Session id is stored in this
        public int ShoppingCartType { get; set; }//Not used item
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [NotMapped]
        public Workshopitems workshopitems { get; set; }
        [NotMapped]
        public BookingDetails bookingDetails { get; set; }
    }
}
//Store the basket items, temporary items are added in this cart , guest user 
