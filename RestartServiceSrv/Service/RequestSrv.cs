using RestartServiceSrv.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace RestartServiceSrv.Service
{
    /// <summary>
    /// Http请求线程
    /// </summary>
    class RequestSrv
    {
        //desc加密密钥
        private const string descKey = "$jingsu$";
        //Http请求线程
        private Thread thread;
        HttpRequest httpRequest = new HttpRequest();
        // 日志前缀
        private const string LOG_PREFIX = "T03=>";

        public RequestSrv()
        {
            thread = new Thread(new ThreadStart(Run));
        }

        //启动线程
        public void Start()
        {
            thread.Start();
        }

        //关闭线程
        public void Abort()
        {
            thread.Abort();
        }

        //运行线程
        private void Run()
        {
            LogHelper.Log("Http请求线程开始", LOG_PREFIX);

            while (true)
            {
                int realCount = 0;
                int maxCount = ConfigHelper.GetValue("ReqUrl_MaxCount", 100);
                for (int i = 1; i <= maxCount; i++)
                {
                    string cfgUrl = ConfigHelper.GetValue("ReqUrl_" + i, "");

                    if (cfgUrl.Trim().Length > 0)
                    {
                        realCount++;
                        monitorUrl(cfgUrl);
                    }
                }
                if (realCount <= 0)
                {
                    LogHelper.Log("没有需要监听Http请求的地址", LOG_PREFIX);
                    break;
                }

                Thread.Sleep(1000 * 60 * ConfigHelper.ReqInterval);
            }

            LogHelper.Log("Http请求线程结束", LOG_PREFIX);
        }

        /// <summary>
        /// 监听url请求
        /// </summary>
        /// <param name="cfgUrl"></param>
        private void monitorUrl(string cfgUrl)
        {
            string[] strTmp = cfgUrl.Split('|');
            if (strTmp.Length < 2)
            {
                return;
            }
            string reqWeb = strTmp[0];//网站名称
            string reqUrl = strTmp[1];//请求地址
            if (reqUrl.Length <= 0)
            {
                return;
            }
            if (reqWeb.Length <= 0)
            {
                reqWeb = reqUrl;
            }

            reqUrl = reqUrl.Replace("&amp;", "&");

            string pattern = "";
            if (strTmp.Length > 2)
            {
                pattern = strTmp[2]; //匹配字符
            }

            httpRequest.Code = "utf-8";
            bool reqOK = httpRequest.sendGet(reqUrl);

            if (reqOK)
            {
                if (pattern.Length <= 0)
                {
                    LogHelper.Log(string.Format("网站【{0}】请求成功", reqWeb), LOG_PREFIX);
                    return;
                }

                string respText = httpRequest.RespText;
                Match m = Regex.Match(respText, pattern);
                if (m.Success)
                {
                    LogHelper.Log(string.Format("网站【{0}】请求成功，响应匹配符【{1}】", reqWeb, m.ToString()), LOG_PREFIX);
                    return;
                }
            }

            // 以下是失败的情况
            string smsTempl = ConfigHelper.smsTemplReq;
            string msg = smsTempl.Replace("${ReqWeb}", reqWeb);
            string respMsg = reqOK ? "响应匹配失败，匹配规则【" + pattern + "】" : httpRequest.RespText;
            if (msg.Length > 0)
            {
                LogHelper.Log(msg + " 响应：" + respMsg, LOG_PREFIX);
            }
            else
            {
                LogHelper.Log(string.Format("网站【{0}】请求失败：{1}", reqWeb, respMsg), LOG_PREFIX);
            }

            //发送短信
            SendSmsUtil.sendSms(msg);

            //发送邮件
            string mailSubject = ConfigHelper.mailSubjectReq.Replace("${ReqWeb}", reqWeb);
            string mailBody = ConfigHelper.mailTemplReq.Replace("${ReqWeb}", reqWeb).Replace("${ErrMsg}", respMsg);
            SendMailUtil.sendCommonMail(ConfigHelper.mailAddrs, mailSubject, mailBody);
        }
    }
}