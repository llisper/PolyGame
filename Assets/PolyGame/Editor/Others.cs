using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

static class Others
{
    public static void SaveInitialSnapshot(PolyGraph graph)
    {
        string path = Paths.SnapshotRes(graph.name);
        string saveName = graph.name + Path.GetFileNameWithoutExtension(PuzzleSnapshot.FileName);

        var go = new GameObject(saveName);
        var holder = go.AddComponent<PuzzleSnapshot.Holder>();
        PuzzleSnapshotOneOff.Take(graph, null, path);
        holder.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.ToAssetPath(path));

        string prefabPath = string.Format(
            "{0}/{1}/{2}.prefab",
            Paths.AssetResArtworks,
            graph.name,
            saveName);
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, go);
        PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(go);
    }

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
}
