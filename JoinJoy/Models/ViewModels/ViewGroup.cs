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
        public string GroupName { get; set; }  // 團名
        public DateTime StartTime { get; set; }  // 開始時間
        public DateTime EndTime { get; set; }  // 結束時間
        public int MaxParticipants { get; set; }  // 滿人數
        public int CurrentParticipants { get; set; } = 1;  // 現況人數(開團時預設就是1)
        [MaxLength(500)]
        public string Description { get; set; }  // 團長想說的話
        public bool IsHomeGroup { get; set; } = false;  // 是否為自家團(預設false)
        public string Address { get; set; }  // 自宅團地址
        public int InitMember { get; set; }  // 內建人數
        public bool Beginner { get; set; } = false;  // 新手團標籤(預設false)
        public bool Expert { get; set; } = false;  // 老手團(預設false)
        public bool Practice { get; set; } = false;  // 經驗切磋(預設false)
        public bool Open { get; set; } = false;  // 不限定(預設false)
        public bool Tutorial { get; set; } = false;  // 教學團(預設false)
        public bool Casual { get; set; } = false;  // 輕鬆(預設false)
        public bool Competitive { get; set; } = false;  // 競技(預設false)
        public DateTime CreationDate { get; set; } = DateTime.Now;  // 創建時間(自動生成)

    }
}