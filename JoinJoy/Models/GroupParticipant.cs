using Newtonsoft.Json;
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
      /*  [ForeignKey("Members")]*/
        public int MemberId { get; set; }
        [Display(Name = "內建人數")]
        public int InitMember { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "出席狀態")]
        public EnumList.JoinGroupState AttendanceStatus { get; set; } = EnumList.JoinGroupState.pending;
        //[JsonIgnore]
        //public virtual ICollection<Member> Members { get; set; }
        [JsonIgnore]
        public virtual Group Group { get; set; }
        [Display(Name = "是否報到")]
        public bool IsPresent { get; set; } = false;
    }
}