using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class MemberRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "評價ID")]
        public int Id { get; set; }

        [Display(Name = "評價者ID")]//這邊有循環參照的問題
        //[ForeignKey("Appraiser")]
        //[JsonIgnore]
        //public virtual Member Member { get; set; }
        public int MemberId { get; set; }


        [Display(Name = "被評價的會員ID")]
        [ForeignKey("RatedId")]
        [JsonIgnore]
        public virtual Member RatedMember { get; set; }
        public int RatedId { get; set; }


        [Display(Name = "團隊ID")]
        public int GroupId { get; set; }


        [Range(1, 5)]
        [Display(Name = "評價分數")]
        public int Score { get; set; }

        [MaxLength(500)]
        [Display(Name = "評價的文字內容")]
        public string Comment { get; set; }

        [Required]
        [Display(Name = "評價時間")]
        public DateTime RatingDate { get; set; } = DateTime.Now;

       


    }
}