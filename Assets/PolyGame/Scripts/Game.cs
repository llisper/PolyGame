﻿using UnityEngine;
using System;
using System.IO;
using DG.Tweening;
using ResourceModule;
using ResourceModule.Hotfix;
using UI;

class GameLog : LogDefine<GameLog> { }

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
        Init();
    }

    async void Init()
    {
        try
        {
            DOTween.Init();
            GameEvent.Init();
            InitSaveDirectory();

            var loadingUI = GameObject.FindObjectOfType<LoadingUI>();
            await loadingUI.Init();

            await SystemManager.Init(
                InitCallback,
                ResourceSystem.Init,
                AssetSystem.Init,
                ConfigLoader.Init,
                HotfixSystem.Init,
                FUI.Init,
                PuzzleHintSystem.Init,
                GameScene.Init);

            await loadingUI.Finish();
        }
        catch (Exception e)
        {
            GameLog.LogException(e);
        }
    }

    void InitSaveDirectory()
    {
        Directory.CreateDirectory(Paths.Saves);
    }

    void InitCallback(int level, string name, float progress)
    {
        GameEvent.Instance.Fire(GameEvent.UpdateProgress, level, name, progress);
    }
}
