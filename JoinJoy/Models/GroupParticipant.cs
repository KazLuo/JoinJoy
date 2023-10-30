using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class GroupParticipant
    {
        [Key]
        [Display(Name = "參與者ID")]
        public int ParticipantId { get; set; }

        [Display(Name = "團ID")]
        [ForeignKey("Group")]
        public int GroupId { get; set; }

        [Display(Name = "會員ID")]
        //[ForeignKey("Member")]
        public int MemberId { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "出席狀態")]
        public EnumList.JoinGroupState AttendanceStatus { get; set; } = EnumList.JoinGroupState.審核中;

        //public virtual Member Member { get; set; }
        public virtual Group Group { get; set; }
    }
}