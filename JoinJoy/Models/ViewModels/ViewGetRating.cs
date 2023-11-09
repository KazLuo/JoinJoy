using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinJoy.Models.ViewModels
{
    public class ViewGetRating
    {
        public int userId { get; set; }
        public EnumList.RatingFilter sortBy { get; set; }


    }
}