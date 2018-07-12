using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;

public partial class Puzzle
{
    const string Filename = "Progress.json";

    [Serializable]
    class Progress
    {
        public string finished;
    }

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

            Progress p = new Progress();
            p.finished = Convert.ToBase64String(finishedBytes);

            File.WriteAllText(
                folder + Filename,
                JsonUtility.ToJson(p, true), 
                Encoding.UTF8);
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
            string path = ProgressFolder() + Filename;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                Progress p = JsonUtility.FromJson<Progress>(json);
                if (null != p)
                {
                    byte[] bytes = Convert.FromBase64String(p.finished);
                    var bitArray = new BitArray(bytes);
                    bool[] loadFinished = new bool[bitArray.Count];
                    bitArray.CopyTo(loadFinished, 0);
                    Array.Copy(loadFinished, finished, finished.Length);
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