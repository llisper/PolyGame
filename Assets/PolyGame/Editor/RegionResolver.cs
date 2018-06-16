using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

class RegionResolver
{
    PolyGraphBehaviour graph;
    Dictionary<Triangle, Region> tri2region = new Dictionary<Triangle, Region>();

    public RegionResolver(PolyGraphBehaviour graph)
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
    }

    public static void Resolve(PolyGraphBehaviour graph)
    {
        var resolver = new RegionResolver(graph);
        resolver.Resolve();
    }

    void Collect()
    {
        for (int i = 0; i < graph.transform.childCount; ++i)
        {
            var region = new Region();
            graph.regions.Add(region);
            var child = graph.transform.GetChild(i);
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
                    vertices = new Vector2[] { p0, p1, p2 },
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
                var regionB = graph.regions[i];
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
}