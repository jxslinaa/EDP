using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducProj.Models
{ 
    public class Users
    {
        [Key]
        public int UserId { get; set; }//Auto key customers unique id
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public int isActive { get; set; }//We can disable the customer login details by this key
        public int FailedLogins { get; set; }//Store the failed login details
        public string Password { get; set; }
        public string PasswordSalt { get; set; }//Password matching key 

        public string UserType { get; set; }

        public string Email { get; set; }//Unique field 
        public int EmailValidated { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
/** 
 * Table name - Customers - used to store the bookign details - ***/