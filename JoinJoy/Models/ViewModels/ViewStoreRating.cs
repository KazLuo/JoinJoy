using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewStoreRating
    {
        
        public int storeId { get; set; }
       
        public int groupId { get; set; }
        [Range(1, 5)]
        public int clean { get; set; }
        [Range(1, 5)]
        public int service { get; set; }
        [Range(1, 5)]
        public int variety { get; set; }
        [Range(1, 5)]
        public int value { get; set; }
        [MaxLength(500)]
        public string comment { get; set; }
    }
}