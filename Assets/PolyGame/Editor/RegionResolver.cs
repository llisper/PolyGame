using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

class RegionResolver
{
    PolyGraph graph;
    Dictionary<Triangle, Region> tri2region = new Dictionary<Triangle, Region>();

    public RegionResolver(PolyGraph graph)
    {
        this.graph = graph;
    }

    public void Resolve()
    {
        graph.triangles.Clear();
        graph.regions.Clear();
        Collect();
        CalculateTriangleAdjacents();
        CalculateRegionAdjacents();
        CalculateRegionBorderEdges();
    }

    public static void Resolve(PolyGraph graph)
    {
        var resolver = new RegionResolver(graph);
        resolver.Resolve();
    }

    void Collect()
    {
        for (int i = 0; i < graph.transform.childCount; ++i)
        {
            var child = graph.transform.GetChild(i);
            var region = new Region();
            region.name = child.name;
            graph.regions.Add(region);

            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
            int[] tris = mesh.triangles;
            Vector3[] vertices = Array.ConvertAll(mesh.vertices, v => v + child.localPosition);
            for (int j = 0; j < tris.Length; j += 3)
            {
                Vector2 p0 = vertices[tris[j]];
                Vector2 p1 = vertices[tris[j + 1]];
                Vector2 p2 = vertices[tris[j + 2]];
                var triangle = new Triangle()
                {
                    region = graph.regions.Count - 1,
                    vertices = new Vector2Int[]
                    {
                        new Vector2Int((int)p0.x, (int)p0.y),
                        new Vector2Int((int)p1.x, (int)p1.y),
                        new Vector2Int((int)p2.x, (int)p2.y)
                    },
                    hashes = new long[] { graph.PointHash(p0), graph.PointHash(p1), graph.PointHash(p2) }
                };
                graph.triangles.Add(triangle);
                region.triangles.Add(graph.triangles.Count - 1);
                tri2region.Add(triangle, region);
            }
        }
    }

    void CalculateTriangleAdjacents()
    {
        for (int i = 0; i < graph.triangles.Count; ++i)
        {
            var triA = graph.triangles[i];
            for (int j = i + 1; j < graph.triangles.Count && triA.adjacents.Count < 3; ++j)
            {
                var triB = graph.triangles[j];
                if (triB.adjacents.Count >= 3)
                    continue;

                if (triA.hashes.Intersect(triB.hashes).Count() == 2)
                {
                    triA.adjacents.Add(j);
                    triB.adjacents.Add(i);
                }
            }
        }
    }

    void CalculateRegionAdjacents()
    {
        for (int i = 0; i < graph.regions.Count; ++i)
        {
            var regionA = graph.regions[i];
            for (int j = i + 1; j < graph.regions.Count; ++j)
            {
                var regionB = graph.regions[j];
                if (IsRegionAdjacent(regionA, regionB))
                {
                    regionA.adjacents.Add(j);
                    regionB.adjacents.Add(i);
                }
            }
        }
    }

    bool IsRegionAdjacent(Region a, Region b)
    {
        foreach (int ta in a.triangles)
        {
            foreach (int tb in b.triangles)
            {
                if (graph.triangles[ta].adjacents.Contains(tb))
                    return true;
            }
        }
        return false;
    }

    void CalculateRegionBorderEdges()
    {
        foreach (var region in graph.regions)
        {
            List<Edge> edges = new List<Edge>();
            List<int> sharedCounts = new List<int>();
            for (int i = 0; i < region.triangles.Count; ++i)
            {
                var tri = graph.triangles[region.triangles[i]];
                CountEdge(tri.vertices[0], tri.vertices[1], edges, sharedCounts);
                CountEdge(tri.vertices[1], tri.vertices[2], edges, sharedCounts);
                CountEdge(tri.vertices[2], tri.vertices[0], edges, sharedCounts);
            }

            for (int i = edges.Count - 1; i >= 0; --i)
            {
                if (sharedCounts[i] > 1)
                    edges.RemoveAt(i);
            }

            region.borderEdges = edges;
        }
    }

    void CountEdge(Vector2Int v0, Vector2Int v1, List<Edge> edges, List<int> sharedCounts)
    {
        int i = edges.FindIndex(val => val.Equals(v0, v1));
        if (i < 0)
        {
            edges.Add(new Edge(v0, v1));
            sharedCounts.Add(1);
        }
        else
        {
            sharedCounts[i] = sharedCounts[i] + 1;
        }
    }
}