using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

class WireframeCreator
{
    public const float wireframeWidth = 1f;
    public static Color wireframeColor = Color.grey;

    public static void Create(PolyGraph graph, float width = wireframeWidth, Color? color = null)
    {
        if (width <= 0f)
            throw new Exception("Width must greater than 0!");

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        foreach (var region in graph.regions)
        {
            foreach (var edge in region.borderEdges)
            {
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
            }
        }

        var wireframeObject = new GameObject(graph.name + "Wireframe", typeof(MeshFilter), typeof(MeshRenderer));
        wireframeObject.layer = Layers.Debris;

        var mesh = new Mesh();
        mesh.name = graph.name + "Wireframe";
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        Color c = color.HasValue ? color.Value : wireframeColor;
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
        AssetDatabase.SaveAssets();
    }
}
