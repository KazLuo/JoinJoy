using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewJoinGroup
    {
        public int? groupId { get; set; }
        public int initNum { get; set; } 
        public bool isPrivate { get; set; }
    }
}