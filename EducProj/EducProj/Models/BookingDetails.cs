using System;
using System.ComponentModel.DataAnnotations;

namespace EducProj.Models
{
    public class BookingDetails
    {
        [Key]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Invalid Error occured, Please try again")]
        public string SessionId { get; set; }
        [Required(ErrorMessage = "Please Login to continue")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Error in Cart, Please try again")]
        [DataType(DataType.Currency)]
        public decimal BookingAmount { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(75)]
        [RegularExpression(@"(\S)+", ErrorMessage = "Alphabets only.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(75)]
        [RegularExpression(@"(\S\D)+", ErrorMessage = "Alphabets only.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        //[DataType(DataType.PhoneNumber)]
        [StringLength(13, MinimumLength = 10)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email address required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public int CartId { get; set; }
        public string Remarks { get; set; }

        [Required(ErrorMessage = "No.of attendees required")]
        [Range(1, 10, ErrorMessage = "Please Provide correct range")]
        public int NumberOfAttendees { get; set; }

        public int PaymentStatus { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}

/** 
 * Table name - BookingDetails - used to store the bookign details - ***/
