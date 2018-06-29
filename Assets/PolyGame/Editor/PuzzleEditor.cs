using UnityEngine;
using UnityEditor;
using System.IO;

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

    [MenuItem("Tools/(tmp) Update Puzzle List")]
    static void UpdatePuzzleList()
    {
        var ui = (GameObject)GameObject.Instantiate(Resources.Load("UI/UI"));
        var menu = (GameObject)GameObject.Instantiate(Resources.Load("UI/MenuPanel"), ui.transform.Find("Canvas/Base"));
        menu.GetComponent<MenuPanel>().AutoFillOptions();

        string prefabPath = "Assets/Resources/UI/MenuPanel.prefab";
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, menu);
        PrefabUtility.ReplacePrefab(menu, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(ui);
    }
}