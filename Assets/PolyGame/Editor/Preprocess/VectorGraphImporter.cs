using UnityEngine;
using UnityEditor;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class VectorGraphImporter : Preprocess.Importer
{
    public const string Suffix = ".svg";

    public string Name { get { return graph.name; } }
    public Mesh[] Meshes { get; private set; }
    public Material Material { get { return null; } }
    public GameObject GameObject { get { return mainObj; } }

    PolyGraph graph;
    GameObject mainObj;
    List<Vector2[]> triangles = new List<Vector2[]>();
    public List<Color> colors = new List<Color>();

    public VectorGraphImporter(string name)
    {
        mainObj = new GameObject(name);
        graph = mainObj.AddComponent<PolyGraph>();
    }

    public void Import(Preprocess.ImporterArgs args)
    {
        TimeCount.Measure(ParseSvg);
        TimeCount.Measure(GenerateMesh);
        TimeCount.Measure(GenerateMaterial);
    }

    public void Dispose()
    {
        if (null != mainObj)
            GameObject.DestroyImmediate(mainObj);
    }

    void ParseSvg()
    {
        string path = string.Format(
            "{0}/{1}/{2}/{2}.svg",
            Application.dataPath,
            Paths.AssetArtworksNoPrefix,
            graph.name);

        var document = XDocument.Load(path);
        graph.size = new Vector2Int(
            Number(document.Root.Attribute("width").Value),
            Number(document.Root.Attribute("height").Value));

        foreach (var ele in document.Root.Elements(document.Root.GetDefaultNamespace().GetName("polygon")))
        {
            string styleVal = ele.Attribute("style").Value;
            string pointsVal = ele.Attribute("points").Value;

            var styleMatch = Regex.Match(styleVal, @"opacity:([^;]+);fill:#([^;]+);");
            if (!styleMatch.Success)
                throw new Exception("invalid style: " + styleVal);

            float alpha = float.Parse(styleMatch.Groups[1].Value);
            Color fill = Utils.ColorFromString(styleMatch.Groups[2].Value);
            fill.a = alpha;

            Vector2[] points = Array.ConvertAll(
                pointsVal.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                v => Vector(v));
            if (points.Length != 3)
                throw new Exception("invalid points count: " + pointsVal);

            Vector3 p0 = points[0];
            Vector3 p1 = points[1];
            Vector3 p2 = points[2];
            float cross = Vector3.Cross(p1 - p0, p2 - p0).z;
            if (cross == 0f)
                throw new Exception("Cross Product is zero, we got some degenerated triangles");
            if (cross < 0f)
                triangles.Add(new Vector2[] { p0, p1, p2 });
            else
                triangles.Add(new Vector2[] { p0, p2, p1 });
            colors.Add(fill);
        }
    }

    int Number(string str)
    {
        var match = Regex.Match(str, @"(\d+)");
        if (!match.Success)
            throw new Exception(str + " is not a number");
        return int.Parse(match.Groups[1].Value);
    }

    Vector2 Vector(string str)
    {
        string[] v = str.Split(',');
        return new Vector2(float.Parse(v[0]), graph.size.y - float.Parse(v[1]));
    }

    void GenerateMesh()
    {
        Meshes = new Mesh[triangles.Count];
        for (int i = 0; i < triangles.Count; ++i)
        {
            GameObject triObj = new GameObject(i.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
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
            mesh.colors = new Color[] { colors[i], colors[i], colors[i] };

            triObj.GetComponent<MeshFilter>().mesh = mesh;
            triObj.transform.localPosition = centroid;
            triObj.AddComponent<BoxCollider>();
            Meshes[i] = mesh;
        }
    }

    void GenerateMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(Paths.PolyGraphMat);
        foreach (var renderer in mainObj.GetComponentsInChildren<MeshRenderer>())
            renderer.sharedMaterial = mat;
    }
}
