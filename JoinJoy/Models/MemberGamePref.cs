using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class MemberGamePref
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "偏好ID")]
        public int Id { get; set; }

        [Display(Name = "會員ID")]
        [ForeignKey("MemberId")]
        [JsonIgnore]

        public virtual Member Member { get; set; }
        public int MemberId { get; set; }

        [Display(Name = "遊戲類型ID")]
        [ForeignKey("GameTypeId")]
        [JsonIgnore]
        public virtual GameType GameType { get; set; }
        public int GameTypeId { get; set; }

        [Required]
        public DateTime InitDate { get; set; } = DateTime.Now;
    }
}