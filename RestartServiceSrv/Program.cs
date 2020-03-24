using RestartServiceSrv.Common;
using RestartServiceSrv.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace RestartServiceSrv
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new Service1() 
            };
            ServiceBase.Run(ServicesToRun);


            ////测试1
            //RestartSrv srv = new RestartSrv();
            //srv.Start();


            //测试2
            RequestSrv req = new RequestSrv();
            req.Start();

            ////测试3
            //ConfigHelper.Refresh();
            //SendMailUtil.sendCommonMail(ConfigHelper.mailAddrs
            //    , ConfigHelper.mailSubjectReq, ConfigHelper.mailTemplReq);

            ////测试4
            //RestartDateSrv srv2 = new RestartDateSrv();
            //srv2.Start();
        }
    }
}
