using Konscious.Security.Cryptography;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JoinJoy.Security
{
    public class Argon2Verify
    {
        //    public byte[] CreateSalt()
        //    {
        //        var buffer = new byte[16];
        //        var rng = new RNGCryptoServiceProvider();
        //        rng.GetBytes(buffer);
        //        return buffer;
        //    }

        //    public byte[] HashPassword(string password, byte[] salt)
        //    {
        //        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        //        argon2.Salt = salt;
        //        argon2.DegreeOfParallelism = 8;
        //        argon2.Iterations = 4;
        //        argon2.MemorySize = 1024 * 1024;
        //        return argon2.GetBytes(16);
        //    }

        //    public bool VerifyHash(string password, byte[] salt, byte[] hash)
        //    {
        //        var newHash = HashPassword(password, salt);
        //        return hash.SequenceEqual(newHash);
        //    }
        //}

        #region "不透過Argon2加密密碼"

        public (string salt, string hashPassword) PasswordHash(string password)
        {
            // 將密碼使用 SHA256 雜湊運算(不可逆)
            //string salt = email.Substring(0, 1).ToLower(); //使用帳號前一碼當作密碼鹽

            // 生成隨機的密碼鹽
            byte[] saltBytes = new byte[16];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(saltBytes);
            }

            string salt = Convert.ToBase64String(saltBytes);

            SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(salt + password); //將密碼鹽及原密碼組合
            byte[] hash = sha256.ComputeHash(bytes);
            StringBuilder newPassword = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                newPassword.Append(hash[i].ToString("X2"));
            }
            string hashPassword = newPassword.ToString(); // 雜湊運算後密碼
            return (salt, hashPassword);
        }

        #endregion "不透過Argon2加密密碼"

        #region "驗證密碼"

        public bool VerifyPassword(string userInputPassword, string storedSalt, string storedHashPassword)
        {
            // 將輸入密碼和儲存的鹽組合
            //byte[] saltBytes = Convert.FromBase64String(storedSalt);  // 用途不明?
            byte[] inputBytes = Encoding.UTF8.GetBytes(storedSalt + userInputPassword);

            // 使用 SHA-256 哈希函數對組合的密碼進行雜湊
            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(inputBytes);
            StringBuilder newPassword = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                newPassword.Append(hash[i].ToString("X2"));
            }
            string hashedInputPassword = newPassword.ToString();

            // 比較雜湊後的密碼是否與儲存的密碼相符
            return hashedInputPassword == storedHashPassword;
        }

        #endregion "驗證密碼"
    }
}
