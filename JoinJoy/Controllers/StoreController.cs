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
    [RoutePrefix("storeinfo")]
    public class StoreController : ApiController
    {

        private Context db = new Context();

        /// <summary>
        /// 店家資訊
        /// </summary>
        /// <param name="storeId">帶入stroeId產生店家資訊(id=7可以測試)</param>
        /// <returns></returns>
        #region"GetStoreInfo"
        [HttpGet]
        [Route("store/{storeId}")]
        public IHttpActionResult GetStoreInfo(int storeId)
        {
            var store = db.Stores.FirstOrDefault(s => s.Id == storeId);
            if (store == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家資訊" });
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳店家資訊成功", data = new { /*id = store.Id, memberId = store.MemberId,*/ storeName = store.Name, address = store.Address, phone = store.Phone, openTime = store.OpenTime, closeTime = store.CloseTime, description = store.Introduce, /*maxPeople = store.MaxPeople*/ cost = store.Price, wifiTag = store.Wifi, teachTag = store.Teach, meal = store.Meal, mealout = store.Mealout, buffet = store.Buffet, HqTag = store.HqTag, popTag = store.PopTag } });
        }
        #endregion

        /// <summary>
        /// 店家遊戲列表
        /// </summary>
        /// <param name="storeId">帶入stroeId產生店家資訊,目前只有storeId=7可以測試</param>
        /// <returns></returns>
        #region"GetStoreGameList"
        [HttpGet]
        [Route("gamelist/{storeId}")]
        public IHttpActionResult GetStoreGameList(int storeId)
        {
            // 檢查店家是否存在
            var storeExists = db.Stores.Any(s => s.Id == storeId);
            if (!storeExists)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家" });
            }
            var gamelist = db.StoreInventories.Where(m => m.StoreId == storeId).Select(m => new { gametype = m.GameDetails.GameType.TypeName, gameName = m.GameDetails.Name, version = m.GameDetails.Language, peopleNum = m.GameDetails.People, qtu = m.StockCount });
            // 找不到遊戲清單
            if (!gamelist.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家的遊戲清單" });
            }
            return Content(HttpStatusCode.OK, new
            {
                statusCode = HttpStatusCode.OK,
                status = true,
                message = "回傳店家遊戲庫存成功",
                data = new
                {
                    gamelist
                }
            });
        }
        #endregion
    }

}
