using UnityEngine;
using UnityEditor;
using System;
using System.IO;

class EmptyResources : IDisposable
{
    static string resourcesPath = Application.dataPath + "/Resources/";
    static string backupPath = Application.dataPath + "/../ResourcesBackup/";
    static string[] ignoreFolders = new string[] { "Slua" };

    public EmptyResources()
    {
        if (Directory.Exists(backupPath))
            Directory.Delete(backupPath, true);
        Directory.CreateDirectory(backupPath);

        foreach (string path in Directory.EnumerateDirectories(resourcesPath))
        {
            string folderName = Path.GetFileName(path);
            if (-1 == Array.IndexOf(ignoreFolders, folderName))
                Directory.Move(path, backupPath + folderName);
        }
        AssetDatabase.Refresh();
    }

    public void Dispose()
    {
        if (Directory.Exists(backupPath))
        {
            foreach (string path in Directory.EnumerateDirectories(backupPath))
            {
                string folderName = Path.GetFileName(path);
                Directory.Move(path, resourcesPath + folderName);
            }
            AssetDatabase.Refresh();
            Directory.Delete(backupPath, true);
        }
    }
}
