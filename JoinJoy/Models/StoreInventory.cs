using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class StoreInventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "StoreId")]       
        [JsonIgnore]//有外鍵時要加,不會產生無限迴圈
        [ForeignKey("StoreId")]
        [Required]
        public virtual Store Store { get; set; }
        public int StoreId { get; set; }


        [Display(Name = "GameID")]
        [JsonIgnore]
        [ForeignKey("GameID")]
        [Required]
        public virtual GameDetails GameDetails { get; set; }
        public int GameID { get; set; }

        [Required]
        public int StockCount { get; set; }

        [Required]
        public DateTime InitDate { get; set; } = DateTime.Now;

        public virtual ICollection<GroupGame> GroupGames { get; set; }

    }
}