using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EducProj.Models
{
    public class Workshopitems
    {
        [Key]
        public int SessionId { get; set; }//Key, Identity column
        public DateTime RegStartDate { get; set; }

        public DateTime RegEndDate { get; set; }
        public DateTime FirstLesDate { get; set; }
        public DateTime EndLesDate { get; set; }

        public string SessionTitle { get; set; }//Title
        public string SessionDetails { get; set; }//COntents
        
        public string SessionSchedule { get; set; }//COntents

     
        public string SessionCategory { get; set; }

        public Double SessionCost { get; set; }//Amount for the session

        public int TotalLessons { get; set; }//Total session

        public int AvailableSlots { get; set; }//Available seats/slots, this field reduced with successfull checkout

        public int TotalSlots { get; set; }//Total seats/slots

        public string LocationCity { get; set; }

        public string LocationDetails { get; set; }

        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }

        [NotMapped]
        public IList<IFormFile> Fileinps { get; set; }

        [NotMapped]
        public IList<WorkShopItemImages> WorkshopImages { get; set; }

    }
}
