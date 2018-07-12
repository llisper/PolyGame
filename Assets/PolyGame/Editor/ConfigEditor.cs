using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

[InitializeOnLoad]
static class ConfigEditor
{
    static ConfigEditor()
    {
        ConfigLoader.LoadAll();
    }

    [MenuItem("Tools/Configs/Generate Default Configs")]
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

    [MenuItem("Tools/Configs/Auto Fill Background Colors")]
    static void AutoFillBackgroundColors()
    {
        foreach (var g in ArtCollection.Instance.groups)
        {
            foreach (var i in g.items)
            {
                if (string.IsNullOrEmpty(i.bgColor))
                {
                    var go = Resources.Load<GameObject>(string.Format("{0}/{1}/{1}", Paths.Artworks, i.name));
                    var color = PuzzleBackground.AvarageColor(go.GetComponent<PolyGraph>());
                    i.bgColor = Utils.ColorToString(color);
                }
            }
        }

        string path = string.Format("{0}/Resources/{1}/ArtCollection.json", Application.dataPath, Paths.Configs);
        string json = JsonUtility.ToJson(ArtCollection.Instance, true); 
        File.WriteAllText(path, json, Encoding.UTF8);
        Debug.Log("Done!");
    }

    [MenuItem("Tools/Configs/Reload All")]
    static void ReloadAll()
    {
        ConfigLoader.LoadAll();
    }
}