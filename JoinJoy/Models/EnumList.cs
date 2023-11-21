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
            已結束,
            已失效,
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


        public enum GroupFilter
        {
            relevance = 0,
            Upcoming, // 即將開團
            Newest //最新開團
            
        }
        public enum GroupTag //遊戲面向
        {
            all, //全部
            beginner, //新手團
            expert, //老手團
            practice,//經驗切磋
            open,//不限定
            tutorial,//教學團
            casual,//輕鬆
            competitive//競技


        }

        public enum Groupppl//揪團總人數
        {
            all, //全部
            twotofour, //2~4
            fivetoseven, //5~7
            eightmore  //8以上
        }
        public enum Joinppl//可加入人數
        {
            all,
            onetothree, //1~3
            fourtosix, //4~6
            sevenmore  //7以上

        }
            
    }
}