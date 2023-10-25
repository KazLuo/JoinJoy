using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class GameDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "遊戲ID")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "遊戲名稱")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "遊戲詳細描述")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "遊戲平均遊玩時間")]
        public int Duration { get; set; }

        [Required]
        [Display(Name = "遊戲建議人數")]
        public string People { get; set; }

        [Required]
        [Display(Name = "語言版本")]
        public string Language { get; set; }

        [Required]
        [Display(Name = "遊戲類型")]
        [JsonIgnore]
        [ForeignKey("GameTypeId")]
        public virtual GameType GameType { get; set; }
        public int GameTypeId { get; set; }

     
        [Required]
        public DateTime InitDate { get; set; } = DateTime.Now;

        // 會員擁有的店家庫存列表
        [JsonIgnore]
        public virtual ICollection<StoreInventory> StoreInventories { get; set; }


    }
}