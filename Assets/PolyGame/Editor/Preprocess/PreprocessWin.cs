using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;

class PreprocessWin : EditorWindow
{
    [MenuItem("Tools/Preprocess/Open")]
    public static void ShowWindow()
    {
        GetWindow<PreprocessWin>().Show();
    }

    string path;
    string[] dirs;
    string[] names;
    Vector2 scrollPosition;

    void Awake()
    {
        path = Application.dataPath + '/' + Paths.AssetArtworksNoPrefix + '/';
    }

    bool HasImported(string name)
    {
        string folder = string.Format("{0}/{1}/{2}", Application.dataPath, Paths.AssetResArtworksNoPrefix, name);
        return Directory.Exists(folder);
    }

    void Process(params string[] names)
    {
        if (names.Length == 0)
            return;

        var sb = new StringBuilder("");
        sb.Append("Artwork(s) below are going to be pre-processed.\n")
          .Append("Any mesh modifications made before will be discarded.\n")
          .Append("Continue ?\n");

        foreach (string n in names)
            sb.AppendFormat("  {0}\n", n);

        if (EditorUtility.DisplayDialog("Confirm pre-processing", sb.ToString(), "Continue!", "Think again..."))
        {
            foreach (string n in names)
            {
                try
                {
                    EditorUtility.DisplayProgressBar("Pre-process", n, 0);
                    Preprocess.Process(n);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    void OnGUI()
    {
        dirs = Directory.GetDirectories(path);
        names = Array.ConvertAll(dirs, v => Path.GetFileName(v));

        EditorGUILayout.LabelField("Root", Paths.AssetArtworks);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pre-process", GUILayout.Width(100));
        if (GUILayout.Button("all", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            Process(names);
            return;
        }
        if (GUILayout.Button("not-imported only", EditorStyles.miniButton, GUILayout.Width(120)))
        {
            Process(names.Where(v => !HasImported(v)).ToArray());
            return;
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.LabelField(new string('-', 100));
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < dirs.Length; ++i)
        {
            string name = names[i];
            string displayName = name;
            if (!HasImported(name))
                displayName += "(not-imported)";

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.LabelField(displayName, GUILayout.Width(250));
            if (GUILayout.Button("pre-process", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                Process(name);
                return;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.LabelField(new string('-', 100));
    }
}
