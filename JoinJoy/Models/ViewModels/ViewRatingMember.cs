using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewRatingMember
    {
        public int groupId { get; set; }  // 被評人
        public int memberId { get; set; }  // 被評人
        [Range(1, 5, ErrorMessage = "評分必須在1到5之間")]
        public int score { get; set; }  // 評價分數
        [MaxLength(50, ErrorMessage = "評論不可超過50個字符")]//團員評價50字店家評價100字
        public string comment { get; set; } // 留言
    }
}