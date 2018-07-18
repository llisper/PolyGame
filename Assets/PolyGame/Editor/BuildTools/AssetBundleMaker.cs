using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ResourceModule;

class AssetInfo
{
    public string path;
    public bool mustInclude;
    public HashSet<AssetInfo> referencesBy = new HashSet<AssetInfo>();
    public HashSet<AssetInfo> dependencies = new HashSet<AssetInfo>();

    public AssetInfo(string path, bool mustInclude)
    {
        this.path = path;
        this.mustInclude = mustInclude;
    }
}

public static class AssetBundleMaker
{
    public const string Prefabs = "Assets/Resources";
    public const string Scenes = "Assets/Scenes";

    static Dictionary<string, AssetInfo> _assetCollection = new Dictionary<string, AssetInfo>();

    [MenuItem("[Build Tools]/AssetBundle/Collect AssetInfo")]
    static void CollectAssetInfo()
    {
        _assetCollection.Clear();
        string[] guids = AssetDatabase.FindAssets(
            "t:Object",
            new string[] { Prefabs, Scenes });

        guids = guids.Distinct().ToArray();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".prefab" || ext == ".unity")
                CreateAssetInfo(path, true);
        }
        PrintAssetInfo();
        Debug.Log("Collect AssetInfo Done");
    }

    static AssetInfo CreateAssetInfo(string path, bool mustInclude = false)
    {
        AssetInfo info;
        if (!_assetCollection.TryGetValue(path, out info))
        {
            info = new AssetInfo(path, mustInclude);
            _assetCollection.Add(path, info);

            foreach (string depPath in AssetDatabase.GetDependencies(path, false))
            {
                AssetInfo dep = CreateAssetInfo(depPath);
                if (info != dep)
                {
                    dep.referencesBy.Add(info);
                    info.dependencies.Add(dep);
                }
            }
        }
        return info;
    }

    static void PrintAssetInfo()
    {
        Action<HashSet<AssetInfo>, StringBuilder> print = (s, l) =>
        {
            foreach (var item in s)
            {
                l.Append(' ', 4);
                l.AppendFormat("{0}\n", item.path);
            }
        };

        var log = new StringBuilder();
        foreach (var kv in _assetCollection)
        {
            var info = kv.Value;
            var type = AssetDatabase.GetMainAssetTypeAtPath(info.path);
            log.AppendFormat(
                "{0}, refs:{1}, deps:{2}, type:{3}\n",
                info.path,
                info.referencesBy.Count,
                info.dependencies.Count,
                type.Name);
            if (info.referencesBy.Count > 0)
            {
                log.Append("  referenced by:\n");
                print(info.referencesBy, log);
            }
            if (info.dependencies.Count > 0)
            {
                log.Append("  dependencies:\n");
                print(info.dependencies, log);
            }
        }

        File.WriteAllText("AssetInfo", log.ToString(), Encoding.UTF8);
        Debug.Log("Result written to AssetInfo");
    }

    [MenuItem("[Build Tools]/AssetBundle/Mark")]
    public static void Mark()
    {
        CollectAssetInfo();
        foreach (var kv in _assetCollection)
        {
            var info = kv.Value;
            var type = AssetDatabase.GetMainAssetTypeAtPath(info.path);
            if (type != typeof(MonoScript))
            {
                var importer = AssetImporter.GetAtPath(info.path);
                importer.assetBundleName = null;
                if (info.mustInclude || info.referencesBy.Count > 1)
                    importer.assetBundleName = AssetBundleName(info.path);
            }
        }
        Debug.Log("Mark Done");
    }

    [MenuItem("[Build Tools]/AssetBundle/Unmark")]
    public static void Unmark()
    {
        CollectAssetInfo();
        foreach (var kv in _assetCollection)
        {
            var info = kv.Value;
            var type = AssetDatabase.GetMainAssetTypeAtPath(info.path);
            if (type != typeof(MonoScript))
            {
                var importer = AssetImporter.GetAtPath(info.path);
                importer.assetBundleName = null;
            }
        }
        Debug.Log("Unmark Done");
    }

    static string AssetBundleName(string path)
    {
        return path.Replace("Assets/", "") + PathRouter.AssetBundleSuffix;
    }

    public static void Build(BuildTarget buildTarget)
    {
        var options =
            BuildAssetBundleOptions.DeterministicAssetBundle |
            BuildAssetBundleOptions.StrictMode;

        string outputPath = PathRouter.ProductPath + buildTarget + '/' + PathRouter.ABFolder;
        Directory.CreateDirectory(outputPath);
        BuildPipeline.BuildAssetBundles(outputPath, options, buildTarget);
        Debug.Log("Build AssetBundles for " + buildTarget);
    }
}
