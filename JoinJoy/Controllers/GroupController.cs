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
        #region"CreateGroup"
        
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
                
            };

            db.Groups.Add(newGroup);
            db.SaveChanges();

            return Ok(new { groupId = newGroup.GroupId, message = "Group created successfully." });
        }
        #endregion

    }
}
