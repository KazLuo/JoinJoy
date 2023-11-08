using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class Group
    {
        [Key]
        [Display(Name = "團ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }

        [Display(Name = "會員ID")]
        [ForeignKey("Member")]
        public int MemberId { get; set; }

        [Display(Name = "店家ID")]
        [ForeignKey("Store")]
        public int? StoreId { get; set; }

        [MaxLength(100)]
        [Display(Name = "團名稱")]
        public string GroupName { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }  // 預設為團主

        [MaxLength(100)]
        public string Description { get; set; }

        public bool IsHomeGroup { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }
        //內建幾人
        public int InitMember { get; set; }
        /// <summary>
        /// 新手團
        /// </summary>
        [Display(Name = "新手團")]
        public bool Beginner { get; set; } = false;

        /// <summary>
        /// 老手團
        /// </summary>
        [Display(Name = "老手團")]
        public bool Expert { get; set; } = false;

        /// <summary>
        /// 經驗切磋
        /// </summary>
        [Display(Name = "經驗切磋")]
        public bool Practice { get; set; } = false;

        /// <summary>
        /// 不限定
        /// </summary>
        [Display(Name = "不限定")]
        public bool Open { get; set; } = false;

        /// <summary>
        /// 教學團
        /// </summary>
        [Display(Name = "教學團")]
        public bool Tutorial { get; set; } = false;

        /// <summary>
        /// 輕鬆
        /// </summary>
        [Display(Name = "輕鬆")]
        public bool Casual { get; set; } = false;

        /// <summary>
        /// 競技
        /// </summary>
        [Display(Name = "競技")]
        public bool Competitive { get; set; } = false;
        [Display(Name = "生成時間")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        [Display(Name = "開團狀態")]
        public EnumList.GroupState GroupState { get; set; } = EnumList.GroupState.開團中;
        [Display(Name = "私人團")]
        public bool isPrivate { get; set; } = false;
        
        public virtual Member Member { get; set; }
        public virtual Store Store { get; set; }
        //參加團
        public virtual ICollection<GroupParticipant> GroupParticipants { get; set; }
        //留言板
        public virtual ICollection<GroupComment> GroupComments { get; set; }

        public virtual ICollection<GroupGame> GroupGames { get; set; }
        
    }
}