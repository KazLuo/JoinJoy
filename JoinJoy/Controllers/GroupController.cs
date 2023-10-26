using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JoinJoy.Controllers
{
    [RoutePrefix("group")]
    public class GroupController : ApiController
    {
        private Context db = new Context();
        #region"CreatGroup"
        
        [HttpPost]
        [Route("creat")]
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

            // Assuming you have a method to get the current user's ID from JWT.
            //int currentUserId = GetCurrentUserIdFromJwt();

            Group newGroup = new Group
            {
                MemberId = memberInfo.Id,//用JWT生成會員資訊
                GroupName = viewModel.GroupName,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime,
                MaxParticipants = viewModel.MaxParticipants,
                CurrentParticipants = 1,
                //CurrentParticipants = viewModel.CurrentParticipants, // Default is 1 as per ViewModel
                Description = viewModel.Description,
                IsHomeGroup = viewModel.IsHomeGroup,
                Address = viewModel.Address,
                InitMember = viewModel.InitMember,
                Beginner = viewModel.Beginner,
                Expert = viewModel.Expert,
                Practice = viewModel.Practice,
                Open = viewModel.Open,
                Tutorial = viewModel.Tutorial,
                Casual = viewModel.Casual,
                Competitive = viewModel.Competitive,
                CreationDate = viewModel.CreationDate
            };

            db.Groups.Add(newGroup);
            db.SaveChanges();

            return Ok(new { groupId = newGroup.GroupId, message = "Group created successfully." });
        }
        #endregion
    }
}
