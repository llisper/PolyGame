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
}