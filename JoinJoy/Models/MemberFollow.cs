using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class MemberFollow
    {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int FollowId { get; set; }

           
            [ForeignKey("MemberId")]
            public virtual Member Member { get; set; }
            public int MemberId { get; set; }

            
            [ForeignKey("StoreId")]
            public virtual Store Store { get; set; }
            public int StoreId { get; set; }

        public DateTime InitDate { get; set; } = DateTime.Now;
    }
}