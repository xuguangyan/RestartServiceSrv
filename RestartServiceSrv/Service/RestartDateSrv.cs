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
    /// 重启服务线程（按日期触发）
    /// </summary>
    class RestartDateSrv
    {
        //重启服务线程
        private Thread thread;
        // 心跳间隔（秒）
        private const int HEART_BEAT_INTERVAL = 60;
        // 日志前缀
        private const string LOG_PREFIX = "T01=>";
        // 记录最后发送消息时间（自动处理）
        DateTime lastSendSMSDt = System.DateTime.MinValue;

        public RestartDateSrv()
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
            LogHelper.Log("重启服务线程开始", LOG_PREFIX);

            while (true)
            {
                string services = ConfigHelper.RestartServiceName.Trim();
                string strTime = ConfigHelper.TriggerTime.Trim();

                if (services == "" || strTime == "")
                {
                    LogHelper.Log("服务名称或触发时间为空，未执行任务操作", LOG_PREFIX);
                    break;
                }

                // 判断到点触发
                DateTime triggerTime = DateTime.MaxValue;
                try
                {
                    triggerTime = DateTime.Parse(strTime + ":00");
                }
                catch
                {
                    LogHelper.Log(string.Format("服务重启时间[{0}]格式有误", strTime), LOG_PREFIX);
                    break;
                }

                if (DateTime.Now < triggerTime || DateTime.Now >= triggerTime.AddSeconds(HEART_BEAT_INTERVAL))
                {
                    // 未到期，继续轮询
                    continue;
                }
                LogHelper.Log(string.Format("【{0}】的定时重启任务触发成功", strTime), LOG_PREFIX);

                string[] srvList = services.Split(',');

                foreach (string serviceName in srvList)
                {
                    restartSrv(serviceName);
                }

                Thread.Sleep(1000 * HEART_BEAT_INTERVAL);
            }

            LogHelper.Log("重启服务线程结束", LOG_PREFIX);
        }

        /// <summary>
        /// 重启服务（强制）
        /// </summary>
        /// <param name="serviceName"></param>
        private void restartSrv(string serviceName)
        {
            string handleMsg = ""; // 处理结果反馈信息
            string handleMsgDetail = "";
            try
            {
                ServiceController sc = new ServiceController(serviceName);

                //服务已停止
                if (sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    handleMsgDetail = handleMsg = string.Format("重启服务【{0}】成功", serviceName);
                    LogHelper.Log(handleMsg, LOG_PREFIX);
                }
                else//服务已运行
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);

                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    handleMsgDetail = handleMsg = string.Format("强制重启服务【{0}】成功", serviceName);
                    LogHelper.Log(handleMsg, LOG_PREFIX);
                }
                sc.Close();
            }
            catch (Exception e)
            {
                handleMsg = string.Format("重启服务【{0}】失败，详看服务器日志", serviceName);
                handleMsgDetail = string.Format("重启服务【{0}】失败：{1}", serviceName, e.Message);
                LogHelper.Log(handleMsgDetail, LOG_PREFIX);
            }

            // 发送自动处理结果
            if (handleMsg.Length > 0)
            {
                // SendMsgInterval分钟后，才触发通知
                DateTime now = System.DateTime.Now;
                if ((now - lastSendSMSDt).TotalMinutes < ConfigHelper.SendMsgInterval)
                {
                    LogHelper.Log(string.Format("{0}分钟内忽略通知", ConfigHelper.SendMsgInterval), LOG_PREFIX);
                    return;
                }
                lastSendSMSDt = now;
                LogHelper.Log(string.Format("{0}分钟内触发通知", ConfigHelper.SendMsgInterval), LOG_PREFIX);

                //发送短信
                SendSmsUtil.sendSms(handleMsg);

                //发送邮件
                string mailSubject = "处理反馈：" + ConfigHelper.mailSubjectSrv.Replace("${ServiceName}", serviceName);
                string mailBody = handleMsgDetail;
                SendMailUtil.sendCommonMail(ConfigHelper.mailAddrs, mailSubject, mailBody);
            }
        }
    }
}
