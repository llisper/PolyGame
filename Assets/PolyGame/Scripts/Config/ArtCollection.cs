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
        public int priority;
    }

    [Serializable]
    public class Group
    {
        public string name;
        public string themeColor;
        public int priority;
        public List<Item> items;
    }

    public List<Group> groups;
    public Dictionary<string, Item> itemMap;

    void AfterLoaded()
    {
        itemMap = new Dictionary<string, Item>();
        groups.Sort((l, r) => r.priority - l.priority);
        foreach (var g in groups)
        {
            g.items.Sort((l, r) => r.priority - l.priority);
            foreach (var i in g.items)
                itemMap.Add(i.name, i);
        }
    }
}