using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewGroupComment
    {
        [MaxLength(500)]
        public string CommentContent { get; set; }
    }
}