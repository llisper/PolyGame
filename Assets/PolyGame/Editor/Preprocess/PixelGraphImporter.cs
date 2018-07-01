using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Delaunay;

public class PixelGraphImporter : Preprocess.Importer
{
    public const string Suffix = ".png";

    public string Name { get { return graph.name; } }
    public Mesh[] Meshes { get; private set; }
    public Material Material { get; private set; }
    public GameObject GameObject { get { return mainObj; } }

    PolyGraph graph;
    GameObject mainObj;
    List<Vector2[]> triangles;
    List<Vector2> points = new List<Vector2>();

    public PixelGraphImporter(string name)
    {
        mainObj = new GameObject(graph.name);
        graph = mainObj.AddComponent<PolyGraph>();
    }

    public void Import()
    {
        ReadConfig();
        GenerateTriangles();
        GenerateMesh();
        GenerateMaterial();
    }

    public void Dispose()
    {
        if (null != mainObj)
            GameObject.DestroyImmediate(mainObj);
    }

    void ReadConfig()
    {
        string path = string.Format("{0}/{1}/{1}.txt", Paths.AssetArtworks, graph.name);
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        using (StringReader reader = new StringReader(asset.text))
        {
            string line = reader.ReadLine();
            Match match = Regex.Match(line, @"([0-9]+)\s+([0-9]+)");
            graph.size = new Vector2Int(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value));

            while (true)
            {
                line = reader.ReadLine();
                if (null == line)
                    break;

                match = Regex.Match(line, @"([0-9.]+), ([0-9.]+)");
                if (match.Success)
                {
                    int x = (int)float.Parse(match.Groups[1].Value);
                    int y = (int)(graph.size.y - float.Parse(match.Groups[2].Value));
                    var p = new Vector2(x, y);
                    if (points.Contains(p))
                        throw new Exception("Duplicated vertex: " + match.Groups[0].Value);
                    else
                        points.Add(p);
                }
            }
        }
    }

    void GenerateTriangles()
    {
        var colors = new List<uint>();
        for (int i = 0; i < points.Count; ++i)
            colors.Add(0);

        var rect = new Rect(0, 0, graph.size.x, graph.size.y);
        Voronoi v = new Voronoi(points, colors, rect);
        var delaunayTriangulation = v.DelaunayTriangulation();
        var resolver = new TriangleResolver(graph.size);
        for (int i = 0; i < delaunayTriangulation.Count; ++i)
        {
            var p0 = (Vector2)delaunayTriangulation[i].p0;
            var p1 = (Vector2)delaunayTriangulation[i].p1;
            resolver.AddEdge(p0, p1);
        }
        resolver.Resolve();
        triangles = resolver.Triangles;
    }

    void GenerateMesh()
    {
        string path = string.Format("{0}/{1}/{1}.png", Paths.AssetArtworks, graph.name);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (null == texture)
            throw new Exception("Failed to load texture " + path);

        Meshes = new Mesh[triangles.Count];
        for (int i = 0; i < triangles.Count; ++i)
        {
            GameObject triObj = new GameObject(i.ToString(), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            Utils.SetupMeshRenderer(triObj);
            triObj.tag = Tags.Debris;
            triObj.layer = Layers.Debris;
            triObj.transform.SetParent(mainObj.transform);
            var mesh = new Mesh();
            mesh.name = "mesh_" + i;

            Vector2[] points = triangles[i];
            Vector2 centroid = PolyGraph.GetCentroid(points);
            Vector2[] vertices = new Vector2[3];
            for (int j = 0; j < 3; ++j)
                vertices[j] = points[j] - centroid;

            mesh.vertices = Array.ConvertAll(vertices, v => (Vector3)v);
            mesh.triangles = new int[] { 0, 1, 2 };

            Color fillColor = texture.GetPixelBilinear(
                centroid.x / graph.size.x,
                centroid.y / graph.size.y);
            mesh.colors = new Color[] { fillColor, fillColor, fillColor };

            triObj.GetComponent<MeshFilter>().mesh = mesh;
            triObj.transform.localPosition = centroid;
            triObj.GetComponent<MeshCollider>().sharedMesh = mesh;
            Meshes[i] = mesh;
        }
    }

    void GenerateMaterial()
    {
        Material mat = new Material(Shader.Find("PolyGame/PolyS"));
        mat.name = graph.name;
        mat.EnableKeyword(ShaderFeatures._USE_VERT_COLOR);
        Material = mat;

        foreach (var renderer in mainObj.GetComponentsInChildren<MeshRenderer>())
            renderer.sharedMaterial = mat;
    }
}