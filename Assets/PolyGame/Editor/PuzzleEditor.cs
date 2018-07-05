using UnityEditor;
using UnityEngine;

class PuzzleEditor
{
    [MenuItem("Tools/Others/[Test]Change Vertex Color")]
    static void ChangeVertexColorTest()
    {
        var prefab = Resources.Load(Paths.Artworks + "/DeadCells/DeadCellsWireframe");
        var go = (GameObject)GameObject.Instantiate(prefab);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        Color[] colors = mesh.colors;
        for (int i = 0; i < colors.Length; ++i)
            colors[i] = Color.red;
        mesh.colors = colors;
        GameObject.DestroyImmediate(go);
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
}