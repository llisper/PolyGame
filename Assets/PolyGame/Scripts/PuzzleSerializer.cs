using UnityEngine;
using System;
using System.IO;
using System.Collections;

public partial class Puzzle
{
    string ProgressFolder()
    {
        return string.Format("{0}/Saves/{1}/", Application.persistentDataPath, puzzleName);
    }

    void SaveProgress()
    {
        string folder = ProgressFolder();
        Directory.CreateDirectory(folder);

        var bitArray = new BitArray(finished);
        byte[] finishedBytes = new byte[Mathf.CeilToInt(finished.Length / 8f)];
        bitArray.CopyTo(finishedBytes, 0);
        File.WriteAllBytes(folder + "Save", finishedBytes);
    }

    void LoadProgress()
    {
        finished = new bool[puzzleObject.transform.childCount];
        string path = ProgressFolder() + "Save";
        if (File.Exists(path))
        {
            var bitArray = new BitArray(File.ReadAllBytes(path));
            bool[] loadFinished = new bool[bitArray.Count];
            bitArray.CopyTo(loadFinished, 0);
            Array.Copy(loadFinished, finished, finished.Length);
        }
    }
}