using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducProj.Models
{
    public class Orders
    {
        [Key]
        public int CartId { get; set; }//Foreign key
        public decimal CartTotal { get; set; }
        public int CustomerId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public IList<OrderItems> orderItems { get; set; } 
    }
}

//Subscribed products mastere







