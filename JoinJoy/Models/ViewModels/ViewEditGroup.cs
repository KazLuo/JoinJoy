using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewEditGroup
    {
        [MaxLength(50)]
        public string groupName { get; set; }  // 團名
        [MaxLength(100)]
        public string description { get; set; }  // 團長想說的話
        public bool beginnerTag { get; set; } = false;  // 新手團標籤(預設false)
        public bool expertTag { get; set; } = false;  // 老手團(預設false)
        public bool practiceTag { get; set; } = false;  // 經驗切磋(預設false)
        public bool openTag { get; set; } = false;  // 不限定(預設false)
        public bool tutorialTag { get; set; } = false;  // 教學團(預設false)
        public bool casualTag { get; set; } = false;  // 輕鬆(預設false)
        public bool competitiveTag { get; set; } = false;  // 競技(預設false)
        public bool isPrivate { get; set; } = false;  // 是否為私人團(預設false)
        // 新增的屬性，用於存儲選擇的遊戲 ID 列表
        public List<int> GameIds { get; set; }
    }
}