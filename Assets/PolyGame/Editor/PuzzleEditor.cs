using UnityEditor;
using UnityEngine;
using System;
using System.IO;

class PuzzleEditor
{
    [MenuItem("Tools/CompleteInitialSnapshots")]
    static void CompleteInitialSnapshots()
    {
        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        string[] names = Array.ConvertAll(dirs, v => Path.GetFileName(v));
        for (int i = 0; i < names.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("CompleteInitialSnapshots", names[i], (float)i / names.Length);
            string path = string.Format(
                "{0}/{1}/{2}/{3}",
                 Application.dataPath,
                 Paths.AssetArtworksNoPrefix,
                 names[i],
                 PuzzleSnapshot.FileName);
            
            PuzzleSnapshotOneOff.Take(names[i], null, path);

            string graphPath = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, names[i]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(graphPath);
            var go = GameObject.Instantiate(prefab);
            go.GetComponent<PolyGraph>().initialSnapshot = AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.ToAssetPath(path));
            PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
            GameObject.DestroyImmediate(go);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    /*
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