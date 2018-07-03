using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class WireframeCreator
{
    class EdgeIndex
    {
        public Edge edge;
        public List<int> index;
        public EdgeIndex(Edge e, List<int> i) { edge = e; index = i; }
    }

    public static void Create(PolyGraph graph, float width = Config.wireframeWidth, Color? color = null)
    {
        if (width <= 0f)
            throw new Exception("Width must greater than 0!");

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<EdgeIndex> edgeIndex = new List<EdgeIndex>();
        foreach (var region in graph.regions)
        {
            foreach (var edge in region.borderEdges)
            {
                var ei = edgeIndex.Find(v => v.edge.EqualTo(edge));
                if (null != ei)
                {
                    Debug.Log("Find existed edge");
                    edge.wireframeTriangles = ei.index;
                    continue;
                }

                Vector3 v0 = new Vector3(edge.v0.x, edge.v0.y, 0f);
                Vector3 v1 = new Vector3(edge.v1.x, edge.v1.y, 0f);

                Vector3 d = Vector3.Cross(Vector3.back, v1 - v0).normalized * 0.5f * width;
                Vector3 p0 = v0 + d;
                Vector3 p1 = v0 - d;
                Vector3 p2 = v1 + d;
                Vector3 p3 = v1 - d;

                int start = verts.Count;
                verts.Add(p0);
                verts.Add(p1);
                verts.Add(p2);
                verts.Add(p3);
                List<int> index = new List<int>();
                if (Vector3.Cross(p2 - p0, p3 - p0).z < 0)
                {
                    index.Add(start);
                    index.Add(start + 2);
                    index.Add(start + 3);
                    index.Add(start);
                    index.Add(start + 3);
                    index.Add(start + 1);
                }
                else
                {
                    index.Add(start);
                    index.Add(start + 1);
                    index.Add(start + 3);
                    index.Add(start);
                    index.Add(start + 3);
                    index.Add(start + 2);
                }
                edge.wireframeTriangles = index;
                tris.AddRange(index);
                edgeIndex.Add(new EdgeIndex(edge, index));
            }
        }

        var wireframeObject = new GameObject(
            graph.name + "Wireframe",
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(PuzzleWireframe));
        wireframeObject.layer = Layers.Debris;

        var mesh = new Mesh();
        mesh.name = graph.name + "Wireframe";
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        Color c = color.HasValue ? color.Value : Config.wireframeColor;
        mesh.colors = Enumerable.Repeat(c, verts.Count).ToArray();
        MeshUtility.Optimize(mesh);
        string savePath = string.Format("{0}/{1}/Meshes/{2}.prefab", Paths.AssetArtworks, graph.name, mesh.name);
        AssetDatabase.CreateAsset(mesh, savePath);

        var meshFilter = wireframeObject.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        var renderer = wireframeObject.GetComponent<MeshRenderer>();
        Utils.SetupMeshRenderer(renderer);
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Paths.PolyWireframe);

        string prefabPath = string.Format("{0}/{1}/{2}.prefab", Paths.AssetResArtworks, graph.name, wireframeObject.name);
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, wireframeObject);
        PrefabUtility.ReplacePrefab(wireframeObject, prefab, ReplacePrefabOptions.ConnectToPrefab);

        GameObject.DestroyImmediate(wireframeObject);
    }

    [MenuItem("Tools/Others/Regenerate Wireframes")]
    static void RegenerateWireframes()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
        for (int g = 0; g < guids.Length; ++g)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            if (path.Contains("Wireframe"))
                continue;

            EditorUtility.DisplayProgressBar("Update Wireframe", path, (float)g / guids.Length);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var go = GameObject.Instantiate(prefab);
                var graph = go.GetComponent<PolyGraph>();
                graph.name = Path.GetFileNameWithoutExtension(path);
                Create(graph);
                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
                GameObject.DestroyImmediate(go);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        AssetDatabase.SaveAssets();
    }
}
