using System.IO;
using UnityEngine;
using XLua;
using XLuaFramework;

namespace UnityLua
{
    class CheckUpdateManager:Manager
    {
        public enum State {
            None,
            /// <summary>
            /// 释放资源
            /// </summary>
            CheckExtractResource,
            /// <summary>
            /// 更新资源
            /// </summary>
            UpdateResourceFromNet,
            /// <summary>
            /// 初始化资源
            /// </summary>
            InitAssetBundle,
            /// <summary>
            /// 登录
            /// </summary>
            StartLogin,
            /// <summary>
            /// 开始游戏
            /// </summary>
            StartGame,
            /// <summary>
            /// 完成启动流程
            /// </summary>
            Playing
        }

        public enum SubState {
            Enter,Update
        }

        private State cur_state = State.None;
        private SubState cur_sub_state = SubState.Enter;

        void Start() {
            JumpToState(State.CheckExtractResource);
        }


        private void JumpToState(State newState) {
            cur_state = newState;
            cur_sub_state = SubState.Enter;
            Debug.Log("new_state : " + newState.ToString());
        }

        private void Update()
        {
            if (cur_state == State.Playing)
            {
                return;
            }

            switch (cur_state)
            {
                case State.None:
                    break;
                case State.CheckExtractResource:
                    if (cur_sub_state == SubState.Enter)
                    {
                        cur_sub_state = SubState.Update;
                        Debug.LogWarning("首次解压游戏数据（不消耗流量）");
                        //loadingView.SetData(0.0f, "首次解压游戏数据（不消耗流量）");
                        this.gameObject.AddComponent<AssetHotFixManager>();
                        AssetHotFixManager.Instance.CheckExtractResource(delegate (float percent)
                        {
                            Debug.LogWarning("首次解压游戏数据（不消耗流量）进度：" + 0.3f * percent);
                            //loadingView.SetData(0.3f * percent, "首次解压游戏数据（不消耗流量）");
                        }, delegate ()
                        {
                            Debug.Log("Main.cs CheckExtractResource OK!!!");
                            JumpToState(State.UpdateResourceFromNet);
                        });
                    }
                    break;
                case State.UpdateResourceFromNet:
                    if (cur_sub_state == SubState.Enter)
                    {
                        cur_sub_state = SubState.Update;
                        //loadingView.SetData(0.3f, "从服务器下载最新的资源文件...");
                        Debug.LogWarning("从服务器下载最新的资源文件...");
                        AssetHotFixManager.Instance.UpdateResource(delegate (float percent, string tip)
                        {
                            Debug.LogWarning(string.Format("进度：{0} - 提示语 {1}", 0.3f + 0.5f * percent, tip));
                            //loadingView.SetData(0.3f + 0.5f * percent, tip);
                        }, delegate (string result)
                        {
                            if (result == "")
                            {
                                Debug.Log("Main.cs UpdateResourceFromNet OK!!!");
                            }
                            else
                            {
                                Debug.Log(result);
                            }
                            JumpToState(State.InitAssetBundle);
                        });
                    }
                    break;
                case State.InitAssetBundle:
                    if (cur_sub_state == SubState.Enter)
                    {
                        cur_sub_state = SubState.Update;
                        //loadingView.SetData(0.8f, "初始化游戏资源...");
                        Debug.LogWarning("InitAssetBundle：初始化游戏资源...");
                        JumpToState(State.StartLogin);
                    }
                    break;
                case State.StartLogin:
                    if (cur_sub_state == SubState.Enter)
                    {
                        cur_sub_state = SubState.Update;
                        Debug.LogWarning("StartLogin：初始化游戏资源完毕...");
                        //loadingView.SetData(1, "初始化游戏资源完毕");
                        //XLuaManager.Instance.InitLuaEnv();
                        // loadingView.SetActive(false, 0.5f);
                        //XLuaManager.Instance.StartLogin(delegate ()
                        //{
                        //    Debug.Log("Main.cs XLuaManager StartLogin OK!!!");
                        JumpToState(State.StartGame);

                        XLuaResourceManager.Instance().OnInitialized(AppConst.AssetDir, delegate ()
                        {
                            Debug.Log("AssetBundleManifest 加载完成");
                            UnltyLuaSceneManager.Instance.LoadScene(SceneEnem.MAIN);

                        });

                        //});
                    }
                    break;
                case State.StartGame:
                    Debug.LogWarning("StartGame：开始游戏...");
                    JumpToState(State.Playing);
                    break;
                case State.Playing:
                    
                   
                    break;
                default:
                    break;
            }

        }

        private LuaEnv luaenv = new LuaEnv();
        static bool IsUseLuaFileWithoutBundle = false;//just for debug 
        private static byte[] CustomLoader(ref string filepath)
        {
            string scriptPath = string.Empty;
            filepath = filepath.Replace(".", "/") + ".lua";

            if (!IsUseLuaFileWithoutBundle)
            {
                scriptPath = Path.Combine(AppConst.LuaAssetsDir, filepath);
                Debug.Log("Load lua script : " + scriptPath);
                string tempPath = Application.dataPath + "/Lua/" + filepath;
                byte[] arr = Util.GetFileBytes(tempPath);
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


    }
}
