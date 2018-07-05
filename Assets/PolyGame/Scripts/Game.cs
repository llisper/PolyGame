using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class Game : MonoBehaviour
{
    public static Game Instance;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(CompleteInitialSnapshots());

        UI.Init();
        GameScene.Init();
	}

    IEnumerator CompleteInitialSnapshots()
    {
        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        string[] names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        for (int i = 0; i < names.Length; ++i)
        {
            string path = PuzzleSnapshot.SavePath(names[i]);
            if (!File.Exists(path))
            {
                PuzzleSnapshotOneOff.Take(names[i]);
                yield return null;
            }
        }
    }
}
