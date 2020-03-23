using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace RestartServiceSrv.Common
{
    /// <summary>
    /// 发送邮件工具类
    /// </summary>
    public class SendMailUtil
    {
        //desc加密密钥
        private const string descKey = "$jingsu$";

        /// <summary>
        /// 邮局地址清单
        /// </summary>
        private static Hashtable hostMap = new Hashtable();

        /// <summary>
        /// 静态构造函数（初始化邮局清单）
        /// </summary>
        static SendMailUtil()
        {
            // 126
            hostMap.Add("smtp.gd-rocent", "smtp.qq.com");
            // 126
            hostMap.Add("smtp.126", "smtp.126.com");
            // qq
            hostMap.Add("smtp.qq", "smtp.qq.com");
            // 163
            hostMap.Add("smtp.163", "smtp.163.com");
            // sina
            hostMap.Add("smtp.sina", "smtp.sina.com");
            // tom
            hostMap.Add("smtp.tom", "smtp.tom.com");
            // 263
            hostMap.Add("smtp.263", "smtp.263.net");
            // yahoo
            hostMap.Add("smtp.yahoo", "smtp.mail.yahoo.com");
            // hotmail
            hostMap.Add("smtp.hotmail", "smtp.live.com");
            // gmail
            hostMap.Add("smtp.gmail", "smtp.gmail.com");
            hostMap.Add("smtp.port.gmail", "465");

            // 139
            hostMap.Add("smtp.139", "smtp.139.com");
        }

        /// <summary>
        /// 获取邮局地址
        /// </summary>
        /// <param name="email">发件邮箱</param>
        /// <returns></returns>
        public static string getSmtpHost(String email)
        {
            string pattern = @"\w+@([\w|-]+)(\.\w+){1,2}";
            Regex reg = new Regex(pattern);
            Match matcher = reg.Match(email);
            string key = "unSupportEmail";
            if (matcher.Success)
            {
                key = "smtp." + matcher.Groups[1];
            }
            if (hostMap.Contains(key))
            {
                return (string)hostMap[key];
            }
            else
            {
                throw new Exception("unSupportEmail");
            }
        }

        /// <summary>
        /// 获取邮局端口
        /// </summary>
        /// <param name="email">发件邮箱</param>
        /// <returns></returns>
        public static int getSmtpPort(String email)
        {
            string pattern = @"\w+@(\w+)(\.\w+){1,2}";
            Regex reg = new Regex(pattern);
            Match matcher = reg.Match(email);
            string key = "unSupportEmail";
            if (matcher.Success)
            {
                key = "smtp.port." + matcher.Groups[1];
            }
            if (hostMap.Contains(key))
            {
                return Convert.ToInt32(hostMap[key]);
            }
            else
            {
                return 25;
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="toMailAddr">收件人</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="message">邮件内容</param>
        /// <returns></returns>
        public static void sendCommonMail(string toMailAddr, string subject, string message)
        {
            bool mailOpen = ConfigHelper.mailOpen;
            if (!mailOpen)
            {
                return;
            }

            if (toMailAddr.Length <= 0 || subject.Length <= 0 || message.Length <= 0)
            {
                LogHelper.Log(string.Format("邮件发送失败,以下参数不能为空：toMailAddr={0},subject={1},msg={2}", toMailAddr, subject, message));
                return;
            }

            try
            {
                //主要处理发送邮件的内容（如：收发人地址、标题、主体、图片等等）
                MailMessage mMailMessage = new MailMessage();
                mMailMessage.To.Add(toMailAddr);
                mMailMessage.From = new MailAddress(ConfigHelper.mailFromAddr, ConfigHelper.mailFromName);
                mMailMessage.Subject = subject;
                mMailMessage.Body = message;
                mMailMessage.IsBodyHtml = true;
                mMailMessage.BodyEncoding = Encoding.GetEncoding(ConfigHelper.mailCharset);
                mMailMessage.Priority = MailPriority.Normal;

                //主要处理用smtp方式发送此邮件的配置信息（如：邮件服务器、发送端口号、验证方式等等）
                SmtpClient mSmtpClient = new SmtpClient();
                //mSmtpClient.Host = "smtp." + mMailMessage.From.Host;
                mSmtpClient.Host = getSmtpHost(ConfigHelper.mailFromAddr);
                mSmtpClient.Port = getSmtpPort(ConfigHelper.mailFromAddr);
                mSmtpClient.UseDefaultCredentials = true;
                mSmtpClient.EnableSsl = false;

                string mailPwd = ConfigHelper.mailPwd;
                if (!ConfigHelper.isPlainPwd)//遇到密文先解密
                {
                    mailPwd = DESCrypt.Decrypt(descKey, mailPwd);
                }

                mSmtpClient.Credentials = new System.Net.NetworkCredential(ConfigHelper.mailUid, mailPwd);
                mSmtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                mSmtpClient.Send(mMailMessage);
            }
            catch (Exception ex)
            {
                LogHelper.Log(string.Format("邮件发送失败,描述：{0}", ex.ToString()));
            }
        }
    }
}
