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

       
        public EnumList.GroupFilter groupFilter { get; set; } = EnumList.GroupFilter.relevance;
        public EnumList.GroupTag groupTag { get; set; } 
        public EnumList.Groupppl groupppl { get; set; }
        public EnumList.Joinppl joinppl { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 9;

    }
}