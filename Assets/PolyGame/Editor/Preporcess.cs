using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Delaunay;

public static class Preporcess
{
    static GameObject mainObj;

    public static void SetupMeshRenderer(GameObject go)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (null != renderer)
        {
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.allowOcclusionWhenDynamic = false;
        }
    }

    [MenuItem("Tools/Preprocess/Test")]
    static void Test()
    {
        Process("DeadCells");
        // Process("Bill");
    }

    [MenuItem("Tools/Preprocess/All")]
    static void All()
    {
        try
        {
            string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.Artworks);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string name = Path.GetFileName(dirs[i]);
                EditorUtility.DisplayProgressBar("Preprocess", name, (float)(i + 1) / dirs.Length);
                Process(name);
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

    static void Process(string name)
    {
        var graph = new PolyGraph();
        graph.name = name;
        Clear(graph);
        CreateFolders(graph);
        ReadConfig(graph);
        GenerateTriangles(graph);
        GenerateMesh(graph);
        GenerateMaterial(graph);
        SavePrefabs(graph);
    }

    static void Clear(PolyGraph graph)
    {
        string parent = string.Format("Assets/{0}/{1}/", Paths.Artworks, graph.name);
        DeleteFolder(parent + "Materials");
        DeleteFolder(parent + "Meshes");
        DeleteFolder(Paths.ResourceArtworks + '/' + graph.name);
        AssetDatabase.Refresh();
    }

    static void DeleteFolder(string name)
    {
        if (Directory.Exists(name))
            Directory.Delete(name, true);
    }

    static void CreateFolders(PolyGraph graph)
    {
        string parent = string.Format("Assets/{0}/{1}", Paths.Artworks, graph.name);
        AssetDatabase.CreateFolder(parent, "Materials");
        AssetDatabase.CreateFolder(parent, "Meshes");
        AssetDatabase.CreateFolder(Paths.ResourceArtworks, graph.name);
    }

    static void ReadConfig(PolyGraph graph)
    {
        HashSet<long> hash = new HashSet<long>();
        string path = string.Format("Assets/{0}/{1}/{1}.txt", Paths.Artworks, graph.name);
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
                    float x = float.Parse(match.Groups[1].Value);
                    float y = graph.size.y - float.Parse(match.Groups[2].Value);
                    var p = new Vector2(x, y);
                    graph.points.Add(p);

                    long hashCode = graph.PointHash(p);
                    if (hash.Contains(hashCode))
                        throw new Exception("Duplicated Point Hash!");
                    else
                        hash.Add(hashCode);
                }
            }
        }
    }

    static void GenerateTriangles(PolyGraph graph)
    {
        var colors = new List<uint>();
        for (int i = 0; i < graph.points.Count; ++i)
            colors.Add(0);
        var rect = new Rect(0, 0, graph.size.x, graph.size.y);
        Voronoi v = new Voronoi(graph.points, colors, rect);
        var delaunayTriangulation = v.DelaunayTriangulation();
        var resolver = new TriangleResolver(graph);
        for (int i = 0; i < delaunayTriangulation.Count; ++i)
        {
            var p0 = (Vector2)delaunayTriangulation[i].p0;
            var p1 = (Vector2)delaunayTriangulation[i].p1;
            resolver.AddLine(p0, p1);
        }
        resolver.Resolve();
    }

    static void GenerateMesh(PolyGraph graph)
    {
        mainObj = new GameObject(graph.name);
        for (int i = 0; i < graph.triangles.Count; ++i)
        {
            GameObject triObj = new GameObject(i.ToString(), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            SetupMeshRenderer(triObj);
            triObj.transform.SetParent(mainObj.transform);
            var mesh = new Mesh();
            mesh.name = "mesh_" + i;

            Vector2[] points = graph.triangles[i].vertices;
            Vector2 centroid = graph.triangles[i].centroid;
            Vector2[] vertices = new Vector2[3];
            for (int j = 0; j < 3; ++j)
                vertices[j] = points[j] - centroid;

            mesh.vertices = Array.ConvertAll(vertices, v => (Vector3)v);
            mesh.triangles = new int[] { 0, 1, 2 };

            Vector2[] uv = new Vector2[3];
            for (int j = 0; j < mesh.vertices.Length; ++j)
            {
                uv[j].x = points[j].x / graph.size.x;
                uv[j].y = points[j].y / graph.size.y;
            }
            mesh.uv = uv;

            triObj.GetComponent<MeshFilter>().mesh = mesh;
            triObj.transform.position = centroid;
            triObj.GetComponent<MeshCollider>().sharedMesh = mesh;

            MeshUtility.Optimize(mesh);
            string savePath = string.Format("Assets/{0}/{1}/Meshes/{2}.prefab", Paths.Artworks, graph.name, mesh.name);
            AssetDatabase.CreateAsset(mesh, savePath);
            AssetDatabase.SaveAssets();
        }
    }

    static void GenerateMaterial(PolyGraph graph)
    {
        Material mat = new Material(Shader.Find("PolyGame/PolyS"));
        mat.name = graph.name;
        string path = string.Format("Assets/{0}/{1}/{1}.png", Paths.Artworks, graph.name);
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
        if (null == texture)
            throw new Exception("Failed to load texture " + path);
        mat.SetTexture("_MainTex", texture);

        string savePath = string.Format("Assets/{0}/{1}/Materials/{2}.mat", Paths.Artworks, graph.name, mat.name);
        AssetDatabase.CreateAsset(mat, savePath);
        AssetDatabase.SaveAssets();

        foreach (var renderer in mainObj.GetComponentsInChildren<MeshRenderer>())
            renderer.sharedMaterial = mat;
    }

    static void SavePrefabs(PolyGraph graph)
    {
        SavePrefab(graph.name, ref mainObj);
    }

    static void SavePrefab(string folder, ref GameObject obj)
    {
        string savePath = string.Format("{0}/{1}/{2}.prefab", Paths.ResourceArtworks, folder, obj.name);
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(savePath, obj);
        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);

        GameObject.DestroyImmediate(obj);
        obj = null;
    }
}
