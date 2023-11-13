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
using System.ComponentModel.DataAnnotations;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Search", Description = "搜尋功能")]
    [RoutePrefix("search")]
    public class SearchController : ApiController
    {
        private Context db = new Context();


        /// <summary>
        /// 搜尋店家(可用於開團)
        /// </summary>
        /// <param name="city"></param>
        /// <param name="storeName"></param>
        /// <returns></returns>

        #region"搜尋店家"
        [HttpGet]
        [Route("search/stores/")]
        public IHttpActionResult SearchStores(int? city = null, string storeName = "")
        {

            var query = db.Stores.AsQueryable();
            // 如果提供了城市ID，則添加城市過濾條件
            if (city.HasValue)
            {
                var cities = db.Cities.FirstOrDefault(c => c.Id == city.Value);
                if (cities != null)
                {
                    query = query.Where(store => store.Address.Contains(cities.CityName));
                }
            }

            // 如果提供了關鍵字，則添加關鍵字過濾條件
            if (!string.IsNullOrEmpty(storeName))
            {
                query = query.Where(store => store.Name.Contains(storeName));
            }

            var matchedStoresData = query.Select(store => new
            {
                store.Id,
                store.Name,
                store.Address,
                ProfileImgPath = store.Photo,
                CoverPhotoPath = store.StorePhotos.FirstOrDefault(sp => sp.StoreId == store.Id && sp.IsCover).PhotoPath,
                store.Price,
                store.Wifi,
                store.Teach,
                store.Meal,
                store.Mealout,
                store.Buffet,
                store.HqTag,
                store.PopTag,
                CommentsCount = db.StoreRatings.Count(m => m.StoreId == store.Id)
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
                cost = store.Price,
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
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { matchedStores } });
        }


        #endregion

        /// <summary>
        /// 搜尋店家
        /// </summary>
        /// <param name="viewStoreSearch"></param>
        /// <returns></returns>

        #region"搜尋店家"
        [HttpPost]
        [Route("search/storesearch")]
        public IHttpActionResult SearchStores(ViewStoreSearch viewStoreSearch)
        {

            var query = db.Stores.AsQueryable();
            // 如果提供了城市ID，則添加城市過濾條件
            if (viewStoreSearch != null && viewStoreSearch.cityId.HasValue)
            {
                var cities = db.Cities.FirstOrDefault(c => c.Id == viewStoreSearch.cityId);
                if (cities != null)
                {
                    query = query.Where(store => store.Address.Contains(cities.CityName));
                }
            }

            // 如果提供了關鍵字，則添加關鍵字過濾條件
            if (!string.IsNullOrEmpty(viewStoreSearch.storeName))
            {
                query = query.Where(store => store.Name.Contains(viewStoreSearch.storeName));
            }

            var matchedStoresData = query.Select(store => new
            {
                store.Id,
                store.Name,
                store.Address,
                ProfileImgPath = store.Photo,
                CoverPhotoPath = store.StorePhotos.FirstOrDefault(sp => sp.StoreId == store.Id && sp.IsCover).PhotoPath,
                store.Price,
                store.Wifi,
                store.Teach,
                store.Meal,
                store.Mealout,
                store.Buffet,
                store.HqTag,
                store.PopTag,
                CommentsCount = db.StoreRatings.Count(m => m.StoreId == store.Id)
            }).ToList();

            // 根據排序過濾條件進行排序
            switch (viewStoreSearch.storeFilter)
            {
                case EnumList.StoreFilter.relevance:
                    matchedStoresData = matchedStoresData.OrderBy(store => store.Name).ToList();
                    break;
                case EnumList.StoreFilter.highestRating:
                    matchedStoresData = matchedStoresData.OrderByDescending(store => CalculateStoreScore(store.Id)).ToList();
                    break;
                case EnumList.StoreFilter.mostReviews:
                    matchedStoresData = matchedStoresData.OrderByDescending(store => store.CommentsCount).ToList();
                    break;
            }

            switch (viewStoreSearch.storeTag)
            {
                case EnumList.StoreTag.wifiTag:
                    matchedStoresData = matchedStoresData.Where(store => store.Wifi == true).ToList();
                    break;
                case EnumList.StoreTag.teachTag:
                    matchedStoresData = matchedStoresData.Where(store => store.Teach == true).ToList();
                    break;
                case EnumList.StoreTag.meal:
                    matchedStoresData = matchedStoresData.Where(store => store.Meal == true).ToList();
                    break;
                case EnumList.StoreTag.mealout:
                    matchedStoresData = matchedStoresData.Where(store => store.Mealout == true).ToList();
                    break;
                case EnumList.StoreTag.buffet:
                    matchedStoresData = matchedStoresData.Where(store => store.Buffet == true).ToList();
                    break;

            }

            if (!matchedStoresData.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "所選地區找不到店家" });
            }
            // 計算分頁
            int skip = (viewStoreSearch.page - 1) * viewStoreSearch.pageSize;

            // 應用分頁
            var pagedStoresData = matchedStoresData.Skip(skip).Take(viewStoreSearch.pageSize).ToList();

            var matchedStores = pagedStoresData.Select(store => new
            {
                storeId = store.Id,
                storeName = store.Name,
                address = store.Address,
                profileImg = string.IsNullOrEmpty(store.ProfileImgPath) ? null : $"http://4.224.16.99/upload/store/{store.ProfileImgPath}",
                cover = string.IsNullOrEmpty(store.CoverPhotoPath) ? null : $"http://4.224.16.99/upload/store/{store.CoverPhotoPath}",
                score = CalculateStoreScore(store.Id),
                cost = store.Price,
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
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { matchedStores } });
        }


        #endregion
        /// <summary>
        /// 搜尋揪團
        /// </summary>
        /// <param name="viewGroupSearch"></param>
        /// <returns></returns>
        #region "搜尋揪團"
        [HttpPost]
        [Route("search/groups/")]
        public IHttpActionResult SearchGroups(ViewGroupSearch viewGroupSearch)
        {

            var query = from Group in db.Groups
                        select new
                        {
                            groupId = Group.GroupId,
                            groupName = Group.GroupName,
                            startTime = Group.StartTime,
                            endTime = Group.EndTime,
                            isHomeGroup = Group.IsHomeGroup,
                            groupState = Group.EndTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : Group.GroupState.ToString(),
                            games = db.GroupGames
                            .Where(gg => gg.GroupId == Group.GroupId)
                            .Select(gg => gg.StoreInventory.GameDetails.Name).ToList(),
                            address = Group.IsHomeGroup ? Group.Address : Group.Store.Address
                        };

            // 城市過濾
            if (viewGroupSearch.cityId != null)
            {
                var city = db.Cities.FirstOrDefault(c => c.Id == viewGroupSearch.cityId);
                if (city != null)
                {
                    query = query.Where(g => g.address.Contains(city.CityName));
                }
            }





            // 日期篩選
            if (viewGroupSearch.startDate.HasValue)
            {
                DateTime startDate = viewGroupSearch.startDate.Value;
                query = query.Where(g => g.startTime.Year == startDate.Year
                                         && g.startTime.Month == startDate.Month
                                         && g.startTime.Day == startDate.Day);
            }



            // 遊戲名稱篩選，可模糊搜尋


            if (!string.IsNullOrEmpty(viewGroupSearch.gameName))
            {
                query = query.Where(g => g.games.Any(game => game.Contains(viewGroupSearch.gameName)));
            }


            var matchedGroups = query.ToList();

            if (!matchedGroups.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到符合條件的揪團活動" });
            }



            return Ok(matchedGroups);
        }
        #endregion



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
