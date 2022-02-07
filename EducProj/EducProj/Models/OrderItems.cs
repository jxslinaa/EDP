using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducProj.Models
{
    public class OrderItems
    {
        [Key]
        public int OrderItemId { get; set; }

        [ForeignKey("CartId")]
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public int CustomerId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string productTitle { get; set; }
        public int AssignedTeacher { get; set; }
        public string BookingStatus { get; set; }


    }
}
//Subscribed products are stored in this table
//Sub table for products list
