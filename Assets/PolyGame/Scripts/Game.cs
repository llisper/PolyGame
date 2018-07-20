using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using ResourceModule;
using ResourceModule.Hotfix;

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
            await SystemManager.Init(
                ResourceSystem.Init,
                AssetSystem.Init,
                ConfigLoader.Init,
                HotfixSystem.Init,
                UI.Init,
                GameScene.Init);
        }
        catch (Exception e)
        {
            GameLog.LogException(e);
        }
    }
}
