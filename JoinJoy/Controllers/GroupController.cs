﻿using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
        //[HttpPost]
        //[Route("create")]
        //[JwtAuthFilter]
        //public IHttpActionResult CreateGroup(ViewGroup viewGroup)
        //{
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);

        //    int id = (int)userToken["Id"];

        //    var memberInfo = db.Members.FirstOrDefault(m => m.Id == id);

        //    //檢查格式
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // 檢查最大參與者人數
        //    if (viewGroup.totalMemberNum > 12)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團人數上限為12人" });
        //    }


        //    Group newGroup = new Group
        //    {
        //        MemberId = memberInfo.Id,//用JWT生成會員資訊
        //        GroupName = viewGroup.groupName,
        //        StartTime = viewGroup.startTime,
        //        EndTime = viewGroup.endTime,
        //        MaxParticipants = viewGroup.totalMemberNum,
        //        //CurrentParticipants = viewModel.CurrentParticipants, // Default is 1 as per ViewModel
        //        Description = viewGroup.description,
        //        IsHomeGroup = viewGroup.isHomeGroup,
        //        Address = viewGroup.place,
        //        InitMember = viewGroup.initNum,
        //        CurrentParticipants = 1 + viewGroup.initNum,
        //        Beginner = viewGroup.beginnerTag,
        //        Expert = viewGroup.expertTag,
        //        Practice = viewGroup.practiceTag,
        //        Open = viewGroup.openTag,
        //        Tutorial = viewGroup.tutorialTag,
        //        Casual = viewGroup.casualTag,
        //        Competitive = viewGroup.competitiveTag,
        //        GroupState = EnumList.GroupState.開團中,
        //        isPrivate = viewGroup.isPrivate

        //    };

        //    db.Groups.Add(newGroup);
        //    db.SaveChanges();

        //    return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "已成功開團!",data=new { groupId = newGroup.GroupId, groupState = newGroup.GroupState.ToString(),isPrivate=newGroup.isPrivate } });
        //}
        [HttpPost]
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

            if (model.GameIds.Count > 5)
            {
                return BadRequest("您只能選擇最多五款遊戲。");
            }

            var store = db.Stores.FirstOrDefault(s => s.Id == model.storeId);
            if (store == null)
            {
                // 如果 storeId 不存在，返回一個錯誤訊息
                return BadRequest($"店家 ID {model.storeId} 不存在。");
            }

            var group = new Group
            {
                MemberId = id,
                StoreId = model.storeId,
                GroupName = model.groupName,
                StartTime = model.startTime,
                EndTime = model.endTime,
                MaxParticipants = model.totalMemberNum,
                Description = model.description,
                IsHomeGroup = model.isHomeGroup,
                Address = model.place,
                InitMember = model.initNum,
                CurrentParticipants = 1 + model.initNum,
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
            foreach (var gameId in model.GameIds)
            {
                // 在這裡檢查每個 gameId 是否存在於 StoreInventories 表中
                var storeInventory = db.StoreInventories.FirstOrDefault(si => si.Id == gameId);
                if (storeInventory == null)
                {
                    // 如果 gameId 不存在，返回一個錯誤訊息
                    return BadRequest($"遊戲 ID {gameId} 不存在於店家庫存中。");
                }

                var groupGame = new GroupGame
                {
                    GroupId = group.GroupId,
                    StoreInventoryId = gameId,
                    InitDate = DateTime.Now
                };
                db.GroupGames.Add(groupGame);
            }

            db.SaveChanges(); // 儲存團體遊戲

            return Ok(new { groupId = group.GroupId, message = "團體和遊戲成功添加。" });
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
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false,  message = "groupId 不能為 null" });
            }

            // 使用 FirstOrDefault 檢查資料庫中是否存在該 groupId
            var groupInDb = db.Groups.FirstOrDefault(m => m.GroupId == groupId.Value);
            if (groupInDb == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false,  message = "該團尚未開放，無法送出留言" });
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
        //[JwtAuthFilter]
        [Route("comments/{groupId}")]
        public IHttpActionResult GetComment(int? groupId)
        {

            if (groupId == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "沒有groupId" });
            }

            var data = db.GroupComments.Where(m => m.GroupId == groupId).Select(m => new { userId=m.MemberId, m.CommentContent, m.CommentDate }).ToList();
            if (data == null || !data.Any())
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "尚未有留言" });
            }

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true,  message = "讀取留言成功",data });
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
            if (group.GroupState != EnumList.GroupState.開團中)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "該團已送出預約，下次請早!" });
            }
            if (group.CurrentParticipants + viewJoinGroup.initNum > group.MaxParticipants)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "已經滿團囉!" });
            }
            if (db.GroupParticipants.Any(m => m.GroupId == viewJoinGroup.groupId && m.MemberId == memberId))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "不可以重複申請入團哦!" });
            }

            db.GroupParticipants.Add(new GroupParticipant
            {
                GroupId = (int)viewJoinGroup.groupId,
                MemberId = memberId,
                InitMember = viewJoinGroup.initNum  // 儲存申請者帶的朋友數量
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
            
            var leader = db.Groups.Where(m => m.GroupId == groupId).Select(m => new { memberId = m.MemberId, userName = m.Member.Nickname,status= EnumList.JoinGroupState.leader.ToString(), initNum=m.InitMember }).ToList();
            var member = db.GroupParticipants
              .Where(gp => gp.GroupId == groupId)
              .Join(db.Members,
                    gp => gp.MemberId,
                    mem => mem.Id,
                    (gp, mem) => new { memberId = gp.MemberId, userName = mem.Nickname, status = gp.AttendanceStatus.ToString(), initNum = gp.InitMember })
              .ToList();
            // 合併leader和member的資料
            var data = leader.Concat(member).ToList();



            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "成功回傳揪團成員",  data  });
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
                int totalParticipants = 1 + joinRequest.InitMember;  // 申請者本身加上他的朋友
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
        /// <param name="storeName">測試可以用"六角學院桌遊店"</param>
        /// <param name="date">測試可以使用2023-11-01</param>
        /// <returns></returns>
        #region"查看店家可預約時段"
        [HttpGet]
        [Route("checkability/{storeName}/{date}")]
        public IHttpActionResult GetStoreOperatingHoursWithAvailability(string storeName, DateTime date)
        {
            // 從資料庫中查找商店實體
            var store = db.Stores.FirstOrDefault(s => s.Name == storeName);
            if (store == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到指定的店家"});
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
                    seat = remainingSeats
                });
            }

            // 返回店家指定日期的每個小時營業時段及其剩餘座位
            
            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "成功回傳",data= operatingHoursList });
        }
        #endregion
        #region"預約遊戲"
        //[HttpPost]
        //[JwtAuthFilter]
        //[Route("bookgame/{groupId}")]
        //public IHttpActionResult BookGame(int? groupId)
        //{
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
        //    int currentUserId = (int)userToken["Id"];
        //    var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);
        //    if (group.MemberId != currentUserId)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團長可以預約遊戲" });
        //    }
        //}
        #endregion
        /// <summary>
        /// 取得開團詳細資訊
        /// </summary>
        /// <param name="groupId">測試可用27</param>
        /// <returns></returns>
        #region"取得開團資訊"
        [HttpGet]
        [Route("detail/{groupId}")]
        public IHttpActionResult GetGroupDetails(int groupId)
        {
            var groupWithGames = db.Groups
                                   .Include("GroupGames.StoreInventory.Game")  // 假設你使用的是 EF6
                                   .Where(g => g.GroupId == groupId)
                                   .Select(g => new
                                   {
                                       storeId =g.StoreId,
                                       groupName = g.GroupName,
                                       startTime = g.StartTime,
                                       endTime = g.EndTime,
                                       maxParticipants = g.MaxParticipants,
                                       description = g.Description,
                                       isHomeGroup = g.IsHomeGroup,
                                       address = g.Address,
                                       initMember = g.InitMember,
                                       beginner = g.Beginner,
                                       expert = g.Expert,
                                       practice = g.Practice,
                                       open = g.Open,
                                       tutorial = g.Tutorial,
                                       casual = g.Casual,
                                       competitive = g.Competitive,
                                       isPrivate = g.isPrivate,
                                       games = g.GroupGames.Select(gg => gg.StoreInventory.GameDetails.Name).ToList(),
                                       createDate = g.CreationDate
                                   })
                                   .FirstOrDefault();

            if (groupWithGames == null)
            {
                return NotFound();
            }

            return Ok(groupWithGames); // 直接返回匿名類型的物件，將由全局的 JSON 序列化設置控制命名
        }

        #endregion
        /// <summary>
        /// 更新開團資訊
        /// </summary>
        /// <param name="groupId">團體ID</param>
        /// <param name="model">更新的團體資訊</param>
        /// <returns></returns>
        #region "更新開團資訊"
        [HttpPost]
        [Route("update/{groupId}")]
        public IHttpActionResult UpdateGroupDetails(int groupId, [FromBody] ViewGroup model)
        {
            var group = db.Groups.Include("GroupGames").FirstOrDefault(g => g.GroupId == groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 更新團體的基本資訊
            group.StoreId = model.storeId;
            group.GroupName = model.groupName;
            group.StartTime = model.startTime;
            group.EndTime = model.endTime;
            group.MaxParticipants = model.totalMemberNum;
            group.Description = model.description;
            group.IsHomeGroup = model.isHomeGroup;
            group.Address = model.place;
            group.InitMember = model.initNum;
            group.Beginner = model.beginnerTag;
            group.Expert = model.expertTag;
            group.Practice = model.practiceTag;
            group.Open = model.openTag;
            group.Tutorial = model.tutorialTag;
            group.Casual = model.casualTag;
            group.Competitive = model.competitiveTag;
            group.isPrivate = model.isPrivate;

            // 更新遊戲列表
            // 假設 model.GameIds 包含了所有更新後的遊戲ID
            var existingGameIds = group.GroupGames.Select(gg => gg.StoreInventoryId).ToList();
            var newGameIds = model.GameIds.Except(existingGameIds).ToList();
            var removedGameIds = existingGameIds.Except(model.GameIds).ToList();

            // 移除不再選擇的遊戲
            foreach (var gameId in removedGameIds)
            {
                var gameToRemove = group.GroupGames.FirstOrDefault(gg => gg.StoreInventoryId == gameId);
                if (gameToRemove != null)
                {
                    db.GroupGames.Remove(gameToRemove);
                }
            }

            // 加入新選擇的遊戲
            foreach (var gameId in newGameIds)
            {
                var gameToAdd = new GroupGame { GroupId = groupId, StoreInventoryId = gameId };
                db.GroupGames.Add(gameToAdd);
            }

            db.SaveChanges();

            return Ok(new { message = "開團資訊已更新。" });
        }
        #endregion


    }
}
