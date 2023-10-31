using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewGroup
    {
        [MaxLength(50)]
        public string groupName { get; set; }  // 團名
        public DateTime startTime { get; set; }  // 開始時間
        public DateTime endTime { get; set; }  // 結束時間
        public int totalMemberNum { get; set; }  // 滿人數
        //public int currentppl { get; set; } = 1;  // 現況人數(開團時預設就是1)
        [MaxLength(100)]
        public string description { get; set; }  // 團長想說的話
        public bool isHomeGroup { get; set; } = false;  // 是否為自家團(預設false)
        public string place { get; set; }  // 自宅團地址
        public int initNum { get; set; }  // 內建人數
        public bool beginnerTag { get; set; } = false;  // 新手團標籤(預設false)
        public bool expertTag { get; set; } = false;  // 老手團(預設false)
        public bool practiceTag { get; set; } = false;  // 經驗切磋(預設false)
        public bool openTag { get; set; } = false;  // 不限定(預設false)
        public bool tutorialTag { get; set; } = false;  // 教學團(預設false)
        public bool casualTag { get; set; } = false;  // 輕鬆(預設false)
        public bool competitiveTag { get; set; } = false;  // 競技(預設false)
        //public DateTime creationDate { get; set; } = DateTime.Now;  // 創建時間(自動生成)

    }
}