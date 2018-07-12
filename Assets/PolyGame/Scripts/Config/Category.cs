using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Category : IConfig<Category>
{
    [Serializable]
    public class CatInfo
    {
        public string name;
        public Color32 themeColor = Color.black;
        public List<string> items;
    }

    public List<CatInfo> catInfos;
}