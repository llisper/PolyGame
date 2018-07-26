using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;

public partial class Puzzle
{
    const string Filename = "Progress.json";

    float saveTimer;
    bool needToSave;

    [Serializable]
    class Progress
    {
        public string finished;

        public bool[] finishedFlags
        {
            get
            {
                byte[] bytes = Convert.FromBase64String(finished);
                var bitArray = new BitArray(bytes);
                bool[] loadFinished = new bool[bitArray.Count];
                bitArray.CopyTo(loadFinished, 0);
                return loadFinished;
            }
            set
            {
                var bitArray = new BitArray(value);
                byte[] finishedBytes = new byte[Mathf.CeilToInt(value.Length / 8f)];
                bitArray.CopyTo(finishedBytes, 0);
                finished = Convert.ToBase64String(finishedBytes);
            }
        }
    }

    static string ProgressFolder(string name)
    {
        return string.Format("{0}/{1}/", Paths.Saves, name);
    }

    public void SaveProgress(bool snapshotMark = false)
    {
        try
        {
            string folder = ProgressFolder(puzzleName);
            Directory.CreateDirectory(folder);

            Progress p = new Progress();
            p.finishedFlags = finished;
            File.WriteAllText(
                folder + Filename,
                JsonUtility.ToJson(p, true), 
                Encoding.UTF8);

            bool snapshotMarkExists = File.Exists(Paths.SnapshotMark);
            if (snapshotMark && !snapshotMarkExists)
                File.WriteAllText(Paths.SnapshotMark, puzzleName, Encoding.UTF8);
            else if (!snapshotMark && snapshotMarkExists)
                File.Delete(Paths.SnapshotMark);

        }
        catch (Exception e)
        {
            GameLog.LogErrorFormat("[{0}] Save progress failed", puzzleName);
            GameLog.LogException(e);
        }
    }

    public static void RetakeExpiredSnapshot()
    {
        string puzzleName = "unknown";
        try
        {
            if (File.Exists(Paths.SnapshotMark))
            {
                puzzleName = File.ReadAllText(Paths.SnapshotMark, Encoding.UTF8);
                string path = ProgressFolder(puzzleName) + Filename;
                string json = File.ReadAllText(path, Encoding.UTF8);
                Progress p = JsonUtility.FromJson<Progress>(json);
                PuzzleSnapshotOneOff.Take(puzzleName, p.finishedFlags);
                File.Delete(Paths.SnapshotMark);
                GameLog.LogFormat("[{0}] Retake snapshot", puzzleName);
            }
        }
        catch (Exception e)
        {
            GameLog.LogErrorFormat("[{0}] Failed to retake expired snapshot", puzzleName);
            GameLog.LogException(e);
        }
    }

    void LoadProgress()
    {
        try
        {
            finished = new bool[puzzleObject.transform.childCount];
            string path = ProgressFolder(puzzleName) + Filename;
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                Progress p = JsonUtility.FromJson<Progress>(json);
                if (null != p)
                    Array.Copy(p.finishedFlags, finished, finished.Length);
            }
        }
        catch (Exception e)
        {
            GameLog.LogErrorFormat("[{0}] Load progress failed", puzzleName);
            GameLog.LogException(e);
        }
    }

    void UpdaetProgress()
    {
        if (needToSave)
        {
            if ((saveTimer += Time.deltaTime) >= Config.Instance.puzzle.saveInterval)
            {
                SaveProgress(true);
                saveTimer = 0f;
                needToSave = false;
                GameLog.VerboseFormat("[{0}] Auto saved", puzzleName);
            }
        }
    }
}