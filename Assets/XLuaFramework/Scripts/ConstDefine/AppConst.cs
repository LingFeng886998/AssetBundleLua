using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace XLuaFramework {

    public class ConfigUnityLua {
        public string ConfigUrl;
        public string FileServerURL;
        public string testResUrl;
        public bool DebugMode;
        public bool UpdateMode;
        public string Url;
        public string StreamingAssets;
        public string AssetBundleUrl;
        public ConfigUnityLua() { }
    }

    public class AppConst {

        public static bool DebugMode = true;                       //调试模式-用于内部测试
        /// <summary>
        /// 如果想删掉框架自带的例子，那这个例子模式必须要
        /// 关闭，否则会出现一些错误。
        /// </summary>
        public const bool ExampleMode = false;                       //例子模式 

        /// <summary>
        /// 如果开启更新模式，前提必须启动框架自带服务器端。
        /// 否则就需要自己将StreamingAssets里面的所有内容
        /// 复制到自己的Webserver上面，并修改下面的WebUrl。
        /// </summary>
        public static bool UpdateMode = false;                       //更新模式-默认关闭 
        public static bool LuaByteMode = false;                       //Lua字节码模式-默认关闭 
        public static bool LuaBundleMode = false;                    //Lua代码AssetBundle模式

        public const int TimerInterval = 1;
        public const int GameFrameRate = 30;                        //游戏帧频
        public const int LogLevel = Log.DEBUG;                      // 输出日志级别

        public const string AppName = "UnityLua";               //应用程序名称
        public const string LuaTempDir = "Lua/";                    //临时目录
        public const string AppPrefix = AppName + "_";              //应用程序前缀
        public const string ExtName = ".unity3d";                   //素材扩展名
        public const string AssetDir = "StreamingAssets";           //素材目录 
        //public const string WebUrl = "http://127.0.0.1:8080/StreamingAssets/";      //测试更新地址

        public static string UserId = string.Empty;                 //用户ID
        public static int SocketPort = 0;                           //Socket服务器端口
        public static string SocketAddress = string.Empty;          //Socket服务器地址

        public static string FrameworkRoot {
            get {
                return Application.dataPath + "/Lua/";
            }
        }

        public static void Init() {
            if (Application.isMobilePlatform) {
                DebugMode = false;
                UpdateMode = true;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                DebugMode = true;
                UpdateMode = false;
            }
        }

        private static string m_DataPath = null;

        /// <summary>
        /// 各个平台assetbundle存放的地方
        /// </summary>
        public static string DataPath {
            get
            {
                string gameName = AppName.ToLower();
                if (Application.isMobilePlatform)
                {
                    if (m_DataPath == null)
                    {
                        m_DataPath = Application.persistentDataPath + "/" + gameName + "/";
                    }
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    if (m_DataPath == null)
                    {
                        int i = Application.dataPath.LastIndexOf('/');
                        m_DataPath = Application.dataPath.Substring(0, i + 1) + gameName + "/";
                    }
                }
                else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    m_DataPath = AppDataPath + "/" + AssetDir + "/";
                }
                else {
                    if (m_DataPath == null)
                        m_DataPath = "c:/" + gameName + "/";
                }
                return m_DataPath;
            }
        }


        public static string AppDataPath {
            get {
                string dataPath = Application.dataPath;
                bool isWindows = (Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.WindowsPlayer);
                if (isWindows)
                {
                    dataPath = dataPath.Replace("/Assets", "/AssetBundle");
                }
                return dataPath;
            }
        }

        private static string m_luaAssetsDir = string.Empty;
        public static string LuaAssetsDir {
            get {

                if (m_luaAssetsDir != string.Empty) {
                    return m_luaAssetsDir;
                }

                if (DebugMode)
                {
                    m_luaAssetsDir = AppDataPath + "/Lua/";
                }
                else {
                    m_luaAssetsDir = DataPath + "Lua/";
                }

                return m_luaAssetsDir;
            }
        }


        private static string relative_Path = null;
        /// <summary>
        /// 相对路径
        /// </summary>
        /// <returns></returns>
        public static string GetRelativePath()
        {
            if (!UpdateMode && !DebugMode)
            {
                if (relative_Path == null)
                {
                    if (Application.platform == RuntimePlatform.WindowsPlayer || 
                        Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                        relative_Path = DataPath;
                    else
                        relative_Path = "jar:file://" + System.Environment.CurrentDirectory.Replace("\\", "/") + "/" + AssetDir + "/";
                }
                return relative_Path;
            }
            else
            {
                if (relative_Path == null)
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        relative_Path = "jar:file://" + DataPath.Replace("\\", "/");
                    }else
                    {
                        relative_Path = "file:///" + DataPath.Replace("\\", "/");
                    }
                }
                return relative_Path;
            }
        }

        //打包后的资源路径,注意不是运行时使用的!
        private static string streamingAssetsTargetPath = string.Empty;
        public static string StreamingAssetsTargetPath {
            get {
                if (streamingAssetsTargetPath != string.Empty)
                    return streamingAssetsTargetPath;

                streamingAssetsTargetPath = GetStreamingAssetsTargetPathByPlatform();

                return streamingAssetsTargetPath;
            }
        }

        /// <summary>
        /// 打assetbundle的存放路径，不是手机运行的资源路径
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private static string GetStreamingAssetsTargetPathByPlatform() {
            string dataPath = Application.dataPath;
            switch (Application.platform) {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WebGLPlayer:
                    dataPath = dataPath + "/" + AssetDir;
                    break;
                case RuntimePlatform.Android:
                    dataPath = dataPath + "/StreamingAssetsAndroid";
                    break;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    dataPath = dataPath + "/StreamingAssetsIOS";
                    break;
                default:
                    dataPath = dataPath + "StreamingAssetsDefault";
                    break;
            }
            return dataPath;
        }

        public static string m_ConfigUrl {
            get {
                if (ConfigUnityLua != null)
                {
                    return ConfigUnityLua.ConfigUrl;
                }
                else {
                     return "http://172.27.224.231/config/";
                }
            }
        }
        public static string ConfigUrl{
            get {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsWebPlayer:
                        //return Application.streamingAssetsPath + "/config/" + "Config.json";
                        return m_ConfigUrl + "windowConfig.json";
                    case RuntimePlatform.Android:
                        //return Application.streamingAssetsPath + "/config/" + "Config.json";
                        return m_ConfigUrl + "AndroidConfig.json";
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        //return Application.streamingAssetsPath + "/config/" + "Config.json";
                        return m_ConfigUrl + "IOSConfig.json";
                    default:
                        //return Application.streamingAssetsPath + "/config/" + "AndroidConfig.json";
                        return m_ConfigUrl + "AndroidConfig.json";
                }
            }
        }

        //热更新用的文件服务器地址,可以自己用IIS或Apache搭建
        private static string webUrl = string.Empty;
        public static string WebUrl
        {
            get
            {
                if (webUrl != string.Empty)
                    return webUrl;

                if (m_ConfigUnityLua == null) {
                    Debug.LogError("文件服务器地址错误");
                    return string.Empty;
                }

                var fileUrl = ConfigUnityLua.FileServerURL;
                Debug.Log("fileUrl : " + fileUrl);
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WebGLPlayer)
                    webUrl = fileUrl + "/WindowsStreamingAssets/";
                else if (Application.platform == RuntimePlatform.Android)
                    webUrl = fileUrl + "/AndroidStreamingAssets/";
                else if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
                    webUrl = fileUrl + "/IOSStreamingAssets/";
                else
                    Debug.Log("Unspport System!");
                return webUrl;
            }
        }


        private static ConfigUnityLua m_ConfigUnityLua = new ConfigUnityLua();
        public static ConfigUnityLua ConfigUnityLua {
            get {
                if (m_ConfigUnityLua == null)
                {
                    m_ConfigUnityLua = new ConfigUnityLua();
                }
                return m_ConfigUnityLua;
            }

            set {
                m_ConfigUnityLua = value;
            }
        }

        /// <summary>
        /// app数据目录
        /// </summary>
        /// <returns></returns>
        public static string AppContentPath()
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = AppConst.AppDataPath + "/" + AppConst.AssetDir + "/";
                    break;
            }

            return path;
        }

    }
}