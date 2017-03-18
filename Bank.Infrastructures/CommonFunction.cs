using System;
using System.Security.Cryptography;
using System.Text;

namespace Bank.Infrastructures
{
    public static class Conts
    {
        public const string PASSWORD_SALT = "BANK@DEMO";
    }
    public class CommonFunction
    {
        public static string CreatePasswordHash(string password, string salt)
        {
            return HashPasswordForStoringInConfigFile(password + salt);
        }
        public static string HashPasswordForStoringInConfigFile(string password)
        {
            HashAlgorithm algorithm;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            algorithm = MD5.Create();
            return HexStringFromBytes(algorithm.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }

    
    public class BankException : Exception
    {
        public BankException()
            : base() { }

        public BankException(string message)
            : base(message) { }

        public BankException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public BankException(string message, Exception innerException)
            : base(message, innerException) { }

        public BankException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

       
    }
}
