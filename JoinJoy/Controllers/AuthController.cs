using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;
using NSwag.Annotations;

namespace JoinJoy.Controllers
{
    [OpenApiTag("Auth", Description = "基本註冊登入功能")]
    [RoutePrefix("auth")]
    public class AuthController : ApiController
    {
        private Context db = new Context();
        //用於驗證加密密碼
        private Argon2Verify argon2 = new Argon2Verify();

        #region "register"
        /// <summary>
        /// 會員註冊
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(ViewRegister viewRegister)
        {
            //"帳號或密碼格式錯誤，請重新輸入"
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "帳號或密碼格式錯誤，請重新輸入" });
            }
            // 檢查帳號是否已存在
            bool existingUser = db.Members.Any(m => m.Account == viewRegister.email);

            if (existingUser)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "帳號已存在" });
            }

            //byte[] salt = argon2.CreateSalt();
            //byte[] hashedPassword = argon2.HashPassword(viewRegister.password, salt);

            var hashedPassword = argon2.PasswordHash(viewRegister.password);







            var newUser = new Member
            {
                Account = viewRegister.email,
                //Password = Convert.ToBase64String(hashedPassword),
                //PasswordSalt = Convert.ToBase64String(salt),
                Password = hashedPassword.hashPassword,
                PasswordSalt = hashedPassword.salt,
                Nickname = viewRegister.nickname
            };

            db.Members.Add(newUser);
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "註冊成功" });
        }
        #endregion "register"

        #region "login"
        /// <summary>
        /// 會員登入
        /// </summary>
        /// <param name="viewlogin">會員登入</param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(Viewlogin viewlogin)
        {
            //帳號或密碼格式錯誤，請重新輸入
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "帳號或密碼格式錯誤，請重新輸入" });
            }
            string account = viewlogin.email;
            string password = viewlogin.password;

            // 尋找帳號
            var user = db.Members.FirstOrDefault(m => m.Account == account);
            if (user == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "帳號不存在，請註冊後登入" });
            }

            Argon2Verify argon2Verifier = new Argon2Verify();

            // 從資料庫中取得鹽並解碼
            byte[] storedSalt = Convert.FromBase64String(user.PasswordSalt);

            // 使用鹽和提供的密碼產生加密的密碼(已經在下方驗證，可以刪掉)
            //byte[] hashPassword = argon2Verifier.HashPassword(password, storedSalt);
            string hashpassword = user.Password;
            string dbsalt = user.PasswordSalt;

            // 檢查計算出的加密密碼是否與資料庫中的匹配
            if (!argon2Verifier.VerifyPassword(password, dbsalt, hashpassword))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "帳號或密碼錯誤，請重新輸入" });
            }



            JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
            string jwtToken = jwtAuthUtil.GenerateToken(user.Id);

            return Ok(new { statusCode = HttpStatusCode.OK, status = true, jwtToken = jwtToken, message = "登入成功" });

        }

        #endregion
        ///// <summary>
        ///// 更改密碼
        ///// </summary>
        ///// <param name="viewPasswordChange">在會員詳細頁中修改密碼</param>
        ///// <returns></returns>
        //#region"更改密碼"
        //[HttpPost]
        //[JwtAuthFilter]
        //[Route("changePaswrd")]
        //public IHttpActionResult ChangePassword(ViewPasswordChange viewPasswordChange)
        //{
        //    Argon2Verify argon2Verifier = new Argon2Verify();
        //    // 檢查格式是否正確
        //    if (!ModelState.IsValid)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "密碼和確認密碼不匹配" });
        //    }
        //    // 從JWT中提取用戶ID
        //    var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
        //    int memberId = (int)userToken["Id"];
        //    // 使用用戶ID查找用戶
        //    var user = db.Members.FirstOrDefault(m => m.Id == memberId);
        //    if (user == null)
        //    {
        //        return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
        //    }
        //    // 使用存儲的鹽值驗證舊密碼
        //    byte[] storedSalt = Convert.FromBase64String(user.PasswordSalt);
        //    if (!argon2Verifier.VerifyPassword(password, dbsalt, hashpassword))
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "舊密碼錯誤，請重新輸入" });
        //    }
        //    // 檢查新密碼是否與舊密碼相同
        //    if (argon2.VerifyHash(viewPasswordChange.newPaswrd, storedSalt, Convert.FromBase64String(user.Password)))
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "新密碼不能與舊密碼相同" });
        //    }

        //    // 為新密碼生成新的Hash&Salt
        //    byte[] newSalt = argon2.CreateSalt();
        //    byte[] newHashedPassword = argon2.HashPassword(viewPasswordChange.newPaswrd, newSalt);

        //    // 更新資料庫中的密碼和Salt
        //    user.Password = Convert.ToBase64String(newHashedPassword);
        //    user.PasswordSalt = Convert.ToBase64String(newSalt);
        //    db.SaveChanges();

        //    return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "密碼修改成功" });
        //}
        /// <summary>
        /// 更改密碼
        /// </summary>
        /// <param name="viewPasswordChange">在會員詳細頁中修改密碼</param>
        /// <returns></returns>
        //#endregion
        #region "更改密碼"
        [HttpPost]
        [JwtAuthFilter]
       
        [Route("changePaswrd")]
        public IHttpActionResult ChangePassword(ViewPasswordChange viewPasswordChange)
        {
            // 檢查格式是否正確
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "密碼和確認密碼不匹配" });
            }

            // 從JWT中提取用戶ID
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int memberId = (int)userToken["Id"];

            // 使用用戶ID查找用戶
            var user = db.Members.FirstOrDefault(m => m.Id == memberId);
            if (user == null)
            {
                return Content(HttpStatusCode.NotFound, new { statusCode = HttpStatusCode.NotFound, status = false, message = "用戶不存在" });
            }

            // 使用存儲的鹽值驗證舊密碼
            string storedHash = user.Password;
            string storedSalt = user.PasswordSalt;

            if (!argon2.VerifyPassword(viewPasswordChange.oldPaswrd, storedSalt, storedHash))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "舊密碼錯誤，請重新輸入" });
            }

            // 檢查新密碼是否與舊密碼相同
            if (argon2.VerifyPassword(viewPasswordChange.newPaswrd, storedSalt, storedHash))
            {
                return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "新密碼不能與舊密碼相同" });
            }

            // 為新密碼生成新的Hash&Salt
            var hashedPassword = argon2.PasswordHash(viewPasswordChange.newPaswrd);

            // 更新資料庫中的密碼和Salt
            user.Password = hashedPassword.hashPassword;
            user.PasswordSalt = hashedPassword.salt;
            db.SaveChanges();

            return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "密碼修改成功" });
        }
        #endregion
        /// <summary>
        /// 確認登入狀態
        /// </summary>
        /// <returns></returns>
        #region"確認入狀態"
        [HttpGet]
        [JwtAuthFilter]
        [Route("checkLoginStatus")]
        public IHttpActionResult CheckLoginStatus()
        {
            // 假設 JwtAuthFilter 會檢查 JWT 的有效性
            var userToken = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            if (userToken != null)
            {
                // 取得用戶資訊，如果需要的話
                int userId = (int)userToken["Id"];
                var user = db.Members.FirstOrDefault(m => m.Id == userId);
                if (user != null)
                {
                    // 用戶已登入且 JWT 是有效的
                    return Content(HttpStatusCode.OK, new { statusCode = HttpStatusCode.OK, status = true, message = "用戶已登入。" });
                }
            }

            // 如果 JWT 無效或者找不到對應的用戶
            
            return Content(HttpStatusCode.BadRequest, new { statusCode = HttpStatusCode.BadRequest, status = false, message = "用戶未登入或會話已過期。" });
        }
        #endregion


    }
}
