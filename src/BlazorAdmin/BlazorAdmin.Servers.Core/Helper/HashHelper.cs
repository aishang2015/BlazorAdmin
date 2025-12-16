using System.Security.Cryptography;
using System.Text;

namespace BlazorAdmin.Servers.Core.Helper
{
    public static class HashHelper
    {
        public static string HashPwd(string pwd)
        {
            byte[] salt = new byte[16]; // 生成16字节的随机盐
            RandomNumberGenerator.Create().GetBytes(salt);

            // 使用PBKDF2算法生成哈希值
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(pwd, salt, 10000, HashAlgorithmName.SHA256, 32);
            return Encoding.UTF8.GetString(hash);
        }

        public static string HashPassword(string password)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Create().GetBytes(salt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
            var saltString = Convert.ToBase64String(salt);
            var hashString = Convert.ToBase64String(hash);
            return $"{saltString}:{hashString}";
        }

        public static bool VerifyPassword(string hashPwd, string password)
        {
            string[] passwordParts = hashPwd.Split(':', 2);
            byte[] salt = Convert.FromBase64String(passwordParts.First());
            byte[] hash = Convert.FromBase64String(passwordParts.Last());

            byte[] hashByte2 = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);
            return hashByte2.SequenceEqual(hash);
        }
    }
}
