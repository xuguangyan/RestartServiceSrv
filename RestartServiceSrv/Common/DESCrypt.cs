using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace RestartServiceSrv.Common
{
    /// <summary>
    ///  DES算法简介：
    /// 数据加密标准（Data Encryption Standard，缩写：DES），它是由IBM公司研制的一种加密算法，
    /// 美国国家标准局于1977年公布把它作为非机要部门使用的数据加密标准；
    /// 它是一个分组加密算法，他以64位为分组对数据加密。
    /// 同时DES也是一个对称算法：加密和解密用的是同一个算法。
    /// 它的密匙长度是56位（因为每个第8 位都用作奇偶校验），
    /// 密匙可以是任意的56位的数，而且可以任意时候改变．
    /// </summary>
    public class DESCrypt
    {
        /// <summary>
        /// 加密IV向量
        /// </summary>
        private static byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="Key">密钥</param>
        /// <param name="plainStr">明文字符串</param>
        /// <param name="ivStr">加密向量</param>
        /// <param name="cMode">加密模式</param>
        /// <param name="pMode">填充模式</param>
        /// <returns>密文</returns>
        public static String Encrypt(String Key, String plainStr, String ivStr = "", CipherMode cMode = CipherMode.ECB, PaddingMode pMode = PaddingMode.PKCS7)
        {
            if (Key.Length != 8)
            {
                return "";
            }

            byte[] bKey = Encoding.UTF8.GetBytes(Key);
            byte[] bIV = IV;
            if (ivStr.Length > 0)
            {
                bIV = Encoding.UTF8.GetBytes(ivStr);
            }
            byte[] bStr = Encoding.UTF8.GetBytes(plainStr);
            try
            {
                DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
                //填充模式
                desc.Padding = pMode;
                desc.Mode = cMode;
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, desc.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write);
                cStream.Write(bStr, 0, bStr.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="Key">密钥</param>
        /// <param name="encryptStr">密文字符串</param>
        /// <param name="ivStr">加密向量</param>
        /// <param name="cMode">加密模式</param>
        /// <param name="pMode">填充模式</param>
        /// <returns>明文</returns>
        public static String Decrypt(String Key, String encryptStr, String ivStr = "", CipherMode cMode = CipherMode.ECB, PaddingMode pMode = PaddingMode.PKCS7)
        {
            if (Key.Length != 8)
            {
                return "";
            }
            try
            {
                byte[] bKey = Encoding.UTF8.GetBytes(Key);
                byte[] bIV = IV;
                if (ivStr.Length > 0)
                {
                    bIV = Encoding.UTF8.GetBytes(ivStr);
                }
                byte[] bStr = Convert.FromBase64String(encryptStr);
                DESCryptoServiceProvider desc = new DESCryptoServiceProvider();
                //填充模式
                desc.Padding = pMode;
                desc.Mode = cMode;
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, desc.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write);
                cStream.Write(bStr, 0, bStr.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
