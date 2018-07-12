using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(I18nUI))]
class I18nUIEditor : Editor
{
    I18nUI script;

    public override void OnInspectorGUI()
    {
        script = (I18nUI)target;
        DrawCollection();
        DrawButtons();
    }

    void DrawCollection()
    {
        if (0 == script.collection.Count)
            return;

        for (int i = 0; i < script.collection.Count; )
        {
            var pair = script.collection[i];
            if (null == pair.text)
            {
                script.collection.RemoveAt(i);
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(pair.text, typeof(Text), true, GUILayout.Width(160f));
            pair.key = EditorGUILayout.TextField(pair.key, GUILayout.Width(40f));
            if (!I18n.Exists(pair.key) || pair.text.text != I18n.Get(pair.key))
                GUI.backgroundColor = Color.red;
            else
                GUI.backgroundColor = Color.green;
            EditorGUILayout.LabelField(pair.text.text, EditorStyles.textField);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            ++i;
        }
    }

    void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Collect Text"))
        {
            foreach (var text in script.GetComponentsInChildren<Text>(true))
            {
                if (null == script.collection.Find(v => v.text == text))
                    script.collection.Add(new I18nUI.TextPair(text));
            }
        }
        if (GUILayout.Button("Apply I18n"))
        {
            script.ApplyI18n();
        }
        EditorGUILayout.EndHorizontal();
    }
}