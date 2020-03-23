using RestartServiceSrv.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace RestartServiceSrv.Service
{
    /// <summary>
    /// 重启服务线程
    /// </summary>
    class RestartSrv
    {
        //重启服务线程
        private Thread thread;

        public RestartSrv()
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
            LogHelper.Log("服务监听线程开始");

            while (true)
            {
                string services = ConfigHelper.ServiceName.Trim();
                if (services == "")
                {
                    LogHelper.Log("服务名称为空，未执行任务操作");
                    break;
                }
                string[] srvList = services.Split(',');

                foreach (string serviceName in srvList)
                {
                    monitorSrv(serviceName);
                }

                Thread.Sleep(1000 * 60 * ConfigHelper.Interval);
            }

            LogHelper.Log("服务监听线程结束");
        }

        /// <summary>
        /// 监测服务
        /// </summary>
        /// <param name="serviceName"></param>
        private void monitorSrv(string serviceName)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);

                //服务已停止
                if (sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    string smsTempl = ConfigHelper.smsTemplSrv;
                    string msg = smsTempl.Replace("${ServiceName}", serviceName);
                    if (msg.Length > 0)
                    {
                        LogHelper.Log(msg);
                    }
                    else
                    {
                        LogHelper.Log(string.Format("服务【{0}】已停止", serviceName));
                    }
                    //发送短信
                    SendSmsUtil.sendSms(msg);

                    //发送邮件
                    string mailSubject = ConfigHelper.mailSubjectSrv.Replace("${ServiceName}", serviceName);
                    string mailBody = ConfigHelper.mailTemplSrv.Replace("${ServiceName}", serviceName);
                    SendMailUtil.sendCommonMail(ConfigHelper.mailAddrs, mailSubject, mailBody);

                    //可启动，且允许启动
                    bool doRestart = ConfigHelper.DoRestart;
                    if (!sc.CanStop && doRestart)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);
                        LogHelper.Log(string.Format("重启服务【{0}】成功", serviceName));
                    }
                }
                else//服务已运行
                {
                    //强制重启服务
                    if (ConfigHelper.ForcedRestart)
                    {
                        //可停止（先停止）
                        if (sc.CanStop)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                        //可启动（再启动）
                        if (!sc.CanStop)
                        {
                            sc.Start();
                            sc.WaitForStatus(ServiceControllerStatus.Running);
                        }
                        LogHelper.Log(string.Format("强制重启服务【{0}】成功", serviceName));
                    }
                    else
                    {
                        LogHelper.Log(string.Format("服务【{0}】运行正常", serviceName));
                    }
                }
                sc.Close();
            }
            catch (Exception e)
            {
                LogHelper.Log(string.Format("重启服务【{0}】失败：{1}", serviceName, e.Message));
            }
        }
    }
}
