using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[InitializeOnLoad]
static class ConfigEditor
{
    static ConfigEditor()
    {
        if (!Application.isPlaying)
            ConfigLoader.LoadAll();
    }

    [MenuItem("[PolyGame]/Configs/Generate Default Configs")]
    static void GenerateDefaultConfigs()
    {
        string dir = string.Format("{0}/Resources/{1}/", Application.dataPath, Paths.Configs);
        Directory.CreateDirectory(dir);
        foreach (var type in ConfigLoader.types)
        {
            string path = dir + type.Name + ".json";
            if (!File.Exists(path))
            {
                object obj = Activator.CreateInstance(type);
                string json = JsonUtility.ToJson(obj, true);
                File.WriteAllText(path, json, Encoding.UTF8);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("Done!");
    }

    [MenuItem("[PolyGame]/Configs/Auto Fill ArtCollection")]
    static void AutoFillArtCollection()
    {
        try
        {
            Dictionary<string, ArtCollection.Group> groupMap = new Dictionary<string, ArtCollection.Group>();
            foreach (var g in ArtCollection.Instance.groups)
                groupMap[I18n.Get(g.name).ToLower()] = g;

            string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string name = Path.GetFileName(dirs[i]);
                string groupName = name;
                var match = Regex.Match(name, @"(\D+)");
                if (match.Success)
                    groupName = match.Groups[1].Value;

                EditorUtility.DisplayProgressBar("Auto Fill ArtCollection", name, (float)i / dirs.Length);

                var group = ArtCollection.Instance.groups.Find(v => 0 == string.Compare(I18n.Get(v.name), groupName, true));
                if (null == group)
                    group = ArtCollection.Instance.groups.Find(v => I18n.Get(v.name) == "Default");

                var item = group.items.Find(v => v.name == name);
                if (null == item)
                {
                    item = new ArtCollection.Item();
                    item.name = name;
                    group.items.Add(item);
                }

                if (string.IsNullOrEmpty(item.bgColor))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(string.Format("{0}/{1}/{1}", Paths.AssetResArtworks, item.name));
                    var color = PuzzleBackground.AvarageColor(go.GetComponent<PolyGraph>());
                    item.bgColor = Utils.ColorToString(color);
                }
            }

            EditorUtility.DisplayProgressBar("Auto Fill ArtCollection", "Saving json", 1f);
            string path = string.Format("{0}/Resources/{1}/ArtCollection.json", Application.dataPath, Paths.Configs);
            string json = JsonUtility.ToJson(ArtCollection.Instance, true);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }


    [MenuItem("[PolyGame]/Configs/Reload All")]
    static void ReloadAll()
    {
        ConfigLoader.LoadAll();
        Debug.Log("Done!");
    }
}