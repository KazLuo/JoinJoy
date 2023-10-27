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
        /// <param name="viewModel">主要用於填寫開團資料(不含遊戲預約店家預約現況)</param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        [JwtAuthFilter]
        public IHttpActionResult CreateGroup(ViewGroup viewModel)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);

            int id = (int)userToken["Id"];

            var memberInfo = db.Members.FirstOrDefault(m => m.Id == id);


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            Group newGroup = new Group
            {
                MemberId = memberInfo.Id,//用JWT生成會員資訊
                GroupName = viewModel.groupName,
                StartTime = viewModel.startTime,
                EndTime = viewModel.endTime,
                MaxParticipants = viewModel.totalMemberQtu,
                CurrentParticipants = 1,
                //CurrentParticipants = viewModel.CurrentParticipants, // Default is 1 as per ViewModel
                Description = viewModel.description,
                IsHomeGroup = viewModel.isHomeGroup,
                Address = viewModel.address,
                InitMember = viewModel.initMember,
                Beginner = viewModel.beginnerTag,
                Expert = viewModel.expertTag,
                Practice = viewModel.practiceTag,
                Open = viewModel.openTag,
                Tutorial = viewModel.tutorialTag,
                Casual = viewModel.casualTag,
                Competitive = viewModel.competitiveTag,
                GroupState = EnumList.GroupState.開團中
                
            };

            db.Groups.Add(newGroup);
            db.SaveChanges();

            return Ok(new { groupId = newGroup.GroupId, groupState=newGroup.GroupState.ToString() , message = "已成功開團!" });
        }
        #endregion
        /// <summary>
        /// 揪團留言板(送出訊息)
        /// </summary>
        /// <param name="viewGroupComment">送出留言功能</param>
        /// <returns></returns>
        #region "GroupComment"
        [HttpPost]
        [Route("comment")]
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
        /// <param name="id">收入該團所有</param>
        /// <returns></returns>
        #region "GroupComment"
        [HttpGet]
        [JwtAuthFilter]
        [Route("getcomment/{id}")]
        public IHttpActionResult GetComment(int? id)
        {
          
            if (id == null)
            {
                return Content(HttpStatusCode.BadRequest, new { message = "沒有groupId" });
            }
            return Content(HttpStatusCode.OK, new {groupId=id, message = "讀取留言成功" });
        }
        #endregion



    }
}
