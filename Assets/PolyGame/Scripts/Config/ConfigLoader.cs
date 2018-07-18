using UnityEngine;
using System;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using ResourceModule;

public abstract class IConfig<T> where T : IConfig<T>
{
    public static T Instance;
}

public static class ConfigLoader
{
    public static Type[] types = new Type[]
    {
        typeof(Config),
        typeof(ArtCollection),
        typeof(I18n),
    };

    public static async Task Init()
    {
        LoadAll();
    }

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
        try
        {
            string json = LoadJson(type);
            var obj = JsonUtility.FromJson(json, type);

            type.InvokeMember(
                "Instance",
                 BindingFlags.Public | BindingFlags.Static | BindingFlags.SetField | BindingFlags.FlattenHierarchy,
                 null, null, new object[] { obj });

            var afterLoaded = type.GetMethod(
                "AfterLoaded",
                 BindingFlags.NonPublic | BindingFlags.Instance);
            if (null != afterLoaded)
                afterLoaded.Invoke(obj, null);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load " + type.Name);
            Debug.LogException(e);
        }
    }

    static string LoadJson(Type type)
    {
        string json = null;
        if (type != typeof(I18n))
        {
            string path = string.Format("{0}/{1}.json", Paths.Configs, type.Name);
            json = FileLoader.LoadString(path);
        }
        else
        {
            var lang = Application.systemLanguage;
            string path = string.Format("{0}/{1}/{2}.json", Paths.Configs, type.Name, lang);
            if (FileLoader.Exists(path))
            {
                json = FileLoader.LoadString(path);
            }
            else if (lang != SystemLanguage.English)
            {
                lang = SystemLanguage.English;
                path = string.Format("{0}/{1}/{2}", Paths.Configs, type.Name, lang);
                if (FileLoader.Exists(path))
                    json = FileLoader.LoadString(path);
            }

            if (null == json)
                throw new Exception(string.Format("Failed to load I18n: {0}, default(English) also failed", Application.systemLanguage));
        }
        return json;
    }
}