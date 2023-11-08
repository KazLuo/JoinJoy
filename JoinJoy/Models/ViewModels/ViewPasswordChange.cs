using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewPasswordChange
    {
        [Required]
        public string oldPaswrd { get; set; }
        [Required]
        public string newPaswrd { get; set; }
        [Required]
        [Compare("newPaswrd", ErrorMessage = "密碼和確認密碼不匹配")]
        public string confPaswrd { get; set; }
    }
}