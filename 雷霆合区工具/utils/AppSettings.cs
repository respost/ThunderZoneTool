using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Configuration;

namespace 雷霆合区工具
{
    /// <summary>
    /// App.config配置类
    /// </summary>
    class AppSettings
    {
        //指定config文件读取
        private static string file = System.Windows.Forms.Application.ExecutablePath;
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(file);
        /// <summary>
        /// 根据连接串名字connectionName返回数据连接字符串 
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>

        public static string GetConnectionString(string connectionName)
        {
            //指定config文件读取
            string connectionString =
                config.ConnectionStrings.ConnectionStrings[connectionName].ConnectionString.ToString();
            return connectionString;
        }
        ///<summary> 
        ///更新连接字符串  
        ///</summary> 
        ///<param name="newName">连接字符串名称</param> 
        ///<param name="newConString">连接字符串内容</param> 
        ///<param name="newProviderName">数据提供程序名称</param> 
        public static void setConnectionString(string name, string conString, string providerName)
        {
            bool exist = false; //记录该连接串是否已经存在  
            //如果要更改的连接串已经存在  
            if (config.ConnectionStrings.ConnectionStrings[name] != null)
                exist = true;
            // 如果连接串已存在，首先删除它  
            if (exist)
                config.ConnectionStrings.ConnectionStrings.Remove(name);
            //新建一个连接字符串实例  
            ConnectionStringSettings mySettings =
                new ConnectionStringSettings(name, conString, providerName);
            // 将新的连接串添加到配置文件中.  
            config.ConnectionStrings.ConnectionStrings.Add(mySettings);
            // 保存对配置文件所作的更改  
            config.Save(ConfigurationSaveMode.Modified);
            // 强制重新载入配置文件的ConnectionStrings配置节  
            ConfigurationManager.RefreshSection("connectionStrings");
        }
        ///<summary> 
        ///获取*.exe.config文件中appSettings配置节点的value值  
        ///</summary> 
        ///<param name="strKey"></param> 
        ///<returns></returns> 
        public static string GetValue(string strKey)
        {
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == strKey)
                    return config.AppSettings.Settings[strKey].Value.ToString();
            }
            return null;
        }
        ///<summary>  
        ///设置*.exe.config文件中appSettings节点的值 
        ///</summary>  
        ///<param name="key"></param>  
        ///<param name="newValue"></param>  
        public static void SetValue(string key, string value)
        {
            //向配置文件中添加键值对，有则修改，无则添加
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                config.AppSettings.Settings.Add(key, value);
                config.Save();
            }
            else
            {
                config.AppSettings.Settings[key].Value = value;
                config.Save();
            }
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
