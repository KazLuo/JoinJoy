using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JoinJoy.Models
{
    public class EnumList
    {
        public enum GroupState
        {
            開團中,
            已預約,
            已結束
        }

        public enum JoinGroupState
        {
            leader, // 團主
            member, // 團員
            pending, // 審核中
            rejected // 已拒絕

        }

        public enum RatingFilter
        {
            newest,
            highest,
            lowest,

        }

        public enum CostFilter
        {
            below30,
            c30to40,
            c40to50,
            c50up,
            all,
        }

        public enum StoreFilter
        {
            relevance = 0,
            highestRating,
            mostReviews,
            
        }

        public enum StoreTag
        {
            none,
            wifiTag,
            teachTag,
            meal,
            mealout,
            buffet,
        }

    }
}