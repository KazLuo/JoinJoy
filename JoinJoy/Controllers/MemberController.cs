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
using System.Threading.Tasks;
using System.Web;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
                    nickname = member.Nickname,
                    account = member.Account,
                    introduce = member.Introduce,
                    gamePref = member.GamePreferences.Select(m => m.GameType.TypeName),
                    cityPref = member.CityPreferences.Select(m => m.City.CityName)
                }
            });

        }
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
                    nickname = member.Nickname,
                    introduce = member.Introduce,
                    gamePref = member.GamePreferences.Select(m => m.GameType.TypeName),
                    cityPref = member.CityPreferences.Select(m => m.City.CityName)
                }
            });
        }
        #endregion
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
        #region"所有喜好城市列表"
        /// <summary>
        /// 取得所有喜好城市列表
        /// </summary>
        /// <returns></returns>
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
        #region"所有喜好遊戲類別列表"
        /// <summary>
        /// 取得所有喜好遊戲類型列表
        /// </summary>
        /// <returns></returns>
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
            var info = db.GroupParticipants.Where(gp => gp.MemberId == userId).Select(gp => new { groupId = gp.GroupId, groupName = gp.Group.GroupName, startTime = gp.Group.StartTime, endTime = gp.Group.EndTime, totalMemberNum = gp.Group.MaxParticipants, currentPeople = gp.Group.CurrentParticipants, place = string.IsNullOrEmpty(gp.Group.Address) ? (string)null : gp.Group.Address, store = new { storeId = gp.Group.StoreId, storeName = gp.Group.Store.Name, address = gp.Group.Store.Address }, status = gp.AttendanceStatus.ToString() }).ToList();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳成功!", data = new { info } });
        }
        #endregion
        /// <summary>
        /// 會員評價
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

            var memberToRate = db.Members.FirstOrDefault(m => m.Id == viewRatingMember.memberId);
            if (memberToRate == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "被評價的會員不存在" });
            }

            // 確保用戶不是在評價自己
            if (userId == viewRatingMember.memberId)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您不能評價自己" });
            }

            // 檢查用戶是否已經給這個會員評過分
            var existingRating = db.MemberRatings.FirstOrDefault(r => r.RatedId == viewRatingMember.memberId && r.MemberId == userId);
            if (existingRating != null)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "您已經給該會員評過分" });
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
        [HttpGet]
        [Route("check-group-ratings/{groupId}")]
        public IHttpActionResult CheckGroupRatings(int groupId)
        {
            // 獲取團隊成員
            var groupMembers = db.GroupParticipants.Where(gp => gp.GroupId == groupId).Select(gp => gp.MemberId).ToList();

            // 檢查每位成員是否被評價
            var memberRatingsStatus = groupMembers.Select(memberId =>
                new {
                    MemberId = memberId,
                    IsRated = db.MemberRatings.Any(mr => mr.RatedId == memberId && mr.GroupId == groupId)
                }).ToList();

            // 檢查是否所有成員都已被評價
            var isAllRated = memberRatingsStatus.All(mrs => mrs.IsRated);

            return Ok(new
            {
                IsAllMembersRated = isAllRated,
                MembersRatingStatus = memberRatingsStatus
            });
        }



    }
}







