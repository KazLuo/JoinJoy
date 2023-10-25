using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using JoinJoy.Models;
using JoinJoy.Models.ViewModels;
using JoinJoy.Security;



namespace JoinJoy.Controllers
{
    [RoutePrefix("auth")]
    public class AuthController : ApiController
    {
        private Context db = new Context();
        //用於驗證加密密碼
        private Argon2Verify argon2 = new Argon2Verify();
        #region "register"
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(ViewRegister viewRegister)
        {
            //"帳號或密碼格式錯誤，請重新輸入"
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, new { StatusCode= HttpStatusCode.BadRequest, Status = false, Message = "帳號或密碼格式錯誤，請重新輸入" });
            }
            // 檢查帳號是否已存在
            var existingUser = db.Members.FirstOrDefault(m => m.Account == viewRegister.Account);
            if (existingUser != null)
            {
                return Content(HttpStatusCode.BadRequest, new { StatusCode = HttpStatusCode.BadRequest, Status = false, Message = "帳號已存在" });
            }

            byte[] salt = argon2.CreateSalt();
            byte[] hashedPassword = argon2.HashPassword(viewRegister.Password, salt);

            var newUser = new Member
            {
                Account = viewRegister.Account,
                Password = Convert.ToBase64String(hashedPassword),
                PasswordSalt = Convert.ToBase64String(salt),
                Nickname = viewRegister.Nickname
            };

            db.Members.Add(newUser);
            db.SaveChanges();

            return Content(HttpStatusCode.OK,new { StatusCode = HttpStatusCode.OK, Status = true, Message="註冊成功"});
        }
        #endregion "register"

        #region "login"
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(Viewlogin viewlogin)
        {
                //帳號或密碼格式錯誤，請重新輸入
                if (!ModelState.IsValid)
                {
                    return Content(HttpStatusCode.BadRequest, new { StatusCode = HttpStatusCode.BadRequest, Status = false, Message = "帳號或密碼格式錯誤，請重新輸入" });
                }
                string account = viewlogin.Account;
                string password = viewlogin.Password;

                // 尋找帳號
                var user = db.Members.FirstOrDefault(m => m.Account == account);
                if (user == null)
                {
                    return Content(HttpStatusCode.NotFound, new { StatusCode = HttpStatusCode.NotFound, Status = false, Message = "帳號不存在，請註冊後登入" });
                }

                Argon2Verify argon2Verifier = new Argon2Verify();

                // 從資料庫中取得鹽並解碼
                byte[] storedSalt = Convert.FromBase64String(user.PasswordSalt);

                // 使用鹽和提供的密碼產生加密的密碼(已經在下方驗證，可以刪掉)
                //byte[] hashPassword = argon2Verifier.HashPassword(password, storedSalt);

                // 檢查計算出的加密密碼是否與資料庫中的匹配
                if (!argon2Verifier.VerifyHash(password, storedSalt, Convert.FromBase64String(user.Password)))
                {
                    return Content(HttpStatusCode.BadRequest, new { StatusCode = HttpStatusCode.BadRequest, Status = false, Message = "帳號或密碼錯誤，請重新輸入" });
                }



                JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                string jwtToken = jwtAuthUtil.GenerateToken(user.Id);

                return Ok(new { StatusCode = HttpStatusCode.OK, Status = true, JwtToken = jwtToken, Message = "登入成功" });
            
        }

        #endregion



    }
}
