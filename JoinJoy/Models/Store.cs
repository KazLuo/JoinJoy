using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class Store
    {

        //這邊待討論
        /// <summary>
        /// 店家編號
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "店家編號")]
        public int Id { get; set; }


        /// <summary>
        /// 會員編號
        /// </summary>

        //[Required(ErrorMessage = "{0}必填")]
        //[JsonIgnore]
        //[ForeignKey("MemberId")]
        //public virtual Member Members { get; set; }

        public int MemberId { get; set; }


        /// <summary>
        /// 店家名稱
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]
        [Display(Name = "店家名稱")]
        public string Name { get; set; }

        /// <summary>
        /// 店家地址
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "店家地址")]
        public string Address { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "店家電話")]
        public int Phone { get; set; }

        /// <summary>
        /// 營業開始時間
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "營業開始時間")]
        public TimeSpan OpenTime { get; set; }

        /// <summary>
        /// 營業結束時間
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "營業結束時間")]
        public TimeSpan CloseTime { get; set; }

        /// <summary>
        /// 店家介紹
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "店家介紹")]
        public string Introduce { get; set; }

        /// <summary>
        /// 店家內部最大人數
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "店家內部最大人數")]
        public int MaxPeople { get; set; }

        /// <summary>
        /// 店家目前人數
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "店家目前人數")]
        public int CurPeople { get; set; }

        /// <summary>
        /// 店家收費
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "店家收費")]
        public int Price { get; set; }

        /// <summary>
        /// 店家Iframe
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "店家Iframe")]
        public string Iframe { get; set; }

        /// <summary>
        /// 提供wifi
        /// </summary>
        [Display(Name = "提供wifi")]
        public bool Wifi { get; set; } = false;

        /// <summary>
        /// 提供教學
        /// </summary>
        [Display(Name = "提供教學")]
        public bool Teach { get; set; } = false;

        /// <summary>
        /// 供餐
        /// </summary>
        [Display(Name = "供餐")]
        public bool Meal { get; set; } = false;

        /// <summary>
        /// 可攜帶外食
        /// </summary>
        [Display(Name = "可攜帶外食")]
        public bool Mealout { get; set; } = false;

        /// <summary>
        /// 提供自助吧
        /// </summary>
        [Display(Name = "提供自助吧")]
        public bool Buffet { get; set; } = false;

        /// <summary>
        /// 優質店家Tag
        /// </summary>
        [Display(Name = "優質店家Tag")]
        public bool HqTag { get; set; } = false;

        /// <summary>
        /// 人氣店家Tag
        /// </summary>
        [Display(Name = "人氣店家Tag")]
        public bool PopTag { get; set; } = false;

        /// <summary>
        /// 建立日期
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "建立日期")]
        public DateTime InitDate { get; set; } = DateTime.Now;

        // 店家的照片集合
        [JsonIgnore]
        public virtual ICollection<StorePhoto> StorePhotos { get; set; }
        // 店家的庫存集合
        [JsonIgnore]
        public virtual ICollection<StoreInventory> StoreInventories { get; set; }
        // 從店家可以訪問與其相關的所有評價
        [JsonIgnore]
        public virtual ICollection<StoreRating> StoreRatings { get; set; }
        // 此店家主辦的所有團體活動
        [JsonIgnore]
        public virtual ICollection<Group> HostedGroups { get; set; } 



    }
}