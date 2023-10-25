using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class GroupComment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "留言ID")]
        public int CommentId { get; set; }

        [Display(Name = "團ID")]
        [ForeignKey("Group")]
        public int GroupId { get; set; }

        [Display(Name = "會員ID")]
        //[ForeignKey("Member")]
        public int MemberId { get; set; }

        [MaxLength(500)]
        public string CommentContent { get; set; }

        public DateTime CommentDate { get; set; } = DateTime.Now;

        //public virtual Member Member { get; set; }
        public virtual Group Group { get; set; }
    }
}