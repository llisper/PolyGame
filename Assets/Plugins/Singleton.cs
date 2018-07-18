using UnityEngine;
using System;
using System.Threading.Tasks;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
{
    public static T Instance { get; private set; }

    public static async Task Init()
    {
        Type type = typeof(T);
        if (null == Instance)
        {
            var go = new GameObject(type.Name);
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<T>();
            Task task = Instance.AsyncInit();
            if (null != task)
                await task;
        }
    }

    public static async Task Init(GameObject go)
    {
        Type type = typeof(T);
        if (null != Instance)
        {
            throw new ApplicationException(string.Format(
                "{0}.Instance is already exist", 
                type.Name));
        }

        DontDestroyOnLoad(go);
        Instance = go.AddComponent<T>();
        Task task = Instance.AsyncInit();
        if (null != task)
            await task;
    }

    protected virtual Task AsyncInit() { return null; }
}

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    public static T Instance { get; private set; }

    public static async Task Init()
    {
        if (null != Instance)
        {
            throw new ApplicationException(string.Format(
                "{0}.Instance is already exist", 
                typeof(T).Name));
        }

        Instance = new T();
        Task task = Instance.AsyncInit();
        if (null != task)
            await task;
    }

    protected virtual Task AsyncInit() { return null; }
}
