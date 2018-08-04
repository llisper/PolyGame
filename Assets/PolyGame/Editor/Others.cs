using UnityEditor;
using UnityEngine;
using System;
using System.IO;

static class Others
{
    public static void SaveInitialSnapshot(PolyGraph graph)
    {
        string path = string.Format(
            "{0}/{1}/{2}/{3}",
            Application.dataPath,
            Paths.AssetArtworksNoPrefix,
            graph.name,
            Paths.SnapshotFile);

        string saveName = graph.name + '_' + Paths.Snapshot;

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

    [MenuItem("[PolyGame]/Others/Clear Saves")]
    static void ClearSaves()
    {
        if (Directory.Exists(Paths.Saves))
            Directory.Delete(Paths.Saves, true);
        Debug.Log("Clear Saves: " + Paths.Saves);
    }

    [MenuItem("[PolyGame]/Others/Generate Initial Snapshots")]
    static void GenerateInitialSnapshots()
    {
        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        string[] names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        for (int i = 0; i < names.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("GenerateInitialSnapshots", names[i], (float)i / names.Length);
            string graphPath = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, names[i]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(graphPath);
            Others.SaveInitialSnapshot(prefab.GetComponent<PolyGraph>());
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    [MenuItem("[PolyGame]/Others/Change Collider")]
    static void ChangeCollider()
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
            for (int i = 0; i < guids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.IndexOf('_') >= 0)
                    continue;

                EditorUtility.DisplayProgressBar("Change Collider", path, (float)i / guids.Length);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var go = GameObject.Instantiate(prefab);
                for (int j = 0; j < go.transform.childCount; ++j)
                {
                    var child = go.transform.GetChild(j);
                    GameObject.DestroyImmediate(child.GetComponent<MeshCollider>());
                    child.gameObject.AddComponent<BoxCollider>();
                }
                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
                GameObject.DestroyImmediate(go);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("[PolyGame]/Others/Cleanup Vertex Layout")]
    static void CleanupVertexLayout()
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Mesh mesh_", new string[] { Paths.AssetArtworks });
            for (int i = 0; i < guids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Cleanup Vertex Layout", path, (float)i / guids.Length);
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                var verts = mesh.vertices;
                var tris = mesh.triangles;
                var colors = mesh.colors;
                var uv = mesh.uv;
                mesh.Clear(false);
                mesh.vertices = verts;
                mesh.triangles = tris;
                mesh.colors = colors;
                mesh.uv = uv;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
    }

    /*
    [MenuItem("[PolyGame]/Others/Rename Assets")]
    static void RenameAssets()
    {
        try
        {
            string artPath = Application.dataPath + '/' + Paths.AssetArtworksNoPrefix;
            string[] dirs = Directory.GetDirectories(artPath);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string dir = dirs[i];
                string dirname = Path.GetFileName(dir);

                EditorUtility.DisplayProgressBar("Rename Art Assets", "(Meshes folder)" + dirname, (float)i / dirs.Length);
                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}/Meshes", Paths.AssetArtworks, dirname),
                    "meshes");

                EditorUtility.DisplayProgressBar("Rename Art Assets", "(Wireframe mesh)" + dirname, (float)i / dirs.Length);
                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}/meshes/{1}Wireframe.prefab", Paths.AssetArtworks, dirname),
                    string.Format("{0}_wireframe.prefab", dirname.ToLower()));

                foreach (string filePath in Directory.GetFiles(dir))
                {
                    if (filePath.EndsWith(".meta"))
                        continue;

                    EditorUtility.DisplayProgressBar("Rename Art Assets", "(Files)" + filePath, (float)i / dirs.Length);
                    string file = Path.GetFileName(filePath);
                    AssetDatabase.RenameAsset(
                        string.Format("{0}/{1}/{2}", Paths.AssetArtworks, dirname, file),
                        file.ToLower());
                }

                EditorUtility.DisplayProgressBar("Rename Art Assets", dirname, (float)i / dirs.Length);
                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}", Paths.AssetArtworks, dirname),
                    dirname.ToLower());

            }

            string prefabPath = Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix;
            dirs = Directory.GetDirectories(prefabPath);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string dir = dirs[i];
                string dirname = Path.GetFileName(dir);

                EditorUtility.DisplayProgressBar("Rename Prefab Assets", dirname, (float)i / dirs.Length);
                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, dirname),
                    string.Format("{0}.prefab", dirname.ToLower()));

                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}/{1}Snapshot.prefab", Paths.AssetResArtworks, dirname),
                    string.Format("{0}_snapshot.prefab", dirname.ToLower()));

                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}/{1}Wireframe.prefab", Paths.AssetResArtworks, dirname),
                    string.Format("{0}_wireframe.prefab", dirname.ToLower()));

                AssetDatabase.RenameAsset(
                    string.Format("{0}/{1}", Paths.AssetResArtworks, dirname),
                    dirname.ToLower());
            }
        }
        catch (Exception e)
        {
    /*
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static void Check(string error)
    {
        if (!string.IsNullOrEmpty(error))
            Debug.LogError(error);
    }


    [MenuItem("[PolyGame]/Others/Set Snapshot Env")]
    static void SetSnapshotEnv()
    {
        var go = new GameObject("PuzzleSnapshot");
        var snapshot = go.AddComponent<PuzzleSnapshot>();
        snapshot.Init("Animal005");
    }

    [MenuItem("[PolyGame]/Others/UseSharedMat")]
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

    [MenuItem("[PolyGame]/Others/RecalculateSize")]
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

    [MenuItem("[PolyGame]/Others/Update PolyGraph")]
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
