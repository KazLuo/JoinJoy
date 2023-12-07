﻿using JoinJoy.Security;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Data.Entity.Validation;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Member", Description = "會員相關功能")]
    [RoutePrefix("member")]
    public class MemberController : ApiController
    {
        private Context db = new Context();

        /// <summary>
        /// 獲取會員詳細資訊(自己的)
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
            return Content(HttpStatusCode.OK, new
            {
                statusCode = HttpStatusCode.OK,
                status = true,
                message = "回傳成功",
                data = new
                {
                    userId = member.Id,
                    nickName = member.Nickname,
                    email = member.Account,
                    description = member.Introduce,
                    games = member.GamePreferences.Select(m => m.GameType.Id),
                    cities = member.CityPreferences.Select(m => m.City.Id)
                }
            });

        }
        #endregion
        /// <summary>
        /// 獲取會員詳細資訊(取得其他會員資訊)
        /// </summary>
        /// <returns></returns>
        #region"GetMemberDetails"
        [HttpGet]
        [Route("memberDetails/{userId}")]
        public IHttpActionResult GetOtherMemberDetails(int? userId)
        {

            var member = db.Members.FirstOrDefault(m => m.Id == userId);
            if (member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }

            return Content(HttpStatusCode.OK, new
            {
                statusCode = HttpStatusCode.OK,
                status = true,
                message = "回傳成功",
                data = new
                {//少一個photo
                    nickName = member.Nickname,
                    description = member.Introduce,
                    profileImg = BuildProfileImageUrl(member.Photo),
                    games = member.GamePreferences.Select(m => m.GameType.Id),
                    cities = member.CityPreferences.Select(m => m.City.Id)
                }
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
            var existingGameIds = db.GameTypes.Where(g => viewUdtMember.games.Contains(g.Id)).Select(g => g.Id).ToList();
            var existingCityIds = db.Cities.Where(c => viewUdtMember.cities.Contains(c.Id)).Select(c => c.Id).ToList();
            if (existingGameIds.Count != viewUdtMember.games.Count || existingCityIds.Count != viewUdtMember.cities.Count)
            {
                return Content(HttpStatusCode.BadRequest, new { status = false, message = "提供了無效的遊戲或城市ID" });
            }
            //驗證喜好項目不可超過3項
            if (viewUdtMember.cities.Count > 3 || viewUdtMember.games.Count > 3)
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
            member.Introduce = viewUdtMember.description;

            // 移除會員城市&遊戲喜好
            db.MemberGamePrefs.RemoveRange(member.GamePreferences);
            db.MemberCityPrefs.RemoveRange(member.CityPreferences);

            // 新增喜好
            member.GamePreferences = viewUdtMember.games.Select(gameId => new MemberGamePref { MemberId = memberId, GameTypeId = gameId }).ToList();
            member.CityPreferences = viewUdtMember.cities.Select(cityId => new MemberCityPref { MemberId = memberId, CityId = cityId }).ToList();

            // 儲存
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "會員詳細資訊已更新", data = new { nickName = member.Nickname, description = member.Introduce, games = member.GamePreferences.Select(m => m.GameTypeId), cities = member.CityPreferences.Select(m => m.CityId) } });
        }
        #endregion
        /// <summary>
        /// 城市喜好
        /// </summary>
        /// <param name="cityPrefIds">輸入對應城市參數 1=基隆 2=新北 3=台北...</param>
        /// <returns></returns>
        #region"城市喜好"
        [HttpPost]
        [JwtAuthFilter]
        [Route("citypref")]
        public IHttpActionResult UpdateCityPreferences([FromBody] List<int> cityPrefIds)
        {
            if (!ModelState.IsValid || cityPrefIds == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "資料格式錯誤。" });
            }

            if (cityPrefIds.Count > 3)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "城市喜好最多不能超過3項。" });
            }

            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];

            var member = db.Members.Include("CityPreferences").FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "會員不存在。" });
            }

            // 移除現有的城市喜好
            db.MemberCityPrefs.RemoveRange(member.CityPreferences);

            // 新增城市喜好
            member.CityPreferences = cityPrefIds.Select(cityId => new MemberCityPref { MemberId = memberId, CityId = cityId }).ToList();

            // 儲存變更
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "城市喜好更新成功。" });
        }
        #endregion
        /// <summary>
        /// 遊戲喜好
        /// </summary>
        /// <param name="gamePrefIds">輸入對照表對應遊戲喜好</param>
        /// <returns></returns>
        #region"遊戲喜好"
        [HttpPost]
        [JwtAuthFilter]
        [Route("gamepref")]
        public IHttpActionResult UpdateGamePreferences([FromBody] List<int> gamePrefIds)
        {
            if (!ModelState.IsValid || gamePrefIds == null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "資料格式錯誤。" });
            }

            if (gamePrefIds.Count > 3)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "遊戲喜好最多不能超過3項。" });
            }

            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];

            var member = db.Members.Include("GamePreferences").FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "會員不存在。" });
            }

            // 移除現有的遊戲喜好
            db.MemberGamePrefs.RemoveRange(member.GamePreferences);

            // 新增遊戲喜好
            member.GamePreferences = gamePrefIds.Select(gameId => new MemberGamePref { MemberId = memberId, GameTypeId = gameId }).ToList();

            // 儲存變更
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "遊戲喜好更新成功。" });
        }
        #endregion
        /// <summary>
        /// 上傳會員頭像
        /// </summary>
        /// <returns></returns>
        #region"上傳頭像"
        [HttpPost]
        [JwtAuthFilter]
        [Route("uploadimg")]
        public async Task<IHttpActionResult> UploadProfile()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];

            var user = db.Members.FirstOrDefault(m => m.Id == memberId);
            if (user == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }

            // 檢查請求是否包含 multipart/form-data。
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // 檢查資料夾是否存在，若不存在則創建
            string root = HttpContext.Current.Server.MapPath("~/upload/profile");
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            try
            {
                // 讀取MIME資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 獲取檔案擴展名，單檔案使用.FirstOrDefault()直接提取，多檔案需使用循環
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // 如 .jpg

                // 定義檔案名稱
                string fileName = "Member_" + memberId + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 儲存圖片，單檔案使用.FirstOrDefault()直接提取，多檔案需使用循環
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                // 讀取圖片並調整尺寸
                using (var image = SixLabors.ImageSharp.Image.Load(outputPath))
                {
                    // 調整圖片尺寸至標準尺寸，例如：128x128像素
                    image.Mutate(x => x.Resize(145, 145));

                    // 檢查圖片大小，如果大於限制則壓縮圖片 (例如：不超過2MB)
                    if (fileBytes.Length > 2 * 1024 * 1024)
                    {
                        // 壓縮圖片以降低大小
                        // 可以使用ImageSharp的壓縮功能或其他工具來完成
                    }

                    // 儲存調整後的圖片
                    image.Save(outputPath);
                }

                // 更新會員資料表中的圖片路徑
                var member = db.Members.FirstOrDefault(m => m.Id == memberId);
                if (member != null)
                {
                    member.Photo = fileName; // 儲存檔案名稱
                    db.SaveChanges(); // 儲存變更到資料庫
                }

                return Ok(new
                {
                    statusCode = HttpStatusCode.OK,
                    status = true,
                    message = "檔案上傳成功。",
                    data = new
                    {
                        FileName = fileName
                    },

                });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = true, message = "上傳失敗，請再試一次。" });
            }
        }
        #endregion
        /// <summary>
        /// 取得會員頭像
        /// </summary>
        /// <param name="userId">輸入會員即可(測試可以用6號)</param>
        /// <returns></returns>
        #region "獲取會員頭像"
        [HttpGet]
        [Route("profileimg/{userId}")]
        public IHttpActionResult GetProfileImage(int userId)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == userId);
            if (member == null || string.IsNullOrEmpty(member.Photo))
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在或未設置頭像。" });
            }

            string root = HttpContext.Current.Server.MapPath("~/upload/profile");
            var filePath = Path.Combine(root, member.Photo);
            if (!File.Exists(filePath))
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "文件不存在。" });
            }

            // 讀取圖片文件為位元組數組
            var fileBytes = File.ReadAllBytes(filePath);
            // 獲取MIME類型
            var contentType = MimeMapping.GetMimeMapping(filePath);
            // 創建一個HttpResponseMessage，設定Content為圖片的位元組數組
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileBytes)
            };
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            return ResponseMessage(result);
        }
        #endregion
        /// <summary>
        /// 取得所有喜好城市列表
        /// </summary>
        /// <returns></returns>
        #region"所有喜好城市列表"
        [HttpGet]
        [Route("city")]
        public IHttpActionResult GetCity()
        {
            try
            {
                var cityData = db.Cities
                    .Select(c => new { c.Id, c.CityName })
                    .ToList();

                if (cityData == null || !cityData.Any()) // 檢查是否有城市資料
                {
                    // 如果沒有資料，返回客戶化的 HTTP 404 Not Found
                    return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "未找到城市資料。", data = new { } });
                }

                // 如果有資料，返回正常的結果
                return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功!", data = cityData });
            }
            catch (Exception ex)
            {
                // 如果有錯誤發生，記錄錯誤並返回客戶化的 HTTP 500 Internal Server Error
                // Log the error (depending on your logging system)
                // e.g. Logger.Error(ex);

                // 返回錯誤資訊
                return Content(HttpStatusCode.InternalServerError, new { statusCode = HttpStatusCode.InternalServerError, status = false, message = "伺服器錯誤。", data = new { }, exception = ex.Message });
            }
        }

        #endregion

        /// <summary>
        /// 取得所有喜好遊戲類型列表
        /// </summary>
        /// <returns></returns>
        #region"所有喜好遊戲類別列表"
        [HttpGet]
        [Route("gameType")]
        public IHttpActionResult GetGameType()
        {
            try
            {
                var gameType = db.GameTypes
                    .Select(c => new { c.Id, c.TypeName })
                    .ToList();

                if (gameType == null || !gameType.Any()) // 檢查是否有城市資料
                {
                    // 如果沒有資料，返回客戶化的 HTTP 404 Not Found
                    return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "未找到遊戲類型資料。", data = new { } });
                }

                // 如果有資料，返回正常的結果
                return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功!", data = gameType });
            }
            catch (Exception ex)
            {
                // 如果有錯誤發生，記錄錯誤並返回客戶化的 HTTP 500 Internal Server Error
                // Log the error (depending on your logging system)
                // e.g. Logger.Error(ex);

                // 返回錯誤資訊
                return Content(HttpStatusCode.InternalServerError, new { statusCode = HttpStatusCode.InternalServerError, status = false, message = "伺服器錯誤。", data = new { }, exception = ex.Message });
            }
        }
        #endregion
        /// <summary>
        /// 取得會員所有揪團紀錄
        /// </summary>
        /// <returns></returns>
        #region"取得會員所有揪團紀錄"
        //[HttpGet]
        //[JwtAuthFilter]
        //[Route("usergrouplist")]
        //public IHttpActionResult GetUserGroupList()
        //{
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
        //    int userId = (int)userToken["Id"];

        //    var user = db.Members.FirstOrDefault(m => m.Id == userId);
        //    if (user == null)
        //    {
        //        return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
        //    }
        //    var member = db.GroupParticipants.Where(gp => gp.MemberId == userId || gp.Group.MemberId == userId)
        //        .Select(gp => new {
        //            groupId = gp.GroupId,
        //            groupName = gp.Group.GroupName,
        //            startTime = gp.Group.StartTime,
        //            endTime = gp.Group.EndTime,
        //            totalMemberNum = gp.Group.MaxParticipants,
        //            currentPeople = gp.Group.CurrentParticipants,
        //            place = string.IsNullOrEmpty(gp.Group.Address) ? (string)null : gp.Group.Address,
        //            store = gp.Group.StoreId == null && gp.Group.Store.Name == null && gp.Group.Store.Address == null
        //                     ? null
        //                     : new
        //                     {
        //                         storeId = gp.Group.StoreId,
        //                         storeName = gp.Group.Store.Name,
        //                         address = gp.Group.Store.Address
        //                     },
        //            status = gp.AttendanceStatus.ToString()
        //        }).ToList();
        //    var leader = db.Groups.Where(gp => gp.MemberId == userId)
        //        .Select(gp => new {
        //            groupId = gp.GroupId,
        //            groupName = gp.GroupName,
        //            startTime = gp.StartTime,
        //            endTime = gp.EndTime,
        //            totalMemberNum = gp.MaxParticipants,
        //            currentPeople = gp.CurrentParticipants,
        //            place = string.IsNullOrEmpty(gp.Address) ? (string)null : gp.Address,
        //            store = gp.StoreId == null && gp.Store.Name == null && gp.Store.Address == null
        //                     ? null
        //                     : new
        //                     {
        //                         storeId = gp.StoreId,
        //                         storeName = gp.Store.Name,
        //                         address = gp.Store.Address
        //                     },
        //            status = gp.GroupState.ToString()
        //        }).ToList();


        //    return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功!", data = new { member, leader } });
        //}

        [HttpGet]
        [JwtAuthFilter]
        [Route("usergrouplist")]
        public IHttpActionResult GetUserGroupList()
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int userId = (int)userToken["Id"];

            var user = db.Members.FirstOrDefault(m => m.Id == userId);
            if (user == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }

            var memberQuery = db.GroupParticipants.Where(gp => gp.MemberId == userId /*|| gp.Group.MemberId == userId*/).ToList();
            
            var member = memberQuery.Select(gp => new
            {
                groupId = gp.GroupId,
                groupName = gp.Group.GroupName,
                startTime = gp.Group.StartTime,
                endTime = gp.Group.EndTime,
                totalMemberNum = gp.Group.MaxParticipants,
                currentPeople = gp.Group.CurrentParticipants,
                place = string.IsNullOrEmpty(gp.Group.Address) ? (string)null : gp.Group.Address,
                store = (gp.Group.Store != null && gp.Group.StoreId != null)
             ? new
             {
                 storeId = gp.Group.StoreId,
                 storeName = gp.Group.Store.Name,
                 address = gp.Group.Store.Address
             }
             : null,
                //memberStatus = JoinGroupStatus(gp.AttendanceStatus, gp.Group, /*gp.Group.StartTime, gp.Group.EndTime*/ DateTime.Now).ToString(),
                memberStatus = gp.AttendanceStatus.ToString(),
                groupStatus = GetUpdatedGroupStatus(gp.Group).ToString()
            }).ToList();

            var leaderQuery = db.Groups.Where(g => g.MemberId == userId).ToList();

            var leader = leaderQuery.Select(g => new
            {
                groupId = g.GroupId,
                groupName = g.GroupName,
                startTime = g.StartTime,
                endTime = g.EndTime,
                totalMemberNum = g.MaxParticipants,
                currentPeople = g.CurrentParticipants,
                place = string.IsNullOrEmpty(g.Address) ? (string)null : g.Address,
                store = (g.Store != null && g.StoreId != null)
             ? new
             {
                 storeId = g.StoreId,
                 storeName = g.Store.Name,
                 address = g.Store.Address
             }
             : null,
                memberStatus = "leader",  // 假設所有leaderQuery的結果都是領導者
                groupStatus = GetUpdatedGroupStatus(g).ToString()
                
            }).ToList();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功!", data = new { member, leader } });
        }

        private string GetUpdatedGroupStatus(Group group)
        {
            DateTime now = DateTime.Now;
            if (group.GroupState == EnumList.GroupState.開團中 && now > group.StartTime)
            {
                return EnumList.GroupState.已失效.ToString();
            }
            else if (group.GroupState == EnumList.GroupState.已預約 && now > group.EndTime)
            {
                return EnumList.GroupState.已結束.ToString();
            }
            else
            {
                return group.GroupState.ToString();
            }
        }

        //private string JoinGroupStatus(EnumList.JoinGroupState joinGroupState, DateTime startTime, DateTime endTime, DateTime now)
        //{

        //    switch (joinGroupState)
        //    {
        //        case EnumList.JoinGroupState.pending:
        //            return now > startTime ? EnumList.GroupState.已失效.ToString() : EnumList.JoinGroupState.pending.ToString();
        //        case EnumList.JoinGroupState.member:
        //            return now > endTime ? EnumList.GroupState.已結束.ToString() : EnumList.JoinGroupState.member.ToString();
        //        default:
        //            return joinGroupState.ToString();
        //    }
        //}

        private string JoinGroupStatus(EnumList.JoinGroupState joinGroupState, Group group, DateTime now)
        {
            // 判断当前时间与团队的时间范围关系
            bool isBeforeStart = now < group.StartTime;
            bool isAfterEnd = now > group.EndTime;

            switch (joinGroupState)
            {
                case EnumList.JoinGroupState.pending:
                    // 对于 pending 状态，在开始时间之前保持为 pending，之后变为已失效
                    return isBeforeStart ? EnumList.JoinGroupState.pending.ToString() : EnumList.GroupState.已失效.ToString();

                case EnumList.JoinGroupState.member:
                    // 对于成员，根据时间和团队的 GroupState 判断
                    if (isBeforeStart)
                    {
                        return group.GroupState == EnumList.GroupState.已預約 ? EnumList.GroupState.已預約.ToString() : EnumList.JoinGroupState.member.ToString();
                    }
                    else if (isAfterEnd)
                    {
                        // 如果结束时间已过，根据 GroupState 判断是已結束还是已失效
                        return group.GroupState == EnumList.GroupState.已預約 ? EnumList.GroupState.已結束.ToString() : EnumList.GroupState.已失效.ToString();
                    }
                    else
                    {
                        return EnumList.JoinGroupState.member.ToString();
                    }

                default:
                    // 其他状态不变
                    return joinGroupState.ToString();
            }
        }


        #endregion
        /// <summary>
        /// 評價會員
        /// </summary>
        /// <param name="viewRatingMember"></param>
        /// <returns></returns>
        #region"評價會員"
        [HttpPost]
        [JwtAuthFilter]
        [Route("ratingmember")]
        public IHttpActionResult RatingMember(ViewRatingMember viewRatingMember)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int userId = (int)userToken["Id"];
            //驗證會員
            var memberToRate = db.Members.FirstOrDefault(m => m.Id == viewRatingMember.memberId);
            if (memberToRate == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "被評價的會員不存在" });
            }
            //驗證會員有參加此團(包含團主)
            var isjoin = db.GroupParticipants.Any(m => m.GroupId == viewRatingMember.groupId && m.MemberId == viewRatingMember.memberId) || db.Groups.Any(g => g.GroupId == viewRatingMember.groupId && g.MemberId == viewRatingMember.memberId);
            if (!isjoin)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "只有參加此團的人員能夠評價" });
            }

            // 檢查是否參加並出席了團隊活動
            //var participant = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == viewRatingMember.groupId && gp.MemberId == userId );
            //if (participant == null || !participant.IsPresent)
            //{
            //    return Content(HttpStatusCode.BadRequest, new { message = "只有參加並出席活動的成員才能進行評價" });
            //}

          

            // 確保用戶不是在評價自己
            if (userId == viewRatingMember.memberId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您不能評價自己" });
            }

            // 檢查用戶是否已經給這個會員評過分
            var existingRating = db.MemberRatings.FirstOrDefault(r => r.RatedId == viewRatingMember.memberId && r.MemberId == userId && r.GroupId == viewRatingMember.groupId);
            if (existingRating != null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您已經給該會員評過分" });
            }
            var group = db.Groups.FirstOrDefault(g => g.GroupId == viewRatingMember.groupId);
            // 檢查團是否為已預約且已經結束
            if (group.GroupState != EnumList.GroupState.已預約 || DateTime.Now <= group.EndTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有在已預約且已結束的團中才能進行評價" });
            }

            // 檢查是否為團主或參加並出席了團隊活動
         
            if (group.MemberId != userId) // 如果評價者不是團主
            {
                var participant = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == viewRatingMember.groupId && gp.MemberId == userId);
                if (participant == null || !participant.IsPresent)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有參加並出席活動的成員才能進行評價" });
                }
            }




            // 創建一個新的評價記錄
            var rating = new MemberRating
            {
                GroupId = viewRatingMember.groupId,
                MemberId = userId,
                RatedId = viewRatingMember.memberId,
                Score = viewRatingMember.score,
                Comment = viewRatingMember.comment,
                RatingDate = DateTime.Now
            };

            // 將評價記錄加入資料庫
            db.MemberRatings.Add(rating);

            // 儲存變更
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // 處理錯誤，例如記錄錯誤或返回一個錯誤信息
                return InternalServerError(ex);
            }


            // 返回成功信息
            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "評價成功" });
        }
        #endregion

        /// <summary>
        /// 確認團員評價狀態
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>

        #region"確認團員評價狀態"
        [HttpGet]
        [JwtAuthFilter]
        [Route("checkgroupratings/{groupId}")]
        public IHttpActionResult CheckGroupRatings(int groupId)
        {
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int userId = (int)userToken["Id"];
            //驗證開團是否處於開團狀態
            var groupstatus = db.Groups.Any(g => g.GroupId == groupId && g.GroupState == EnumList.GroupState.開團中);
            if (groupstatus)
            {
                var groupInvalid = db.Groups.Any(g => g.GroupId == groupId && g.GroupState == EnumList.GroupState.開團中 && g.StartTime <= DateTime.Now);
                if (groupInvalid)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "開團已過時效", });
                }

                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "團隊還在開團狀態，尚無團隊評價資訊" });
            }




            // 獲取團隊成員，但排除登入者本人
            var groupMembers = db.GroupParticipants
                                 .Where(gp => gp.GroupId == groupId && gp.MemberId != userId && gp.IsPresent==true)
                                 .Select(gp => gp.MemberId )
                                 .ToList();

            // 添加團主的MemberId，如果團主不在GroupParticipants中且團主不是登入者
            var groupLeaderId = db.Groups.Where(g => g.GroupId == groupId).Select(g => g.MemberId).FirstOrDefault();
            if (groupLeaderId != userId && !groupMembers.Contains(groupLeaderId))
            {
                groupMembers.Add(groupLeaderId);
            }

            //驗證會員有參加此團
            var isjoin = db.GroupParticipants.Any(m => m.GroupId == groupId && m.MemberId == userId);
            if (!isjoin)
            {
                if (groupLeaderId != userId)
                {
                    return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有參加此團的人能驗證評價狀態" });
                }

            }

            // 檢查每位成員是否被評價，包括團主
            var memberRatingsStatus = groupMembers.Select(memberId =>
                new
                {
                    memberId = memberId,
                    memberName = db.Members.Where(m => m.Id == memberId).Select(m => m.Nickname).FirstOrDefault(),
                    memberPhoto = BuildProfileImageUrl( db.Members.Where(m => m.Id == memberId).Select(m => m.Photo).FirstOrDefault()),
                    isRated = db.MemberRatings.Any(mr => mr.MemberId == userId && mr.RatedId == memberId && mr.GroupId == groupId), 
                    score = db.MemberRatings.Where(mr => mr.MemberId == userId && mr.RatedId == memberId && mr.GroupId == groupId).Select(mr => mr.Score).FirstOrDefault(), 
                    comment = db.MemberRatings.Where(mr => mr.MemberId == userId && mr.RatedId == memberId && mr.GroupId == groupId).Select(mr => mr.Comment).FirstOrDefault(),
                    status = memberId == groupLeaderId ? "leader" : "member"
                }).ToList();

            // 檢查是否所有成員都已被評價（包括團主）
            var isAllRated = memberRatingsStatus.All(mrs => mrs.isRated);

            return Content(HttpStatusCode.OK, new
            {
                statusCode = HttpStatusCode.OK,
                status = true,
                message = "取得評價狀態成功",
                data = new
                {
                    isAllRated = isAllRated,
                    ratingStatus = memberRatingsStatus

                }
            });
        }
        #endregion

        /// <summary>
        /// 取得會員評價
        /// </summary>
        /// <param name="userId">輸入會員id</param>
        /// <param name="sortBy">用於排序，0=newest,1=highest,2=lowest</param>
        /// <returns></returns>
        #region "取得會員評價"
        [HttpGet]
        [Route("getrating/{userId}/{sortBy}")]
        public IHttpActionResult GetRating(int userId, EnumList.RatingFilter sortBy)
        {

            var ratingsWithGroup = (from rating in db.MemberRatings
                                    join Group in db.Groups on rating.GroupId equals Group.GroupId
                                    join Members in db.Members on rating.MemberId equals Members.Id
                                    where rating.RatedId == userId
                                    select new
                                    {
                                        rating,
                                        Group,
                                        Members
                                    }).ToList();  // 先將數據轉換為List

            var comments = ratingsWithGroup.Select(r => new
            {
                commentId = r.rating.Id,
                msg = r.rating.Comment,
                score = r.rating.Score,
                commentDate = r.rating.RatingDate,
                commentBy = new
                {
                    userid = r.rating.MemberId,
                    userName = r.Members.Nickname,
                    profileImg = BuildProfileImageUrl(r.Members.Photo)
                },
                groups = new
                {
                    groupId = r.Group.GroupId,
                    groupName = r.Group.GroupName,
                    memberQtu = r.Group.CurrentParticipants,
                    groupDate = r.Group.StartTime,
                    storeName = r.Group.IsHomeGroup ? r.Group.Address : r.Group.Store.Name,
                    storeId = r.Group.Store.Id
                }
            }).AsQueryable(); // 將結果轉換回 IQueryable 以便進行後續排序

            //var comments = ratingsWithGroup.ToList();

            // 如果沒有評價，則返回空結果
            if (!comments.Any())
            {
                return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "沒有評價", allCommentCount = 0, comments = new List<object>() });
            }

            double? averageScore = comments.Average(a => a.score);

            // 根據 sortBy 參數對結果進行排序
            switch (sortBy)
            {
                case EnumList.RatingFilter.newest:
                    comments = comments.OrderByDescending(r => r.commentDate);
                    break;
                case EnumList.RatingFilter.highest:
                    comments = comments.OrderByDescending(r => r.score);
                    break;
                case EnumList.RatingFilter.lowest:
                    comments = comments.OrderBy(r => r.score);
                    break;
                default:
                    comments = comments.OrderByDescending(r => r.commentDate);
                    break;
            }

            var finalComments = comments.ToList();  // 將結果轉換為最終列表
            var allCommentCount = finalComments.Count;

            // 返回響應
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", allCommentCount, finalComments });

        }
        #endregion



        /// <summary>
        /// 取得所有會員ID
        /// </summary>
        /// <returns></returns>
        #region"取得所有會員ID"
        [HttpGet]
        [Route("getallmemberid")]
        public IHttpActionResult GetAllGroupId()
        {
            try
            {
                // 檢索所有店家的基本信息
                var stores = db.Members.Select(m => new { m.Id, m.Nickname, m.Photo }).ToList();

                // 在記憶體中構建照片URL
                var data = stores.Select(m => new
                {
                    userId = m.Id,
                    nickName = m.Nickname,
                    photo = string.IsNullOrEmpty(m.Photo) ? null : BuildProfileImageUrl(m.Photo)
                }).ToList();

                return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功", data });
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "回傳失敗" });
            }
        }
        #endregion

        /// <summary>
        /// 評價店家
        /// </summary>
        /// <param name="viewStoreRating"></param>
        /// <returns></returns>
        #region"評價店家"
        [HttpPost]
        [JwtAuthFilter]
        [Route("ratingstore")]
        public IHttpActionResult RatingStore(ViewStoreRating viewStoreRating)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int userId = (int)userToken["Id"];
            //驗證店家
            var storeToRate = db.Stores.FirstOrDefault(m => m.Id == viewStoreRating.storeId);
            if (storeToRate == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "被評價的店家不存在" });
            }
            //驗證會員有參加此團


            var isParticipantOrLeader = db.GroupParticipants.Any(gp => gp.GroupId == viewStoreRating.groupId && gp.MemberId == userId)
                            || db.Groups.Any(g => g.GroupId == viewStoreRating.groupId && g.MemberId == userId);

            if (!isParticipantOrLeader)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "只有參加此團的人員或團主能夠評價" });
            }

            // 檢查是否參加並出席了團隊活動
            var participant = db.GroupParticipants.FirstOrDefault(gp => gp.GroupId == viewStoreRating.groupId && gp.MemberId == userId);
            if (participant == null || !participant.IsPresent)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有參加並出席活動的成員才能進行評價" });
            }

            // 確保用戶不是在評價自己店家
            if (db.Stores.Any(m => m.MemberId == userId))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您不能評價自己店家" });
            }

            // 確定店家是否已經預約
            if (!db.Groups.Any(m => m.GroupId == viewStoreRating.groupId && m.GroupState == EnumList.GroupState.已預約))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "店家尚未預約無法進行評價" });
            }

            // 檢查用戶是否已經給這個店家評過分
            var existingRating = db.StoreRatings.FirstOrDefault(r => r.StoreId == viewStoreRating.storeId && r.MemberId == userId && r.GroupId == viewStoreRating.groupId);
            if (existingRating != null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您已經給該店家評過分" });
            }
            var group = db.Groups.FirstOrDefault(g => g.GroupId == viewStoreRating.groupId);
            // 檢查團是否為已預約且已經結束
            if (group.GroupState != EnumList.GroupState.已預約 || DateTime.Now <= group.EndTime)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "只有在已預約且已結束的團中才能進行評價" });
            }



            // 創建一個新的評價記錄
            var Storerating = new StoreRating
            {
                GroupId = viewStoreRating.groupId,
                StoreId = viewStoreRating.storeId,
                Clean = viewStoreRating.clean,
                Service = viewStoreRating.service,
                Variety = viewStoreRating.variety,
                Value = viewStoreRating.value,
                Comment = viewStoreRating.comment,
                RatingDate = DateTime.Now,
                MemberId = userId
            };

            // 將評價記錄加入資料庫
            db.StoreRatings.Add(Storerating);

            // 儲存變更
            try
            {
                db.SaveChanges();
            }
            //驗證錯誤原因
            catch (DbEntityValidationException dbEx)
            {
                var errorMessages = dbEx.EntityValidationErrors
                                        .SelectMany(validationError => validationError.ValidationErrors)
                                        .Select(e => e.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);

                var exceptionMessage = string.Concat(dbEx.Message, " 驗證失敗: ", fullErrorMessage);

                // 記錄錯誤或返回一個錯誤信息
                return Content(HttpStatusCode.InternalServerError, new { message = exceptionMessage });
            }
            catch (Exception ex)
            {
                // 處理其他類型的錯誤
                return InternalServerError(ex);
            }





            // 返回成功信息
            return Ok(new { statusCode = HttpStatusCode.OK, status = true, message = "評價成功" });
        }
        #endregion
        /// <summary>
        /// 取得團隊店家資訊
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        #region"取得團隊店家資訊"
        [HttpGet]
        [Route("storeinfobygroup/{groupId}")]
        public IHttpActionResult GetStoreInfoByGroup(int groupId)
        {
            // 檢查團隊是否存在
            var groupExists = db.Groups.Any(g => g.GroupId == groupId && g.IsHomeGroup!= true);
            if (!groupExists)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到團隊，或是團隊是自家團" });
            }

            // 獲取與團隊相關的店家資料
            var storeInfo = db.Groups
                .Where(g => g.GroupId == groupId)
                .Select(g => new {
                     g.Store.Id,
                     g.Store.Name,
                     g.Store.Address,
                     g.StartTime,
                     g.EndTime,
                     g.Store.Photo,
            // 其他需要的店家資訊，例如地址、聯絡方式等
        })
                .FirstOrDefault();

            var finalmatch =  new 
            {
                storeId=storeInfo.Id,
                storeName = storeInfo.Name,
                address = storeInfo.Address,
                date = storeInfo.StartTime.ToString("yyyy/MM/dd"),
                startTime = storeInfo.StartTime.ToString("HH:mm"),
                endTime = storeInfo.EndTime.ToString("HH:mm"),
                photo = BuildStoreImageUrl( storeInfo.Photo),

                };

            // 如果找不到店家資料
            if (finalmatch == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到與此團隊相關的店家資訊" });
            }

            // 返回店家資料
            return Ok(new
            {
                statusCode = HttpStatusCode.OK,
                status = true,
                message = "取得店家資訊成功",
                data = finalmatch
            });
        }

        #endregion





        private string BuildProfileImageUrl(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return null; // 或者返回一個默認的圖片路徑
            }
            //return $"http://4.224.16.99/upload/profile/{photo}";
            //return $"https://2be5-4-224-16-99.ngrok-free.app/upload/profile/{photo}";
            return $"https://ec2-54-238-108-195.ap-northeast-1.compute.amazonaws.com/upload/profile/{photo}";

        }

        private string BuildStoreImageUrl(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return null; // 或者返回一個默認的圖片路徑
            }
            //return $"http://4.224.16.99/upload/store/profile/{photo}";
            //return $"https://2be5-4-224-16-99.ngrok-free.app/upload/store/profile/{photo}";
            return $"https://ec2-54-238-108-195.ap-northeast-1.compute.amazonaws.com/upload/store/profile/{photo}";
        }



    }


}







