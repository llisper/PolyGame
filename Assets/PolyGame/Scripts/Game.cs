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

    class System
    {
        public object instance;
        public MethodInfo update;
        public MethodInfo fixedUpdate;
        public MethodInfo lateUpdate;
        public MethodInfo onApplicationQuit;

        public System(object instance)
        {
            this.instance = instance;
            FindMethod("Update", out update);
            FindMethod("FixedUpdate", out fixedUpdate);
            FindMethod("LateUpdate", out lateUpdate);
            FindMethod("OnApplicationQuit", out onApplicationQuit);
        }

        void FindMethod(string name, out MethodInfo method)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            method = instance.GetType().GetMethod(name, flags);
        }
    }

    List<System> systems = new List<System>();

    public GameObject SystemRoot { get; private set; }

    void Awake()
    {
        Instance = this;
        SystemRoot = new GameObject("Systems");
        SystemRoot.transform.parent = transform;

        RegisterInitSequence();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ConfigLoader.LoadAll();
        Init();
        UI.Init();
        GameScene.Init();
    }

    void Update()
    {
        for (int i = 0; i < systems.Count; ++i)
        {
            var s = systems[i];
            if (null != s.update)
                s.update.Invoke(s.instance, null);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < systems.Count; ++i)
        {
            var s = systems[i];
            if (null != s.fixedUpdate)
                s.fixedUpdate.Invoke(s.instance, null);
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < systems.Count; ++i)
        {
            var s = systems[i];
            if (null != s.lateUpdate)
                s.lateUpdate.Invoke(s.instance, null);
        }
    }

    void OnApplicationQuit()
    {
        for (int i = 0; i < systems.Count; ++i)
        {
            var s = systems[i];
            if (null != s.onApplicationQuit)
                s.onApplicationQuit.Invoke(s.instance, null);
        }
    }

    List<Func<Task>> initSequence = new List<Func<Task>>();

    void RegisterInitSequence()
    {
        initSequence.Add(ResourceSystem.Init);
        initSequence.Add(AssetSystem.Init);
        // initSequence.Add(HotfixSystem.Init);
    }

    async void Init()
    {
        GameLog.Log("----------Initialize Game----------");
        for (int i = 0; i < initSequence.Count; ++i)
        {
            var func = initSequence[i];
            GameLog.Log(GetName(func));
            await func();
            AddSystem(func);
        }
        GameLog.Log("----------Initialize Finished----------");
    }

    string GetName(Func<Task> func)
    {
        try
        {
            if (func.Method.DeclaringType.IsGenericType)
                return func.Method.DeclaringType.GenericTypeArguments[0].Name + '.' + func.Method.Name;
            else
                return func.Method.DeclaringType.Name + '.' + func.Method.Name;
        }
        catch (Exception)
        {
            return "Unknown.Init";
        }
    }

    void AddSystem(Func<Task> func)
    {
        try
        {
            var declaringType = func.Method.DeclaringType;
            if (declaringType.IsGenericType &&
                declaringType.GenericTypeArguments[0].Name.Contains("System"))
            {
                var prop = declaringType.GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static);
                systems.Add(new System(prop.GetValue(null)));
            }
        }
        catch (Exception e)
        {
            GameLog.LogException(e);
        }
    }
}
