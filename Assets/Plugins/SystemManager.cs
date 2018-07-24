using UnityEngine;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

class SystemInitLog : LogDefine<SystemInitLog> { }

public class SystemManager : MonoBehaviour
{
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

    public delegate void InitCallback(int level, string name, float progress);

    public static SystemManager Instance;

    List<System> systems = new List<System>();

    public static async Task Init(InitCallback initCallback, params Func<Task>[] initTasks)
    {
        if (null != Instance)
            throw new ApplicationException("SystemManager already exsited, can't run Init more than once!");

        var go = new GameObject("Systems", typeof(SystemManager));
        DontDestroyOnLoad(go);

        SystemInitLog.Log("----------Initialize Game----------");
        for (int i = 0; i < initTasks.Length; ++i)
        {
            var func = initTasks[i];
            string sysName = GetName(func);
            initCallback(0, sysName, (float)i / initTasks.Length);
            SystemInitLog.Log(sysName);
            await func();
            Instance.AddSystem(func);
            initCallback(0, sysName, (float)(i + 1) / initTasks.Length);
        }
        SystemInitLog.Log("----------Initialize Finished----------");
    }

    static string GetName(Func<Task> func)
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
            SystemInitLog.LogException(e);
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
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
}