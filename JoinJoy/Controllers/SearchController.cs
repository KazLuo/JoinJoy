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
                profileImg = string.IsNullOrEmpty(store.ProfileImgPath) ? null : BuildStoreImageUrl( store.ProfileImgPath),
                cover = string.IsNullOrEmpty(store.CoverPhotoPath) ? null : BuildStoreImageUrl(store.CoverPhotoPath),
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
                profileImg = string.IsNullOrEmpty(store.ProfileImgPath) ? null : BuildStoreImageUrl(store.ProfileImgPath),
                cover = string.IsNullOrEmpty(store.CoverPhotoPath) ? null : BuildStoreImageUrl(store.CoverPhotoPath),
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
        //#region "搜尋揪團"
        //[HttpPost]
        //[Route("search/groups/")]
        //public IHttpActionResult SearchGroups(ViewGroupSearch viewGroupSearch)
        //{

        //    var query = from Group in db.Groups
        //                join GroupParticipant in db.GroupParticipants on Group.GroupId equals GroupParticipant.GroupId
        //                join Member in db.Members on GroupParticipant.MemberId equals Member.Id
        //                select new
        //                {
        //                    groupId = Group.GroupId,
        //                    groupName = Group.GroupName,
        //                    startTime = Group.StartTime,
        //                    endTime = Group.EndTime,
        //                    isHomeGroup = Group.IsHomeGroup,
        //                    //groupState = Group.EndTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : Group.GroupState.ToString(),
        //                    games = db.GroupGames
        //                    .Where(gg => gg.GroupId == Group.GroupId)
        //                    .Select(gg => gg.StoreInventory.GameDetails.Name).ToList(),
        //                    address = Group.IsHomeGroup ? Group.Address : Group.Store.Name,
        //                    beginnerTag = Group.Beginner,
        //                    expertTag = Group.Expert,
        //                    practiceTag = Group.Practice,
        //                    openTag = Group.Open,
        //                    tutorialTag = Group.Tutorial,
        //                    casualTag = Group.Casual,
        //                    competitiveTag = Group.Casual,
        //                    currentpeople = Group.CurrentParticipants,
        //                    totalMemberNum = Group.MaxParticipants,
        //                    leader = new
        //                    {
        //                        memberId = Group.MemberId,
        //                        memberName = Group.Member.Nickname,
        //                        initNum = Group.InitMember + 1//用於前端邏輯
        //                    },
        //                    member = new
        //                    {
        //                        memberId = GroupParticipant.MemberId,
        //                        memberName = Member.Nickname,
        //                        initNum = GroupParticipant.InitMember + 1//用於前端邏輯
        //                    }

        //                };

        //    // 城市過濾
        //    if (viewGroupSearch.cityId != null)
        //    {
        //        var city = db.Cities.FirstOrDefault(c => c.Id == viewGroupSearch.cityId);
        //        if (city != null)
        //        {
        //            query = query.Where(g => g.address.Contains(city.CityName));
        //        }
        //    }

        //    // 日期篩選
        //    if (viewGroupSearch.startDate.HasValue)
        //    {
        //        DateTime startDate = viewGroupSearch.startDate.Value;
        //        query = query.Where(g => g.startTime.Year == startDate.Year
        //                                 && g.startTime.Month == startDate.Month
        //                                 && g.startTime.Day == startDate.Day);
        //    }



        //    // 遊戲名稱篩選，可模糊搜尋


        //    if (!string.IsNullOrEmpty(viewGroupSearch.gameName))
        //    {
        //        query = query.Where(g => g.games.Any(game => game.Contains(viewGroupSearch.gameName)));
        //    }


        //    var matchedGroups = query.ToList();

        //    if (!matchedGroups.Any())
        //    {
        //        return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到符合條件的揪團活動" });
        //    }



        //    return Ok(matchedGroups);
        //}
        //#endregion

        #region"搜尋揪團2"
        [HttpPost]
        [Route("search/groups2/")]
        public IHttpActionResult SearchGroups2(ViewGroupSearch viewGroupSearch)
        {
           
                var query = db.Groups.AsQueryable();

                // 篩選城市
                if (viewGroupSearch.cityId.HasValue)
                {
                    var city = db.Cities.FirstOrDefault(c => c.Id == viewGroupSearch.cityId);
                    if (city != null)
                    {
                        query = query.Where(g => g.Address.Contains(city.CityName) || (g.IsHomeGroup == false && g.Store.Address.Contains(city.CityName)));
                    }
                }

                // 篩選日期
                if (viewGroupSearch.startDate.HasValue)
                {
                    var startDate = viewGroupSearch.startDate.Value.Date;
                    var endDate = startDate.AddDays(1);

                    query = query.Where(g => g.StartTime >= startDate && g.StartTime < endDate);
                }

                // 篩選遊戲名稱
                if (!string.IsNullOrEmpty(viewGroupSearch.gameName))
                {
                    query = query.Where(g => g.GroupGames.Any(game => game.StoreInventory.GameDetails.Name.Contains(viewGroupSearch.gameName)));
                }

                // 選取匹配群組
                var matchedGroups = query.Select(g => new
                {
              

                     groupId = g.GroupId,
                    groupName = g.GroupName,
                    startTime = g.StartTime,
                    endTime = g.EndTime,
                    isHomeGroup = g.IsHomeGroup,
                    groupState = g.EndTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : g.GroupState.ToString(),
                    address = g.IsHomeGroup ? g.Address : g.Store.Name,
                    beginnerTag = g.Beginner,
                    expertTag = g.Expert,
                    practiceTag = g.Practice,
                    openTag = g.Open,
                    tutorialTag = g.Tutorial,
                    casualTag = g.Casual,
                    competitiveTag = g.Casual,
                    currentpeople = g.CurrentParticipants,
                    totalMemberNum = g.MaxParticipants,
                    LeaderMemberId = g.MemberId,
                    LeaderNickname = g.Member.Nickname,
                    LeaderInitMember = g.InitMember,
                    members = db.GroupParticipants
                    .Where(gp => gp.GroupId == g.GroupId)
                    .Select(gp => new
                    {
                        memberId = gp.MemberId,
                        memberName = db.Members.Where(mn => mn.Id == gp.MemberId).Select(mn => mn.Nickname),
                        initNum = gp.InitMember + 1//前端邏輯需+1
                    }).ToList()
                }).ToList();

                // 然後，為每個群組獲取遊戲名稱
                var finalGroups = matchedGroups.Select(g => new
                {
                    groupId = g.groupId,
                    groupName = g.groupName,
                    startTime = g.startTime,
                    endTime = g.endTime,
                    isHomeGroup = g.isHomeGroup,
                    groupState = g.endTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : g.groupState.ToString(),
                    address = g.address,
                    beginnerTag = g.beginnerTag,
                    expertTag = g.expertTag,
                    practiceTag = g.practiceTag,
                    openTag = g.openTag,
                    tutorialTag = g.tutorialTag,
                    casualTag = g.casualTag,
                    competitiveTag = g.competitiveTag,
                    currentpeople = g.currentpeople,
                    totalMemberNum = g.totalMemberNum,
                    games = g.isHomeGroup ? new List<string>() : db.GroupGames
                                                                    .Where(gg => gg.GroupId == g.groupId)
                                                                    .Select(gg => gg.StoreInventory.GameDetails.Name)
                                                                    .ToList(),
                    leader = new
                    {
                        memberId = g.LeaderMemberId,
                        memberName = g.LeaderNickname,
                        initNum = g.LeaderInitMember + 1//前端邏輯需+1
                        
                    },
                    members = db.GroupParticipants
                    .Where(gp => gp.GroupId == g.groupId)
                    .Select(gp => new
                    {
                        memberId = gp.MemberId,
                        memberName = db.Members.Where(mn => mn.Id == gp.MemberId).Select(mn => mn.Nickname),
                        initNum = gp.InitMember + 1//前端邏輯需+1
                    }).ToList()

                }).ToList();

                if (!finalGroups.Any())
                {
                    return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到符合條件的揪團活動" });
                }

                return Ok(finalGroups);

        }

        #endregion



        private double CalculateStoreScore(int storeId)
        {

            var ratings = db.StoreRatings.Where(s => s.StoreId == storeId);
            if (!ratings.Any())
            {
                return 0; // 假設沒有評分則返回 0
            }

            return ratings.Average(s => (s.Value + s.Variety + s.Service + s.Clean) / 4.0);
        }
        private string BuildProfileImageUrl(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return null; // 或者返回一個默認的圖片路徑
            }
            return $"http://4.224.16.99/upload/profile/{photo}";
        }

        private string BuildStoreImageUrl(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return null; // 或者返回一個默認的圖片路徑
            }
            return $"http://4.224.16.99/upload/store/{photo}";
        }
    }
}
