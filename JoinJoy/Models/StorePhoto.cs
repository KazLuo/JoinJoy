using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class StorePhoto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string PhotoPath { get; set; }
        [JsonIgnore]
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }  // Navigation property
        public int StoreId { get; set; } // 這將作為外鍵連接到Store

        public bool IsCover { get; set; } = false;

        [Required]
        public DateTime InitDate { get; set; } = DateTime.Now;
    }
}