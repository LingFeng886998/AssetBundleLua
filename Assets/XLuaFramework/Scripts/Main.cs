using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using XLua;
using System.IO;
using UnityLua;

namespace XLuaFramework {

    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour {

        private LuaEnv luaenv = new LuaEnv();
        private string AssetBundleUrl = string.Empty;

        void Start() {
            AppConst.Init();
            Debug.LogError(" ---------------------- 标记：2");

            //luaenv.AddLoader(CustomLoader);
            //luaenv.DoString("require('Main')");

            Debug.Log(string.Format("dataPath : {0}", Application.dataPath));
            Debug.Log(string.Format("persistentDataPath : {0}", Application.persistentDataPath));
            Debug.Log(string.Format("CurrentDirectory : {0}", System.Environment.CurrentDirectory));
            //StartCoroutine(LoadConfig(()=> {
            //    XLuaResourceManager.Instance().OnInitialized(AppConst.AssetDir,delegate() {
            //        Debug.Log("AssetBundleManifest 加载完成");
            //        UnltyLuaSceneManager.Instance.LoadScene(SceneEnem.MAIN);
            //    });
            //}));

            StartCoroutine(LoadLocalConfig());

            //TestLuajit();
        }

        private IEnumerator LoadLocalConfig() {
            string configUrl = AppConst.AppContentPath() + "Config.json";

            WWW wWW = new WWW(configUrl);
            yield return wWW;

            if (wWW.error != null)
            {
                Debug.LogWarning("请求本地配置表错误");
                yield return null;
            }

            if (wWW.isDone)
            {
                string data = wWW.text;

                AppConst.ConfigUnityLua = JsonUtility.FromJson<ConfigUnityLua>(data);
                AssetBundleUrl = AppConst.ConfigUnityLua.AssetBundleUrl;
                StartCoroutine(LoadConfig(() =>
                {
                    AppFacade.Instance.StartUp();   //启动游戏
                    //XLuaResourceManager.Instance().OnInitialized(AppConst.AssetDir, delegate ()
                    //{
                    //    Debug.Log("AssetBundleManifest 加载完成");
                    //    UnltyLuaSceneManager.Instance.LoadScene(SceneEnem.MAIN);

                    //});
                }));
                Debug.LogWarning("请求本地配置表成功：" + AppConst.ConfigUnityLua.ConfigUrl);
            }

            //using (UnityWebRequest request = UnityWebRequest.Get(configUrl))
            //{
            //    yield return request.Send();
            //    if (request.isError)
            //    {
            //        Debug.LogWarning("请求本地配置表错误");
            //        yield return null;
            //    }

            //    bool Exists = File.Exists(configUrl);
            //}
        }


        private IEnumerator LoadConfig(Action action) {
            string webUrl = AppConst.ConfigUrl;
            Debug.Log("开始请求资源服务配置");
            using (UnityWebRequest request = UnityWebRequest.Get(webUrl)) {
                yield return request.Send();
                if (request.isError) {
                    Debug.LogWarning("请求配置表错误");
                    yield return null;
                }
                var data = request.downloadHandler.text;

                string ConfigUrl = AppConst.ConfigUnityLua.ConfigUrl;
                string Url = AppConst.ConfigUnityLua.Url;
                string StreamingAssets = AppConst.ConfigUnityLua.StreamingAssets;
                AppConst.ConfigUnityLua = JsonUtility.FromJson<ConfigUnityLua>(data);

                AppConst.ConfigUnityLua.ConfigUrl = ConfigUrl;
                AppConst.ConfigUnityLua.Url = Url;
                AppConst.ConfigUnityLua.StreamingAssets = StreamingAssets;
                AppConst.ConfigUnityLua.AssetBundleUrl = AssetBundleUrl;
                AppConst.UpdateMode = AppConst.ConfigUnityLua.UpdateMode;
                AppConst.DebugMode = AppConst.ConfigUnityLua.DebugMode;
                resUrl = AppConst.ConfigUnityLua.testResUrl;
                Debug.Log(string.Format("资源服务地址：{0}", AppConst.ConfigUnityLua.FileServerURL));
                Debug.Log(string.Format("测试资源地址：{0}", AppConst.ConfigUnityLua.testResUrl));
                
                if (action != null) action();
            }
        }

        private void TestLuajit() {
            luaenv.AddLoader(CustomLoader);
            luaenv.DoString("require('testLuac1')");

            //OnLuajit();
            //SafeDoString(string.Format("require('{0}')", "testjit"));
        }

        public void SafeDoString(string scriptContent, string chunkName = "chunk")
        {
            if (luaenv != null)
            {
                try
                {
                    luaenv.DoString(scriptContent, chunkName);
                }
                catch (System.Exception ex)
                {
                    string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    Debug.LogError(msg, null);
                }
            }
        }

        static bool IsUseLuaFileWithoutBundle = false;//just for debug 
        private static byte[] CustomLoader(ref string filepath) {
            string scriptPath = string.Empty;
            filepath = filepath.Replace(".", "/") + ".lua";

            if (!IsUseLuaFileWithoutBundle)
            {
                scriptPath = Path.Combine(AppConst.LuaAssetsDir, filepath);
                Debug.Log("Load lua script : " + scriptPath);
                string tempPath = Application.dataPath + "/Lua/" + filepath;
                string _temp = Application.platform == RuntimePlatform.WindowsEditor ? tempPath : scriptPath;
                byte[] arr = Util.GetFileBytes(_temp);
                return arr;
            }
            else
            {
                string dataPath = Application.dataPath;
                dataPath = dataPath.Replace("/Assets", "");
                dataPath = dataPath.Replace(AppConst.AppName + "/App/PC/" + AppConst.AppName + "_Data", AppConst.AppName);
                scriptPath = Path.Combine(dataPath + "/Lua/", filepath);
                // Debug.Log("Load lua script : " + scriptPath);
                return Util.GetFileBytes(scriptPath);
            }
        }

        private string PUBLIC_KEY = "BgIAAACkAABSU0ExAAQAAAEAAQBBpoha5okAQ0TzbQKDYFp9hL2b1shPhZc5ZwcCq5SG0k3COwoAHM6n9V7K/rdsYa0Y5MCYFSLH0w3+rfxypqbmOVGA5hJXf6m/DfgRs4ArRBXTh7iikggKvXMIB8YUkW8341yO0IZ8Ba+B2WjwTOycg7UzuEpOL8IJT0FMzLpx8g==";

        //public static string PUBLIC_KEY = "BgIAAACkAABSU0ExAAQAAAEAAQBVDDC5QJ+0uSCJA+EysIC9JBzIsd6wcXa+FuTGXcsJuwyUkabwIiT2+QEjP454RwfSQP8s4VZE1m4npeVD2aDnY4W6ZNJe+V+d9Drt9b+9fc/jushj/5vlEksGBIIC/plU4ZaR6/nDdMIs/JLvhN8lDQthwIYnSLVlPmY1Wgyatw==";

        private void OnLuajit()
        {
            
#if UNITY_EDITOR
            luaenv.AddLoader(new SignatureLoader(PUBLIC_KEY, (ref string filepath) =>
            {
                //filepath = Application.dataPath + "/XLua/Examples/10_SignatureLoader/" + filepath.Replace('.', '/') + ".lua";
                filepath = Application.dataPath + "/Lua/" + filepath.Replace('.', '/') + ".lua";
                if (File.Exists(filepath))
                {
                    return File.ReadAllBytes(filepath);
                }
                else
                {
                    return null;
                }
            }));
#else //为了让手机也能测试
        luaenv.AddLoader(new SignatureLoader(PUBLIC_KEY, (ref string filepath) =>
        {
            filepath = filepath.Replace('.', '/') + ".lua";
            TextAsset file = (TextAsset)Resources.Load(filepath);
            if (file != null)
            {
                return file.bytes;
            }
            else
            {
                return null;
            }
        }));
#endif
            luaenv.DoString(@"require 'hellow'");

            //luaenv.Dispose();
        }

        public IEnumerator TestLoad() {

            string url = AppConst.ConfigUnityLua.Url;
            string url1 = AppConst.DataPath + AppConst.ConfigUnityLua.StreamingAssets;

            if (File.Exists(url1))
            {
                Debug.Log("文件存在url1");
            }
            else {
                Debug.Log("文件不存在url1");
                yield return null;
            }

            if (File.Exists(url))
            {
                Debug.Log("文件存在");
            }
            else {
                Debug.Log("文件不存在");
                yield return null;
            }

            UnityWebRequest webRequest = UnityWebRequest.GetAssetBundle(url);
            yield return webRequest.Send();
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
            Debug.Log("");
        }

        public class AssetBundleUrlInfo {
            public string url;
        }

        private IEnumerator LoadModelFromLocal()
        {
            string _uri = AssetBundleUrl;
            UnityWebRequest request = UnityWebRequest.Get(_uri);
            yield return request.Send();
            if (request.isError)
            {
                Debug.LogWarning("请求配置表错误");
                yield return null;
            }
            var data = request.downloadHandler.text;
            AssetBundleUrlInfo assetBundleUrlInfo = JsonUtility.FromJson<AssetBundleUrlInfo>(data);
            string uri = assetBundleUrlInfo.url;

            string dir = uri;
            UnityWebRequest _request = UnityWebRequest.GetAssetBundle(uri, 0);
            // 任务: 抛出异常如何处理?
            yield return _request.Send();
            try
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(_request);
                AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                string[] m_AllManifest = manifest.GetAllAssetBundles();
                Debug.Log(String.Format("本地模型{0}加载完成", uri));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public string resUrl = "Assets/Art/Prefabs/Task/TaskView";
        public Transform parent;
        public void OnClick()
        {

            //StartCoroutine(TestLoad());
            //StartCoroutine(LoadModelFromLocal());

            StartCoroutine(LoadConfig(
                () => {
                    XLuaResourceManager.Instance().LoadPrefabs(resUrl, delegate (UnityEngine.Object[] objs)
                    {
                        if (objs.Length < 1)
                        {
                            return;
                        }
                        GameObject gameObject = objs[0] as GameObject;
                        if (gameObject == null)
                        {
                            return;
                        }
                        GameObject go = Instantiate(gameObject) as GameObject;
                        if (parent != null)
                        {
                            go.transform.SetParent(parent);
                            go.transform.localPosition = Vector3.zero;
                            go.SetActive(true);
                        }
                    });
                }
             ));

            
        }
    }
}