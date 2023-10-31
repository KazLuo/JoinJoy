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

namespace JoinJoy.Controllers
{
    [OpenApiTag("Group", Description = "核心開團功能")]
    [RoutePrefix("group")]
    public class GroupController : ApiController
    {
        private Context db = new Context();
        #region"CreateGroup"
        /// <summary>
        /// 開團表單
        /// </summary>
        /// <param name="viewGroup">主要用於填寫開團資料(不含遊戲預約店家預約現況)</param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        [JwtAuthFilter]
        public IHttpActionResult CreateGroup(ViewGroup viewGroup)
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
            if (viewGroup.totalMemberQtu > 12)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團人數上限為12人" });
            }


            Group newGroup = new Group
            {
                MemberId = memberInfo.Id,//用JWT生成會員資訊
                GroupName = viewGroup.groupName,
                StartTime = viewGroup.startTime,
                EndTime = viewGroup.endTime,
                MaxParticipants = viewGroup.totalMemberQtu,
                //CurrentParticipants = viewModel.CurrentParticipants, // Default is 1 as per ViewModel
                Description = viewGroup.description,
                IsHomeGroup = viewGroup.isHomeGroup,
                Address = viewGroup.address,
                InitMember = viewGroup.initMember,
                CurrentParticipants = 1+viewGroup.initMember,
                Beginner = viewGroup.beginnerTag,
                Expert = viewGroup.expertTag,
                Practice = viewGroup.practiceTag,
                Open = viewGroup.openTag,
                Tutorial = viewGroup.tutorialTag,
                Casual = viewGroup.casualTag,
                Competitive = viewGroup.competitiveTag,
                GroupState = EnumList.GroupState.開團中
                
            };

            db.Groups.Add(newGroup);
            db.SaveChanges();

            return Ok(new { statusCode = HttpStatusCode.OK, status = true, groupId = newGroup.GroupId, groupState=newGroup.GroupState.ToString() , message = "已成功開團!" });
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
                return Content(HttpStatusCode.BadRequest, new { message = "groupId 不能為 null" });
            }

            // 使用 FirstOrDefault 檢查資料庫中是否存在該 groupId
            var groupInDb = db.Groups.FirstOrDefault(m => m.GroupId == groupId.Value);
            if (groupInDb == null)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "該團尚未開放，無法送出留言" });
            }


            GroupComment newgroupComment = new GroupComment
            {
                GroupId = viewGroupComment.groupId,
                MemberId = memberInfo.Id,
                CommentContent = viewGroupComment.commentTxt,
            };
            db.GroupComments.Add(newgroupComment);
            db.SaveChanges();
            return Ok(new { memberId = newgroupComment.MemberId, groupId = newgroupComment.GroupId, message = "已成功留言" });
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
                return Content(HttpStatusCode.BadRequest, new { message = "沒有groupId" });
            }

            var data = db.GroupComments.Where(m => m.GroupId == groupId).Select(m=>new {m.MemberId,m.CommentContent,m.CommentDate }).ToList();
            if(data == null || !data.Any())
            {
                return Content(HttpStatusCode.BadRequest, new { message = "尚未有留言" });
            }
            
            return Content(HttpStatusCode.OK, new {groupId= groupId, message = "讀取留言成功", data });
        }
        #endregion
        /// <summary>
        /// 申請入團
        /// </summary>
        /// <param name="groupId">帶入groupId</param>
        /// <returns></returns>
        #region "JoinGroup"
        [HttpPost]
        [JwtAuthFilter]
        [Route("join")]
        public IHttpActionResult JoinGroup(int? groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];
            var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            
            var group = db.Groups.FirstOrDefault(m => m.GroupId == groupId);
            //當用戶不存在時
            if(member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }
            //開團尚未開放時
            if (group == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "該團尚未開放" });
            }
            //確保隊長不能申請入隊
            if (group.MemberId == memberId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "隊長不能申請入隊" });
            }

            //判斷是否為開團狀態
            if (group.GroupState == EnumList.GroupState.開團中)
            {
                if(group.CurrentParticipants >= group.MaxParticipants)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "已經滿團囉!" });
                }
                var isInGroup = db.GroupParticipants.Any(m => m.GroupId == groupId && m.MemberId == memberId);
                if (isInGroup)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "不可以重複申請入團哦!" });
                }
                var memberJoin = new GroupParticipant
                {
                    GroupId = (int)groupId,
                    MemberId = memberId,

                };
                
                group.CurrentParticipants += 1;
                db.GroupParticipants.Add(memberJoin);
                db.SaveChanges();
                return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "已經加入成功囉!",joinStatus = memberJoin.AttendanceStatus.ToString() });

            }
            else
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "該團已送出預約，下次請早!" });
            }

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
            var leader = db.Groups.Where(m => m.GroupId == groupId).Select(m => new {id = m.MemberId,name = m.Member.Nickname });
            var member = db.GroupParticipants
              .Where(gp => gp.GroupId == groupId)
              .Join(db.Members,
                    gp => gp.MemberId,
                    mem => mem.Id,
                    (gp, mem) => new { Id = gp.MemberId, NickName = mem.Nickname })
              .ToList();
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "成功回傳揪團成員",leader, member });
        }
        #endregion
        /// <summary>
        /// 審核團員
        /// </summary>
        /// <param name="groupId">團隊id</param>
        /// <param name="memberId">受審者id</param>
        /// <param name="status">0="審查中",1="已加入",2="已拒絕"</param>
        /// <returns></returns>
        #region"審核團員"
        [HttpPost]
        [JwtAuthFilter]
        [Route("reviewGroup")]
        public IHttpActionResult ReviewGroup(int groupId, int memberId, EnumList.JoinGroupState status)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int currentUserId = (int)userToken["Id"];

            var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);

            //檢查團隊是否存在
            if (group == null)
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在" });

            //檢查當前用戶是否是團長
            if (group.MemberId != currentUserId)
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團長可以審核團員" });

            var joinRequest = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == groupId && gp.MemberId == memberId);

            //檢查入團申請是否存在
            if (joinRequest == null)
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "入團申請不存在" });

            //根據status更新入團申請的狀態
            if (status == EnumList.JoinGroupState.已加入)
            {
                joinRequest.AttendanceStatus = EnumList.JoinGroupState.已加入;
            }
            else if (status == EnumList.JoinGroupState.已拒絕)
            {
                db.GroupParticipants.Remove(joinRequest);
            }
            //如果status為審核中，則不需要進行任何操作

            db.SaveChanges();
            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = $"入團申請的狀態已更新為：{status.ToString()}。" });
        }
        #endregion
    }
}
