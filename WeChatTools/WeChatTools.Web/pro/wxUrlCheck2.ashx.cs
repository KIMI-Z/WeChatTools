﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using WeChatTools.Core;

namespace WeChatTools.Web
{
    /// <summary>
    /// 微信域名检测工具
    /// </summary>
    public class WXUrlCheck2 : IHttpHandler
    {
        private ServiceApi _service = null;
        private const int DURATION = 24 * 60;
        private static string userIP = "127.0.0.1";
        public void ProcessRequest(HttpContext context)
        {
            //正式用
            userIP = GetWebClientIp();
            context.Response.ContentType = "text/plain";

            if (!string.IsNullOrEmpty(context.Request["url"]) && !string.IsNullOrEmpty(context.Request["key"]) &&  context.Request["key"].Length ==32)
            {
                string userKey = context.Request["key"]; //key ,md5值
                //需要检测的网址
                string urlCheck = context.Request["url"]; //检测的值
                urlCheck = urlCheck.Replace("https://", "").Replace("http://", "");
                string json = "{\"Mode\":\"WXCheckUrl\",\"Param\":\"{\'CheckUrl\':\'" + urlCheck + "\',\'UserKey\':\'" + userKey + "\'}\"}";

                ServiceApiClient SpVoiceObj = new ServiceApiClient("NetTcpBinding_IServiceApi");
                SpVoiceObj.Open();
                string result = SpVoiceObj.Api(json);
                SpVoiceObj.Close();
                Logger.WriteLoggger(userIP + ":" + userKey+":"+result);
                context.Response.Write(result);
            }
            else
            {
                context.Response.Write("参数错误,进qq群交流:41977413！");

            }

            context.Response.End();



        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        /// <summary>  
        /// 获取外网ip地址  
        /// </summary>  
        public static string GetExtenalIpAddress()
        {
            String url = "http://city.ip138.com/ip2city.asp";
            string IP = "未获取到外网ip";
            try
            {
                //从网址中获取本机ip数据    
                System.Net.WebClient client = new System.Net.WebClient();
                client.Encoding = System.Text.Encoding.Default;
                string str = client.DownloadString(url);
                client.Dispose();

                //提取外网ip数据 [218.104.71.178]    
                int i1 = str.IndexOf("["), i2 = str.IndexOf("]");

                if (!str.Equals("")) IP = str.Substring(i1 + 1, i2 - 1 - i1);
                else IP = GetExtenalIpAddress_0();
            }
            catch (Exception) { }

            return IP;
        }

        /// <summary>  
        /// 获取外网ip地址  
        /// </summary>  
        public static string GetExtenalIpAddress_0()
        {
            var tempIp = "";
            try
            {
                WebRequest wr = (HttpWebRequest)WebRequest.Create("http://ip.chinaz.com/getip.aspx");
                var stream = wr.GetResponse().GetResponseStream();
                var sr = new StreamReader(stream, Encoding.GetEncoding("gb2312"));
                var all = sr.ReadToEnd();
                //读取网站的数据
                int start = all.IndexOf("[") + 1;
                int end = all.IndexOf("]", start);
                tempIp = all.Substring(start, end - start);
                sr.Close();
                stream.Close();
            }
            catch
            {
                // ignored
            }
            return tempIp;
        }


        public static string GetWebClientIp()
        {
            string CustomerIP = "";

            try
            {
                if (System.Web.HttpContext.Current == null
            || System.Web.HttpContext.Current.Request == null
            || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return "";



                //CDN加速后取到的IP   
                CustomerIP = System.Web.HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP) && IsIP(CustomerIP))
                {
                    return CustomerIP;
                }

                CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];


                if (!String.IsNullOrWhiteSpace(GetRealIP(CustomerIP)) && IsIP(CustomerIP))
                    return CustomerIP;

                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (CustomerIP == null)
                        CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

                }

                if (string.Compare(CustomerIP, "unknown", true) == 0)
                    CustomerIP = System.Web.HttpContext.Current.Request.UserHostAddress;

                if (!IsIP(CustomerIP))
                {
                    CustomerIP = "127.0.0.1";
                }
            }
            catch { }

            return CustomerIP;
        }

        public static string GetRealIP(string CustomerIP)
        {
            string result = String.Empty;

            result = CustomerIP;

            //可能有代理   
            if (!string.IsNullOrWhiteSpace(result))
            {
                //没有"." 肯定是非IP格式  
                if (result.IndexOf(".") == -1)
                {
                    result = null;
                }
                else
                {
                    //有","，估计多个代理。取第一个不是内网的IP。  
                    if (result.IndexOf(",") != -1)
                    {
                        result = result.Replace(" ", string.Empty).Replace("\"", string.Empty);

                        string[] temparyip = result.Split(",;".ToCharArray());

                        if (temparyip != null && temparyip.Length > 0)
                        {
                            for (int i = 0; i < temparyip.Length; i++)
                            {
                                //找到不是内网的地址  
                                if (IsIP(temparyip[i])
                                    && temparyip[i].Substring(0, 3) != "10."
                                    && temparyip[i].Substring(0, 7) != "192.168"
                                    && temparyip[i].Substring(0, 7) != "172.16.")
                                {
                                    return temparyip[i];
                                }
                            }
                        }
                    }
                    //代理即是IP格式  
                    else if (IsIP(result))
                    {
                        return result;
                    }
                    //代理中的内容非IP  
                    else
                    {
                        result = "";
                    }
                }
            }


            return result;
        }

        /// <summary>
        /// 检查IP地址格式
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");

        }

        public static bool IsValid(HttpContext context)
        {
            if (context.Request.Browser.Crawler) return false;
            string key = userIP;

            int hit = (Int32)(context.Cache[key] ?? 0);
            if (hit > 9) return false;
            else hit++;

            if (hit == 1)
            {
                context.Cache.Add(key, hit, null, DateTime.Now.AddMinutes(DURATION), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);

            }
            else
            {
                context.Cache[key] = hit;
            }
            return true;
        }


    }
}