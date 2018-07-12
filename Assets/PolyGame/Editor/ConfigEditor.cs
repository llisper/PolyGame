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

    [MenuItem("Tools/Configs/Reload All")]
    static void ReloadAll()
    {
        ConfigLoader.LoadAll();
    }
}