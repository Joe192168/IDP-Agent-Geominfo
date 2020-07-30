using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace IDP_Agent_Geominfo
{
    [RunInstaller(true)]
    public partial class CustomeInstaller : System.Configuration.Install.Installer
    {
        public CustomeInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 安装完成之后的操作，可以保留安装路径到
        /// 使用跨调用保留并传入 Install、Commit、Rollback和 Uninstall 方法的 IDictionary。
        /// IDictionary savedState
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnAfterInstall(IDictionary savedState)
        {
            //获取自定义安装用户界面上的端口值
            //string portId = this.Context.Parameters["PortId"];

            string path = this.Context.Parameters["targetdir"];
            Logger(string.Format("OnAfterInstall添加 targetdir savedState:{0}", path));
            //开机启动 1、硬编码，2设置Setup Projects的注册表编辑器
            //1、安装完成以后可以把硬编码把该软件写到注册表中，这样可以设置开机启动，
            //2、当然还有另外一种开机启动的方式是可以使用Setup Projects的注册表编辑器的来进行注册
            savedState.Add("savedState", path);
            Assembly asm = Assembly.GetExecutingAssembly();
            string asmpath = asm.Location.Remove(asm.Location.LastIndexOf("\\")) + "\\";
            Logger(string.Format("OnAfterInstall asmpath:{0}", asmpath));
            SetAutoStart(true, "IDP-Agent-Geominfo", asmpath + "IDP-Agent-Geominfo.exe");
            //创建进程启动信息实例
            ProcessStartInfo startinfo = new ProcessStartInfo();
            //调用的exe的名称
            startinfo.FileName = asmpath + "IDP-Agent-Geominfo.exe";
            //设置启动动作,确保以管理员身份运行
            startinfo.Verb = "runas";
            startinfo.UseShellExecute = true;
            //如果应启动该进程而不创建包含它的新窗口，则为 true；否则为 false
            startinfo.CreateNoWindow = true;
            Process.Start(startinfo);//要执行的程序
            base.OnAfterInstall(savedState);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            base.OnBeforeUninstall(savedState);
            Trace.Listeners.Clear();
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(@"C:\IDP-Logs\OnBeforeUninstall.txt"));

            Trace.WriteLine(string.Format("{0} OnBeforeUninstall", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            Process[] processes = Process.GetProcessesByName("IDP-Agent-Geominfo");
            foreach (Process item in processes)
            {
                Trace.WriteLine(string.Format("{0} OnBeforeUninstall 进程：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), item));
                item.Kill();
                item.WaitForExit();
                item.Close();
            }
        }

        /// <summary>
        /// 卸载软件的时候删除多余的文件
        /// </summary>
        /// <param name="savedState"></param>
        protected override void OnAfterUninstall(IDictionary savedState)
        {
            //Install、Commit、Rollback和 Uninstall 方法并不总是在 Installer的同一实例上调用。 
            //例如，你可以使用 Installer 来安装和提交应用程序，然后释放对该 Installer的引用。 
            //稍后，卸载应用程序会创建对 Installer的新引用，这意味着 Uninstall 方法在 Installer的其他实例上调用。 
            //出于此原因，请不要在安装程序中保存计算机的状态。 
            //相反，请使用跨调用保留并传入 Install、Commit、Rollback和 Uninstall 方法的 IDictionary。

            Trace.Listeners.Clear();
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(@"C:\IDP-Logs\OnBeforeUninstall.txt"));

            Trace.WriteLine(string.Format("{0} OnBeforeUninstall", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            var savedStateValue = savedState.Contains("savedState") ? savedState["savedState"] : "未获取到安装的目录";
            Trace.WriteLine(string.Format("OnAfterUninstall从OnAfterInstall获取 savedState，值为:{0}", savedStateValue));
            string path = this.Context.Parameters["targetdir"];
            Trace.WriteLine(string.Format("targetdir:{0}", path));
            Trace.WriteLine(string.Format("开始删除目录：{0}", path));
            if (Directory.Exists(path))
            {
                RemoveSubDirectory(new DirectoryInfo(path));
                Trace.WriteLine(string.Format(@"删除目录：{0} 成功", path));
            }
            Logger("OnAfterUninstall 进入。。。。");
            Trace.WriteLine("OnAfterUninstall  完成了。。。。");
            base.OnAfterUninstall(savedState);
            savedState.Add("xiezai", true);
        }

        protected override void OnCommitted(IDictionary savedState)
        {
            base.OnCommitted(savedState);
            Trace.Listeners.Clear();
            Trace.AutoFlush = true;
            Trace.Listeners.Add(new TextWriterTraceListener(@"C:\IDP-Logs\OnCommitted.txt"));
            Trace.WriteLine(string.Format("{0} OnCommitted", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            if (savedState == null)
            {
                Trace.WriteLine(string.Format("{0} savedState={1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), savedState));
            }
            else
            {
                Trace.WriteLine(string.Format("{0} savedState={1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), savedState));
            }
            var isxizai = savedState.Contains("xiezai") ? savedState["xiezai"] : "";
            var savedStateValue = savedState.Contains("savedState") ? savedState["savedState"] : "未获取到安装的目录";
            Trace.WriteLine(string.Format("{0} isxizai:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), isxizai));
            Trace.WriteLine(string.Format("{0} savedStateValue:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), savedStateValue));
        }

        /// <summary>
        /// 卸载完成后删除多余的文件
        /// </summary>
        /// <param name="uper"></param>
        private static void RemoveSubDirectory(DirectoryInfo directory)
        {
            Logger(string.Format("目录信息 directory:{0}", directory));
            //foreach (FileInfo subFile in uper.GetFiles())
            //{
            //    subFile.Delete();
            //}
            foreach (DirectoryInfo sub in directory.GetDirectories())
            {
                if (sub.GetFiles().Length > 0 || sub.GetDirectories().Length > 0)
                    RemoveSubDirectory(sub);
                sub.Delete(true);
                Logger(string.Format("要删除的目录信息 sub:{0}", sub));
            }
            Logger("目录成功");
        }

        /// <summary>
        /// 将应用程序设为或不设为开机启动
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <param name="appName">应用程序名</param>
        /// <param name="appPath">应用程序完全路径</param>
        public static bool SetAutoStart(bool onOff, string appName, string appPath)
        {
            Logger(string.Format("注册表设置的开机启动项：{0},{1},{2}", onOff, appName, appPath));
            #region MyRegion
            bool isOk = false;
            //如果从没有设为开机启动设置到要设为开机启动
            if (!IsExistKey(appName) && onOff)
            {
                Logger("------设置注册表自动启动----不存在开机启动项，即将添加开机启动项------");
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            //如果从设为开机启动设置到不要设为开机启动
            else if (IsExistKey(appName) && !onOff)
            {
                Logger("------设置注册表自动启动----存在开机启动项，但未开启，即将开启启动项------");
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            return isOk;
            #endregion
        }

        /// <summary>
        /// 判断注册键值对是否存在，即是否处于开机启动状态
        /// </summary>
        /// <param name="keyName">键值名</param>
        /// <returns></returns>
        private static bool IsExistKey(string keyName)
        {
            try
            {
                bool _exist = false;
                RegistryKey local = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                //RegistryKey local = Registry.LocalMachine;
                RegistryKey runs = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runs == null)
                {
                    RegistryKey key2 = local.CreateSubKey("SOFTWARE");
                    RegistryKey key3 = key2.CreateSubKey("Microsoft");
                    RegistryKey key4 = key3.CreateSubKey("Windows");
                    RegistryKey key5 = key4.CreateSubKey("CurrentVersion");
                    RegistryKey key6 = key5.CreateSubKey("Run");
                    runs = key6;
                }
                string[] runsName = runs.GetValueNames();
                foreach (string strName in runsName)
                {
                    if (strName.ToUpper() == keyName.ToUpper())
                    {
                        _exist = true;
                        return _exist;
                    }
                }
                return _exist;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入或删除注册表键值对,即设为开机启动或开机不启动
        /// </summary>
        /// <param name="isStart">是否开机启动</param>
        /// <param name="exeName">应用程序名</param>
        /// <param name="path">应用程序路径带程序名</param>
        /// <returns></returns>
        private static bool SelfRunning(bool isStart, string exeName, string path)
        {
            try
            {
                RegistryKey local = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                //RegistryKey local = Registry.LocalMachine;
                RegistryKey key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                }
                //若开机自启动则添加键值对
                if (isStart)
                {
                    key.SetValue(exeName, path);
                    key.Close();
                    Logger("------设置注册表自动启动----开启----成功------");
                }
                else//否则删除键值对
                {
                    string[] keyNames = key.GetValueNames();
                    foreach (string keyName in keyNames)
                    {
                        if (keyName.ToUpper() == exeName.ToUpper())
                        {
                            key.DeleteValue(exeName);
                            key.Close();
                            Logger("------设置注册表自动启动----关闭----成功------");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger(string.Format("------设置注册表自动启动----异常----原因{0}------", ex));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="content"></param>
        public static void Logger(string content)
        {
            StreamWriter writer = null;
            try
            {
                // 检查文件夹
                string folderPath = @"C:\IDP-Logs\";
                if (false == Directory.Exists(folderPath))
                {
                    //创建文件夹
                    Directory.CreateDirectory(folderPath);
                }
                if (Directory.Exists(folderPath))
                {
                    //存在/成功创建 文件夹
                    folderPath = folderPath + @"\";
                }
                else
                {
                    //无则当前路径创建文件
                    folderPath = folderPath + @"-";
                }
                //写入日志 
                string filePath = string.Format(folderPath + @"logs{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));
                FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(fs);
                writer.WriteLine(string.Format("日志开始时间：{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                //foreach (var item in content)
                //{
                writer.WriteLine(content);
                //}
                writer.WriteLine("****************************************************************");
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// 根据程序名称获取路径
        /// </summary>
        /// <param name="softName"></param>
        /// <returns></returns>
        public static string GetPath(string softName)
        {
            string softPath = null;
            try
            {
                RegistryKey currentKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                Logger(string.Format("获取程序路径，SubKeyCount={0}", currentKey.SubKeyCount));
                RegistryKey key = currentKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\UFH\SHC",true);
                Logger(string.Format("获取程序路径，key={0}", key.SubKeyCount));
                if (key != null)
                {
                    int[] axm = new int[3];
                    string[] maxvalue = key.GetValueNames();
                    //可能存在多个遗留的安装路径，获取最后的安装路径即可
                    Dictionary<int, string> dic = new Dictionary<int, string>();
                    foreach (string valuename in key.GetValueNames())
                    {
                        string[] softwareName = (string[])key.GetValue(valuename, "");
                        //根据软件名称匹配相应的路径
                        if (softwareName[1].Contains(softName))
                        {
                            dic.Add(int.Parse(valuename), softwareName[1]);
                        }
                    }
                    softPath = dic.Values.Max();
                }
            }
            catch (Exception ex)
            {
                Logger(string.Format("获取程序路径失败！，异常：{0}", ex));
                softPath = "获取程序路径失败！";
            }
            return softPath;
        }
    }
}
