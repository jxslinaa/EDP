using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EducProj.Models
{
    public class WorkShopItemImages
    {
        [Key]
        public int SessionImageId { get; set; }//Key, Identity column
        public int SessionId { get; set; }//

        public string ImageDetails { get; set; }//Title
        
        public DateTime CreatedDate { get; set; }


    }
}

