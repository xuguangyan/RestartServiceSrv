using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace RestartServiceSrv.Common
{
    /// <summary>
    /// 写日志操作类
    /// </summary>
    public class LogHelper
    {
        //静态对象
        private static readonly object synObject = new object();

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="msg">日志内容</param>
        /// <param name="prefix">日志前缀</param>
        public static void Log(string msg, string prefix = "")
        {
            if (!ConfigHelper.WriteLog)
                return;

            msg = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + prefix + msg;
            try
            {
                Monitor.Enter(synObject);
                StreamWriter sw = System.IO.File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "\\Log.txt");
                sw.WriteLine(msg);
                sw.Close();
                Monitor.Exit(synObject);
            }
            catch
            { }
        }
    }
}
