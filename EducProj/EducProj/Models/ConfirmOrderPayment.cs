using System;
using System.ComponentModel.DataAnnotations;

namespace EducProj.Models
{
    public class ConfirmOrderPayment
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(120)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Alphabets only.")]
        public string NameOnCard { get; set; }

        [Required(ErrorMessage = "Invalid Card number")]
        [StringLength(16, ErrorMessage = "Invalid card number", MinimumLength = 12)]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Expirydate Required")]
        [StringLength(7, ErrorMessage = "Invalid Expirydate", MinimumLength = 7)]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVV required")]
        [StringLength(4, ErrorMessage = "Please Provide correct CVV", MinimumLength = 3)]
        public string Cvv { get; set; }

        public string Comments { get; set; }
       
    }
}
