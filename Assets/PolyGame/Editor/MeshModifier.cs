﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class MeshModifier : EditorWindow
{
    public static MeshModifier Instance { get; private set; }

    [MenuItem("[Tools]/Mesh Modifier")]
    public static void ShowWindow()
    {
        GetWindow<MeshModifier>().Show();
    }

    class Info
    {
        public string name;
        public int vertices;
        public int triangles;
        public int regions;

        public Info(GameObject obj)
        {
            Update(obj);
        }

        public void Update(GameObject obj)
        {
            name = obj.name;
            regions = obj.transform.childCount;
            vertices = triangles = 0;
            foreach (var meshFilter in obj.GetComponentsInChildren<MeshFilter>())
            {
                vertices += meshFilter.sharedMesh.vertices.Length;
                triangles += meshFilter.sharedMesh.triangles.Length / 3;
            }
        }
    }

    int selected;
    string filter;
    string[] names;
    List<string> filteredNames = new List<string>();
    GameObject editObj;
    Material selectedMat;
    Info info;
    bool unsavedModification;
    GUIStyle labelStyle;

    public bool IsEditing { get { return null != editObj; } }

    void Awake()
    {
        Instance = this;
        string[] dirs = Directory.GetDirectories(Application.dataPath + Paths.ResourceArtworks.Remove(0, 6));
        names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        labelStyle = new GUIStyle(EditorStyles.boldLabel);
    }

    void OnDestroy()
    {
        ClearCurrent();
        Instance = null;
    }

    void ClearCurrent()
    {
        if (null != editObj)
        {
            ClearUnusedMeshes(editObj.name);
            GameObject.DestroyImmediate(editObj);
            editObj = null;
            info = null;
            if (null != selectedMat)
            {
                UnityEngine.Object.DestroyImmediate(selectedMat);
                selectedMat = null;
            }
        }
    }

    void OnGUI()
    {
        if (!IsEditing)
        {
            string[] options = Options();
            selected = Mathf.Clamp(selected, 0, options.Length - 1);
            EditorGUILayout.BeginHorizontal();
            selected = EditorGUILayout.Popup(selected, options, GUILayout.Width(250f));
            if (GUILayout.Button("Start", EditorStyles.miniButton, GUILayout.Width(60f)))
            {
                if (selected >= 0 && selected < options.Length)
                {
                    string name = options[selected];
                    string path = string.Format("{0}/{1}/{1}.prefab", Paths.ResourceArtworks, name);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    editObj = GameObject.Instantiate<GameObject>(prefab);
                    editObj.name = name;
                    info = new Info(editObj);
                    unsavedModification = false;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Width(60f)))
            {
                Save();
            }
            if (GUILayout.Button("Save&Finish", GUILayout.Width(80f)))
            {
                Save();
                ClearCurrent();
            }
            if (GUILayout.Button("Close Without Saving", GUILayout.Width(140f)) &&
                (!unsavedModification || EditorUtility.DisplayDialog("Close without saving ?", "All unsaved modifications will be discard!", "Do it!", "Think again...")))
            {
                ClearCurrent();
            }
            EditorGUILayout.EndHorizontal();

            if (IsEditing)
            {
                ShowInfo();
                EditorGUILayout.LabelField(new String('-', 50));
                EditObj();
            }
        }

    }

    string[] Options()
    {
        filter = EditorGUILayout.TextField(filter, GUILayout.Width(314f));
        if (string.IsNullOrEmpty(filter))
        {
            return names;
        }
        else
        {
            filteredNames.Clear();
            foreach (string n in names)
            {
                if (n.ToLower().Contains(filter.ToLower()))
                    filteredNames.Add(n);
            }
            return filteredNames.ToArray();
        }
    }

    void ShowInfo()
    {
        EditorGUILayout.LabelField("name", info.name);
        EditorGUILayout.LabelField("vertices", info.vertices.ToString());
        EditorGUILayout.LabelField("triangles", info.triangles.ToString());
        EditorGUILayout.LabelField("regions", info.regions.ToString());
        labelStyle.normal.textColor = unsavedModification ? Color.yellow : Color.green;
        EditorGUILayout.LabelField(unsavedModification ? "modified" : "clear", labelStyle);
    }

    void EditObj()
    {
        var meshPicker = editObj.GetComponent<MeshPicker>();
        if (null == meshPicker)
            meshPicker = editObj.AddComponent<MeshPicker>();

        if (null == selectedMat)
        {
            string path = string.Format("Assets/{0}/{1}/Materials/{1}.mat", Paths.Artworks, editObj.name);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            selectedMat = UnityEngine.Object.Instantiate<Material>(mat);
            selectedMat.color = Color.green;
            meshPicker.selectedMat = selectedMat;
        }

        if (GUILayout.Button("Select Regions", GUILayout.Width(100f)))
            Selection.activeGameObject = editObj;

        if (meshPicker.renderers.Count > 0)
        {
            if (GUILayout.Button("Drop Regions", GUILayout.Width(100f)))
            {
                meshPicker.renderers.ForEach(v => v.gameObject.SetActive(false));
                meshPicker.renderers.Clear();
                unsavedModification = true;
            }

            if (GUILayout.Button("Join Regions", GUILayout.Width(100f)))
            {
                RegionCombiner.Combine(editObj, meshPicker.renderers.ConvertAll(v => v.gameObject));
                meshPicker.renderers.ForEach(v => v.gameObject.SetActive(false));
                meshPicker.renderers.Clear();
                unsavedModification = true;
            }
        }
    }

    void Save()
    {
        if (!IsEditing)
            return;

        var copy = GameObject.Instantiate<GameObject>(editObj);
        copy.name = editObj.name;

        GameObject.DestroyImmediate(copy.GetComponent<MeshPicker>());
        var inactiveMeshes = copy.GetComponentsInChildren<MeshFilter>(true).Where(v => !v.gameObject.activeSelf);
        foreach (var m in inactiveMeshes)
            GameObject.DestroyImmediate(m.gameObject);

        string path = string.Format("{0}/{1}/{1}.prefab", Paths.ResourceArtworks, copy.name);
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(path, copy);
        PrefabUtility.ReplacePrefab(copy, prefab, ReplacePrefabOptions.ConnectToPrefab);

        GameObject.DestroyImmediate(copy);
        unsavedModification = false;
    }

    void ClearUnusedMeshes(string name)
    {
        string prefabPath = string.Format("{0}/{1}/{1}.prefab", Paths.ResourceArtworks, name);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();

        string path = string.Format("Assets/{0}/{1}/Meshes", Paths.Artworks, name);
        foreach (var guid in AssetDatabase.FindAssets("mesh", new string[] { path }))
        {
            string meshPath = AssetDatabase.GUIDToAssetPath(guid);
            string meshName = Path.GetFileNameWithoutExtension(meshPath);
            if (null == Array.Find(meshFilters, v => v.sharedMesh.name == meshName))
                AssetDatabase.DeleteAsset(meshPath);
        }
    }
}
