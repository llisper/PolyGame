using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SvgXml;

namespace SvgXml
{
    [XmlRoot(ElementName = "polygon", Namespace = "http://www.w3.org/2000/svg")]
    public class Polygon
    {
        [XmlAttribute(AttributeName = "points")]
        public string Points { get; set; }
        [XmlAttribute(AttributeName = "fill")]
        public string Fill { get; set; }
        [XmlAttribute(AttributeName = "stroke")]
        public string Stroke { get; set; }
        [XmlAttribute(AttributeName = "stroke-width")]
        public string Strokewidth { get; set; }
        [XmlAttribute(AttributeName = "stroke-linejoin")]
        public string Strokelinejoin { get; set; }
    }

    [XmlRoot(ElementName = "svg", Namespace = "http://www.w3.org/2000/svg")]
    public class Svg
    {
        [XmlElement(ElementName = "polygon", Namespace = "http://www.w3.org/2000/svg")]
        public List<Polygon> Polygon { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "height")]
        public string Height { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
    }

}

public class VectorGraphImporter : Preprocess.Importer
{
    public const string Suffix = ".svg";

    public string Name { get { return graph.name; } }
    public Mesh[] Meshes { get; private set; }
    public Material Material { get; private set; }
    public GameObject GameObject { get { return mainObj; } }

    PolyGraph graph;
    GameObject mainObj;

    public VectorGraphImporter(string name)
    {
        graph = new PolyGraph() { name = name };
    }

    public void Import()
    {
        ParseSvg();
        GenerateMesh();
        GenerateMaterial();

        var polyGraphBehaviour = mainObj.AddComponent<PolyGraphBehaviour>();
        polyGraphBehaviour.size = graph.size;
        RegionResolver.Resolve(polyGraphBehaviour);
    }

    void ParseSvg()
    {
        string path = string.Format(
            "{0}/{1}/{2}/{2}.svg",
            Application.dataPath,
            Paths.AssetArtworksNoPrefix,
            graph.name);

        Svg svg;
        var serializer = new XmlSerializer(typeof(Svg));
        using (var stream = new FileStream(path, FileMode.Open))
            svg = (Svg)serializer.Deserialize(stream);

        graph.size = new Vector2Int(int.Parse(svg.Width), int.Parse(svg.Height));
        for (int i = 0; i < svg.Polygon.Count; ++i)
        {
            var polygon = svg.Polygon[i];
            var match = Regex.Match(polygon.Points, @"(\d+),(\d+) (\d+),(\d+) (\d+),(\d+)");
            if (!match.Success)
                throw new Exception("Failed to parse svg points " + polygon.Points);

            Vector2 p0 = new Vector2(int.Parse(match.Groups[1].Value), graph.size.y - int.Parse(match.Groups[2].Value));
            Vector2 p2 = new Vector2(int.Parse(match.Groups[3].Value), graph.size.y - int.Parse(match.Groups[4].Value));
            Vector2 p1 = new Vector2(int.Parse(match.Groups[5].Value), graph.size.y - int.Parse(match.Groups[6].Value));
            graph.AddTriangle(p0, p1, p2);

            match = Regex.Match(polygon.Fill, @"rgb\((\d+), (\d+), (\d+)\)");
            if (!match.Success)
                throw new Exception("Failed to parse svg fill color " + polygon.Fill);

            Color color = new Color32(
                byte.Parse(match.Groups[1].Value),
                byte.Parse(match.Groups[2].Value),
                byte.Parse(match.Groups[3].Value), 255);
            graph.fillColors.Add(color);
        }
    }

    void GenerateMesh()
    {
        mainObj = new GameObject(graph.name);
        Meshes = new Mesh[graph.triangles.Count];
        for (int i = 0; i < graph.triangles.Count; ++i)
        {
            GameObject triObj = new GameObject(i.ToString(), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            Utils.SetupMeshRenderer(triObj);
            triObj.tag = Tags.Debris;
            triObj.layer = Layers.Debris;
            triObj.transform.SetParent(mainObj.transform);
            var mesh = new Mesh();
            mesh.name = "mesh_" + i;

            Vector2[] points = graph.triangles[i];
            Vector2 centroid = PolyGraph.GetCentroid(points);
            Vector2[] vertices = new Vector2[3];
            for (int j = 0; j < 3; ++j)
                vertices[j] = points[j] - centroid;

            mesh.vertices = Array.ConvertAll(vertices, v => (Vector3)v);
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.colors = new Color[] { graph.fillColors[i], graph.fillColors[i], graph.fillColors[i] };

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
        Material = mat;

        foreach (var renderer in mainObj.GetComponentsInChildren<MeshRenderer>())
            renderer.sharedMaterial = mat;
    }
}
