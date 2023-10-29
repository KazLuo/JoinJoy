using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewRegister
    {
        //信箱等同於帳號
        [Required(ErrorMessage = "{0}必填")]
        [EmailAddress(ErrorMessage = "{0} 格式錯誤")]
        [MaxLength(200)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "帳號")]
        public string email { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        //設定6~12字英文數字混合密碼(至少1個英文)
        //[StringLength(12, ErrorMessage = "{0} 長度必須為 {2} 到 {1} 個字元。", MinimumLength = 6)]
        //[RegularExpression(@"^((?=.*[a-zA-Z])[a-zA-Z0-9]{6,12})$", ErrorMessage = "{0} 必須是6到12個字符，且至少包含一個英文字母。")]
        //用於指定一個字串應該被當作密碼處理
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string password { get; set; }

        [Required]
        [Compare("password", ErrorMessage = "密碼和確認密碼不匹配")]
        public string confirmPassword { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        [Display(Name = "名稱")]
        public string nickname { get; set; }

  
    }
}