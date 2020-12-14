using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using XLuaFramework;
using System.Diagnostics;

public class BuildAssetBundle : Editor
{
    /// <summary>
    /// assetBundle输出路径
    /// </summary>
    private static string m_out_bundle_path = Application.dataPath;


    private static string m_BundlePath = string.Empty;

    /// <summary>
    /// 需要打bundle包的路径root
    /// </summary>
    private static string m_art_path = "Assets/Art";

    /// <summary>
    /// 存放文件夹下的文件
    /// </summary>
    private static List<string> m_files = new List<string>();

    /// <summary>
    /// 文件夹路径
    /// </summary>
    private static List<string> m_dir_paths = new List<string>();

    static List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

    [MenuItem("BulidBundle/Android")]
    public static void BuildAndroid() {
        BuildBundle(BuildTarget.Android);
    }

    [MenuItem("BulidBundle/Window")]
    public static void BuildWindow()
    {
        BuildBundle(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("BulidBundle/BuildAndroidAndCopy")]
    public static void BuildAndroidAndCopy() {
        //BuildBundle(BuildTarget.Android);
        CopyToService(BuildTarget.Android);
    }


    private static void BuildBundle(BuildTarget target) {
        //确定输出bundle目录是否存在
        m_BundlePath = GetOutAssetBundlePathByPlatform(target);
        if (Directory.Exists(m_BundlePath))
        {
            Directory.Delete(m_BundlePath, true);//递归删除
        }

        Directory.CreateDirectory(m_BundlePath);
        AssetDatabase.Refresh();

        m_files.Clear();
        m_dir_paths.Clear();

        if (AppConst.LuaBundleMode)
        {
            HandleLuaBundle();
        }
        else
        {
            HandleLuaFile();
        }
        Recursive(m_art_path,true);

        BuildPipeline.BuildAssetBundles(m_BundlePath, assetBundleBuilds.ToArray(),BuildAssetBundleOptions.None,target);
        BundleScene(target);
        BuildFileIndex();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    static void HandleLuaBundle()
    {
        string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
        if (!Directory.Exists(streamDir)) Directory.CreateDirectory(streamDir);

        string[] srcDirs = { m_LuaPath };
        for (int i = 0; i < srcDirs.Length; i++)
        {
            if (AppConst.LuaByteMode)
            {
                string sourceDir = srcDirs[i];
                string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                int len = sourceDir.Length;

                if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
                {
                    --len;
                }
                for (int j = 0; j < files.Length; j++)
                {
                    string str = files[j].Remove(0, len);
                    string dest = streamDir + str + ".bytes";
                    string dir = Path.GetDirectoryName(dest);
                    Directory.CreateDirectory(dir);
                    EncodeLuaFile(files[j], dest);
                }
            }
            else
            {
                //ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], streamDir);
            }
        }
        string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirs.Length; i++)
        {
            string name = dirs[i].Replace(streamDir, string.Empty);
            name = name.Replace('\\', '_').Replace('/', '_');
            name = "lua/lua_" + name.ToLower() + AppConst.ExtName;

            string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
            AddBuildMap(name, "*.bytes", path);
        }
        AddBuildMap("lua/lua" + AppConst.ExtName, "*.bytes", "Assets/" + AppConst.LuaTempDir);

        //-------------------------------处理非Lua文件----------------------------------
        string luaPath = m_BundlePath +  "/lua/";
        for (int i = 0; i < srcDirs.Length; i++)
        {
            m_dir_paths.Clear(); m_files.Clear();
            string luaDataPath = srcDirs[i].ToLower();
            RecursiveAll(luaDataPath);
            foreach (string f in m_files)
            {
                if (f.EndsWith(".meta") || f.EndsWith(".lua")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string path = Path.GetDirectoryName(luaPath + newfile);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string destfile = path + "/" + Path.GetFileName(f);
                File.Copy(f, destfile, true);
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void RecursiveAll(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            m_files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            m_dir_paths.Add(dir.Replace('\\', '/'));
            RecursiveAll(dir);
        }
    }

    static void AddBuildMap(string bundleName, string pattern, string path)
    {
        string[] files = Directory.GetFiles(path, pattern);
        if (files.Length == 0) return;

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Replace('\\', '/');
        }
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = files;
        assetBundleBuilds.Add(build);
    }

    private static string LuaEncoder {
        get {
            return Application.dataPath.Replace("Assets","") + "Tools/LuaEncoder/";
        }
    }

    public static void EncodeLuaFile(string srcFile, string outFile)
    {
        if (!srcFile.ToLower().EndsWith(".lua"))
        {
            File.Copy(srcFile, outFile, true);
            return;
        }


        bool isWin = true;
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = LuaEncoder + "luajit/";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            isWin = false;
            luaexe = "./luac";
            args = "-o " + outFile + " " + srcFile;
            exedir = LuaEncoder + "luavm/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.ErrorDialog = true;
        info.UseShellExecute = isWin;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    private static string m_LuaPath {
        get {
            return Application.dataPath + "/Lua/";
        }
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile()
    {
        UnityEngine.Debug.Log(string.Format("LuaEncoder : {0}", LuaEncoder));
        UnityEngine.Debug.Log(string.Format("m_LuaPath : {0}", m_LuaPath));

        string resPath = m_BundlePath;
        string luaPath = resPath + "/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath))
        {
            Directory.CreateDirectory(luaPath);
        }
        string[] luaPaths = { m_LuaPath };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            m_dir_paths.Clear(); m_files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            RecursiveAll(luaDataPath);
            int n = 0;
            foreach (string f in m_files)
            {
                if (f.EndsWith(".meta")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath))
                {
                    File.Delete(newpath);
                }
                if (AppConst.LuaByteMode)
                {
                    EncodeLuaFile(f, newpath);
                }
                else
                {
                    File.Copy(f, newpath, true);
                }
                UpdateProgress(n++, m_files.Count, newpath);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }


    /// <summary>
    /// 构建scene bundle
    /// </summary>
    /// <param name="target"></param>
    private static void BundleScene(BuildTarget target) {
        string scene_path = "Assets/Art/Scenes";
        string[] files = Directory.GetFiles(scene_path);
        foreach (var file in files)
        {
            string ext = Path.GetExtension(file);
            if (ext.Equals(".unity"))
            {
                string fileName= Path.GetFileNameWithoutExtension(file);
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.locationPathName = m_BundlePath + "/scene_" + fileName;
                buildPlayerOptions.scenes = new string[] { file };
                buildPlayerOptions.target = target;
                buildPlayerOptions.options = BuildOptions.BuildAdditionalStreamedScenes;
                string str = BuildPipeline.BuildPlayer(buildPlayerOptions);
                UnityEngine.Debug.Log(str);
                //UnityEditor.Build.Reporting.BuildReport report =  BuildPipeline.BuildPlayer(buildPlayerOptions);
                //UnityEditor.Build.Reporting.BuildSummary summary = report.summary;
                //if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                //{
                //    Debug.Log(string.Format("场景：{0} 构建成功！！！", fileName));
                //}
                //else if (summary.result == UnityEditor.Build.Reporting.BuildResult.Failed) {
                //    Debug.LogError(string.Format("场景：{0} 构建失败！！！", fileName));
                //}
                //Debug.Log("summary.result : " + summary.result.ToString());
            }
        }
    }

    /// <summary>
    /// 递归文件夹查找文件
    /// </summary>
    /// <param name="dir_path"> 文件夹路径</param>
    /// <param name="ifnore_meta"> 是否忽略meta文件</param>
    private static void Recursive(string dir_path,bool ignore_meta = true) {
        string[] files = Directory.GetFiles(dir_path);
        string[] dirs = Directory.GetDirectories(dir_path);
        string dirName = Path.GetFileName(dir_path);
        m_files.Clear();
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            string ext = Path.GetExtension(filePath);
            if (ignore_meta && ext.Equals(".meta")) continue;
            m_files.Add(filePath.Replace("\\","/"));
        }

        if (m_files.Count > 0) {
            AssetBundleBuild assetBundle = new AssetBundleBuild();
            assetBundle.assetBundleName = dirName;
            assetBundle.assetNames = m_files.ToArray();
            assetBundleBuilds.Add(assetBundle);
        }

        for (int i = 0; i < dirs.Length; i++)
        {
            string dirPath = dirs[i];
            dirName = Path.GetFileName(dirPath);
            if (dirName.Equals("Scenes".ToLower())) continue;

            m_dir_paths.Add(dirPath.Replace("\\", "/"));
            Recursive(dirPath);
        }
    }

    private static void CopyToService(BuildTarget platform) {
        string from_path = GetOutAssetBundlePathByPlatform(platform);
        string dir_name = Path.GetFileName(from_path);
        string to_path = "F:/resource/" + dir_name;

        if (Directory.Exists(to_path)) {
            Directory.Delete(to_path, true);
        }

        Directory.CreateDirectory(to_path);
        CopyFile(from_path, to_path);

        UnityEngine.Debug.Log("源文件夹：" + dir_name);
    }

    private static void CopyFile(string from_path,string to_path) {
        from_path = from_path.Replace('\\', '/');
        to_path = to_path.Replace('\\', '/');

        if (!Directory.Exists(from_path)) {
            Directory.CreateDirectory(from_path);
        }

        if (!Directory.Exists(to_path)) {
            Directory.CreateDirectory(to_path);
        }


        //获取当前文件夹下所有文件
        string[] files =  Directory.GetFiles(from_path);
        foreach (string file in files) {
            string destFileName = Path.GetFileName(file);
            File.Copy(file, to_path + "/" + destFileName,true);
        }

        //创建DirectoryInfo实例
        DirectoryInfo dirInfo = new DirectoryInfo(from_path);
        //取得源文件夹下的所有子文件夹名称
        DirectoryInfo[] ZiPath = dirInfo.GetDirectories();

        //获取当前文件夹下所有文件夹
        string[] dirs = Directory.GetDirectories(from_path);
        foreach (string dir in dirs)
        {
            string _dir = dir.Replace('\\', '/');
            string dir_name = Path.GetFileName(_dir);
            CopyFile(_dir, to_path + "/" + dir_name);
        }
    }


    /// <summary>
    /// 根据平台设置bundle输出路径
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    private static string GetOutAssetBundlePathByPlatform(BuildTarget platform) {
        string out_path = string.Empty;
        string _out_bundle_path = m_out_bundle_path.Replace("/Assets", "");
        switch (platform) {
            case BuildTarget.Android:
                out_path = _out_bundle_path + "/AssetBundle/StreamingAssets";
                break;
            case BuildTarget.iOS:
                out_path = _out_bundle_path + "/AssetBundle/StreamingAssets";
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                out_path = _out_bundle_path + "/AssetBundle/StreamingAssets";
                break;

        }
        return out_path;
    }

    private static void BuildFileIndex()
    {
        string resPath = m_BundlePath;
        //string resPath = AppDataPath + "/StreamingAssets/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        m_dir_paths.Clear();
        m_files.Clear();
        RecursiveAll(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < m_files.Count; i++)
        {
            string file = m_files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
    }
}
