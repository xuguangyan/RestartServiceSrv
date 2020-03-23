using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;

namespace RestartServiceSrv.Common
{
    public class SettingHelper : IDisposable
    {
        #region 私有成员
        private string _ServiceName;
        private string _DisplayName;
        private string _Description;
        #endregion

        #region 构造函数
        /// <summary> 
        /// 初始化服务配置帮助类 
        /// </summary> 
        public SettingHelper()
        {
            InitSettings();
        }
        #endregion

        #region 属性
        /// <summary> 
        /// 系统用于标志此服务的名称 
        /// </summary> 
        public string ServiceName
        {
            get { return _ServiceName; }
        }
        /// <summary> 
        /// 向用户标志服务的友好名称 
        /// </summary> 
        public string DisplayName
        {
            get { return _DisplayName; }
        }
        /// <summary> 
        /// 服务的说明 
        /// </summary> 
        public string Description
        {
            get { return _Description; }
        }
        #endregion

        #region 私有方法
        #region 初始化服务配置信息
        /// <summary> 
        /// 初始化服务配置信息 
        /// </summary> 
        private void InitSettings()
        {
            string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string xmlfile = root.Remove(root.LastIndexOf('\\') + 1) + "ServiceConfig.xml";
            if (File.Exists(xmlfile))
            {
                //系统用于标志此服务名称(唯一性)
                _ServiceName = Get_ConfigValue(xmlfile, "serviceName");
                //向用户标志服务的显示名称(可以重复)
                _DisplayName = Get_ConfigValue(xmlfile, "displayName");
                //服务的说明(描述)
                _Description = Get_ConfigValue(xmlfile, "discription");
            }
            else
            {
                Log("未能找到服务名称配置文件" + xmlfile);
            }

        }
        /// <summary>
        /// 读取 XML中指定节点值
        /// </summary>
        /// <param name="configpath">配置文件路径</param>
        /// <param name="strKeyName">键值</param>        
        /// <returns></returns>
        protected static string Get_ConfigValue(string configpath, string strKeyName)
        {
            using (XmlTextReader tr = new XmlTextReader(configpath))
            {
                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Element)
                    {
                        if (tr.Name == strKeyName)
                        {
                            return tr.ReadString();
                        }
                    }
                }
            }
            return "";
        }
        #endregion
        #endregion

        #region IDisposable 成员
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //managed dispose 
                    _ServiceName = null;
                    _DisplayName = null;
                    _Description = null;
                }
                //unmanaged dispose 
            }
            disposed = true;
        }
        ~SettingHelper()
        {
            Dispose(false);
        }
        #endregion

        #region 写日志
         //静态对象
        private static readonly object synObject=new object();

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg"></param>
        public static void Log(string msg)
        {
            msg = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg;
            try
            {
                Monitor.Enter(synObject);
                string appRoot = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string basePath = appRoot.Substring(0, appRoot.LastIndexOf('\\'));
                StreamWriter sw = System.IO.File.AppendText(basePath + "\\Log.txt");
                sw.WriteLine(msg);
                sw.Close();
                Monitor.Exit(synObject);
            }
            catch
            { }
        }
        #endregion
    }
}
