using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewUdtMemberDetail
    {
        [MaxLength(50)]
        [Required]
        public string nickName { get; set; }
        [MaxLength(100)]
        [Required]
        public string description { get; set; }
        //[MaxLength(3, ErrorMessage = "遊戲喜好最多為3項")]
        public List<int> games { get; set; }
        //[MaxLength(3, ErrorMessage = "城市喜好最多為3項")]
        public List<int> cities { get; set; }
    }
}