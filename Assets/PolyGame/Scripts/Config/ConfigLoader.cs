using UnityEngine;
using System;
using System.Text;
using System.Reflection;

public abstract class IConfig<T> where T : IConfig<T>
{
    public static T Instance;
}

public static class ConfigLoader
{
    public static Type[] types = new Type[]
    {
        typeof(Config),
    };

    public static void LoadAll()
    {
        for (int i = 0; i < types.Length; ++i)
            Load(types[i]);
    }

    public static void Load(string name)
    {
        Type type = Array.Find(types, v => v.Name == name);
        if (null == type)
        {
            Debug.LogError("ConfigLoader.Load: type is not found, " + name);
            return;
        }
        Load(type);
    }

    public static void Load<T>()
    {
        Load(typeof(T));
    }

    public static void Load(Type type)
    {
        var asset = Resources.Load<TextAsset>(string.Format("{0}/{1}", Paths.Configs, type.Name));
        string json = Encoding.UTF8.GetString(Utils.RemoveBOM(asset.bytes));
        var obj = JsonUtility.FromJson(json, type);
        type.InvokeMember(
            "Instance",
             BindingFlags.Public | BindingFlags.Static | BindingFlags.SetField | BindingFlags.FlattenHierarchy,
             null, null, new object[] { obj });
    }
}