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
            已預約
        }

        public enum JoinGroupState
        {
            團主,
            團員,
            審核中,
            已拒絕,

            //已加入,
            //已拒絕,


        }


    }
}