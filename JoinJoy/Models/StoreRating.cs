using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class StoreRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "會員ID")]
        [Required]
        [ForeignKey("MemberId")]
        [JsonIgnore]
        
        public virtual Member Member { get; set; }
        public int MemberId { get; set; }


        [Display(Name = "店家ID")]
        [Required]
        [ForeignKey("StoreId")]
        [JsonIgnore]
        
        public virtual Store Store { get; set; }
        public int StoreId { get; set; }

        [Range(1, 5)]
        [Display(Name = "環境整潔")]
        public int Clean { get; set; }

        [Range(1, 5)]
        [Display(Name = "服務態度")]
        public int Service { get; set; }

        [Range(1, 5)]
        [Display(Name = "遊戲多樣性")]
        public int Variety { get; set; }

        [Range(1, 5)]
        [Display(Name = "性價比")]
        public int Value { get; set; }

        [MaxLength(500)]
        [Display(Name = "評論")]
        public string Comment { get; set; }

        [Display(Name = "評價時間")]
        public DateTime RatingDate { get; set; } = DateTime.Now;
    }
}