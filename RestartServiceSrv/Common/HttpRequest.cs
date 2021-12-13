using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RestartServiceSrv.Common
{
    public class HttpRequest
    {
        /// <summary> 
        /// 网站Cookies 
        /// </summary> 
        private string _cookie = string.Empty;
        public string Cookie
        {
            get{ return _cookie; }
            set{ _cookie = value; }
        }

        /// <summary> 
        /// 网站编码 
        /// </summary> 
        private string _code = "utf-8";
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }

        /// <summary> 
        /// 请求IP 
        /// </summary> 
        private string _reqIP = string.Empty;
        public string ReqIP
        {
            get { return _reqIP; }
            set { _reqIP = value; }
        }

        /// <summary> 
        /// 请求参数 
        /// </summary> 
        private string _param = string.Empty;
        public string Param
        {
            get { return _param; }
            set { _param = value; }
        }

        /// <summary> 
        /// 响应文本 
        /// </summary> 
        private string _respText = string.Empty;
        public string RespText
        {
            get { return _respText; }
            set { _respText = value; }
        }

        /// <summary> 
        /// 内容类型 
        /// </summary> 
        private string _contentType = "application/x-www-form-urlencoded";
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        /// <summary> 
        /// 响应流 
        /// </summary> 
        private Stream _respStream;
        public Stream RespStream
        {
            get { return _respStream; }
            set { _respStream = value; }
        }

        /// <summary>
        /// 请求超时时间（秒）
        /// </summary>
        private int _timeout = 30 * 1000;
        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="url">发送请求的URL</param>
        /// <returns>请求是否成功</returns>
        public bool sendGet(string url)
        {
            try
            {
                HttpWebRequest request = null;

                //追加请求参数
                if (this._param.Length > 0)
                {
                    url += "?" + this._param;
                }

                //创建请求对象
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                }

                request.Method = "GET";
                request.Accept = "*/*";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1;SV1)";
                request.Headers.Add("X-Forwarded-For", this._reqIP);
                request.KeepAlive = true;
                request.Timeout = this._timeout * 1000;

                //写入cookie
                if (request.CookieContainer == null)
                {
                    request.CookieContainer = new CookieContainer();
                }
                if (this._cookie.Length > 0)
                {
                    request.Headers.Add("cookie:" + this._cookie);
                    request.CookieContainer.SetCookies(new Uri(url), this._cookie);
                }

                //获取响应对象
                HttpWebResponse httpWebResponse = request.GetResponse() as HttpWebResponse;

                //返回cookie
                if (request.CookieContainer != null)
                {
                    this._cookie = request.CookieContainer.GetCookieHeader(new Uri(url));
                }

                //获取响应流
                Stream s = httpWebResponse.GetResponseStream();
                this.RespStream = new MemoryStream();
                s.CopyTo(this.RespStream);
                this.RespStream.Seek(0, SeekOrigin.Begin);

                //获取响应文本
                StreamReader reader = new StreamReader(this.RespStream, Encoding.GetEncoding(this.Code));
                if (httpWebResponse.ContentLength > 1)
                {
                    this._respText = reader.ReadToEnd();
                }
                else
                {
                    char[] buffer = new char[256];
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer, 0, count));
                    }
                    this._respText = sb.ToString();
                }
            }
            catch (Exception e)
            {
                this._respText = e.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="url">发送请求的URL</param>
        /// <returns>请求是否成功</returns>
        public bool sendPost(string url)
        {
            try
            {
                HttpWebRequest request = null;

                //创建请求对象
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request = WebRequest.Create(url) as HttpWebRequest;
                    //request.ProtocolVersion = HttpVersion.Version10;
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                }

                request.Method = "POST";
                request.ContentType = this._contentType;
                request.Accept = "*/*";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1;SV1)";
                request.Headers.Add("X-Forwarded-For", this._reqIP);
                request.KeepAlive = true;
                request.Timeout = this._timeout * 1000;

                //写入cookie
                if (request.CookieContainer == null)
                {
                    request.CookieContainer = new CookieContainer();
                }
                if (this._cookie.Length > 0)
                {
                    request.Headers.Add("cookie:" + this._cookie);
                    request.CookieContainer.SetCookies(new Uri(url), this._cookie);
                }

                //发送POST数据
                byte[] data = Encoding.GetEncoding(this._code).GetBytes(this._param);

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                //获取响应对象
                HttpWebResponse httpWebResponse = request.GetResponse() as HttpWebResponse;

                //返回cookie
                if (request.CookieContainer != null)
                {
                    this._cookie = request.CookieContainer.GetCookieHeader(new Uri(url));
                }

                //获取响应流
                Stream s = httpWebResponse.GetResponseStream();
                this.RespStream = new MemoryStream();
                s.CopyTo(this.RespStream);
                this.RespStream.Seek(0, SeekOrigin.Begin);

                //获取响应文本
                StreamReader reader = new StreamReader(this.RespStream, Encoding.GetEncoding(this.Code));
                if (httpWebResponse.ContentLength > 1)
                {
                    this._respText = reader.ReadToEnd();
                }
                else
                {
                    char[] buffer = new char[256];
                    int count = 0;
                    StringBuilder sb = new StringBuilder();
                    while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Append(new string(buffer, 0, count));
                    }
                    this._respText = sb.ToString();
                } 
            }
            catch (Exception e)
            {
                this._respText = e.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }
    }
}
