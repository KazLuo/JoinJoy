using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class Member
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]
        [Display(Name = "名稱")]
        public string Nickname { get; set; }

        //信箱等同於帳號
        [Required(ErrorMessage = "{0}必填")]
        [EmailAddress(ErrorMessage = "{0} 格式錯誤")]
        [MaxLength(200)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "帳號")]
        public string Account { get; set; }



        [Required(ErrorMessage = "{0}必填")]
        //設定6~12字英文數字混合密碼(至少1個英文)
        //[StringLength(12, ErrorMessage = "{0} 長度必須為 {2} 到 {1} 個字元。", MinimumLength = 6)]
        //[RegularExpression(@"^((?=.*[a-zA-Z])[a-zA-Z0-9]{6,12})$", ErrorMessage = "{0} 必須是6到12個字符，且至少包含一個英文字母。")]
        //用於指定一個字串應該被當作密碼處理
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; }


        [MaxLength(100)]
        [Display(Name = "密碼鹽")]
        public string PasswordSalt { get; set; }


        //這邊不使用原先的權限資料表，只需要改成是否為店家就好
        [Display(Name = "是否為店家")]
        public bool IsStoreOwner { get; set; } = false;

        [Display(Name = "會員圖片")]
        public string Photo { get; set; }

        [Display(Name = "會員介紹")]
        [MaxLength(100)]
        public string Introduce { get; set; }

        //使用"="讓他預設
        [Display(Name = "建立日期")]
        public DateTime InitDate { get; set; } = DateTime.Now;

        //[JsonIgnore]
        //public virtual ICollection<Store> Stores { get; set; }
        // 會員的縣市偏好列表
        [JsonIgnore]
        public virtual ICollection<MemberCityPref> CityPreferences { get; set; }
        // 會員的遊戲類型偏好列表
        [JsonIgnore]
        public virtual ICollection<MemberGamePref> GamePreferences { get; set; }
        // 會員給出的店家評價列表
        [JsonIgnore]
        public virtual ICollection<StoreRating> StoreRatings { get; set; }  // 從會員可以訪問他的所有評價
        ////此會員送出的所有評價(功能上不需要)
        //[JsonIgnore]
        //[Display(Name = "給出的評價")]
        //public virtual ICollection<MemberRating> GivenRatings { get; set; }

        //此會員收到的所有評價
       [JsonIgnore]
       [Display(Name = "收到的評價")]
        public virtual ICollection<MemberRating> ReceivedRatings { get; set; }

        // 這個會員開設的所有團
        [JsonIgnore]
        public virtual ICollection<Group> CreatedGroups { get; set; }

        //// 這個會員參加的所有團
        //[JsonIgnore]
        //public virtual ICollection<GroupParticipant> ParticipatedGroups { get; set; }

        //這個會員在不同團中的留言
        //[JsonIgnore]
        //public virtual ICollection<GroupComment> GroupComments { get; set; }

        // 這個會員追蹤的店家
        public virtual ICollection<MemberFollow> FollowedStores { get; set; }

        // 這個會員被哪些店家追蹤
        public virtual ICollection<StoreFollow> FollowedByStores { get; set; }




    }
}