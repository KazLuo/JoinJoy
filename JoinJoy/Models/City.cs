using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class City
    {
        [Key]
        [Display(Name = "城市ID")]
        public int Id { get; set; }

        [Display(Name = "城市名稱")]
        [MaxLength(100)]
        public string CityName { get; set; }

        [Required]
        public DateTime InitDate { get; set; } = DateTime.Now;
    }
}