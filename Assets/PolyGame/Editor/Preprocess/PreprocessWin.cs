using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

class PreprocessWin : EditorWindow
{
    [MenuItem("[PolyGame]/Preprocess/Open")]
    public static void ShowWindow()
    {
        GetWindow<PreprocessWin>().Show();
    }

    class Item
    {
        public string name;
        public bool isPixel;
        public Preprocess.ImporterArgs args = new Preprocess.ImporterArgs();
    }

    string path;
    string filter;
    Vector2 scrollPosition;
    List<Item> items = new List<Item>();

    void Awake()
    {
        path = Application.dataPath + '/' + Paths.AssetArtworksNoPrefix + '/';
    }

    bool HasImported(Item item)
    {
        string folder = string.Format("{0}/{1}/{2}", Application.dataPath, Paths.AssetResArtworksNoPrefix, item.name);
        return Directory.Exists(folder);
    }

    void Process(params Item[] selectedItems)
    {
        if (selectedItems.Length == 0)
            return;

        var sb = new StringBuilder("");
        sb.Append("Artwork(s) below are going to be pre-processed.\n")
          .Append("Any mesh modifications made before will be discarded.\n")
          .Append("Continue ?\n");

        foreach (var i in selectedItems)
            sb.AppendFormat("  {0}\n", i.name);

        if (EditorUtility.DisplayDialog("Confirm pre-processing", sb.ToString(), "Continue!", "Think again..."))
        {
            foreach (var i in selectedItems)
            {
                try
                {
                    EditorUtility.DisplayProgressBar("Pre-process", i.name, 0);
                    Preprocess.Process(i.name, i.args);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    void UpdateItems()
    {
        string[] names = Array.ConvertAll(Directory.GetDirectories(path), v => Path.GetFileName(v));
        items.RemoveAll(v => Array.IndexOf(names, v.name) < 0);
        foreach (string n in names)
        {
            if (items.FindIndex(v => v.name == n) < 0)
            {
                var newItem = new Item() { name = n };
                string pixelPath = string.Format(
                    "{0}/{1}/{2}/{2}{3}",
                    Application.dataPath,
                    Paths.AssetArtworksNoPrefix,
                    n,
                    PixelGraphImporter.Suffix);
                newItem.isPixel = File.Exists(pixelPath);
                items.Add(newItem);
            }
        }
    }

    void OnGUI()
    {
        UpdateItems();
        EditorGUIUtility.labelWidth = 104;
        EditorGUILayout.LabelField("Root", Paths.AssetArtworks);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pre-process", GUILayout.Width(100));
        if (GUILayout.Button("all", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            Process(items.ToArray());
            return;
        }
        if (GUILayout.Button("not-imported only", EditorStyles.miniButton, GUILayout.Width(120)))
        {
            Process(items.Where(v => !HasImported(v)).ToArray());
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        filter = EditorGUILayout.TextField("Filter", filter);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(new string('-', 100));
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < items.Count; ++i)
        {
            var item = items[i];
            string displayName = item.name;
            if (!HasImported(item))
                displayName += "(not-imported)";

            if (!string.IsNullOrEmpty(filter) &&
                -1 == displayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField(displayName, GUILayout.Width(250));
            if (GUILayout.Button("pre-process", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                Process(item);
                return;
            }

            if (item.isPixel)
                item.args.useVertColor = EditorGUILayout.ToggleLeft("use vertex color", item.args.useVertColor);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.LabelField(new string('-', 100));
    }
}
