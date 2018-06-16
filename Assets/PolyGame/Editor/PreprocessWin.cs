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
        path = Application.dataPath + '/' + Paths.Artworks + '/';
    }

    bool HasMark(string name, string mark)
    {
        string p = string.Format("{0}/{1}/{2}", path, name, mark);
        if (File.Exists(p))
            return true;
        return false;
    }

    void RemoveMark(string name, string mark)
    {
        string p = string.Format("{0}/{1}/{2}", path, name, mark);
        if (File.Exists(p))
            File.Delete(p);
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
                    RemoveMark(n, "(update)");
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

        EditorGUILayout.LabelField("Root", Paths.Artworks);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pre-process", GUILayout.Width(100));
        if (GUILayout.Button("all", EditorStyles.miniButton, GUILayout.Width(60)))
        {
            Process(names);
            return;
        }
        if (GUILayout.Button("update-only", EditorStyles.miniButton, GUILayout.Width(90)))
        {
            Process(names.Where(v => HasMark(v, "(update)")).ToArray());
            return;
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.LabelField(new string('-', 100));
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < dirs.Length; ++i)
        {
            string name = names[i];
            string displayName = name;
            if (HasMark(name, "(update)"))
                displayName += "(update)";

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
