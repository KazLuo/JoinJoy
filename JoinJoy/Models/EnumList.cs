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
            leader, // 團主
            member, // 團員
            pending, // 審核中
            rejected // 已拒絕

        }


    }
}