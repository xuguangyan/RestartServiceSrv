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
    /// 配置辅助类
    /// </summary>
    public class ConfigHelper
    {
        //配置文件xml对象
        private static XmlDocument xDoc = new XmlDocument();

        #region 固定配置项(写死)
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public static string CNF_PATH = "\\app.config";

        #endregion

        #region 初始化与刷新接口

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ConfigHelper()
        {
            Refresh();
        }

        /// <summary>
        /// 刷新App.config文档配置
        /// </summary>
        public static void Refresh()
        {
            try
            {
                //加载配置文件
                xDoc.Load(AppDomain.CurrentDomain.BaseDirectory + CNF_PATH);
            }
            catch { }
            WriteLog = GetValue("WriteLog", false);

            //服务监测配置
            ServiceName = GetValue("ServiceName", "");
            Interval = GetValue("Interval", 3);
            ForcedRestart = GetValue("ForcedRestart", false);
            DoRestart = GetValue("DoRestart", false);
            smsTemplSrv = GetValue("smsTemplSrv", "");
            mailSubjectSrv = GetValue("mailSubjectSrv", "");
            mailTemplSrv = GetValue("mailTemplSrv", "");

            //网址监测配置
            ReqUrl_1 = GetValue("ReqUrl_1", "");
            ReqUrl_2 = GetValue("ReqUrl_2", "");
            ReqUrl_3 = GetValue("ReqUrl_3", "");
            ReqUrl_4 = GetValue("ReqUrl_4", "");
            ReqUrl_5 = GetValue("ReqUrl_5", "");
            ReqUrl_6 = GetValue("ReqUrl_6", "");
            ReqUrl_7 = GetValue("ReqUrl_7", "");
            ReqUrl_8 = GetValue("ReqUrl_8", "");
            ReqUrl_9 = GetValue("ReqUrl_9", "");
            ReqUrl_10 = GetValue("ReqUrl_10", "");
            ReqInterval = GetValue("ReqInterval", 10);
            smsTemplReq = GetValue("smsTemplReq", "");
            mailSubjectReq = GetValue("mailSubjectReq", "");
            mailTemplReq = GetValue("mailTemplReq", "");

            //短信配置
            smsOpen = GetValue("smsOpen", false);
            smsUrl = GetValue("smsUrl", "");
            phones = GetValue("phones", "");

            //邮件配置
            mailOpen = GetValue("mailOpen", false);
            mailUid = GetValue("mailUid", "");
            mailPwd = GetValue("mailPwd", "");
            mailFromAddr = GetValue("mailFromAddr", "");
            mailFromName = GetValue("mailFromName", "");
            mailCharset = GetValue("mailCharset", "utf-8");
            mailAddrs = GetValue("mailAddrs", "");
            isPlainPwd = GetValue("isPlainPwd", true);

            if (mailFromAddr.Length <= 0)
            {
                mailFromAddr = mailUid;
            }
        }
        #endregion

        #region 读写配置文件

        /// <summary>
        /// 设置配置中某个key的value.
        /// </summary>
        /// <param name="AppKey">key</param>
        /// <param name="AppValue">value</param>
        public static void SetValue(string AppKey, string AppValue)
        {
            //加载配置文件
            xDoc.Load(AppDomain.CurrentDomain.BaseDirectory + CNF_PATH);
            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null)
            {
                xElem1.SetAttribute("value", AppValue);
            }
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(Application.StartupPath + "/app.cfg");
        }
        /// <summary>
        /// 读取配置中某个key的value(string型)
        /// </summary>
        /// <param name="AppKey">key</param>
        /// <param name="defValue">value</param>
        /// <returns></returns>
        public static string GetValue(string AppKey, string defValue)
        {
            XmlNode xNode;
            XmlElement xElem;
            try
            {
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            }
            catch { return defValue; }

            if (xElem != null)
            {
                string value = xElem.GetAttribute("value");
                return value;
            }
            else
            {
                return defValue;
            }
        }
        /// <summary>
        /// 读取配置中某个key的value(int型)
        /// </summary>
        /// <param name="AppKey">key</param>
        /// <param name="defValue">value</param>
        /// <returns></returns>
        public static int GetValue(string AppKey, int defValue)
        {
            XmlNode xNode;
            XmlElement xElem;
            try
            {
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            }
            catch { return defValue; }

            if (xElem != null)
            {
                string value = xElem.GetAttribute("value");
                try { return Convert.ToInt32(value); }
                catch { return defValue; }
            }
            else
            {
                return defValue;
            }
        }
        /// <summary>
        /// 读取配置中某个key的value(bool型)
        /// </summary>
        /// <param name="AppKey">key</param>
        /// <param name="defValue">value</param>
        /// <returns></returns>
        public static bool GetValue(string AppKey, bool defValue)
        {
            XmlNode xNode;
            XmlElement xElem;
            try
            {
                xNode = xDoc.SelectSingleNode("//appSettings");
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            }
            catch { return defValue; }
            
            if (xElem != null)
            {
                string value = xElem.GetAttribute("value");
                try { return Convert.ToBoolean(value); }
                catch { return defValue; }
            }
            else
            {
                return defValue;
            }
        }

        #endregion

        #region 来自App.config的配置信息

        /// <summary>
        /// 是否写日志(默认false)
        /// </summary>
        public static bool WriteLog;

        #region 关于服务监测的配置
        
        /// <summary>
        /// 需要监控的服务名称（不监控留空，多个逗号隔开）
        /// </summary>
        public static string ServiceName;
        /// <summary>
        /// 检查服务启动状态间隔(分钟,默认3)
        /// </summary>
        public static int Interval;
        /// <summary>
        /// 如果服务停止，是否进行重启(默认false)
        /// </summary>
        public static bool DoRestart;
        /// <summary>
        /// 如果服务已启动，是否强制重启(默认false)
        /// </summary>
        public static bool ForcedRestart;
        /// <summary>
        /// 短信模板(服务监听)，变量${ServiceName}可选
        /// </summary>
        public static string smsTemplSrv;
        /// <summary>
        /// 邮件标题(服务监听)，变量${ServiceName}可选
        /// </summary>
        public static string mailSubjectSrv;
        /// <summary>
        /// 邮件内容模板(服务监听)，变量${ServiceName}可选
        /// </summary>
        public static string mailTemplSrv;

        #endregion

        #region 关于网址监测的配置
        
        /// <summary>
        /// 发起Http请求地址清单（key格式如ReqUrl_*，取1-10个，value空则不执行）
        /// </summary>
        public static string ReqUrl_1;
        public static string ReqUrl_2;
        public static string ReqUrl_3;
        public static string ReqUrl_4;
        public static string ReqUrl_5;
        public static string ReqUrl_6;
        public static string ReqUrl_7;
        public static string ReqUrl_8;
        public static string ReqUrl_9;
        public static string ReqUrl_10;
        /// <summary>
        /// 发起Http请求间隔(分钟,默认10)
        /// </summary>
        public static int ReqInterval;
        /// <summary>
        /// 短信模板(网址请求),变量${ReqWeb}可选
        /// </summary>
        public static string smsTemplReq;
        /// <summary>
        /// 邮件标题(网址请求),变量${ReqWeb}可选
        /// </summary>
        public static string mailSubjectReq;
        /// <summary>
        /// 邮件内容模板(网址请求),变量${ReqWeb}可选
        /// </summary>
        public static string mailTemplReq;

        #endregion

        #region 关于短信的配置
        /// <summary>
        /// 短信通知开关（默认关闭）
        /// </summary>
        public static bool smsOpen;
        /// <summary>
        /// 短信代理地址
        /// </summary>
        public static string smsUrl;
        /// <summary>
        /// 接收短信的号码（多个逗号隔开）
        /// </summary>
        public static string phones;

        #endregion

        #region 关于邮件的配置
        /// <summary>
        /// 邮件通知开关（默认关闭）
        /// </summary>
        public static bool mailOpen;
        /// <summary>
        /// 邮箱账号
        /// </summary>
        public static string mailUid;
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public static string mailPwd;
        /// <summary>
        /// 发送者地址，默认为账号
        /// </summary>
        public static string mailFromAddr;
        /// <summary>
        /// 发送者称呼
        /// </summary>
        public static string mailFromName;
        /// <summary>
        /// 邮件字符编码，默认utf-8
        /// </summary>
        public static string mailCharset;
        /// <summary>
        /// 接收邮件的地址（多个分号隔开）
        /// </summary>
        public static string mailAddrs;
        /// <summary>
        /// 是否明文密码，默认true
        /// </summary>
        public static bool isPlainPwd;
        #endregion

        #endregion
    }
}
