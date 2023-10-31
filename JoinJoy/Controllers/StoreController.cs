using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Store", Description = "店家相關功能")]
    [RoutePrefix("store")]
    public class StoreController : ApiController
    {

        private Context db = new Context();
        /// <summary>
        /// 店家資訊
        /// </summary>
        /// <param name="storeId">帶入stroeId產生店家資訊</param>
        /// <returns></returns>
        [HttpGet]
        [Route("store/{storeId}")]
        public IHttpActionResult GetStoreInfo(int storeId)
        {
            var store = db.Stores.FirstOrDefault(s => s.Id == storeId);
            if (store == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家資訊" });
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳店家資訊成功", data = new {id=store.Id,memberId=store.MemberId,name=store.Name,plane=store.Address,tel=store.Phone,openTime=store.OpenTime,closeTime=store.CloseTime,introduce=store.Introduce,maxPeople=store.MaxPeople,price=store.Price,wifiTag=store.Wifi,teachTag=store.Teach,meal=store.Meal,mealout=store.Mealout,buffet=store.Buffet,HqTag=store.HqTag,popTag=store.PopTag  } });
        }
    }
}
