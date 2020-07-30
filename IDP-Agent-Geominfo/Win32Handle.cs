using System;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace IDP_Agent_Geominfo
{
    public class Win32Handle
    {
        public static IntPtr GetHiveHandle(RegistryHive hive)
        {
            IntPtr preexistingHandle = IntPtr.Zero;

            IntPtr HKEY_CLASSES_ROOT = new IntPtr(-2147483648);
            IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);
            IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);
            IntPtr HKEY_USERS = new IntPtr(-2147483645);
            IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(-2147483644);
            IntPtr HKEY_CURRENT_CONFIG = new IntPtr(-2147483643);
            IntPtr HKEY_DYN_DATA = new IntPtr(-2147483642);
            switch (hive)
            {
                case RegistryHive.ClassesRoot: preexistingHandle = HKEY_CLASSES_ROOT; break;
                case RegistryHive.CurrentUser: preexistingHandle = HKEY_CURRENT_USER; break;
                case RegistryHive.LocalMachine: preexistingHandle = HKEY_LOCAL_MACHINE; break;
                case RegistryHive.Users: preexistingHandle = HKEY_USERS; break;
                case RegistryHive.PerformanceData: preexistingHandle = HKEY_PERFORMANCE_DATA; break;
                case RegistryHive.CurrentConfig: preexistingHandle = HKEY_CURRENT_CONFIG; break;
                case RegistryHive.DynData: preexistingHandle = HKEY_DYN_DATA; break;
            }
            return preexistingHandle;
        }

        /// <summary>
        /// 用于32位程序访问64位注册表
        /// </summary>
        /// <param name="hive">根级别的名称</param>
        /// <param name="keyName">不包括根级别的名称</param>
        /// <param name="valueName">项名称</param>
        /// <param name="view">注册表视图</param>
        /// <returns>key</returns>
        public static RegistryKey GetValueWithRegView(RegistryHive hive, string keyName, string valueName, RegistryView view)
        {

            SafeRegistryHandle handle = new SafeRegistryHandle(GetHiveHandle(hive), true);//获得根节点的安全句柄

            RegistryKey subkey = RegistryKey.FromHandle(handle, view).OpenSubKey(keyName);//获得要访问的键

            RegistryKey key = RegistryKey.FromHandle(subkey.Handle, view);//根据键的句柄和视图获得要访问的键

            //key.GetValue(valueName);//获得键下指定项的值
            return key;
        }
        /// <summary>
        /// 用于32位的程序设置64位的注册表
        /// </summary>
        /// <param name="hive">根级别的名称</param>
        /// <param name="keyName">不包括根级别的名称</param>
        /// <param name="valueName">项名称</param>
        /// <param name="value">值</param>
        /// <param name="kind">值类型</param>
        /// <param name="view">注册表视图</param>
        public static void SetValueWithRegView(RegistryHive hive, string keyName, string valueName, object value, RegistryValueKind kind, RegistryView view)
        {
            SafeRegistryHandle handle = new SafeRegistryHandle(GetHiveHandle(hive), true);

            RegistryKey subkey = RegistryKey.FromHandle(handle, view).OpenSubKey(keyName, true);//需要写的权限,这里的true是关键。0227更新

            RegistryKey key = RegistryKey.FromHandle(subkey.Handle, view);

            key.SetValue(valueName, value, kind);
        }
    }
}
