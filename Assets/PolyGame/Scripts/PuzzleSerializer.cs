using UnityEngine;
using System;
using System.IO;
using System.Collections;

public partial class Puzzle
{
    const int saveVersion = 1;

    string ProgressFolder()
    {
        return string.Format("{0}/{1}/", Paths.Saves, puzzleName);
    }

    public void SaveProgress()
    {
        try
        {
            string folder = ProgressFolder();
            Directory.CreateDirectory(folder);

            var bitArray = new BitArray(finished);
            byte[] finishedBytes = new byte[Mathf.CeilToInt(finished.Length / 8f)];
            bitArray.CopyTo(finishedBytes, 0);

            using (var stream = new FileStream(folder + "Save", FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(saveVersion);
                    writer.Write(finishedBytes.Length);
                    writer.Write(finishedBytes);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("[{0}] Save progress failed", puzzleName);
            Debug.LogException(e);
        }
    }

    void LoadProgress()
    {
        try
        {
            finished = new bool[puzzleObject.transform.childCount];
            string path = ProgressFolder() + "Save";
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        int version = reader.ReadInt32();
                        if (version != saveVersion)
                        {
                            Debug.LogFormat(
                                "[{0}]: Save version not match, save({1}) != current({2})",
                                puzzleName, version, saveVersion);
                            return;
                        }
                        int len = reader.ReadInt32();
                        byte[] finishedBytes = reader.ReadBytes(len);

                        var bitArray = new BitArray(finishedBytes);
                        bool[] loadFinished = new bool[bitArray.Count];
                        bitArray.CopyTo(loadFinished, 0);
                        Array.Copy(loadFinished, finished, finished.Length);

                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("[{0}] Load progress failed", puzzleName);
            Debug.LogException(e);
        }
    }
}