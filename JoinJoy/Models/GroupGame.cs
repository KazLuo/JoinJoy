using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class GroupGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "團ID")]
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        public int GroupId { get; set; }

        [Display(Name = "店家庫存ID")]
        [ForeignKey("StoreInventoryId")]
        public virtual StoreInventory StoreInventory { get; set; }
        public int StoreInventoryId { get; set; }

        //使用"="讓他預設
        [Display(Name = "建立日期")]
        public DateTime InitDate { get; set; } = DateTime.Now;



    }
}