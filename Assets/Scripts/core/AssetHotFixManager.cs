using System.Collections.Generic;
using System.Collections;
using XLuaFramework;
using UnityEngine;
using System;
using System.IO;

namespace UnityLua
{
    public class AssetHotFixManager:Manager
    {
        /// <summary>
        /// 已经下载的资源
        /// </summary>
        private List<string> downLoadFiles = new List<string>();

        private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(0.1f);

        public static AssetHotFixManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void CheckExtractResource(Action<float> on_update,Action on_ok) {
            bool isExists = Directory.Exists(AppConst.DataPath) && Directory.Exists(AppConst.DataPath + "lua/") && File.Exists(AppConst.DataPath + "files.txt");
            Debug.Log("CheckExtraceResource AppConfig.DataPath:" + AppConst.DataPath + " isExists:" + isExists.ToString() + " debugmode : " + AppConst.DebugMode.ToString());

            if (isExists || AppConst.DebugMode)
            {
                if(on_ok != null)on_ok();
                return;
            }

            StartCoroutine(OnExtractResource(on_ok));
        }


        public IEnumerator OnExtractResource(Action onAction) {
            string dataPath = AppConst.DataPath;
            string resPath = AppConst.AppContentPath();

            if (Directory.Exists(dataPath))
            {
                Directory.Delete(dataPath, true);
            }
            Directory.CreateDirectory(dataPath);
            string fileName = "files.txt";
            string inFile = resPath + fileName;
            string outFile = dataPath + fileName;
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            string mes = "正在解包文件:>files.txt";
            Debug.Log(mes);
            Debug.Log(string.Format("infile:{0}",inFile));
            Debug.Log(string.Format("outFile:{0}", outFile));

            if (Application.platform == RuntimePlatform.Android)
            {
                WWW www = new WWW(inFile);
                yield return www;

                if (www.isDone)
                {
                    File.WriteAllBytes(outFile,www.bytes);
                }
                yield return 0;
            }
            else {
                File.Copy(inFile,outFile);
            }

            yield return new WaitForEndOfFrame();

            //释放所有文件到数据目录
            string[] files = File.ReadAllLines(outFile);
            foreach (var file in files)
            {
                string[] fls = file.Split('|');
                inFile = resPath + fls[0];
                outFile = dataPath + fls[0];
                Debug.Log(string.Format("正在解包文件:>{0}", fls[0]));
                Debug.Log(string.Format("正在解包文件路径:>{0}", inFile));

                string dir = Path.GetDirectoryName(outFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (Application.platform == RuntimePlatform.Android)
                {
                    WWW _www = new WWW(inFile);
                    yield return _www;
                    if (_www.isDone)
                    {
                        File.WriteAllBytes(outFile, _www.bytes);
                    }
                    yield return 0;
                }
                else {
                    if (File.Exists(outFile))
                    {
                        File.Delete(outFile);
                    }

                    File.Copy(inFile,outFile,true);
                }
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("解包完成！！！！");
            yield return m_WaitForSeconds;
            if (onAction != null)
            {
                onAction();
            }
        }

        /// <summary>
        /// 更新资源
        /// </summary>
        /// <param name="on_update"> 进度 名称</param>
        /// <param name="actionOk"></param>
        public void UpdateResource(Action<float,string> on_update,Action<string> actionOk) {
            if (AppConst.UpdateMode)
            {
                StartCoroutine(OnUpdateResource(on_update, actionOk));
            }
            else {
                actionOk("");
            }
        }

        public IEnumerator OnUpdateResource(Action<float, string> on_update, Action<string> actionOk) {
            Debug.Log("OnUpdateResource() AppConfig.UpdateMode:" + AppConst.UpdateMode.ToString());
            string dataPath = AppConst.DataPath;
            string url = AppConst.WebUrl;
            string mes = string.Empty;
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            string listUrl = url + "files.txt?v=" + random;
            Debug.LogWarning("OnUpdateResource() LoadUpdate---->>>" + listUrl);
            Debug.LogWarning("下载最新资源列表文件...");
            on_update(0.01f, "下载最新资源列表文件...");
            WWW www = new WWW(listUrl);
            yield return www;
            if (www.error != null)
            {
                actionOk("下载最新资源列表文件失败!");
                yield break;
            }
            Debug.Log("is data path exist : " + Directory.Exists(dataPath) + " small:" + Directory.Exists(dataPath.Trim()));
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            Debug.LogWarning("更新资源列表文件...");
            on_update(0.1f, "更新资源列表文件...");
            File.WriteAllBytes(dataPath + "files.txt", www.bytes);
            string filesText = www.text;
            string[] files = filesText.Split('\n');
            on_update(0.15f, "开始下载最新的资源文件...");
            float percent = 0.15f;
            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');
                string f = keyValue[0];
                on_update(0.15f + 0.85f * ((i + 1) / files.Length), "下载文件:" + f);
                string localfile = (dataPath + f).Trim();
                //C:\MyDir\MySubDir\myfile.ext ---- C:\MyDir\MySubDir
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string fileUrl = url + f + "?v=" + random;
                bool canUpdate = !File.Exists(localfile);
                if (!canUpdate)
                {
                    string remoteMd5 = keyValue[1].Trim();
                    string localMd5 = Util.md5file(localfile);
                    canUpdate = !remoteMd5.Equals(localMd5);
                    if (canUpdate)
                    {
                        File.Delete(localfile);
                    }
                }
                if (canUpdate)
                {   //本地缺少文件
                    Debug.Log(fileUrl);
                    //这里都是资源文件，用线程下载
                    BeginDownload(fileUrl, localfile);
                    while (!(IsDownOK(localfile)))
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            yield return new WaitForEndOfFrame();

            mes = "更新完成!!";
            Debug.Log(mes);
            actionOk("");
        }

        void BeginDownload(string url, string file)
        {     //线程下载
            object[] param = new object[2] { url, file };
            Debug.LogWarning(string.Format("更新資源url：{0}", url));
            Debug.LogWarning(string.Format("更新資源file：{0}", file));
            ThreadEvent ev = new ThreadEvent();
            ev.Key = NotiData.UPDATE_DOWNLOAD;
            ev.evParams.AddRange(param);
            UnityLuaThreadManager.Instance.AddEvent(ev, OnThreadCompleted);   //线程下载
        }

        bool IsDownOK(string file)
        {
            return downLoadFiles.Contains(file);
        }

        void OnThreadCompleted(NotiData data)
        {
            Debug.Log("OnThreadCompleted " + data.evName + " data.evParam.ToString():" + data.evParam.ToString());
            switch (data.evName)
            {
                case NotiData.UPDATE_EXTRACT:  //解压一个完成
                                               //
                    break;
                case NotiData.UPDATE_DOWNLOAD: //下载一个完成
                    downLoadFiles.Add(data.evParam.ToString());
                    break;
            }
        }

        

    }
}
