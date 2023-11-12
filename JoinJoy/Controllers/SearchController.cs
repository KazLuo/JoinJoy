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
    [OpenApiTag("Search", Description = "搜尋功能")]
    [RoutePrefix("search")]
    public class SearchController : ApiController
    {
        private Context db = new Context();
        /// <summary>
        /// 搜尋店家功能
        /// </summary>
        /// <param name="city">帶入城市Id 目前有 2 & 15(台北 & 高雄)</param>
        /// <returns></returns>
        [HttpGet]
        [Route("search/stores")]
        public IHttpActionResult SearchStores(int city/*, string district = ""*/)
        {
            var cities = db.Cities.FirstOrDefault(c => c.Id == city);
            if (cities == null || string.IsNullOrEmpty(city.ToString()))
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "縣市不可為空，或是城市Id不存在" });
            }
      
            var query = db.Stores.AsQueryable();
            query = query.Where(store => store.Address.Contains(cities.CityName));
           
            var matchedStoresData = query.Select(store => new
            {
                store.Id,
                store.Name,
                store.Address,
                ProfileImgPath = store.Photo,
                CoverPhotoPath = store.StorePhotos.FirstOrDefault(sp => sp.StoreId == store.Id && sp.IsCover).PhotoPath,
                store.Wifi,
                store.Teach,
                store.Meal,
                store.Mealout,
                store.Buffet,
                store.HqTag,
                store.PopTag
            }).ToList();
            if (!matchedStoresData.Any()) 
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "所選地區找不到店家" });
            }
            var matchedStores = matchedStoresData.Select(store => new
            {
                storeId = store.Id,
                storeName = store.Name,
                address = store.Address,
                profileImg = string.IsNullOrEmpty(store.ProfileImgPath) ? null : $"http://4.224.16.99/upload/store/{store.ProfileImgPath}",
                cover = string.IsNullOrEmpty(store.CoverPhotoPath) ? null : $"http://4.224.16.99/upload/store/{store.CoverPhotoPath}",
                score = CalculateStoreScore(store.Id),
                tag = new
                {
                    wifiTag = store.Wifi,
                    teachTag = store.Teach,
                    meal = store.Meal,
                    mealout = store.Mealout,
                    buffet = store.Buffet,
                    hqTag = store.HqTag,
                    popTag = store.PopTag
                },
            }).ToList();

            //if (!matchedStores.Any())
            //{
            //    return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "所選地區找不到店家" });
            //}
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功",data=new { matchedStores } });
        }

        private double CalculateStoreScore(int storeId)
        {
            // 這裡實現計算商店評分的邏輯
            // 注意：這需要根據您的實際數據模型來調整
            var ratings = db.StoreRatings.Where(s => s.StoreId == storeId);
            if (!ratings.Any())
            {
                return 0; // 假設沒有評分則返回 0
            }

            return ratings.Average(s => (s.Value + s.Variety + s.Service + s.Clean) / 4.0);
        }

    }
}
