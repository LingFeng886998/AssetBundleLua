using UnityEngine;
using XLua;
using System.Collections.Generic;
using System;
using UObject = UnityEngine.Object;
using System.Collections;
using UnityEngine.Networking;
using XLuaFramework;

namespace UnityLua
{
    public class AssetBundleInfo {
        public AssetBundle m_AssetBundle;
        /// <summary>
        /// 引用數量
        /// </summary>
        public int m_ReferencedCount;
        public AssetBundleInfo(AssetBundle AssetBundle) {
            m_AssetBundle = AssetBundle;
            m_ReferencedCount = 0;
        }
    }


    public class LoadAssetRequest {
        public Type assetType;
        public string[] assetNames;
        public LuaFunction luaFunc;
        public Action<UObject[]> sharpFunc;
    }

    [LuaCallCSharp]
    class XLuaResourceManager : Manager
    {

        private static XLuaResourceManager _instance;
        public static XLuaResourceManager Instance() { return _instance; }

        

        /// <summary>
        /// 已经加载过的assetBundle
        /// </summary>
        private Dictionary<string, AssetBundleInfo> m_LoadedAssetBundle = new Dictionary<string, AssetBundleInfo>();

        /// <summary>
        /// 正在加载中的assetBundle请求
        /// </summary>
        private Dictionary<string, List<LoadAssetRequest>> m_LoadRequest = new Dictionary<string, List<LoadAssetRequest>>();

        /// <summary>
        /// assetBundle对应的依赖列表
        /// </summary>
        private Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

        /// <summary>
        /// 其实是全部bundle的名字
        /// </summary>
        private string[] m_AllManifest = null;

        private AssetBundleManifest m_AssetBundleManifest = null;

        public object UnityWebRequestAssetBundle { get; private set; }
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.05f);

        private void Awake()
        {
            Debug.Log("xlua resource manager start");
            _instance = this;

        }

        private string _m_BaseDownloadingUrl = string.Empty;
        //TODO
        private string m_BaseDownloadingUrl {
            get {
                if (_m_BaseDownloadingUrl == string.Empty)
                {
                    string file = Application.platform == RuntimePlatform.Android ? "file://" : string.Empty;

                    _m_BaseDownloadingUrl = file + AppConst.DataPath;
                }

                return _m_BaseDownloadingUrl;
            }
        }


        public void OnInitialized(string manifest,Action initOK)
        {
            Debug.Log("ResourceManager:Initialize() m_BaseDownloadingURL:" + m_BaseDownloadingUrl);
            if (AppConst.DebugMode)
            {
                if (initOK != null) initOK();
                return;
            }
            LoadAsset<AssetBundleManifest>(manifest,new string[] { "AssetBundleManifest" },delegate(UObject[] objs) {
                if (objs.Length > 0)
                {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();

                }
                if (initOK != null) initOK();
                else
                {
                    Debug.LogError("ResourceManager:Initialize failed!");
                }
            });

        }


        private string GetRealAssetPath(string abName) {
            if (abName.Contains("/") || abName.Equals(AppConst.AssetDir))
            {
                return abName;
            }

            abName = abName.ToLower();

            for (int i = 0; i < m_AllManifest.Length; i++)
            {
                string path = m_AllManifest[i];
                int lastIndex = path.LastIndexOf("/");
                string pathName = path.Substring(lastIndex + 1);
                if (pathName.Equals(abName)) {
                    return m_AllManifest[i];
                }
            }
            return string.Empty;
        }

        private string GetAbNameByPath(string filePath) {
            string abName = string.Empty;
            if (abName == filePath)
            {
                return abName;
            }
            //Assets/Art/Prefabs/Task/TaskView.prefab
            int index = filePath.LastIndexOf("/") + 1;
            abName = filePath.Substring(index);

            string[] names = filePath.Split('/');
            if (names.Length > 1)
            {
                abName = names[names.Length - 2];
            }
            else {
                abName = names[0];
            }
            return abName.ToLower();
        }

        public void LoadPrefabs(string filePath, Action<UObject[]> action, LuaFunction luaFunction = null) {
            LoadAsset<GameObject>(filePath,action,luaFunction);
        }

        public void LoadAsset<T>(string filePath,Action<UObject[]> action,LuaFunction luaFunction = null)where T:UObject {

#if UNITY_EDITOR
            if (AppConst.DebugMode)
            {
                StartCoroutine(LoadAssetInLocal<T>(filePath, action, luaFunction));
                return;
            }
#endif
            string abName = GetAbNameByPath(filePath);
            Debug.Log(" *abName* : " + abName);
            if (abName == string.Empty)
            {
                Debug.LogError("资源名称错误：" + filePath);
                return;
            }
            string resName = filePath.ToLower();
            this.LoadAsset<T>(abName, new string[] { resName }, action, luaFunction);
        }


        IEnumerator LoadAssetInLocal<T>(string filePatn,Action<UObject[]> action = null,LuaFunction luaFunction = null)where T:UObject {
            yield return waitForSeconds;
#if UNITY_EDITOR
            T res = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(filePatn);
            if (res != null)
            {
                if (action != null)
                {
                    List<UObject> objects = new List<UObject>();
                    objects.Add(res);
                    action(objects.ToArray());
                }
                if (luaFunction != null)
                {
                    List<T> list = new List<T>();
                    list.Add(res);
                    object[] args = new object[] { list.ToArray() };
                    luaFunction.Call(args);
                    luaFunction.Dispose();
                    luaFunction = null;
                }
            }
            else {
                Debug.LogError(string.Format("本地资源加载失败：{0}",filePatn));
            }
#endif
        }



        private void LoadAsset<T>(string abName,string[] assetNames,Action<UObject[]> action = null,LuaFunction luaFunction = null) where T : UObject
        {
            abName = GetRealAssetPath(abName);

            Debug.Log(" -- GetRealAssetPath : " +  abName);

            if (abName == string.Empty)
            {
                if (action != null) {
                    action(null);
                }

                if (luaFunction != null)
                {
                    luaFunction.Call(null);
                    luaFunction.Dispose();
                    luaFunction = null;
                }
            }


            LoadAssetRequest request = new LoadAssetRequest();
            request.assetNames = assetNames;
            request.assetType = typeof(T);
            request.luaFunc = luaFunction;
            request.sharpFunc = action;

            List<LoadAssetRequest> requests;
            if (!m_LoadRequest.TryGetValue(abName, out requests))
            {
                requests = new List<LoadAssetRequest>();
                requests.Add(request);
                m_LoadRequest.Add(abName,requests);
                StartCoroutine(OnLoadAsset<T>(abName));
            }
            else {
                requests.Add(request);
            }
        }

        IEnumerator OnLoadAsset<T>(string abName) where T : UObject {
            AssetBundleInfo assetBundleInfo = GetLoadedAssetBundle(abName);
            if (assetBundleInfo == null)
            {
                yield return StartCoroutine(OnLoadAssetBundle(abName,typeof(T)));
                assetBundleInfo =  GetLoadedAssetBundle(abName);
                if (assetBundleInfo == null)
                {
                    m_LoadRequest.Remove(abName);
                    Debug.LogError("OnLoadAsset failed!--->>>" + abName);
                    yield break;
                }
            }

            List<LoadAssetRequest> list = null;
            if (!m_LoadRequest.TryGetValue(abName,out list))
            {
                m_LoadRequest.Remove(abName);
                yield break;
            }

            for (int i = 0; i < list.Count; i++)
            {
                LoadAssetRequest request = list[i];
                string[] names = request.assetNames;
                List<UObject> result = new List<UObject>();
                AssetBundle bundle = assetBundleInfo.m_AssetBundle;
                for (int j = 0; j < names.Length; j++)
                {
                    string assetPath = names[j];
                    AssetBundleRequest assetBundleRequest = bundle.LoadAssetAsync(assetPath,typeof(T));
                    yield return assetBundleRequest;
                    result.Add(assetBundleRequest.asset);
                }

                if (request.luaFunc != null)
                {
                    request.luaFunc.Call((object)result.ToArray());
                    request.luaFunc.Dispose();
                    request.luaFunc = null;
                }

                if (request.sharpFunc != null)
                {
                    request.sharpFunc(result.ToArray());
                    request.sharpFunc = null;
                }
            }
            m_LoadRequest.Remove(abName);
        }

        /// <summary>
        /// 开始加载资源
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerator OnLoadAssetBundle(string abName,Type type) {

            string url = m_BaseDownloadingUrl + abName;
            UnityWebRequest webRequest = null;
            if (type == typeof(AssetBundleManifest))
            {
                //url = url + ".manifest";
                webRequest = UnityWebRequest.GetAssetBundle(url,0);
            }
            else {
                Hash128 hash128 = new Hash128();
                if (m_AssetBundleManifest != null)
                {
                    string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string dependenciesName = dependencies[i];
                        AssetBundleInfo assetBundleInfo;
                        if (m_LoadedAssetBundle.TryGetValue(dependenciesName, out assetBundleInfo))
                        {
                            assetBundleInfo.m_ReferencedCount++;
                        }
                        else if (m_LoadRequest.ContainsKey(dependenciesName))
                        {
                            yield return WaitForAssetBundleLoaded(dependenciesName);
                        }
                        else {
                            m_LoadRequest.Add(dependenciesName,null);
                            yield return StartCoroutine(OnLoadAssetBundle(dependenciesName,type));
                            m_LoadRequest.Remove(dependenciesName);
                        }
                    }
                    hash128 = m_AssetBundleManifest.GetAssetBundleHash(abName);
                }
                webRequest = UnityWebRequest.GetAssetBundle(url,hash128,0);
            }
            yield return webRequest.Send();
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
            if (assetBundle != null)
            {
                m_LoadedAssetBundle.Add(abName, new AssetBundleInfo(assetBundle));
            }
            else {
                Debug.LogWarning(string.Format("加载失败：{0} ：{1}",abName,url));
            }
        }

        IEnumerator WaitForAssetBundleLoaded(string abName)
        {
            float tryTime = 30.0f;
            float tryDuration = 0.03f;
            while (true)
            {
                yield return new WaitForSeconds(tryDuration);
                if (m_LoadedAssetBundle.ContainsKey(abName))
                {
                    break;
                }
                tryTime -= tryDuration;
                if (tryTime <= 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 获取已经加载的asssetBundle
        /// </summary>
        /// <param name="abName"> asssetBundle 名字</param>
        /// <returns></returns>
        AssetBundleInfo GetLoadedAssetBundle(string abName) {

            AssetBundleInfo assetBundleInfo;
            m_LoadedAssetBundle.TryGetValue(abName,out assetBundleInfo);

            if (assetBundleInfo == null)
                return null;

            //没有依赖需要加载，直接返回asssetBundle
            string[] dependencies;
            if (!m_Dependencies.TryGetValue(abName,out dependencies))
            {
                return assetBundleInfo;
            }

            //确定所有依赖是否被加载
            foreach (var dependency in dependencies)
            {
                AssetBundleInfo depAssetBundleInfo;
                if (!m_LoadedAssetBundle.TryGetValue(dependency,out depAssetBundleInfo))
                {
                    return null;
                }
            }

            //已经被加载并且所以依赖也被加载
            return assetBundleInfo;
        }

        /// <summary>
        /// 卸载
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        public void UnLoadAssetBundle(string abName , bool unloadAllLoadedObjects = false) {

        }

        /// <summary>
        /// 卸载依赖
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        private void UnloadDependencies(string abName, bool unloadAllLoadedObjects = false) {
            string[] dependencies;
            if (!m_Dependencies.TryGetValue(abName,out dependencies))
            {
                return;
            }

            foreach (var depName in dependencies)
            {
                UnloadAssetBundleInternal(depName, unloadAllLoadedObjects);
            }
        }

        /// <summary>
        /// 执行卸载
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="unloadAllLoadedObjects"></param>
        private void UnloadAssetBundleInternal(string abName,bool unloadAllLoadedObjects = false) {
            AssetBundleInfo assetBundle = GetLoadedAssetBundle(abName);
            if (assetBundle == null)
            {
                return;
            }

            if (--assetBundle.m_ReferencedCount <= 0)
            {
                if (m_LoadRequest.ContainsKey(abName))
                {
                    return;
                }
                assetBundle.m_AssetBundle.Unload(unloadAllLoadedObjects);
                m_LoadedAssetBundle.Remove(abName);
            }

        }

    }
}
