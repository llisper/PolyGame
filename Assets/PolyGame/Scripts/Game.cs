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

    void Start()
    {
        ConfigLoader.LoadAll();
        UI.Init();
        GameScene.Init();
	}
}
