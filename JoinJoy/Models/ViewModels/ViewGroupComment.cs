using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewGroupComment
    {
        [Required]
        public int groupId { get; set; }
        [MaxLength(500)]
        public string commentTxt { get; set; }
    }
}