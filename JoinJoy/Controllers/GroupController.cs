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

        /// <summary>
        /// 開團表單
        /// </summary>
        /// <param name="viewGroup">主要用於填寫開團資料(不含遊戲預約店家預約現況)</param>
        /// <returns></returns>
        #region"CreateGroup"
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
            if (viewGroup.totalMemberNum > 12)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團人數上限為12人" });
            }


            Group newGroup = new Group
            {
                MemberId = memberInfo.Id,//用JWT生成會員資訊
                GroupName = viewGroup.groupName,
                StartTime = viewGroup.startTime,
                EndTime = viewGroup.endTime,
                MaxParticipants = viewGroup.totalMemberNum,
                //CurrentParticipants = viewModel.CurrentParticipants, // Default is 1 as per ViewModel
                Description = viewGroup.description,
                IsHomeGroup = viewGroup.isHomeGroup,
                Address = viewGroup.place,
                InitMember = viewGroup.initNum,
                CurrentParticipants = 1 + viewGroup.initNum,
                Beginner = viewGroup.beginnerTag,
                Expert = viewGroup.expertTag,
                Practice = viewGroup.practiceTag,
                Open = viewGroup.openTag,
                Tutorial = viewGroup.tutorialTag,
                Casual = viewGroup.casualTag,
                Competitive = viewGroup.competitiveTag,
                GroupState = EnumList.GroupState.開團中,
                isPrivate = viewGroup.isPrivate

            };

            db.Groups.Add(newGroup);
            db.SaveChanges();

            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "已成功開團!",data=new { groupId = newGroup.GroupId, groupState = newGroup.GroupState.ToString(),isPrivate=newGroup.isPrivate } });
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
        //[HttpPost]
        //[JwtAuthFilter]
        //[Route("reviewGroup")]
        //public IHttpActionResult ReviewGroup(int groupId, int memberId, EnumList.JoinGroupState status)
        //{
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
        //    int currentUserId = (int)userToken["Id"];
        //    var group = db.Groups.FirstOrDefault(g => g.GroupId == groupId);

        //    if (group == null)
        //    {
        //        return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "團隊不存在" });
        //    }

        //    if (group.MemberId != currentUserId)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有團長可以審核團員" });
        //    }

        //    var joinRequest = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == groupId && gp.MemberId == memberId);

        //    if (joinRequest == null)
        //    {
        //        return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "入團申請不存在" });
        //    }
        //    //如果審核為團員後，才可以加入group中
        //    if (status == EnumList.JoinGroupState.團員)
        //    {
        //        joinRequest.AttendanceStatus = EnumList.JoinGroupState.團員;
        //        int totalParticipants = 1 + joinRequest.InitMember;  // 申請者本身加上他的朋友
        //        group.CurrentParticipants += totalParticipants;
        //    }
        //    else if (status == EnumList.JoinGroupState.已拒絕)
        //    {
        //        db.GroupParticipants.Remove(joinRequest);
        //    }

        //    db.SaveChanges();
        //    return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = $"入團申請的狀態已更新為：{status.ToString()}。" });
        //}

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
    }
}
