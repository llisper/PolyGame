using UnityEngine;
using System;
using System.IO;

public class Game : MonoBehaviour
{
    public static Game Instance;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CompleteInitialSnapshots();
    }

    void Start()
    {
        UI.Init();
        GameScene.Init();
	}

    /* NOTE: temporary code generate snapshots 
     */
    public static void CompleteInitialSnapshots()
    {
        /*
        var go = new GameObject("PuzzleSnapshot");
        var snapshot = go.AddComponent<PuzzleSnapshot>();

        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        string[] names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        for (int i = 0; i < names.Length; ++i)
        {
            string path = PuzzleSnapshot.SavePath(names[i]);
            if (!File.Exists(path))
            {
                snapshot.Init(names[i]);
                snapshot.Save();
            }
        }

        Destroy(go);
        */
    }
}
