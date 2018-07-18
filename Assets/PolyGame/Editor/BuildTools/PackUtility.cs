using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using ResourceModule;

public class PackUtility
{
    static string _versionName = "0.1";
    static string _cdn = "http://10.1.38.218:8080/Product";
    static string[] _copyFolders = new string[] { "Configs" };

    public static void Setup(string versionName, string cdn)
    {
        _versionName = versionName;
        _cdn = cdn;
    }

    #region build resources
    [MenuItem("[Build Tools]/PackUtility/Build Resources/All")]
    public static void BuildAllResources()
    {
        BuildResources(BuildTarget.Android, BuildTarget.iOS);
    }

    public static void BuildResources(params BuildTarget[] targets)
    {
        EditorUtility.DisplayProgressBar("General", "Mark AssetBundles", 0f);
        AssetBundleMaker.Mark();

        foreach (var target in targets)
            BuildSelectedResources(target);

        AssetBundleMaker.Unmark();
        EditorUtility.ClearProgressBar();
        Debug.Log("Done!");
    }

    public static void BuildSelectedResources(BuildTarget buildTarget)
    {
        EditorUtility.DisplayProgressBar(buildTarget.ToString(), "Make AssetBundles", 0f);
        using (var hideCompileFlags = new HideCompileFlags())
            AssetBundleMaker.Build(buildTarget);

        EditorUtility.DisplayProgressBar(buildTarget.ToString(), "Copy To Product", 0f);
        CopyFilesToProduct(buildTarget);

        // EditorUtility.DisplayProgressBar(buildTarget.ToString(), "Generate FileManifest", 0f);
        // GenerateFileManifest.Generate(buildTarget);

        // EditorUtility.DisplayProgressBar(buildTarget.ToString(), "Generate Version", 0f);
        // GenerateVersion.Generate(_versionName, _cdn, buildTarget);

        Debug.Log("Build resources for " + buildTarget);
    }

    [MenuItem("[Build Tools]/PackUtility/Build Resources/Android")]
    public static void BuildAndroidResources()
    {
        BuildResources(BuildTarget.Android);
    }

    [MenuItem("[Build Tools]/PackUtility/Build Resources/iOS")]
    public static void BuildiOSResources()
    {
        BuildResources(BuildTarget.iOS);
    }
    #endregion build resources

    #region clear resources
    [MenuItem("[Build Tools]/PackUtility/Clear Resources/All")]
    public static void ClearResources()
    {
        ClearResources(BuildTarget.Android);
        ClearResources(BuildTarget.iOS);
    }

    public static void ClearResources(BuildTarget buildTarget)
    {
        string path = PathRouter.ProductPath + buildTarget + '/';
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        Debug.Log("Clear resources for " + buildTarget);
    }

    [MenuItem("[Build Tools]/PackUtility/Clear Resources/Android")]
    public static void ClearAndroidResources()
    {
        ClearResources(BuildTarget.Android);
    }

    [MenuItem("[Build Tools]/PackUtility/Clear Resources/iOS")]
    public static void CleariOSResources()
    {
        ClearResources(BuildTarget.iOS);
    }
    #endregion clear resources

    #region generate streaming assets
    [MenuItem("[Build Tools]/PackUtility/Generate StreamingAssets/Android")]
    public static void GenerateAndroidStreamingAssets()
    {
        GenerateStreamingAssets(BuildTarget.Android);
    }

    [MenuItem("[Build Tools]/PackUtility/Generate StreamingAssets/iOS")]
    public static void GenerateiOSStreamingAssets()
    {
        GenerateStreamingAssets(BuildTarget.iOS);
    }

    [MenuItem("[Build Tools]/PackUtility/Generate StreamingAssets/Clear")]
    public static void ClearStreamingAssets()
    {
        if (Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.Delete(Application.streamingAssetsPath, true);
            AssetDatabase.Refresh();
        }
    }

    public static void GenerateStreamingAssets(BuildTarget buildTarget)
    {
        ClearStreamingAssets();

        // TODO: select AssetBundles to copy according to rules
        string srcRoot = PathRouter.ProductPath + buildTarget + '/';
        string destRoot = Application.streamingAssetsPath + '/';
        CopyDirectory(srcRoot, destRoot, "*.*", v => Path.GetExtension(v) != ".manifest");
        AssetDatabase.Refresh();
        Debug.LogFormat("Generate StreamingAssets for " + buildTarget);
    }
    #endregion generate streaming assets

    [MenuItem("[Build Tools]/PackUtility/Clear Sandbox")]
    static void ClearSandbox()
    {
        string path = Application.persistentDataPath + '/' + PathRouter.Sandbox;
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    static void CopyFilesToProduct(BuildTarget buildTarget)
    {
        string srcRoot = string.Format("{0}/{1}/", Application.dataPath, PathRouter.NoPrefix(PathRouter.Res));
        string destRoot = PathRouter.ProductPath + buildTarget + '/';
        foreach (string folder in _copyFolders)
        {
            CopyDirectory(
                srcRoot + folder + '/',
                destRoot + folder + '/',
                "*.*",
                v => Path.GetExtension(v) != ".meta");
            Debug.LogFormat("{0} folder copied", folder);
        }
    }

    static void CopyDirectory(
        string src,
        string dest,
        string pattern = "*.*",
        Func<string, bool> filter = null)
    {
        if (Directory.Exists(dest))
            Directory.Delete(dest, true);

        foreach (string path in Directory.EnumerateFiles(src, pattern, SearchOption.AllDirectories))
        {
            if (null != filter && !filter(path))
                continue;

            string dirName = Path.GetDirectoryName(path) + '/';
            string destFolder = dest + dirName.Remove(0, src.Length);
            Directory.CreateDirectory(destFolder);
            File.Copy(path, destFolder + Path.GetFileName(path));
        }
    }
}
