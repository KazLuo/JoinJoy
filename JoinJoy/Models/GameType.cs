using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class GameType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "遊戲類型ID")]
        public int Id { get; set; }

        [Display(Name = "遊戲類型名稱")]
        [MaxLength(100)]
        public string TypeName { get; set; }

        [Required]
        public DateTime InitDate { get; set; } = DateTime.Now;
        // 會員的遊戲偏好列表
        [JsonIgnore]
        public virtual ICollection<MemberGamePref> MemberPreferences { get; set; }




    }
}