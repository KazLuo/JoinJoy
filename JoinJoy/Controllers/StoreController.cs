using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System.Data.Entity.Validation;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Store", Description = "店家相關功能")]
    [RoutePrefix("storeinfo")]
    public class StoreController : ApiController
    {

        private Context db = new Context();

        /// <summary>
        /// 店家資訊
        /// </summary>
        /// <param name="storeId">帶入stroeId產生店家資訊(id=7可以測試)</param>
        /// <returns></returns>
        #region"GetStoreInfo"
        [HttpGet]
        [Route("store/{storeId}")]
        public IHttpActionResult GetStoreInfo(int storeId)
        {
            var store = db.Stores.FirstOrDefault(s => s.Id == storeId);
            if (store == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家資訊" });
            }
            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "回傳店家資訊成功", data = new { /*id = store.Id, memberId = store.MemberId,*/ storeName = store.Name, address = store.Address, phone = store.Phone, openTime = store.OpenTime, closeTime = store.CloseTime, description = store.Introduce, /*maxPeople = store.MaxPeople*/ cost = store.Price, wifiTag = store.Wifi, teachTag = store.Teach, meal = store.Meal, mealout = store.Mealout, buffet = store.Buffet, HqTag = store.HqTag, popTag = store.PopTag,  photo = string.IsNullOrEmpty(store.Photo) ? null : $"http://4.224.16.99/upload/store/{store.Photo}" } });
        }
        #endregion

        /// <summary>
        /// 店家遊戲列表
        /// </summary>
        /// <param name="storeId">帶入stroeId產生店家資訊,目前只有storeId=7可以測試</param>
        /// <returns></returns>
        #region"GetStoreGameList"
        [HttpGet]
        [Route("gamelist/{storeId}")]
        public IHttpActionResult GetStoreGameList(int storeId)
        {
            // 檢查店家是否存在
            var storeExists = db.Stores.Any(s => s.Id == storeId);
            if (!storeExists)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家" });
            }
            var gamelist = db.StoreInventories.Where(m => m.StoreId == storeId).Select(m => new {gameId=m.Id, gametype = m.GameDetails.GameType.TypeName, gameName = m.GameDetails.Name, version = m.GameDetails.Language, peopleNum = m.GameDetails.People, qtu = m.StockCount });
            // 找不到遊戲清單
            if (!gamelist.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "找不到店家的遊戲清單" });
            }
            return Content(HttpStatusCode.OK, new
            {
                statusCode = HttpStatusCode.OK,
                status = true,
                message = "回傳店家遊戲庫存成功",
                data = new
                {
                    gamelist
                }
            });
        }
        #endregion


        /// <summary>
        /// 上傳店家頭像
        /// </summary>
        /// <returns></returns>
        #region "上傳店家頭像"
        //[HttpPost]
        //[JwtAuthFilter]
        //[Route("uploadimg")]
        //public async Task<IHttpActionResult> UploadStoreProfile()
        //{
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
        //    int memberId = (int)userToken["Id"];

        //    // 檢查是否為店主
        //    var store = db.Stores.FirstOrDefault(s => s.MemberId == memberId);
        //    if (store == null)
        //    {
        //        return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不是店主或店家不存在。" });
        //    }

        //    // 檢查請求是否包含 multipart/form-data。
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    // 檢查資料夾是否存在，若不存在則創建
        //    string root = HttpContext.Current.Server.MapPath("~/upload/store/profile");
        //    if (!Directory.Exists(root))
        //    {
        //        Directory.CreateDirectory(root);
        //    }

        //    try
        //    {
        //        // 讀取MIME資料
        //        var provider = new MultipartMemoryStreamProvider();
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        // 獲取檔案擴展名，單檔案使用.FirstOrDefault()直接提取，多檔案需使用循環
        //        string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
        //        string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // 如 .jpg

        //        // 定義檔案名稱
        //        string fileName = "Store_" + store.Id + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

        //        // 儲存圖片，單檔案使用.FirstOrDefault()直接提取，多檔案需使用循環
        //        var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
        //        var outputPath = Path.Combine(root, fileName);
        //        using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        //        {
        //            await output.WriteAsync(fileBytes, 0, fileBytes.Length);
        //        }

        //        // 讀取圖片並調整尺寸
        //        using (var image = SixLabors.ImageSharp.Image.Load(outputPath))
        //        {
        //            // 調整圖片尺寸至標準尺寸，例如：128x128像素
        //            image.Mutate(x => x.Resize(120, 120));

        //            // 檢查圖片大小，如果大於限制則壓縮圖片 (例如：不超過2MB)
        //            if (fileBytes.Length > 2 * 1024 * 1024)
        //            {
        //                // 壓縮圖片以降低大小
        //                // 可以使用ImageSharp的壓縮功能或其他工具來完成
        //            }

        //            // 儲存調整後的圖片
        //            image.Save(outputPath);
        //        }

        //        // 更新資料庫中的店家資訊
        //        store.Photo = fileName; // 儲存檔案名稱
        //        db.SaveChanges(); // 儲存變更到資料庫

        //        return Ok(new
        //        {
        //            statusCode = HttpStatusCode.OK,
        //            status = true,
        //            message = "檔案上傳成功。",
        //            data = new
        //            {
        //                FileName = fileName,
        //                FilePath = Path.Combine("/upload/store/profile", fileName) // 返回文件路徑
        //            },
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "上傳失敗，請再試一次。" + e.Message });
        //    }
        //}
        #region "上傳店家頭像"
        [HttpPost]
        [JwtAuthFilter]
        [Route("uploadimg")]
        public async Task<IHttpActionResult> UploadStoreProfile()
        {
            try
            {
                var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
                int memberId = (int)userToken["Id"];

                // 檢查是否為店主
                var store = db.Stores.FirstOrDefault(s => s.MemberId == memberId);
                if (store == null)
                {
                    return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不是店主或店家不存在。" });
                }

                // 檢查請求是否包含 multipart/form-data。
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                // 檢查資料夾是否存在，若不存在則創建
                string root = HttpContext.Current.Server.MapPath("~/upload/store/profile");
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                // 讀取MIME資料
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 獲取檔案擴展名
                string fileNameData = provider.Contents.FirstOrDefault().Headers.ContentDisposition.FileName.Trim('\"');
                string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // 如 .jpg

                // 定義檔案名稱
                string fileName = "Store_" + store.Id + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + fileType;

                // 儲存圖片
                var fileBytes = await provider.Contents.FirstOrDefault().ReadAsByteArrayAsync();
                var outputPath = Path.Combine(root, fileName);
                using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                }

                // 讀取圖片並調整尺寸
                using (var image = SixLabors.ImageSharp.Image.Load(outputPath))
                {
                    image.Mutate(x => x.Resize(120, 120));

                    if (fileBytes.Length > 2 * 1024 * 1024)
                    {
                        // 壓縮圖片
                    }

                    image.Save(outputPath);
                }

                // 更新資料庫中的店家資訊
                store.Photo = fileName;
                db.SaveChanges();

                return Ok(new
                {
                    statusCode = HttpStatusCode.OK,
                    status = true,
                    message = "檔案上傳成功。",
                    data = new
                    {
                        FileName = fileName,
                        FilePath = Path.Combine("/upload/store/profile", fileName)
                    },
                });
            }
            catch (DbEntityValidationException dbEx)
            {
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);

                var exceptionMessage = string.Concat(dbEx.Message, " 驗證錯誤訊息: ", fullErrorMessage);

                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "資料庫驗證失敗: " + exceptionMessage });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "上傳失敗，請再試一次。" + e.Message });
            }
        }
        #endregion

        #endregion

        /// <summary>
        /// 取得店家頭像
        /// </summary>
        /// <param name="storeId">測試可以用7</param>
        /// <returns></returns>
        #region "獲取店家頭像"
        [HttpGet]
        //[JwtAuthFilter]
        [Route("profileimg/{storeId}")]
        public IHttpActionResult GetStoreProfileImage(int storeId)
        {
            var store = db.Stores.FirstOrDefault(s => s.Id == storeId);
            if (store == null || string.IsNullOrEmpty(store.Photo))
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "店家不存在或未設置頭像。" });
            }

            string root = HttpContext.Current.Server.MapPath("~/upload/store/profile");
            var filePath = Path.Combine(root, store.Photo);
            if (!File.Exists(filePath))
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "文件不存在。" });
            }

            // 返回圖片的 MIME 類型和檔案流
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); // 根據實際檔案類型設置

            return ResponseMessage(result);
        }
        #endregion

        /// <summary>
        /// 取得所有店家ID
        /// </summary>
        /// <returns></returns>
        #region"取得所有店家ID"
        [HttpGet]
        [Route("getallstoreid")]
        public IHttpActionResult GetAllGroupId()
        {
            try
            {
                // 檢索所有店家的基本信息
                var stores = db.Stores.Select(m => new { m.Id, m.Name, m.Photo }).ToList();

                // 在記憶體中構建照片URL
                var data = stores.Select(m => new {
                    storeId = m.Id,
                    name = m.Name,
                    photo = string.IsNullOrEmpty(m.Photo) ? null : $"http://4.224.16.99/upload/store/{m.Photo}"
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
        /// 取得所有店家評價
        /// </summary>
        /// <param name="storeId">輸入店家目前可用7測試</param>
        /// <param name="sortBy">0 =newest,1=highest,2=lowest</param>
        /// <returns></returns>
        #region"取得店家所有評價"
        [HttpGet]
        [Route("getstorerating/{storeId}/{sortBy}")]
        public IHttpActionResult GetStoreRating(int storeId,EnumList.RatingFilter sortBy)
        {
            var storeRatings = from StoreRating in db.StoreRatings
                               join Group in db.Groups on StoreRating.GroupId equals Group.GroupId
                               join Member in db.Members on StoreRating.MemberId equals Member.Id
                               join Store in db.Stores on Group.StoreId equals Store.Id
                               where StoreRating.StoreId == storeId
                               select new
                               {
                                   userId = Member.Id,
                                   userName = Member.Nickname, // 或其他識別會員的字段
                                   userImg = Member.Photo,
                                   groupName = Group.GroupName, // 或其他識別團隊的字段
                                   groupDate = Group.StartTime,
                                   memberNum = Group.CurrentParticipants,
                                   storeName = Store.Name,
                                   storeId = Store.Id,
                                   environment = StoreRating.Clean,
                                   service = StoreRating.Service,
                                   game = StoreRating.Variety,
                                   costValue = StoreRating.Value,
                                   commentId =StoreRating.Id,
                                   msg = StoreRating.Comment,
                                   commentDate = StoreRating.RatingDate
                                   
                               };
            if (!db.Stores.Any(s=>s.Id == storeId))
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "店家不存在。" });
            }
            if (!storeRatings.Any())
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "店家目前尚未有評價。" });
            }
            var ratingsList = storeRatings.ToList();
            // 計算每個評價的平均值
            var ratingsWithAverage = storeRatings.ToList().Select(sr => new
            {
                commentBy = new 
                {
                    sr.userId,
                    sr.userName,
                    sr.userImg,
                },
                group = new 
                {
                    sr.groupName,
                    sr.groupDate,
                    sr.memberNum,
                    sr.storeId,
                    sr.storeName,
                },
                sr.commentId,
                sr.msg,
                sr.commentDate,
                score = (sr.environment + sr.service + sr.game + sr.costValue) / 4.0 // 有四個評分項目
            });

           

            var totalCleanAverage = ratingsList.Average(sr => sr.environment);
            var totalServiceAverage = ratingsList.Average(sr => sr.service);
            var totalVarietyAverage = ratingsList.Average(sr => sr.game);
            var totalValueAverage = ratingsList.Average(sr => sr.costValue);

            // 根據 sortBy 參數對結果進行排序
            switch (sortBy)
            {
                case EnumList.RatingFilter.newest:
                    ratingsWithAverage = ratingsWithAverage.OrderByDescending(r => r.commentDate);
                    break;
                case EnumList.RatingFilter.highest:
                    ratingsWithAverage = ratingsWithAverage.OrderByDescending(r => r.score);
                    break;
                case EnumList.RatingFilter.lowest:
                    ratingsWithAverage = ratingsWithAverage.OrderBy(r => r.score);
                    break;
                default:
                    ratingsWithAverage = ratingsWithAverage.OrderByDescending(r => r.commentDate);
                    break;
            }

            // 執行查詢並轉換為列表
            var comment = ratingsWithAverage.ToList();

            // 計算所有評價的整體平均值
            var overallAvgRating = ratingsWithAverage.Average(r => r.score);

            // 計算評價的總數量
            var totalRatingsCount = ratingsList.Count;

            return Ok(new
            {
                averageScore =new
                {
                    environment = totalCleanAverage,
                    service = totalServiceAverage,
                    game = totalVarietyAverage,
                    costValue = totalValueAverage,
                    overall = overallAvgRating,
                    
                },
                comments = comment
               
                //totalRatingsCount = totalRatingsCount,
                //allAverageRating = overallAvgRating,
                //cleanAverageRating = totalCleanAverage,
                //serviceAverage = totalServiceAverage,
                //varietyAverage = totalVarietyAverage,
                //valueAverage = totalValueAverage,
                //data,

            });
        }
        #endregion


    }

}
