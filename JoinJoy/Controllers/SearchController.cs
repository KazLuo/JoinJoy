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
                profileImg = string.IsNullOrEmpty(store.ProfileImgPath) ? null : BuildStoreImageUrl(store.ProfileImgPath),
                cover = string.IsNullOrEmpty(store.CoverPhotoPath) ? null : BuildStoreImageUrl(store.CoverPhotoPath),
                score = CalculateStoreScore(store.Id),
                cost = store.Price,

                tags = new List<string>
                    {
                        store.Wifi ? "wifiTag" : null,
                        store.Teach ? "teachTag" : null,
                        store.Meal ? "meal" : null,
                        store.Mealout ? "mealout" : null,
                        store.Buffet ? "buffet" : null,

                    }.Where(t => t != null).ToList(),
                hqTag = store.HqTag,
                popTag = store.PopTag
                //tag = new
                //{
                //    wifiTag = store.Wifi,
                //    teachTag = store.Teach,
                //    meal = store.Meal,
                //    mealout = store.Mealout,
                //    buffet = store.Buffet,
                //    hqTag = store.HqTag,
                //    popTag = store.PopTag
                //},
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
            var storeCount = query.Count();
            var matchedStoresData = query.Select(store => new
            {
                store.Id,
                store.Name,
                store.Address,
                ProfileImgPath = store.Photo,
                CoverPhotoPath = store.StorePhotos.FirstOrDefault(sp => sp.StoreId == store.Id && sp.IsCover).PhotoPath,
                store.OpenTime,
                store.CloseTime,
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
           

            // 檢查是否應用分頁
            if (viewStoreSearch.page != 0 && viewStoreSearch.pageSize != 0)
            {
                int skip = (viewStoreSearch.page - 1) * viewStoreSearch.pageSize;
                matchedStoresData = matchedStoresData.Skip(skip).Take(viewStoreSearch.pageSize).ToList();
            }

            var matchedStores = matchedStoresData.Select(store => new
            {
                storeId = store.Id,
                storeName = store.Name,
                address = store.Address,
                profileImg = string.IsNullOrEmpty(store.ProfileImgPath) ? null : BuildStoreImageUrl(store.ProfileImgPath),
                cover = string.IsNullOrEmpty(store.CoverPhotoPath) ? null : BuildStoreImageUrl(store.CoverPhotoPath),
                openTime = store.OpenTime,
                closeTime = store.CloseTime,
                score = CalculateStoreScore(store.Id),
                cost = store.Price,
                tags = new List<string>
                    {
                        store.Wifi ? "wifiTag" : null,
                        store.Teach ? "teachTag" : null,
                        store.Meal ? "meal" : null,
                        store.Mealout ? "mealout" : null,
                        store.Buffet ? "buffet" : null,

                    }.Where(t => t != null).ToList(),
                hqTag = store.HqTag,
                popTag = store.PopTag
            }).ToList();
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { matchedStores,storeCount } });
        }


        #endregion
        /// <summary>
        /// 搜尋揪團
        /// </summary>
        /// <param name="viewGroupSearch">篩選(0=最相關 1=即將開團 2=最新開團)</param>
        /// <param >遊戲面向(0=beginner 1=expert 2=practice 3=open 4=tutorial 5=casual 6=competitive)</param>
        /// <param >揪團總人數(0=all 1=twotofour 2=fivetoseven 3=eightmore)</param>
        /// <param >可加入人數(0=all 1=onetothree 2=fourtosix 3=sevenmore)</param>
        /// <returns></returns>
        #region"搜尋揪團"
        [HttpPost]
        [Route("search/groups/")]
        public IHttpActionResult SearchGroups2(ViewGroupSearch viewGroupSearch)
        {

            var query = db.Groups.AsQueryable();
            query = query.Where(g => g.EndTime > DateTime.Now && g.GroupState == EnumList.GroupState.開團中 && g.isPrivate != true && (g.MaxParticipants - g.CurrentParticipants) != 0);

            // 篩選城市
            if (viewGroupSearch.cityId.HasValue)
            {
                var city = db.Cities.FirstOrDefault(c => c.Id == viewGroupSearch.cityId);
                if (city != null)
                {
                    query = query.Where(g => g.Address.Contains(city.CityName) || (g.IsHomeGroup == false && g.Store.Address.Contains(city.CityName)));
                }
            }

            //篩選日期
            if (viewGroupSearch.startDate.HasValue)
            {
                var startDate = viewGroupSearch.startDate.Value.Date;
                var endDate = startDate.AddDays(1);

                query = query.Where(g => g.StartTime >= startDate && g.StartTime < endDate);
            }





            //// 篩選多種 遊戲名稱 & 團名 & 遊戲類型 (使用split ","分割)
            if (!string.IsNullOrEmpty(viewGroupSearch.gameName))
            {
                var keywords = viewGroupSearch.gameName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                query = query.Where(g => keywords.Any(kw => g.GroupName.Contains(kw))
                                         || g.GroupGames.Any(game => keywords.Any(kw => game.StoreInventory.GameDetails.Name.Contains(kw)))
                                         || g.GroupGames.Any(game => keywords.Any(kw => game.StoreInventory.GameDetails.GameType.TypeName.Contains(kw))));
            }

            //篩選關聯性

            switch (viewGroupSearch.groupFilter)
            {
                case EnumList.GroupFilter.relevance:
                    break;
                case EnumList.GroupFilter.Upcoming:
                    query = query.OrderBy(g => g.StartTime);
                    break;
                case EnumList.GroupFilter.Newest:
                    query = query.OrderByDescending(g => g.CreationDate);
                    break;
            }
            //篩選揪團總人數
            switch (viewGroupSearch.groupppl)
            {
                case EnumList.Groupppl.all:
                    break;
                case EnumList.Groupppl.twotofour:
                    query = query.Where(g => g.MaxParticipants >= 2 && g.MaxParticipants <= 4).OrderBy(m => m.MaxParticipants);
                    break;
                case EnumList.Groupppl.fivetoseven:
                    query = query.Where(g => g.MaxParticipants >= 5 && g.MaxParticipants <= 7).OrderBy(m => m.MaxParticipants);
                    break;
                case EnumList.Groupppl.eightmore:
                    query = query.Where(g => g.MaxParticipants >= 8).OrderBy(m => m.MaxParticipants);
                    break;

            }

            //篩選可加入人數
            switch (viewGroupSearch.joinppl)
            {
                case EnumList.Joinppl.all:
                    break;
                case EnumList.Joinppl.onetothree:
                    query = query.Where(g => (g.MaxParticipants - g.CurrentParticipants) >= 1 && (g.MaxParticipants - g.CurrentParticipants) <= 3).OrderBy(g => (g.MaxParticipants - g.CurrentParticipants));
                    break;
                case EnumList.Joinppl.fourtosix:
                    query = query.Where(g => (g.MaxParticipants - g.CurrentParticipants) >= 4 && (g.MaxParticipants - g.CurrentParticipants) <= 6).OrderBy(g => (g.MaxParticipants - g.CurrentParticipants));
                    break;
                case EnumList.Joinppl.sevenmore:
                    query = query.Where(g => (g.MaxParticipants - g.CurrentParticipants) >= 7).OrderBy(g => (g.MaxParticipants - g.CurrentParticipants));
                    break;

            }

            //篩選Tag
            switch (viewGroupSearch.groupTag)
            {
                case EnumList.GroupTag.all:
                    break;
                case EnumList.GroupTag.beginner:
                    query = query.Where(g => g.Beginner);
                    break;
                case EnumList.GroupTag.expert:
                    query = query.Where(g => g.Expert);
                    break;
                case EnumList.GroupTag.practice:
                    query = query.Where(g => g.Practice);
                    break;
                case EnumList.GroupTag.open:
                    query = query.Where(g => g.Open);
                    break;
                case EnumList.GroupTag.tutorial:
                    query = query.Where(g => g.Tutorial);
                    break;
                case EnumList.GroupTag.casual:
                    query = query.Where(g => g.Casual);
                    break;
                case EnumList.GroupTag.competitive:
                    query = query.Where(g => g.Competitive);
                    break;

            }


            var groupCount = query.Count();

            // 選取匹配群組
            var matchedGroups = query.Select(g => new
            {


                groupId = g.GroupId,
                groupName = g.GroupName,
                startTime = g.StartTime,
                endTime = g.EndTime,
                isHomeGroup = g.IsHomeGroup,
                isprivate = g.isPrivate,
                groupState = g.EndTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : g.GroupState.ToString(),
                place = g.Address,
                //address = g.IsHomeGroup ? g.Address : g.Store.Name,
                storeName = g.Store.Name,
                storeId = g.StoreId,
                Storeaddress = g.Store.Address,
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
                LeaderProfileImg = g.Member.Photo,
                members = db.GroupParticipants
                    .Where(gp => gp.GroupId == g.GroupId)
                    .Select(gp => new
                    {
                        memberId = gp.MemberId,
                        memberName = db.Members.Where(mn => mn.Id == gp.MemberId).Select(mn => mn.Nickname).FirstOrDefault(),
                        initNum = gp.InitMember,
                        profileImg = db.Members.Where(mn => mn.Id == gp.MemberId).Select(mn => mn.Photo).FirstOrDefault(),
                    }).ToList()
            }).ToList();


            //分頁
            if (viewGroupSearch.page != 0 && viewGroupSearch.pageSize != 0)
            {
                int skip = (viewGroupSearch.page - 1) * viewGroupSearch.pageSize;
                matchedGroups = matchedGroups.Skip(skip).Take(viewGroupSearch.pageSize).ToList();
            }



            // 然後，為每個群組獲取遊戲名稱
            var finalGroups = matchedGroups.Select(g => new
            {
                groupId = g.groupId,
                groupName = g.groupName,
                place = g.place,
                groupStatus = g.endTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : g.groupState.ToString(),
                isPrivate = g.isprivate,
                isHomeGroup = g.isHomeGroup,
                store = g.storeId == null && g.storeName == null && g.Storeaddress == null
            ? null
            : new
            {
                storeId = g.storeId,
                storeName = g.storeName,
                address = g.Storeaddress,
            },
                date = g.startTime.ToString("yyyy-MM-dd"),
                startTime = g.startTime.ToString("HH:mm"),
                endTime = g.endTime.ToString("HH:mm"),




                games = g.isHomeGroup ? null : db.GroupGames
                                                                .Where(gg => gg.GroupId == g.groupId)
                                                                .Select(gg => new { gameName = gg.StoreInventory.GameDetails.Name, gameType = gg.StoreInventory.GameDetails.GameTypeId })
                                                                .ToList(),




                leader = new
                {
                    userId = g.LeaderMemberId,
                    userName = g.LeaderNickname,
                    status = "leader",
                    initNum = g.LeaderInitMember ,
                    profileImg = BuildProfileImageUrl(g.LeaderProfileImg)

                },

                members = g.members.Select(m => new
                {
                    userId = m.memberId,
                    userName = m.memberName,
                    status = db.GroupParticipants.Where(gp => gp.MemberId == m.memberId && g.groupId == gp.GroupId).Select(gp => gp.AttendanceStatus.ToString()).FirstOrDefault(),
                    initNum = m.initNum ,
                    profileImg = BuildProfileImageUrl(m.profileImg),
                }).ToList(),

                tags = new List<string>
                    {
                        g.beginnerTag ? "新手團" : null,
                        g.expertTag ? "老手團" : null,
                        g.practiceTag ? "經驗切磋" : null,
                        g.openTag ? "不限定" : null,
                        g.tutorialTag ? "教學團" : null,
                        g.casualTag ? "輕鬆" : null,
                        g.competitiveTag ? "競技" : null
                    }.Where(t => t != null).ToList(),
                currentpeople = g.currentpeople,
                totalMemberNum = g.totalMemberNum,

            }).ToList();

            if (!finalGroups.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到符合條件的揪團活動" });
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { finalGroups, groupCount } });

        }

        #endregion
        /// <summary>
        /// 取得會員有興趣的團
        /// </summary>
        /// <param name="viewInterestingGroup"></param>
        /// <returns></returns>
        #region
        [HttpPost]
        [JwtAuthFilter]
        [Route("search/groupsinterest/")]
        public IHttpActionResult GroupsInterest(ViewInterestingGroup viewInterestingGroup)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int userId = (int)userToken["Id"];
            var memberInterests = db.MemberGamePrefs.Where(m => m.MemberId == userId).Select(m => m.GameType.TypeName).ToList(); // 獲取興趣列表

            var query = db.Groups.AsQueryable();
            query = query.Where(g => g.EndTime > DateTime.Now && g.GroupState == EnumList.GroupState.開團中 && g.isPrivate!= true);

            // 如果有興趣列表，基於會員興趣篩選群組
            if (memberInterests != null && memberInterests.Any())
            {
                query = query.Where(g => g.GroupGames.Any(game => memberInterests.Contains(game.StoreInventory.GameDetails.GameType.TypeName)));
            }

            

            var matchedGroups = query.Select(g => new
            {


                groupId = g.GroupId,
                groupName = g.GroupName,
                startTime = g.StartTime,
                endTime = g.EndTime,
                isHomeGroup = g.IsHomeGroup,
                isprivate = g.isPrivate,
                groupState = g.EndTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : g.GroupState.ToString(),
                place = g.Address,
                //address = g.IsHomeGroup ? g.Address : g.Store.Name,
                storeName = g.Store.Name,
                storeId = g.StoreId,
                Storeaddress = g.Store.Address,
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
                LeaderProfileImg = g.Member.Photo,
                members = db.GroupParticipants
                    .Where(gp => gp.GroupId == g.GroupId)
                    .Select(gp => new
                    {
                        memberId = gp.MemberId,
                        memberName = db.Members.Where(mn => mn.Id == gp.MemberId).Select(mn => mn.Nickname).FirstOrDefault(),
                        initNum = gp.InitMember,
                        profileImg = db.Members.Where(mn => mn.Id == gp.MemberId).Select(mn => mn.Photo).FirstOrDefault(),
                    }).ToList()
            }).ToList();

            //分頁
            if (viewInterestingGroup.page != 0 && viewInterestingGroup.pageSize != 0)
            {
                int skip = (viewInterestingGroup.page - 1) * viewInterestingGroup.pageSize;
                matchedGroups = matchedGroups.Skip(skip).Take(viewInterestingGroup.pageSize).ToList();
            }

            var finalGroups = matchedGroups.Select(g => new
            {
                groupId = g.groupId,
                groupName = g.groupName,
                place = g.place,
                groupStatus = g.endTime < DateTime.Now ? EnumList.GroupState.已結束.ToString() : g.groupState.ToString(),
                isPrivate = g.isprivate,
                isHomeGroup = g.isHomeGroup,
                store = g.storeId == null && g.storeName == null && g.Storeaddress == null
           ? null
           : new
           {
               storeId = g.storeId,
               storeName = g.storeName,
               address = g.Storeaddress,
           },
                date = g.startTime.ToString("yyyy-MM-dd"),
                startTime = g.startTime.ToString("HH:mm"),
                endTime = g.endTime.ToString("HH:mm"),




                games = g.isHomeGroup ? null : db.GroupGames
                                                               .Where(gg => gg.GroupId == g.groupId)
                                                               .Select(gg => new { gameName = gg.StoreInventory.GameDetails.Name, gameType = gg.StoreInventory.GameDetails.GameTypeId })
                                                               .ToList(),




                leader = new
                {
                    userId = g.LeaderMemberId,
                    userName = g.LeaderNickname,
                    status = "leader",
                    initNum = g.LeaderInitMember ,//前端邏輯需+1
                    profileImg = BuildProfileImageUrl(g.LeaderProfileImg)

                },

                members = g.members.Select(m => new
                {
                    userId = m.memberId,
                    userName = m.memberName,
                    status = db.GroupParticipants.Where(gp => gp.MemberId == m.memberId && g.groupId == gp.GroupId).Select(gp => gp.AttendanceStatus.ToString()).FirstOrDefault(),
                    initNum = m.initNum , //前端邏輯需+1
                    profileImg = BuildProfileImageUrl(m.profileImg),
                }).ToList(),

                tags = new List<string>
                    {
                        g.beginnerTag ? "新手團" : null,
                        g.expertTag ? "老手團" : null,
                        g.practiceTag ? "經驗切磋" : null,
                        g.openTag ? "不限定" : null,
                        g.tutorialTag ? "教學團" : null,
                        g.casualTag ? "輕鬆" : null,
                        g.competitiveTag ? "競技" : null
                    }.Where(t => t != null).ToList(),
                currentpeople = g.currentpeople,
                totalMemberNum = g.totalMemberNum,

            }).ToList();

            if (!finalGroups.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到符合條件的揪團活動" });
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data =  finalGroups  });
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
        private string BuildStoreImageUrl(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return null; // 或者返回一個默認的圖片路徑
            }
            //return $"http://4.224.16.99/upload/store/profile/{photo}";
            return $"https://2be5-4-224-16-99.ngrok-free.app/upload/store/profile/{photo}";
        }

        private string BuildProfileImageUrl(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return null; // 或者返回一個默認的圖片路徑
            }
            //return $"http://4.224.16.99/upload/profile/{photo}";
            return $"https://2be5-4-224-16-99.ngrok-free.app/upload/profile/{photo}";
        }

    }
}
