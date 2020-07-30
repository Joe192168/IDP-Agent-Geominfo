using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IDP_Agent_Geominfo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //调用监听器方法
            AppListerner();
        }

        //根据ip和端口来监听，并接收参数启动应用程序
        private static void AppListerner()
        {
            HttpListener listerner = new HttpListener();
            while (true)
            {
                try
                {
                    listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;//指定身份验证 Anonymous匿名访问
                    listerner.Prefixes.Add("http://127.0.0.1:19090/");
                    listerner.Start();
                }
                catch (Exception ex)
                {
                    CustomeInstaller.Logger(string.Format("服务启动失败...异常：", ex));
                    break;
                }
                CustomeInstaller.Logger("服务器启动成功.......");

                //线程池
                int maxThreadNum = 0;
                int portThreadNum = 0;
                int minThreadNum = 0;
                ThreadPool.GetMaxThreads(out maxThreadNum, out portThreadNum);
                ThreadPool.GetMinThreads(out minThreadNum, out portThreadNum);
                CustomeInstaller.Logger(string.Format("最大线程数：{0}", maxThreadNum));
                CustomeInstaller.Logger(string.Format("最小空闲线程数：{0}", minThreadNum));
                CustomeInstaller.Logger("\n等待客户连接中。。。。");
                while (true)
                {
                    //等待请求连接
                    //没有请求则GetContext处于阻塞状态
                    HttpListenerContext ctx = listerner.GetContext();
                    //处理跨域问题
                    ctx.Response.ContentType = "application/json";
                    ctx.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(TaskProc), ctx);
                }
                listerner.Stop();
            }
        }

        //执行任务
        private static void TaskProc(object o)
        {
            HttpListenerContext ctx = (HttpListenerContext)o;
            try
            {
                HttpListenerRequest request = ctx.Request;
                //请求类型
                String _method = request.HttpMethod;

                ctx.Response.StatusCode = 200;//设置返回给客服端http状态代码
                //处理跨域问题
                ctx.Response.ContentType = "application/json";
                ctx.Response.AppendHeader("Access-Control-Allow-Methods", "*");
                // HttpListenerResponse response = ctx.Response;
                //exe执行路径参数
                string executePath = "";
                //idToken信息
                string idToken = "";
                //程序名称
                string softName = "";
                //协议类型
                string agreementType = "";
                //账户参数
                string accountParameters = "";
                //自定义参数
                string transferParam = "";
                if (request.HttpMethod == "OPTIONS")
                {
                    ctx.Response.AddHeader("Access-Control-Allow-Headers", "*");
                }
                else
                {
                    if ("POST".Equals(_method))
                    {
                        //接收POST参数
                        Stream stream = ctx.Request.InputStream;
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        String body = reader.ReadToEnd();
                        JObject jo = (JObject)JsonConvert.DeserializeObject(body);
                        //Console.WriteLine("收到POST数据:" + HttpUtility.UrlDecode(body));
                        //执行路径
                        if (jo["executePath"] != null)
                        {
                            executePath = jo["executePath"].ToString();
                        }
                        //获取idToken
                        if (jo["idToken"] != null)
                        {
                            idToken = jo["idToken"].ToString();
                        }
                        //获取程序名称
                        if (jo["softName"] != null)
                        {
                            softName = jo["softName"].ToString();
                        }
                        //获取自定义参数
                        if (jo["transferParam"] != null)
                        {
                            transferParam = jo["transferParam"].ToString();
                        }
                        //获取协议类型
                        if (jo["agreementType"] != null)
                        {
                            agreementType = jo["agreementType"].ToString();
                        }
                        //账号参数
                        if (jo["accountParameters"] != null)
                        {
                            accountParameters = jo["accountParameters"].ToString();
                        }
                    }
                    else
                    {
                        //接收Get参数
                        executePath = ctx.Request.QueryString["executePath"];
                        idToken = ctx.Request.QueryString["idToken"];
                        softName = ctx.Request.QueryString["softName"];
                        softName = ctx.Request.QueryString["softName"];
                        accountParameters = ctx.Request.QueryString["accountParameters"];
                        agreementType = ctx.Request.QueryString["agreementType"];
                        /*string filename = Path.GetFileName(ctx.Request.RawUrl);
                        string userName = HttpUtility.ParseQueryString(filename).Get("userName");//避免中文乱码*/
                        //进行处理
                        CustomeInstaller.Logger("收到数据:" + executePath);
                    }
                }
                //创建进程启动信息实例
                ProcessStartInfo startinfo = new ProcessStartInfo();
                //非标准协议
                if ("non_standard".Equals(agreementType))
                {
                    //传递进exe的参数
                    if (!string.IsNullOrEmpty(accountParameters))
                    {
                        accountParameters = accountParameters.Replace("&", " ");
                        startinfo.Arguments = accountParameters;
                    }
                    //如果程序名称不为空，则从注册表中获取
                    /*if (!string.IsNullOrEmpty(softName))
                    {
                        executePath = CustomeInstaller.GetPath(softName);
                    }*/
                }
                else
                {
                    //传递进exe的参数
                    if (!string.IsNullOrEmpty(transferParam))
                    {
                        transferParam = transferParam.Replace("&", " ");
                        startinfo.Arguments = idToken + " " + transferParam;
                    }
                    else
                    {
                        startinfo.Arguments = idToken;
                    }
                }
                CustomeInstaller.Logger(string.Format("调起程序路径，数据是executPath={0}", executePath));
                //调用的exe的名称
                startinfo.FileName = executePath;
                //设置启动动作,确保以管理员身份运行
                startinfo.Verb = "runas";
                startinfo.UseShellExecute = true;
                
                //如果应启动该进程而不创建包含它的新窗口，则为 true；否则为 false
                startinfo.CreateNoWindow = true;
                //startinfo.RedirectStandardInput = true;
                //startinfo.RedirectStandardOutput = true;
                //启动进程
                Process.Start(startinfo);
                //使用Writer输出http响应代码,UTF8格式
                using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
                {
                    CustomeInstaller.Logger(string.Format("处理结果，数据是executPath={0}", executePath));
                    writer.Write("处理结果，数据是executPath={0}", executePath);
                    writer.Close();
                    ctx.Response.Close();
                }
            }
            catch (Exception ex)
            {
                CustomeInstaller.Logger(string.Format("调起应用程序失败...异常：{0}", ex));
                //使用Writer输出http响应代码,UTF8格式
                using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
                {
                    writer.Write("异常信息：{0}", ex.Message);
                    writer.Close();
                    ctx.Response.Close();
                }
            }

        }

    }
}
