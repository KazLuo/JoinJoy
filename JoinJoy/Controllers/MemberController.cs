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
                gamePref = member.GamePreferences.Select(m => m.GameType.TypeName),  
                cityPref = member.CityPreferences.Select(m => m.City.CityName)      
            });
        }
        #endregion
        /// <summary>
        /// 修改會員詳細資訊
        /// </summary>
        /// <param name="viewUdtMember">獲取會員詳細資訊</param>
        /// <returns></returns>
        #region"EditMemberDetails"
        [HttpPost]
        [JwtAuthFilter]
        [Route("memberDetails")]
        public IHttpActionResult EditMemberDetails(ViewUdtMemberDetail viewUdtMember)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "名稱或是介紹字數過長" });
            }
            // 驗證gamePrefId和cityPreId的有效性
            var existingGameIds = db.GameTypes.Where(g => viewUdtMember.gamePrefId.Contains(g.Id)).Select(g => g.Id).ToList();
            var existingCityIds = db.Cities.Where(c => viewUdtMember.cityPreId.Contains(c.Id)).Select(c => c.Id).ToList();
            if (existingGameIds.Count != viewUdtMember.gamePrefId.Count || existingCityIds.Count != viewUdtMember.cityPreId.Count)
            {
                return Content(HttpStatusCode.BadRequest, new { status = false, message = "提供了無效的遊戲或城市ID" });
            }
            //驗證喜好項目不可超過3項
            if (viewUdtMember.cityPreId.Count > 3 || viewUdtMember.gamePrefId.Count > 3)
            {
                return Content(HttpStatusCode.BadRequest, new { status = false, message = "城市與遊戲喜好最多不能超過3項" });
            }
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];
            //var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            //記得Include使用的是"導覽屬性，並非資料庫表單"
            var member = db.Members.Include("CityPreferences").Include("GamePreferences").FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }
            // 新增會員資料
            member.Nickname = viewUdtMember.nickName;
            member.Introduce = viewUdtMember.introduct;

            // 移除會員城市&遊戲喜好
            db.MemberGamePrefs.RemoveRange(member.GamePreferences);
            db.MemberCityPrefs.RemoveRange(member.CityPreferences);

            // 新增喜好
            member.GamePreferences = viewUdtMember.gamePrefId.Select(gameId => new MemberGamePref { MemberId = memberId, GameTypeId = gameId }).ToList();
            member.CityPreferences = viewUdtMember.cityPreId.Select(cityId => new MemberCityPref { MemberId = memberId, CityId = cityId }).ToList();

            // 儲存
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "會員詳細資訊已更新" });
        }
        #endregion

    }
}
