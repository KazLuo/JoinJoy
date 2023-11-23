using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Group", Description = "核心開團功能")]
    [RoutePrefix("group")]
    public class GroupController : ApiController
    {
        private Context db = new Context();

        /// <summary>
        /// 開團表單
        /// </summary>
        /// <param name="model">主要用於填寫開團資料(不含遊戲預約店家預約現況)</param>
        /// <returns></returns>
        #region"CreateGroup"
        [HttpPost]
        [JwtAuthFilter]
        [Route("create")]
        public IHttpActionResult CreateGroupWithGames([FromBody] ViewGroup model)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);

            int id = (int)userToken["Id"];

            var memberInfo = db.Members.FirstOrDefault(m => m.Id == id);

            //檢查格式
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 檢查最大參與者人數
            if (model.totalMemberNum > 12)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團人數上限為12人" });
            }

            // 檢查最大參與者人數
            if (model.totalMemberNum == 0)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團最大人數不可為0" });
            }

            // 檢查開團者與攜帶人數是否超過totalMemberNum
            if (model.initNum > model.totalMemberNum)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團主及同行親友不可超過上限人數" });
            }
            // 是否預約到過去的時間
            if (model.startTime < DateTime.Now)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "過去的時間是不能預約的" });
            }
            // 檢查結束時間是否在開始時間之後
            if (model.endTime <= model.startTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "結束時間必須在開始時間之後。" });
            }
            if (!model.isHomeGroup)
            {

                if (model.gameId.Count > 5)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您只能選擇最多五款遊戲" });
                }
                var store = db.Stores.FirstOrDefault(s => s.Id == model.storeId);
                if (store == null)
                {
                    // 如果 storeId 不存在，返回一個錯誤訊息
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = $"店家 ID {model.storeId} 不存在。" });
                }

                // 獲取當天的日期部分
                DateTime dateOnly = model.startTime.Date;
                //由於儲存店家的時間是使用TimeSpan而不是DateTime,因此要做轉換
                // 將 TimeSpans 轉換為當天的 DateTime
                DateTime openingDateTime = dateOnly + store.OpenTime;
                DateTime closingDateTime = dateOnly + store.CloseTime;



                // 檢查開團時間是否在店家營業時間內
                if (model.startTime < openingDateTime || model.endTime > closingDateTime)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團時間必須在店家營業時間內。" });
                }
            }


            var group = new Group
            {
                MemberId = id,
                //StoreId = model.storeId,
                StoreId = model.isHomeGroup ? (int?)null : model.storeId, // 如果是 HomeGroup，則 StoreId 可為 null
                GroupName = model.groupName,
                StartTime = model.startTime,
                EndTime = model.endTime,
                MaxParticipants = model.totalMemberNum,
                Description = model.description,
                IsHomeGroup = model.isHomeGroup,
                Address = model.place,
                InitMember = model.initNum,
                CurrentParticipants = model.initNum,
                //修改邏輯
                //CurrentParticipants = 1 + model.initNum,
                Beginner = model.beginnerTag,
                Expert = model.expertTag,
                Practice = model.practiceTag,
                Open = model.openTag,
                Tutorial = model.tutorialTag,
                Casual = model.casualTag,
                Competitive = model.competitiveTag,
                CreationDate = DateTime.Now,
                GroupState = EnumList.GroupState.開團中,
                isPrivate = model.isPrivate
            };

            db.Groups.Add(group);
            db.SaveChanges(); // 儲存團體以獲取 GroupId
            if (!model.isHomeGroup)
            {
                foreach (var gameId in model.gameId)
                {
                    // 在這裡檢查每個 gameId 是否存在於 StoreInventories 表中
                    var storeInventory = db.StoreInventories.FirstOrDefault(si => si.Id == gameId);
                    if (storeInventory == null)
                    {
                        // 如果 gameId 不存在，返回一個錯誤訊息
                        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = $"遊戲 ID {gameId} 不存在於店家庫存中。" });
                    }

                    var groupGame = new GroupGame
                    {
                        GroupId = group.GroupId,
                        StoreInventoryId = gameId,
                        InitDate = DateTime.Now
                    };
                    db.GroupGames.Add(groupGame);
                }
            }

            db.SaveChanges(); // 儲存團體遊戲
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "團體和遊戲成功添加。", groupId = group.GroupId });
        }
        #endregion
        /// <summary>
        /// 揪團留言板(送出訊息)
        /// </summary>
        /// <param name="viewGroupComment">送出留言功能</param>
        /// <returns></returns>
        #region "GroupComment"
        [HttpPost]
        [Route("comments")]
        [JwtAuthFilter]
        public IHttpActionResult GroupComment(ViewGroupComment viewGroupComment)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];
            var memberInfo = db.Members.FirstOrDefault(m => m.Id == memberId);
            //可以使用int? 形成可空的int
            int? groupId = viewGroupComment.groupId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            if (!groupId.HasValue)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "groupId 不能為 null" });
            }

            // 使用 FirstOrDefault 檢查資料庫中是否存在該 groupId
            var groupInDb = db.Groups.FirstOrDefault(m => m.GroupId == groupId.Value);
            if (groupInDb == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "該團尚未開放，無法送出留言" });
            }


            GroupComment newgroupComment = new GroupComment
            {
                GroupId = viewGroupComment.groupId,
                MemberId = memberInfo.Id,
                CommentContent = viewGroupComment.commentTxt,
            };
            db.GroupComments.Add(newgroupComment);
            db.SaveChanges();
            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "已成功留言", data = new { userId = newgroupComment.MemberId, groupId = newgroupComment.GroupId } });
        }
        #endregion

        /// <summary>
        /// 揪團留言板(接收訊息)
        /// </summary>
        /// <param>收入該團所有訊息</param>
        /// <returns></returns>
        #region "GroupComment"
        [HttpGet]
        [Route("comments/{groupId}")]
        public IHttpActionResult GetComment(int? groupId)
        {
            if (groupId == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "沒有groupId" });
            }


            var commentsWithMemberDetails = db.GroupComments
            .Where(m => m.GroupId == groupId)
            .Join(db.Members,
            comment => comment.MemberId,
            member => member.Id,
            (comment, member) => new
            {
                Member = member,
                Comment = comment
            })
            .ToList() // 執行查詢，將結果帶到記憶體中
            .Select(x => new
            {
                userId = x.Comment.MemberId,
                userName = x.Member.Nickname,
                userPhoto = BuildProfileImageUrl(x.Member.Photo), // 在記憶體中調用
                commentContent = x.Comment.CommentContent,
                commentDate = x.Comment.CommentDate
            })
            .ToList();

            if (commentsWithMemberDetails == null || !commentsWithMemberDetails.Any())
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "尚未有留言" });
            }

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "讀取留言成功", data = commentsWithMemberDetails });
        }


        #endregion
        /// <summary>
        /// 申請入團
        /// </summary>
        /// <param name="viewJoinGroup"></param>
        /// <returns></returns>
        #region "JoinGroup"
        [HttpPost]
        [JwtAuthFilter]
        [Route("join")]
        public IHttpActionResult JoinGroup(ViewJoinGroup viewJoinGroup)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];
            var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            var group = db.Groups.FirstOrDefault(m => m.GroupId == viewJoinGroup.groupId);

            if (member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }
            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "該團尚未開放" });
            }

            if (group.MemberId == memberId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團主不能加入自己團" });
            }

            if (DateTime.Now > group.StartTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "該團已逾時，無法加入" });
            }
            if (group.GroupState != EnumList.GroupState.開團中)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "該團已送出預約，下次請早!" });
            }
            //if (group.CurrentParticipants + viewJoinGroup.initNum + 1 > group.MaxParticipants)
            //{
            //    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "已經滿團囉!" });
            //}
            //修改邏輯
            if (group.CurrentParticipants + viewJoinGroup.initNum > group.MaxParticipants)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "已經滿團囉!" });
            }
            if (viewJoinGroup.initNum == 0)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "參加人數不可為0!" });
            }
            if (db.GroupParticipants.Any(m => m.GroupId == viewJoinGroup.groupId && m.MemberId == memberId))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "不可以重複申請入團哦!" });
            }

            db.GroupParticipants.Add(new GroupParticipant
            {
                GroupId = (int)viewJoinGroup.groupId,
                MemberId = memberId,
                InitMember = viewJoinGroup.initNum  // 儲存申請者帶的朋友數量(包含自己)
            });

            db.SaveChanges();
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = $"已經成功申請入團，待審核!" });
        }

        #endregion
        /// <summary>
        /// 列出所有成員(包含審核及未審核)
        /// </summary>
        /// <param name="groupId">帶入groupId</param>
        /// <returns></returns>
        #region "JoinGroupList"
        [HttpGet]
        //[JwtAuthFilter]
        [Route("joinList")]
        public IHttpActionResult JoinGroupList(int? groupId)
        {
            // 檢查團隊是否存在
            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                // 團隊不存在的回應
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在" });
            }

            var leaderData = db.Groups
                .Where(m => m.GroupId == groupId)
                .Select(m => new
                {
                    m.MemberId,
                    m.Member.Nickname,
                    m.Member.Photo,
                    m.InitMember
                })
                .ToList();  // 先將數據轉換為List，這樣就不在數據庫中處理了

            // 現在在內存中處理數據
            var leader = leaderData.Select(m => new
            {
                userId = m.MemberId,
                userName = m.Nickname,
                profileImg = BuildProfileImageUrl(m.Photo),
                status = EnumList.JoinGroupState.leader.ToString(),
                initNum = m.InitMember
            }).ToList();

            // 同理對於參與者數據
            var memberData = db.GroupParticipants
                .Where(gp => gp.GroupId == groupId)
                .Join(db.Members,
                      gp => gp.MemberId,
                      mem => mem.Id,
                      (gp, mem) => new
                      {
                          gp.MemberId,
                          mem.Nickname,
                          mem.Photo,
                          gp.AttendanceStatus,
                          gp.InitMember
                      })
                .ToList();  // 先將數據轉換為List

            var member = memberData.Select(gp => new
            {
                userId = gp.MemberId,
                userName = gp.Nickname,
                profileImg = BuildProfileImageUrl(gp.Photo),
                status = gp.AttendanceStatus.ToString(),
                initNum = gp.InitMember
            }).ToList();

            // 合併leader和member的資料
            var data = leader.Concat(member).ToList();



            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "成功回傳揪團成員", data });
        }
        #endregion
        /// <summary>
        /// 審核團員
        /// </summary>
        /// <param name="groupId">團隊id</param>
        /// <param name="viewReviewGroup">受審者受審狀態</param>
        /// <returns></returns>
        #region"審核團員"
        [HttpPost]
        [JwtAuthFilter]
        [Route("reviewGroup/{groupId}")]
        public IHttpActionResult ReviewGroup(int groupId, ViewReviewGroup viewReviewGroup)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"];
            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);

            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在" });
            }

            if (group.MemberId != currentUserId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團長可以審核團員" });
            }

            var joinRequest = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == groupId && gp.MemberId == viewReviewGroup.userId);

            if (joinRequest == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "入團申請不存在" });
            }
            //逾時後不可審核
            if (DateTime.Now > group.StartTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團隊已逾時，無法審核人員" });
            }

            // 將前端傳送的具名值轉換為對應的枚舉值
            if (!Enum.TryParse(viewReviewGroup.status, out EnumList.JoinGroupState status))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "無效的審核狀態" });
            }
            // 檢查是否嘗試將成員設為團主，這是不允許的
            if (status == EnumList.JoinGroupState.leader)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "不允許將成員設定為團主" });
            }

            if (status == EnumList.JoinGroupState.member)
            {
                joinRequest.AttendanceStatus = EnumList.JoinGroupState.member;
                int totalParticipants = joinRequest.InitMember;
                group.CurrentParticipants += totalParticipants;
            }
            else if (status == EnumList.JoinGroupState.rejected)
            {
                db.GroupParticipants.Remove(joinRequest);
            }

            db.SaveChanges();
            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = $"入團申請的狀態已更新為：{status.ToString()}。" });
        }
        #endregion
        /// <summary>
        /// 查詢預約時段剩餘位置
        /// </summary>
        /// <param name="storeId">測試可以用"7"</param>
        /// <param name="date">測試可以使用2023-11-01 ,有滿人的 2023-11-07</param>
        /// <returns></returns>
        #region"查看店家可預約時段"
        [HttpGet]
        [Route("checkability/{storeId}/{date}")]
        public IHttpActionResult GetStoreOperatingHoursWithAvailability(int storeId, DateTime date)
        {
            // 從資料庫中查找商店實體
            var store = db.Stores.FirstOrDefault(s => s.Id == storeId);
            if (store == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到指定的店家" });
            }

            // 初始化營業時段及剩餘座位的列表
            var operatingHoursList = new List<object>();
            var startTime = date.Date.Add(store.OpenTime); // 指定日期加上營業開始時間
            var endTime = date.Date.Add(store.CloseTime); // 指定日期加上營業結束時間


            // 計算每個小時段的營業時間及剩餘座位
            for (var hour = startTime; hour < endTime; hour = hour.AddHours(1))
            {
                var nextHour = hour.AddHours(1);

                // 查詢該時段內所有的團體預約
                var reservations = db.Groups.Where(g => g.StoreId == store.Id
                                                        && g.StartTime < nextHour
                                                        && g.EndTime > hour
                                                        && g.GroupState == EnumList.GroupState.已預約) // 假設 Status 為已確認的預約
                                                .ToList();

                // 計算該時段內已預約的總人數
                var reservedSeats = reservations.Sum(g => g.CurrentParticipants);

                // 計算剩餘座位數
                var remainingSeats = store.MaxPeople - reservedSeats;

                // 如果剩餘座位數小於 0，設為 0
                remainingSeats = Math.Max(remainingSeats, 0);

                operatingHoursList.Add(new
                {
                    time = $"{hour.ToString("HH:mm")}~{nextHour.ToString("HH:mm")}",
                    seats = remainingSeats
                });
            }

            // 返回店家指定日期的每個小時營業時段及其剩餘座位

            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "成功回傳", data = operatingHoursList });
        }
        #endregion
        /// <summary>
        /// 取得開團詳細資訊
        /// </summary>
        /// <param name="groupId">測試可用27</param>
        /// <returns></returns>
        #region"取得開團資訊"
        [HttpGet]
        [JwtAuthFilter]
        [Route("detail/{groupId}")]
        public IHttpActionResult GetGroupDetails(int groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];
            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId && g.MemberId == memberId);

            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在,或是非團主" });
            }
            var groupWithGames = db.Groups
                                   .Include("GroupGames.StoreInventory.Game")  // EF6
                                   .Where(g => g.GroupId == groupId)
                                   .Select(g => new
                                   {
                                       store = g.IsHomeGroup ? null : new
                                       {
                                           storeId = g.Store != null ? g.Store.Id : (int?)null,
                                           storeName = g.Store != null ? g.Store.Name : null,
                                           address = g.Store != null ? g.Store.Address : null,
                                       },
                                       groupName = g.GroupName,
                                       startTime = g.StartTime,
                                       endTime = g.EndTime,
                                       totalMemberNum = g.MaxParticipants,
                                       description = g.Description,
                                       isHomeGroup = g.IsHomeGroup,
                                       plane = g.Address,
                                       initMember = g.InitMember,
                                       tags = new List<string> {
                                           g.Beginner ? "新手團" : null,
                                           g.Expert ? "老手團" : null,
                                           g.Practice ? "經驗切磋" : null,
                                           g.Open ? "不限定" : null,
                                           g.Tutorial ? "教學團" : null,
                                           g.Casual ? "輕鬆" : null,
                                           g.Competitive ? "競技" : null
                                       }.Where(tag => tag != null).ToList(),
                                       isPrivate = g.isPrivate,
                                       games = g.GroupGames
                                                .Select(gg => new
                                                {
                                                    gameId = gg.StoreInventory.Id,
                                                    gameName = gg.StoreInventory.GameDetails.Name,
                                                    gameType = gg.StoreInventory.GameDetails.GameType.TypeName
                                                }).ToList(),
                                       createDate = g.CreationDate
                                   })
                                   .FirstOrDefault();


            if (groupWithGames == null)
            {
                return NotFound();
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { groupWithGames } });
        }

        #endregion
        /// <summary>
        /// 編輯開團資訊
        /// </summary>
        /// <param name="groupId">團體ID</param>
        /// <param name="model">編輯的團體資訊</param>
        /// <returns></returns>
        #region "編輯開團資訊"
        [HttpPost]
        [JwtAuthFilter]
        [Route("edit/{groupId}")]
        public IHttpActionResult UpdateGroupDetails(int groupId, [FromBody] ViewEditGroup model)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"];
            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);

            // 在更新之前檢查團隊狀態
            if (group.GroupState == EnumList.GroupState.已預約) // 假設 GroupState 是一個枚舉類型，包含不同的團隊狀態
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團隊已預約，無法更新資訊" });
            }

            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在" });
            }

            if (group.MemberId != currentUserId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團主可以修改資訊" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (DateTime.Now > group.StartTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團隊已逾時，無法進行修改" });
            }

            // 更新團體的基本資訊
            group.GroupName = model.groupName;
            group.Description = model.description;
            group.Beginner = model.beginnerTag;
            group.Expert = model.expertTag;
            group.Practice = model.practiceTag;
            group.Open = model.openTag;
            group.Tutorial = model.tutorialTag;
            group.Casual = model.casualTag;
            group.Competitive = model.competitiveTag;
            group.isPrivate = model.isPrivate;
            group.CreationDate = DateTime.Now;

            // 更新遊戲列表
            var existingGames = group.GroupGames.ToList();

            // 移除與該團體ID關聯的所有遊戲
            var gamesToRemove = db.GroupGames.Where(gg => gg.GroupId == groupId).ToList();
            foreach (var game in gamesToRemove)
            {
                db.GroupGames.Remove(game);
            }

            db.SaveChanges(); // 確保移除操作立即生效

            // 加入新選擇的遊戲
            foreach (var gameId in model.GameIds)
            {
                var gameToAdd = new GroupGame { GroupId = groupId, StoreInventoryId = gameId, InitDate = DateTime.Now };
                db.GroupGames.Add(gameToAdd);
            }

            // 儲存所有更改
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // 處理異常情況，例如列印日誌或返回特定的錯誤信息
                return Content(HttpStatusCode.InternalServerError, new { statusCode = HttpStatusCode.InternalServerError, status = false, message = "無法更新開團資訊，請聯繫系統管理員。" });
            }

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "開團資訊已更新" });



        }
        #endregion
        /// <summary>
        /// 取得開團簡易資訊
        /// </summary>
        /// <param name="groupId">測試可用6和28</param>
        /// <returns></returns>
        #region"取得開團簡易資訊"
        [HttpGet]
        [Route("easydetail/{groupId}")]
        public IHttpActionResult GetGroupEasyDetails(int groupId)
        {

            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);

            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在" });
            }

            // 假設團主的信息存儲在Group表的LeaderId欄位
            var leaderId = group.MemberId; // 或者是 MemberId，取決於您的資料庫結構

            // 獲取該團隊所有成員的詳細信息，包括團主
            var membersDetails = db.GroupParticipants
                .Where(gp => gp.GroupId == groupId)
                .Join(db.Members,
                      gp => gp.MemberId,
                      mem => mem.Id,
                      (gp, mem) => new
                      {
                          gp.MemberId,
                          mem.Nickname,
                          gp.AttendanceStatus,
                          gp.InitMember,
                          mem.Photo
                      })
                .ToList();

            var membersdata = membersDetails.Select(jr => new
            {
                userId = jr.MemberId,
                userName = jr.Nickname,
                status = jr.AttendanceStatus.ToString(),
                initNum = jr.InitMember, // 加1
                profileImg = BuildProfileImageUrl(jr.Photo) // 假设这是构建图片 URL 的方法
            }).ToList();

            // 添加團主的詳細信息
            if (!membersDetails.Any(m => m.MemberId == leaderId)) // 如果團主不在成員列表中
            {
                var leaderDetails = db.Members
                    .Where(m => m.Id == leaderId)
                    .FirstOrDefault();

                var leaderdata = new
                {
                    userId = leaderDetails.Id,
                    userName = leaderDetails.Nickname,
                    status = EnumList.JoinGroupState.leader.ToString(), // 或其他適合您需求的狀態
                    initNum = group.InitMember, //前端希望init等於加入總數所以+1本人
                    profileImg = BuildProfileImageUrl(leaderDetails.Photo)
                };

                if (leaderdata != null)
                {
                    // 將團主插入到列表的第一位
                    membersdata.Insert(0, leaderdata);
                }
            }

            // 接著，我們獲取這個團隊的其他信息
            var groupQuery = db.Groups
                .Include(g => g.Store)
                .Include(g => g.GroupGames.Select(gg => gg.StoreInventory.GameDetails))
                .Where(g => g.GroupId == groupId)
                .Select(g => new
                {
                    g.GroupName,
                    g.GroupState,
                    g.IsHomeGroup,
                    g.Address,
                    g.isPrivate,
                    Price = g.IsHomeGroup || g.Store == null ? (int?)null : g.Store.Price,
                    StoreId = g.Store != null ? g.Store.Id : (int?)null,
                    StoreName = g.Store != null ? g.Store.Name : null,
                    StoreAddress = g.Store != null ? g.Store.Address : null,

                    g.StartTime,
                    g.EndTime,
                    g.MaxParticipants,
                    Games = g.GroupGames.Select(gg => new
                    {
                        gameId = gg.StoreInventory.Id,
                        gameName = gg.StoreInventory.GameDetails.Name,
                        gameType = gg.StoreInventory.GameDetails.GameType.Id
                    }).ToList(),
                    g.Description,
                    Tags = new // 假設 Tags 是一個匿名類型
                    {
                        Beginner = g.Beginner,
                        Expert = g.Expert,
                        Practice = g.Practice,
                        Open = g.Open,
                        Tutorial = g.Tutorial,
                        Casual = g.Casual,
                        Competitive = g.Competitive
                    }
                })
                .FirstOrDefault();

            if (groupQuery == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到團隊相關訊息" });
            }

            // 處理 tags
            var tags = new List<string>();
            if (groupQuery.Tags.Beginner) tags.Add("新手團");
            if (groupQuery.Tags.Expert) tags.Add("老手團");
            if (groupQuery.Tags.Practice) tags.Add("經驗切磋");
            if (groupQuery.Tags.Open) tags.Add("不限定");
            if (groupQuery.Tags.Tutorial) tags.Add("教學團");
            if (groupQuery.Tags.Casual) tags.Add("輕鬆");
            if (groupQuery.Tags.Competitive) tags.Add("競技");

            // 根據是否為自家團隊來決定是否顯示商店和遊戲資訊
            object storeInfo = groupQuery.IsHomeGroup ? null : new
            {
                storeId = groupQuery.StoreId,
                storeName = groupQuery.StoreName,
                address = groupQuery.StoreAddress
            };

            object gamesInfo = groupQuery.IsHomeGroup ? null : groupQuery.Games;

            // 獲取當前時間
            DateTime now = DateTime.Now;

            string groupStatus;

            // 判斷組的狀態並根據時間進行修改
            switch (groupQuery.GroupState)
            {
                case EnumList.GroupState.開團中:
                    groupStatus = now > groupQuery.StartTime ? EnumList.GroupState.已失效.ToString() : EnumList.GroupState.開團中.ToString();
                    break;
                case EnumList.GroupState.已預約:
                    groupStatus = now > groupQuery.EndTime ? EnumList.GroupState.已結束.ToString() : EnumList.GroupState.已預約.ToString();
                    break;
                default:
                    groupStatus = groupQuery.GroupState.ToString();
                    break;
            }

            // 建立最終的物件
            var groupWithGames = new
            {
                groupName = groupQuery.GroupName,
                //groupStatus = groupQuery.GroupState.ToString(),
                groupStatus = groupStatus,
                place = groupQuery.IsHomeGroup ? groupQuery.Address : null,
                isPrivate = groupQuery.isPrivate,
                store = storeInfo,
                date = groupQuery.StartTime.ToString("yyyy/MM/dd"),
                startTime = groupQuery.StartTime.ToString("HH:mm"),
                endTime = groupQuery.EndTime.ToString("HH:mm"),
                cost = groupQuery.Price.HasValue ? $"NT${groupQuery.Price.Value} 元 / 每人每小時" : null,
                totalMemberNum = groupQuery.MaxParticipants,
                games = gamesInfo,
                description = groupQuery.Description,
                members = membersdata,
                tags = tags // 使用處理後的標籤列表
            };
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { groupWithGames } });

        }
        #endregion
        /// <summary>
        /// 取得開團詳細資訊(JWT驗證)
        /// </summary>
        /// <param name="groupId">輸入揪團ID</param>
        /// <returns></returns>
        #region"取得開團簡易資訊(JWT)"
        [HttpGet]
        [JwtAuthFilter]
        [Route("detailJWT/{groupId}")]
        public IHttpActionResult GetGroupDetailsJWT(int groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"];

            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId && g.MemberId == currentUserId);

            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在，或是非團主" });
            }

            // 假設團主的信息存儲在Group表的LeaderId欄位
            var leaderId = group.MemberId; // 或者是 MemberId，取決於您的資料庫結構

            // 獲取該團隊所有成員的詳細信息，包括團主
            var membersDetails = db.GroupParticipants
                .Where(gp => gp.GroupId == groupId)
                .Join(db.Members,
                      gp => gp.MemberId,
                      mem => mem.Id,
                      (gp, mem) => new
                      {
                          gp.MemberId,
                          mem.Nickname,
                          gp.AttendanceStatus,
                          gp.InitMember,
                          mem.Photo
                      })
                .ToList();

            var membersdata = membersDetails.Select(jr => new
            {
                userId = jr.MemberId,
                userName = jr.Nickname,
                status = jr.AttendanceStatus.ToString(),
                initNum = jr.InitMember, // 加1
                profileImg = BuildProfileImageUrl(jr.Photo) // 假设这是构建图片 URL 的方法
            }).ToList();

            // 添加團主的詳細信息
            if (!membersDetails.Any(m => m.MemberId == leaderId)) // 如果團主不在成員列表中
            {
                var leaderDetails = db.Members
                    .Where(m => m.Id == leaderId)
                    .FirstOrDefault();

                var leaderdata = new
                {
                    userId = leaderDetails.Id,
                    userName = leaderDetails.Nickname,
                    status = EnumList.JoinGroupState.leader.ToString(), // 或其他適合您需求的狀態
                    initNum = group.InitMember, //前端希望init等於加入總數所以+1本人
                    profileImg = BuildProfileImageUrl(leaderDetails.Photo)
                };

                if (leaderdata != null)
                {
                    // 將團主插入到列表的第一位
                    membersdata.Insert(0, leaderdata);
                }
            }

            // 接著，我們獲取這個團隊的其他信息
            var groupQuery = db.Groups
                .Include(g => g.Store)
                .Include(g => g.GroupGames.Select(gg => gg.StoreInventory.GameDetails))
                .Where(g => g.GroupId == groupId)
                .Select(g => new
                {
                    g.GroupName,
                    g.GroupState,
                    g.IsHomeGroup,
                    g.Address,
                    g.isPrivate,
                    Price = g.IsHomeGroup || g.Store == null ? (int?)null : g.Store.Price,
                    StoreId = g.Store != null ? g.Store.Id : (int?)null,
                    StoreName = g.Store != null ? g.Store.Name : null,
                    StoreAddress = g.Store != null ? g.Store.Address : null,

                    g.StartTime,
                    g.EndTime,
                    g.MaxParticipants,
                    Games = g.GroupGames.Select(gg => new
                    {
                        gameId = gg.StoreInventory.Id,
                        gameName = gg.StoreInventory.GameDetails.Name,
                        gameType = gg.StoreInventory.GameDetails.GameType.Id
                    }).ToList(),
                    g.Description,
                    Tags = new // 假設 Tags 是一個匿名類型
                    {
                        Beginner = g.Beginner,
                        Expert = g.Expert,
                        Practice = g.Practice,
                        Open = g.Open,
                        Tutorial = g.Tutorial,
                        Casual = g.Casual,
                        Competitive = g.Competitive
                    }
                })
                .FirstOrDefault();

            if (groupQuery == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到團隊相關訊息" });
            }

            // 處理 tags
            var tags = new List<string>();
            if (groupQuery.Tags.Beginner) tags.Add("新手團");
            if (groupQuery.Tags.Expert) tags.Add("老手團");
            if (groupQuery.Tags.Practice) tags.Add("經驗切磋");
            if (groupQuery.Tags.Open) tags.Add("不限定");
            if (groupQuery.Tags.Tutorial) tags.Add("教學團");
            if (groupQuery.Tags.Casual) tags.Add("輕鬆");
            if (groupQuery.Tags.Competitive) tags.Add("競技");

            // 根據是否為自家團隊來決定是否顯示商店和遊戲資訊
            object storeInfo = groupQuery.IsHomeGroup ? null : new
            {
                storeId = groupQuery.StoreId,
                storeName = groupQuery.StoreName,
                address = groupQuery.StoreAddress
            };

            object gamesInfo = groupQuery.IsHomeGroup ? null : groupQuery.Games;

            // 獲取當前時間
            DateTime now = DateTime.Now;

            string groupStatus;

            // 判斷組的狀態並根據時間進行修改
            switch (groupQuery.GroupState)
            {
                case EnumList.GroupState.開團中:
                    groupStatus = now > groupQuery.StartTime ? EnumList.GroupState.已失效.ToString() : EnumList.GroupState.開團中.ToString();
                    break;
                case EnumList.GroupState.已預約:
                    groupStatus = now > groupQuery.EndTime ? EnumList.GroupState.已結束.ToString() : EnumList.GroupState.已預約.ToString();
                    break;
                default:
                    groupStatus = groupQuery.GroupState.ToString();
                    break;
            }

            // 建立最終的物件
            var groupWithGames = new
            {
                groupName = groupQuery.GroupName,
                //groupStatus = groupQuery.GroupState.ToString(),
                groupStatus = groupStatus,
                place = groupQuery.IsHomeGroup ? groupQuery.Address : null,
                isPrivate = groupQuery.isPrivate,
                store = storeInfo,
                date = groupQuery.StartTime.ToString("yyyy/MM/dd"),
                startTime = groupQuery.StartTime.ToString("HH:mm"),
                endTime = groupQuery.EndTime.ToString("HH:mm"),
                cost = groupQuery.Price.HasValue ? $"NT${groupQuery.Price.Value} 元 / 每人每小時" : null,
                totalMemberNum = groupQuery.MaxParticipants,
                games = gamesInfo,
                description = groupQuery.Description,
                members = membersdata,
                tags = tags // 使用處理後的標籤列表
            };
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data = new { groupWithGames } });

        }
        #endregion
        /// <summary>
        /// 團員退出團隊
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        #region"團員退出團隊"
        [HttpPost]
        [JwtAuthFilter]
        [Route("leavegroup/{groupId}")]
        public IHttpActionResult LeaveGroup(int groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];
            // 檢查團隊是否存在
            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到該團" });

            }

            // 確保用戶是團隊的成員
            var member = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == groupId && gp.MemberId == memberId);
            if (member == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "用戶不是此團隊的成員。" });
            }

            // 檢查團隊狀態，只有在開團中才能退出
            if (group.GroupState != EnumList.GroupState.開團中)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "無法退出，因為團隊目前不是開團中狀態。如果是團主請使用解散團隊" });
            }

            // 從團隊中移除成員
            db.GroupParticipants.Remove(member);
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "成功退出團隊。" });
        }

        #endregion
        /// <summary>
        /// 更改開團狀態(開團中/預約中)
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        #region"更改開團狀態(開團中/預約中)"
        [HttpPost]
        [JwtAuthFilter]
        [Route("reservegroup/{groupId}")]
        public IHttpActionResult ReserveGroup(int groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"];// 獲取當前登錄用戶的ID
            // 檢查該團隊是否存在
            var group = db.Groups.Include(g => g.GroupParticipants).FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到該團" });
            }

            // 檢查是否為團主的請求
            if (group.MemberId != currentUserId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團主才能預約。" });
            }

            // 確認所有成員都已經審核通過
            if (group.GroupParticipants.Any(gp => gp.AttendanceStatus != EnumList.JoinGroupState.member))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "無法預約，因為並非所有成員都已審核通過。" });
            }


            // 確認團隊目前的狀態允許預約
            if (group.GroupState != EnumList.GroupState.開團中 || DateTime.Now >= group.StartTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團隊狀態不允許進行預約，或已超過預約時間。" });
            }
            //如果是自家團
            if (group.IsHomeGroup == true)
            {
                group.GroupState = EnumList.GroupState.已預約;
            }
            // 在此處進行預約前的座位檢查
            var startTime = group.StartTime;
            var endTime = group.EndTime;
            var store = db.Stores.FirstOrDefault(s => s.Id == group.StoreId);
            if (store != null)
            {
                // 查詢該時段內所有的團體預約
                var reservations = db.Groups.Where(g => g.StoreId == store.Id
                                                        && g.StartTime < endTime
                                                        && g.EndTime > startTime
                                                        && g.GroupState == EnumList.GroupState.已預約)
                                            .ToList();

                // 計算該時段內已預約的總人數
                var reservedSeats = reservations.Sum(g => g.CurrentParticipants);

                // 計算剩餘座位數
                var remainingSeats = store.MaxPeople - reservedSeats;

                // 如果沒有足夠的剩餘座位
                if (remainingSeats < group.CurrentParticipants)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "該時段內沒有足夠的剩餘座位。" });
                }
            }




            // 更新團隊狀態為已預約
            group.GroupState = EnumList.GroupState.已預約;

            //// 更新店家當前人數(暫時不需要，改用group表單計算current總數)
            //var store = db.Stores.FirstOrDefault(s => s.Id == group.StoreId);
            //if (store != null)
            //{
            //    store.CurPeople += group.GroupParticipants.Count();
            //}

            // 儲存所有更改
            db.SaveChanges();
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "預約成功，團隊狀態已更新，店家人數已更新。" });
        }
        #endregion
        /// <summary>
        /// 團主解散揪團
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        #region"團主解散揪團"
        [HttpPost]
        [JwtAuthFilter]
        [Route("disbandgroup/{groupId}")]
        public IHttpActionResult DisbandGroup(int groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"];// 獲取當前登錄用戶的ID
            // 查找團隊
            var group = db.Groups.Include(g => g.GroupParticipants).FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到該團" });
            }

            // 驗證請求者是否為團主
            if (group.MemberId != currentUserId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團主可以解散團隊。" });
            }

            // 確認團隊狀態是否為開團中
            if (group.GroupState != EnumList.GroupState.開團中)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有在開團中的狀態才可以解散團隊" });
            }

            // 刪除所有參加人員的參加紀錄
            db.GroupParticipants.RemoveRange(group.GroupParticipants);

            // 刪除團隊的開團紀錄
            db.Groups.Remove(group);

            // 儲存更改到數據庫
            db.SaveChanges();
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "團隊已成功解散。" });
        }

        #endregion

        /// <summary>
        /// 取得所有揪團ID
        /// </summary>
        /// <returns></returns>
        #region"取得所有揪團ID"
        [HttpGet]
        [Route("getallgroupid")]
        public IHttpActionResult GetAllGroupId()
        {

            try
            {
                var data = db.Groups.Select(m => new { groupId = m.GroupId, groupName = m.GroupName }).ToList();
                return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data });
            }
            catch (Exception)
            {

                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "回傳失敗" });
            }
        }
        #endregion
        /// <summary>
        /// 點名系統
        /// </summary>
        /// <param name="groupId">要點名的groupId</param>
        /// <param name="viewRollcall">body帶入memberId 並輸入true or false來判定是否報到</param>
        /// <returns></returns>
        #region "點名系統"
        [HttpPost]
        [JwtAuthFilter]
        [Route("rollcall")]
        public IHttpActionResult RollCall(int groupId, ViewRollcall viewRollcall)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"]; // 獲取當前登錄用戶的ID
            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團隊不存在" }); // 團隊不存在
            }

            // 確認是否由團主操作
            if (group.MemberId != currentUserId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "不是團主無法點名" });
            }

            // 查找對應的 GroupParticipant 實體
            var participant = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == groupId && gp.MemberId == viewRollcall.memberId);

            if (participant == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "找不到對應的參與者" }); // 如果找不到對應的參與者
            }

            // 更新出席狀態
            participant.IsPresent = viewRollcall.isPresent;

            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "點名成功", isPresent = participant.IsPresent });
        }


        #endregion


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

