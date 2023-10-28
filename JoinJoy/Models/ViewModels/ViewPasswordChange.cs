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
        public string oldPasswrd { get; set; }
        [Required]
        public string newPasswrd { get; set; }
        [Required]
        [Compare("newPasswrd", ErrorMessage = "密碼和確認密碼不匹配")]
        public string confPasswrd { get; set; }
    }
}