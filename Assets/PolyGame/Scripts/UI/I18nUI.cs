using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class I18nUI : MonoBehaviour
{
    [Serializable]
    public class TextPair
    {
        public string key;
        public Text text;
        public TextPair(Text text) { this.text = text; }
    }
    public List<TextPair> collection = new List<TextPair>();

    void Start()
    {
        ApplyI18n();
    }

    public void ApplyI18n()
    {
        for (int i = 0; i < collection.Count; ++i)        
        {
            var c = collection[i];
            if (null == c.text)
                continue;

            string key = c.key;
            if (string.IsNullOrEmpty(key))
                c.text.text = c.text.text + "(i18n)";
            else
                c.text.text = I18n.Get(key);
        }
    }
}