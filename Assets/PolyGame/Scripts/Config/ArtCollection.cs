using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ArtCollection : IConfig<ArtCollection>
{
    [Serializable]
    public class Item
    {
        public string name;
        public string bgColor;
    }

    [Serializable]
    public class Group
    {
        public string name;
        public string themeColor;
        public List<Item> items;
    }

    public List<Group> groups;
    public Dictionary<string, Item> itemMap;

    void AfterLoaded()
    {
        itemMap = new Dictionary<string, Item>();
        foreach (var g in groups)
        {
            foreach (var i in g.items)
                itemMap.Add(i.name, i);
        }
    }
}