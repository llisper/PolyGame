using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

class PuzzleEditor
{
    [MenuItem("Tools/Clear Saves")]
    static void ClearSaves()
    {
        if (Directory.Exists(Paths.Saves))
        {
            Directory.Delete(Paths.Saves, true);
            Debug.Log("Clear Saves");
        }
    }

    [MenuItem("Tools/Update Puzzle List")]
    static void UpdatePuzzleList()
    {
        var ui = (GameObject)GameObject.Instantiate(Resources.Load("UI/UI"));
        var menu = (GameObject)GameObject.Instantiate(Resources.Load("UI/MenuPanel"), ui.transform.Find("Canvas/Base"));

        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        menu.GetComponent<MenuPanel>().options = new List<string>(Array.ConvertAll(dirs, v => Path.GetFileName(v)));

        string prefabPath = "Assets/Resources/UI/MenuPanel.prefab";
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, menu);
        PrefabUtility.ReplacePrefab(menu, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(ui);
    }

    [MenuItem("Tools/Test")]
    static void Test()
    {
        string path = string.Format("{0}/{1}/{1}.png", Paths.AssetArtworks, "Bill");
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
        if (null == texture)
            throw new Exception("Failed to load texture " + path);

        Debug.Log(texture);
        Debug.Log(texture.GetType().Name);
        GameObject.Destroy(texture);
    }
}