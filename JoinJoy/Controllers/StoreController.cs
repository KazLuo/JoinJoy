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
        [HttpGet]
        [Route("store/{storeId}")]
        public IHttpActionResult GetStoreInfo(int storeId)
        {
            var store = db.Stores.FirstOrDefault(s => s.Id == storeId);
            if (store == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家資訊" });
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳店家資訊成功", data = new { store } });
        }
    }
}
