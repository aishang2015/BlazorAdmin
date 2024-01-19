using System.Security.Cryptography;
using System.Text;

namespace BlazorAdmin.Core.Helper
{
    public static class HashHelper
    {
        public static string HashPwd(string pwd)
        {
            byte[] salt = new byte[16]; // 生成16字节的随机盐
            RandomNumberGenerator.Create().GetBytes(salt);

            // 使用PBKDF2算法生成哈希值
            var pbkdf2 = new Rfc2898DeriveBytes(pwd, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return Encoding.UTF8.GetString(hash);
        }

        public static string HashPassword(string password)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Create().GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var saltString = Convert.ToBase64String(salt);
            var hashString = Convert.ToBase64String(pbkdf2.GetBytes(32));
            return $"{saltString}:{hashString}";
        }

        public static bool VerifyPassword(string hashPwd, string password)
        {
            string[] passwordParts = hashPwd.Split(':', 2);
            byte[] salt = Convert.FromBase64String(passwordParts.First());
            byte[] hash = Convert.FromBase64String(passwordParts.Last());

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32).SequenceEqual(hash);
        }
    }
}
