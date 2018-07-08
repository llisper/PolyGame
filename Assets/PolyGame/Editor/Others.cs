using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

static class Others
{
    public static void SaveInitialSnapshot(PolyGraph graph)
    {
        string path = Paths.SnapshotRes(graph.name);
        string saveName = graph.name + Path.GetFileNameWithoutExtension(PuzzleSnapshot.FileName);

        var go = new GameObject(saveName);
        var holder = go.AddComponent<PuzzleSnapshotHolder>();
        PuzzleSnapshotOneOff.Take(graph, null, path);
        holder.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.ToAssetPath(path));

        string prefabPath = string.Format(
            "{0}/{1}/{2}.prefab",
            Paths.AssetResArtworks,
            graph.name,
            saveName);
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, go);
        PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(go);
    }

    [MenuItem("Tools/Others/Clear Saves")]
    static void ClearSaves()
    {
        if (Directory.Exists(Paths.Saves))
        {
            Directory.Delete(Paths.Saves, true);
            Debug.Log("Clear Saves");
        }
    }

    [MenuItem("Tools/Others/Update Puzzle List")]
    static void UpdatePuzzleList()
    {
        var ui = (GameObject)GameObject.Instantiate(Resources.Load("UI/UI"));
        var menu = (GameObject)GameObject.Instantiate(Resources.Load("UI/MenuPanel"), ui.transform.Find("Canvas/Base"));

        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        menu.GetComponent<MenuPanel>().options = new List<string>(Array.ConvertAll(dirs, v => Path.GetFileName(v)));

        string prefabPath = "Assets/Resources/UI/MenuPanel.prefab";
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, menu);
        PrefabUtility.ReplacePrefab(menu, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(ui);
    }

    /*
    [MenuItem("Tools/Others/UseSharedMat")]
    static void UseSharedMat()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(Paths.PolyGraphMat);
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
        for (int g = 0; g < guids.Length; ++g)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (null == prefab.GetComponent<PolyGraph>())
                continue;

            GameObject go = null;
            try
            {
                EditorUtility.DisplayProgressBar("UseSharedMat", path, (float)g / guids.Length);
                go = GameObject.Instantiate<GameObject>(prefab);
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                    r.sharedMaterial = mat;

                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            finally
            {
                if (null != go)
                    GameObject.DestroyImmediate(go);
                EditorUtility.ClearProgressBar();
            }
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Others/CompleteInitialSnapshots")]
    static void CompleteInitialSnapshots()
    {
        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        string[] names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        for (int i = 0; i < names.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("CompleteInitialSnapshots", names[i], (float)i / names.Length);
            string graphPath = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, names[i]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(graphPath);
            Others.SaveInitialSnapshot(prefab.GetComponent<PolyGraph>());
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Others/RecalculateSize")]
    static void RecalucateSize()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
        for (int g = 0; g < guids.Length; ++g)
        {
            GameObject go = null;
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            if (path.Contains("Wireframe"))
                continue;

            EditorUtility.DisplayProgressBar("Recalculate Size", path, (float)g / guids.Length);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                go = GameObject.Instantiate<GameObject>(prefab);
                var graph = go.GetComponent<PolyGraph>();
                go.name = prefab.name;

                MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; ++i)
                    bounds.Encapsulate(renderers[i].bounds);

                var newSize = new Vector2Int((int)bounds.size.x, (int)bounds.size.y);
                if (newSize.x != bounds.size.x || newSize.y != bounds.size.y)
                {
                    Debug.LogErrorFormat("{0}: bounds.size can't convert to integers, {1}", graph.name);
                    continue;
                }
                graph.size = newSize;

                Vector2 centerOffset = bounds.extents - bounds.center;
                for (int i = 0; i < renderers.Length; ++i)
                    renderers[i].transform.localPosition += (Vector3)centerOffset;

                RegionResolver.Resolve(graph);
                WireframeCreator.Create(graph);
                string ssPath = Paths.SnapshotRes(graph.name);
                PuzzleSnapshotOneOff.Take(graph, null, ssPath);

                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            finally
            {
                if (null != go)
                    GameObject.DestroyImmediate(go);
                EditorUtility.ClearProgressBar();
            }
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Others/Update PolyGraph")]
    static void UpdatePolyGraph()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
        for (int g = 0; g < guids.Length; ++g)
        {
            GameObject go = null;
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            EditorUtility.DisplayProgressBar("Update PolyGraph", path, (float)g / guids.Length);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                go = GameObject.Instantiate<GameObject>(prefab);
                go.name = prefab.name;
                bool modified = UpdatePolyGraph(go);
                modified |= ConvertToUseVertexColor(go);
                if (modified)
                    PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            finally
            {
                if (null != go)
                    GameObject.DestroyImmediate(go);
                EditorUtility.ClearProgressBar();
            }
        }
        AssetDatabase.SaveAssets();
    }

    static bool UpdatePolyGraph(GameObject go)
    {
        var polyGraphBehaviour = go.GetComponent<PolyGraphBehaviour>();
        if (null != polyGraphBehaviour)
        {
            var polyGraph = go.AddComponent<PolyGraph>();
            polyGraph.size = polyGraphBehaviour.size;
            GameObject.DestroyImmediate(polyGraphBehaviour);
            RegionResolver.Resolve(polyGraph);
            WireframeCreator.Create(polyGraph);
            return true;
        }
        else
        {
            return false;
        }
    }

    static bool ConvertToUseVertexColor(GameObject go)
    {
        var mat = go.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        Texture2D texture = null;
        if (mat.HasProperty("_MainTex") && 
            null != (texture = mat.mainTexture as Texture2D))
        {
            foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = meshFilter.sharedMesh;
                Vector3[] verts = mesh.vertices;
                int[] tris = mesh.triangles;
                Vector2[] uv = mesh.uv;

                Vector3[] newVerts = new Vector3[tris.Length];
                int[] newTris = new int[tris.Length];
                for (int i = 0; i < tris.Length; ++i)
                {
                    newVerts[i] = verts[tris[i]];
                    newTris[i] = i;
                }

                Color[] colors = new Color[tris.Length];
                for (int i = 0; i < tris.Length; i += 3)
                {
                    Vector2 centroidUV = PolyGraph.GetCentroid(uv[tris[i]], uv[tris[i + 1]], uv[tris[i + 2]]);
                    Color c = texture.GetPixelBilinear(centroidUV.x, centroidUV.y);
                    colors[i] = colors[i + 1] = colors[i + 2] = c;
                }

                mesh.vertices = newVerts;
                mesh.triangles = newTris;
                mesh.uv = null;
                mesh.colors = colors;
            }

            mat.mainTexture = null;
            mat.EnableKeyword(ShaderFeatures._USE_VERT_COLOR);
            return true;
        }
        else
        {
            return false;
        }
    }
    */
}
