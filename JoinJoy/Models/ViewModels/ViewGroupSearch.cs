using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewGroupSearch
    {
        public int? cityId { get; set; }
        public DateTime? startDate { get; set; } = DateTime.Now;
        public string gameName { get; set; } = "";

    }
}