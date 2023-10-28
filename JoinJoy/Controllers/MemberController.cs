using JoinJoy.Security;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Member", Description = "會員相關功能")]
    [RoutePrefix("member")]
    public class MemberController : ApiController
    {
        private Context db = new Context();
        /// <summary>
        /// 獲取會員詳細資訊
        /// </summary>
        /// <returns></returns>
        #region"GetMemberDetails"
        [HttpGet]
        [JwtAuthFilter]
        [Route("memberDetails")]
        public IHttpActionResult GetMemberDetails()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];

            var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }

            return Ok(new
            {
                memberId = member.Id,
                nickname = member.Nickname,
                account = member.Account,
                introduce = member.Introduce,
                gamePreferences = member.GamePreferences.Select(m => m.GameType.TypeName),  // Assuming GameType is a property of MemberGamePref
                cityPreferences = member.CityPreferences.Select(m => m.City.CityName)      // Assuming City is a property of MemberCityPref
            });
        }
        #endregion


    }
}
