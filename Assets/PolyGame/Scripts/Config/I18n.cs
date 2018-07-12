using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class I18n : IConfig<I18n>
{
    [Serializable]
    public class Pair 
    {
        public string k;
        public string v;
    }
    public List<Pair> collection;

    const string ErrorValue = "##ERROR##";

    public static string Get(string key)
    {
        if (null == Instance || null == Instance.query)
        {
            Debug.LogError("I18n has not been initialized!");
            return ErrorValue ;
        }

        string value;
        if (!Instance.query.TryGetValue(key, out value))
            value = ErrorValue;
        return value;
    }

    Dictionary<string, string> query;

    void AfterLoaded()
    {
        query = new Dictionary<string, string>();
        foreach (var pair in collection)
            query[pair.k] = pair.v;
    }
}