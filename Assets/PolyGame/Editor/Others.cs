using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

static class Others
{
    [MenuItem("Tools/Others/Clear Saves")]
    static void ClearSaves()
    {
        if (Directory.Exists(Paths.Saves))
        {
            Directory.Delete(Paths.Saves, true);
            Debug.Log("Clear Saves");
        }
    }

    [MenuItem("Tools/Others/Update Puzzle List")]
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

    [MenuItem("Tools/Others/Complete Initial Snapshots")]
    static void CompleteInitialSnapshots()
    {
        Game.CompleteInitialSnapshots();
    }
}
