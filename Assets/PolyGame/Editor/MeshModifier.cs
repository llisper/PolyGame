using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class MeshModifier : EditorWindow
{
    public static MeshModifier Instance { get; private set; }

    [MenuItem("Tools/Mesh Modifier")]
    public static void ShowWindow()
    {
        GetWindow<MeshModifier>().Show();
    }

    public static void DoRepaint()
    {
        if (null != Instance)
            Instance.Repaint();
    }

    public class Info : IDisposable
    {
        public GameObject editObj;
        public Material originalMat;
        public Material selectedMat;
        public int vertices;
        public int triangles;
        public int regions;

        public Info(string name)
        {
            string path = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, name);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            editObj = GameObject.Instantiate<GameObject>(prefab);
            editObj.name = name;

            originalMat = editObj.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            selectedMat = UnityEngine.Object.Instantiate<Material>(originalMat);
            selectedMat.name = originalMat.name + "_selected";
            selectedMat.color = Color.green;
            Update();
        }

        public void Update()
        {
            regions = editObj.transform.childCount;
            vertices = triangles = 0;
            foreach (var meshFilter in editObj.GetComponentsInChildren<MeshFilter>())
            {
                vertices += meshFilter.sharedMesh.vertices.Length;
                triangles += meshFilter.sharedMesh.triangles.Length / 3;
            }
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(editObj);
            GameObject.DestroyImmediate(selectedMat);
        }
    }

    int selected;
    string filter;
    string[] names;
    List<string> filteredNames = new List<string>();
    Info info;
    bool unsavedModification;
    GUIStyle labelStyle;
    Stack<Command> undoStack = new Stack<Command>();

    public bool IsEditing { get { return null != info; } }

    void Awake()
    {
        Instance = this;
        string[] dirs = Directory.GetDirectories(Application.dataPath + Paths.AssetResArtworksNoPrefix);
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
        if (null != info)
        {
            ClearUnusedMeshes(info.editObj.name);
            info.Dispose();
            info = null;
            undoStack.Clear();
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
                    info = new Info(options[selected]);
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
        EditorGUILayout.LabelField("name", info.editObj.name);
        EditorGUILayout.LabelField("vertices", info.vertices.ToString());
        EditorGUILayout.LabelField("triangles", info.triangles.ToString());
        EditorGUILayout.LabelField("regions", info.regions.ToString());
        labelStyle.normal.textColor = unsavedModification ? Color.yellow : Color.green;
        EditorGUILayout.LabelField(unsavedModification ? "modified" : "clear", labelStyle);
    }

    void EditObj()
    {
        var meshPicker = info.editObj.GetComponent<MeshPicker>();
        if (null == meshPicker)
        {
            meshPicker = info.editObj.AddComponent<MeshPicker>();
            meshPicker.selectedMat = info.selectedMat;
        }

        if (GUILayout.Button("Select Regions", GUILayout.Width(100f)))
            Selection.activeGameObject = info.editObj;

        if (meshPicker.renderers.Count > 0)
        {
            if (GUILayout.Button("Drop Regions", GUILayout.Width(100f)))
            {
                meshPicker.renderers.ForEach(v => v.gameObject.SetActive(false));
                undoStack.Push(new DropCommand(info, meshPicker.renderers));
                meshPicker.renderers.Clear();
                MarkModified();
            }

            if (GUILayout.Button("Join Regions", GUILayout.Width(100f)))
            {
                var newRegion = RegionCombiner.Combine(info.editObj, meshPicker.renderers.ConvertAll(v => v.gameObject));
                newRegion.GetComponent<MeshRenderer>().sharedMaterial = info.originalMat;
                meshPicker.renderers.ForEach(v => v.gameObject.SetActive(false));
                undoStack.Push(new JoinCommand(info, newRegion, meshPicker.renderers));
                meshPicker.renderers.Clear();
                MarkModified();
            }
        }

        if (undoStack.Count > 0)
        {
            if (GUILayout.Button(string.Format("Undo({0})", undoStack.Count), GUILayout.Width(100f)))
            {
                var command = undoStack.Pop();
                command.Undo();
                MarkModified();
            }
        }
    }

    void Save()
    {
        if (!IsEditing)
            return;

        try
        {
            EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Copying instance", 0f);
            var copy = GameObject.Instantiate<GameObject>(info.editObj);
            copy.name = info.editObj.name;

            EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Destroying inactive meshes", 0.25f);
            GameObject.DestroyImmediate(copy.GetComponent<MeshPicker>());
            var inactiveMeshes = copy.GetComponentsInChildren<MeshFilter>(true).Where(v => !v.gameObject.activeSelf);
            foreach (var m in inactiveMeshes)
                GameObject.DestroyImmediate(m.gameObject);

            EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Resolving regions", 0.5f);
            RegionResolver.Resolve(copy.GetComponent<PolyGraphBehaviour>());

            EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Saving prefab", 0.75f);
            string path = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, copy.name);
            UnityEngine.Object prefab = PrefabUtility.CreatePrefab(path, copy);
            PrefabUtility.ReplacePrefab(copy, prefab, ReplacePrefabOptions.ConnectToPrefab);

            GameObject.DestroyImmediate(copy);
            unsavedModification = false;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    void ClearUnusedMeshes(string name)
    {
        EditorUtility.DisplayProgressBar("ClearUnusedMeshes", name, 0);
        try
        {
            string prefabPath = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, name);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var meshFilters = prefab.GetComponentsInChildren<MeshFilter>();

            string path = string.Format("{0}/{1}/Meshes", Paths.AssetArtworks, name);
            foreach (var guid in AssetDatabase.FindAssets("mesh", new string[] { path }))
            {
                string meshPath = AssetDatabase.GUIDToAssetPath(guid);
                string meshName = Path.GetFileNameWithoutExtension(meshPath);
                if (null == Array.Find(meshFilters, v => v.sharedMesh.name == meshName))
                    AssetDatabase.DeleteAsset(meshPath);
            }
        } 
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    void MarkModified()
    {
        unsavedModification = true;
        if (null != info)
            info.Update();
    }
}
