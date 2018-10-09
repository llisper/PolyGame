using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class MeshModifier : EditorWindow
{
    public static MeshModifier Instance { get; private set; }

    [MenuItem("[PolyGame]/Mesh Modifier")]
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
        public MeshPicker meshPicker;
        public Material originalMat;
        public int vertices;
        public int triangles;
        public int regions;

        public Info(string name)
        {
            string path = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, name);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            editObj = GameObject.Instantiate<GameObject>(prefab);
            editObj.name = name;
            meshPicker = editObj.AddComponent<MeshPicker>();

            originalMat = editObj.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            meshPicker.selectedMat = UnityEngine.Object.Instantiate<Material>(originalMat);
            meshPicker.selectedMat.name = originalMat.name + "_selected";
            meshPicker.selectedMat.color = Color.green;

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
            GameObject.DestroyImmediate(meshPicker.selectedMat);
            GameObject.DestroyImmediate(editObj);
        }
    }

    int selected;
    string filter;
    string[] names;
    Scene scene;
    List<string> filteredNames = new List<string>();
    Info info;
    bool unsavedModification;
    //GUIStyle labelStyle;
    Stack<Command> undoStack = new Stack<Command>();

    public bool IsEditing { get { return null != info; } }

    void Awake()
    {
        Instance = this;
        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        // labelStyle = new GUIStyle(EditorStyles.boldLabel);
    }

    void OnEnable()
    {
        scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    void OnDestroy()
    {
        ClearCurrent();
        Instance = null;
        if (null != scene)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Start.unity", OpenSceneMode.Single);
            EditorSceneManager.CloseScene(scene, true);
        }
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
        EditorGUILayout.LabelField("selected regions", info.meshPicker.renderers.Count.ToString());
        //labelStyle.normal.textColor = unsavedModification ? Color.yellow : Color.green;
        //EditorGUILayout.LabelField(unsavedModification ? "modified" : "clear", labelStyle);
        EditorGUILayout.LabelField(unsavedModification ? "modified" : "clear");
    }

    static KeyCode[] keys = new KeyCode[] { KeyCode.S, KeyCode.C, KeyCode.D, KeyCode.J, KeyCode.U };
    static Dictionary<KeyCode, bool> keyPressed = new Dictionary<KeyCode, bool>()
    {
        { KeyCode.S, false },
        { KeyCode.C, false },
        { KeyCode.D, false },
        { KeyCode.J, false },
        { KeyCode.U, false },
    };

    public static void ShortcutCheck(bool repaint = false)
    {
        var current = Event.current;
        if (current.type == EventType.KeyDown)
        {
            foreach (var k in keys)
            {
                if (current.keyCode == k)
                {
                    keyPressed[k] = true;
                    current.Use();
                    break;
                }
            }
        }

        if (repaint && null != Instance)
            Instance.Repaint();
    }

    static void ClearKeys()
    {
        foreach (var k in keys)
            keyPressed[k] = false;
    }

    void EditObj()
    {
        ShortcutCheck();
        var meshPicker = info.meshPicker;

        if (GUILayout.Button("[S]elect Regions", GUILayout.Width(100f)) || keyPressed[KeyCode.S])
            Selection.activeGameObject = info.editObj;

        if (meshPicker.renderers.Count <= 0)
        {
            // TODO: clip follow vertecies on convex hull            
        }
        else
        {
            if (GUILayout.Button("[C]lear Selection", GUILayout.Width(100f)) || keyPressed[KeyCode.C])
            {
                meshPicker.Clear();
            }
            if (GUILayout.Button("[D]rop Regions", GUILayout.Width(100f)) || keyPressed[KeyCode.D])
            {
                meshPicker.renderers.ForEach(v => v.gameObject.SetActive(false));
                undoStack.Push(new DropCommand(info, meshPicker.renderers));
                meshPicker.renderers.Clear();
                MarkModified();
            }

            if (GUILayout.Button("[J]oin Regions", GUILayout.Width(100f)) || keyPressed[KeyCode.J])
            {
                var newRegion = RegionCombiner.Combine(
                    info.editObj.GetComponent<PolyGraph>(),
                    meshPicker.renderers.ConvertAll(v => v.gameObject));

                if (null != newRegion)
                {
                    newRegion.GetComponent<MeshRenderer>().sharedMaterial = info.originalMat;
                    meshPicker.renderers.ForEach(v => v.gameObject.SetActive(false));
                    undoStack.Push(new JoinCommand(info, newRegion, meshPicker.renderers));
                    meshPicker.renderers.Clear();
                    MarkModified();
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Failed to join regions, check Console for error log", "Got it");
                }
            }
        }

        if (undoStack.Count > 0)
        {
            if (GUILayout.Button(string.Format("[U]ndo({0})", undoStack.Count), GUILayout.Width(100f)) || keyPressed[KeyCode.U])
            {
                var command = undoStack.Pop();
                command.Undo();
                MarkModified();
            }
        }
        ClearKeys();
    }

    void Save()
    {
        if (!IsEditing)
            return;

        try
        {
            using (TimeCount.Start("Saving " + info.editObj.name))
            {
                EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Copying instance", 0f);
                GameObject copy = null;
                using (TimeCount.Start("Copying instance"))
                {
                    copy = GameObject.Instantiate<GameObject>(info.editObj);
                    copy.name = info.editObj.name;
                }

                EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Destroying inactive meshes", 0.25f);
                using (TimeCount.Start("Destroying inactive meshes"))
                {
                    GameObject.DestroyImmediate(copy.GetComponent<MeshPicker>());
                    var inactiveMeshes = copy.GetComponentsInChildren<MeshFilter>(true).Where(v => !v.gameObject.activeSelf);
                    foreach (var m in inactiveMeshes)
                        GameObject.DestroyImmediate(m.gameObject);
                }

                EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Resolving regions & create wireframe", 0.5f);
                var polyGraph = copy.GetComponent<PolyGraph>();
                using (TimeCount.Start("Recenter Graph"))
                    RecenterGraph(polyGraph);
                using (TimeCount.Start("Resolve Regions"))
                    RegionResolver.Resolve(polyGraph);
                using (TimeCount.Start("Create Wireframe"))
                    WireframeCreator.Create(polyGraph);
                using (TimeCount.Start("Saving initial snapshot"))
                    Others.SaveInitialSnapshot(polyGraph);

                EditorUtility.DisplayProgressBar("Saving " + info.editObj.name, "Saving prefab", 0.75f);
                using (TimeCount.Start("Saving prefab"))
                {
                    string path = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, copy.name);
                    UnityEngine.Object prefab = PrefabUtility.CreatePrefab(path, copy);
                    PrefabUtility.ReplacePrefab(copy, prefab, ReplacePrefabOptions.ConnectToPrefab);
                }

                using (TimeCount.Start("Saving assets"))
                {
                    GameObject.DestroyImmediate(copy);
                    AssetDatabase.SaveAssets();
                }
                unsavedModification = false;
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

    void RecenterGraph(PolyGraph graph)
    {
        MeshRenderer[] renderers = graph.GetComponentsInChildren<MeshRenderer>();
        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; ++i)
            bounds.Encapsulate(renderers[i].bounds);

        var newSize = new Vector2Int((int)bounds.size.x, (int)bounds.size.y);
        if (newSize.x != bounds.size.x || newSize.y != bounds.size.y)
        {
            Debug.LogErrorFormat("{0}: bounds.size can't convert to integers, {1}", graph.name);
            return;
        }
        graph.size = newSize;

        Vector2 centerOffset = bounds.extents - bounds.center;
        for (int i = 0; i < renderers.Length; ++i)
            renderers[i].transform.localPosition += (Vector3)centerOffset;
    }
}
