using RestartServiceSrv.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using RestartServiceSrv.Service;

namespace RestartServiceSrv
{
    public partial class Service1 : ServiceBase
    {
        //desc加密密钥
        private const string descKey = "$jingsu$";

        private RestartSrv restartSrv;
        private RequestSrv requestSrv;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //刷新配置
            ConfigHelper.Refresh();

            LogHelper.Log("服务已启动");

            if (ConfigHelper.isPlainPwd)
            {
                string mailPwd = ConfigHelper.mailPwd;
                if (mailPwd.Length > 0)
                {
                    LogHelper.Log("当前配置邮箱密码，密文为：" + DESCrypt.Encrypt(descKey, mailPwd));
                }
            }

            //重启服务线程
            restartSrv = new RestartSrv();
            restartSrv.Start();

            //Http请求线程
            requestSrv = new RequestSrv();
            requestSrv.Start();
        }

        protected override void OnStop()
        {
            LogHelper.Log("服务已停止");
        }
    }
}
