using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using ResourceModule;

class AssetSystemInspector
{
    static string filterText;
    static Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    public static void OnInspectorGUI()
    {
        DrawBase();

        string[] filters = null;
        filterText = EditorGUILayout.TextField(filterText);
        if (!string.IsNullOrEmpty(filterText))
        {
            filters = filterText.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
        }

        foreach (var kv in AssetSystem.Instance.assets)
            DrawAssetHandle(kv.Value, filters);
    }

    static void DrawBase()
    {
        var script = AssetSystem.Instance;
        script.disposeDelay = EditorGUILayout.FloatField("disposeDelay", script.disposeDelay);
        script.disposePerFrame = EditorGUILayout.IntField("disposePerFrame", script.disposePerFrame);
        script.tickInterval = EditorGUILayout.FloatField("tickInterval", script.tickInterval);
    }

    static void DrawAssetHandle(AssetHandle h, string[] filters = null)
    {
        if (null != filters)
        {
            if (!filters.Any(h.Path.Contains))
                return;
        }

        string text = string.Format("R:{0} {1} {2}", h.RefCount, Status(h), h.Path);
        bool foldout = foldouts.TryGetValue(h.Path, out foldout) && foldout;
        foldout = EditorGUILayout.Foldout(foldout, text);
        foldouts[h.Path] = foldout;
        if (foldout)
        {
            ++EditorGUI.indentLevel;
            foreach (var dep in h.dependencies)
                DrawAssetHandle(dep);
            --EditorGUI.indentLevel;
        }
    }

    static string Status(AssetHandle h)
    {
        if (h.WaitForDispose)
            return "WD";
        else if (h.AleadyDisposed)
            return "AD";
        else
            return "";
    }
}
