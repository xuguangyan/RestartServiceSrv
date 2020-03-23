using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;

namespace RestartServiceSrv.Common
{
    /// <summary>
    /// 短信工具类
    /// </summary>
    public class SendSmsUtil
    {
        //desc加密密钥
        private const string descKey = "$jingsu$";

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="msg">消息内容</param>
        public static void sendSms(string msg)
        {
            bool smsOpen = ConfigHelper.smsOpen;
            if (!smsOpen)
            {
                return;
            }
            string reqUrl = ConfigHelper.smsUrl;
            string phones = ConfigHelper.phones;
            if (reqUrl.Length <= 0 || phones.Length <= 0 || msg.Length <= 0)
            {
                LogHelper.Log(string.Format("短信发送失败,以下参数不能为空：smsUrl={0},phones={1},msg={2}", reqUrl, phones, msg));
                return;
            }
            string param = string.Format("&phones={0}&content={1}", phones, msg.Replace("&", "%26"));
            HttpRequest httpRequest = new HttpRequest();
            string enctyptParam = Common.DESCrypt.Encrypt(descKey, param);
            string tmpStr = Common.DESCrypt.Decrypt(descKey, enctyptParam);
            httpRequest.Param = enctyptParam;
            bool reqOK = httpRequest.sendPost(reqUrl);
            if (reqOK)
            {
                bool rspOK = httpRequest.RespText.IndexOf("Http Request is accept.") >= 0;
                if (rspOK)
                {
                    LogHelper.Log(string.Format("短信发送成功,phones={0},msg={1}", phones, msg));
                }
                else
                {
                    LogHelper.Log(string.Format("短信发送失败：{0}", httpRequest.RespText));
                }
            }
            else
            {
                LogHelper.Log(string.Format("短信发送失败：{0}", httpRequest.RespText));
            }
        }
    }
}
