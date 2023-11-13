using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewStoreSearch
    {
        public int? cityId {get;set;}
        public string storeName { get; set; }
        public EnumList.StoreFilter storeFilter { get; set; } = EnumList.StoreFilter.relevance;
        public EnumList.StoreTag storeTag { get; set; } = EnumList.StoreTag.none;
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 9;
    }
}