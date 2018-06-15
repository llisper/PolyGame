using UnityEngine;
using System;
using System.Collections.Generic;

class RegionAdjacentResolver : MonoBehaviour
{
    public GameObject target;
    public Vector2Int size;

    class Triangle
    {
        public long[] hashes = new long[3];
    }

    class Region
    {
        public List<Triangle> triangles = new List<Triangle>();
        public List<Region> adjacents = new List<Region>();
    }

    List<Region> regions = new List<Region>();
    Dictionary<Triangle, Region> tri2region = new Dictionary<Triangle, Region>();

    public long PointHash(Vector2 p)
    {
        return (long)p.y * size.x + (long)p.x;
    }

    [ContextMenu("Resolve")]
    void Resolve()
    {
        for (int i = 0; i < target.transform.childCount; ++i)
        {
            var region = new Region();
            var child = target.transform.GetChild(i);
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
            int[] tris = mesh.triangles;
            Vector3[] vertices = Array.ConvertAll(mesh.vertices, v => v + child.localPosition);
            for (int j = 0; j < tris.Length; j += 3)
            {
                var triangle = new Triangle();
                triangle.hashes[0] = PointHash(vertices[tris[j]]);
                triangle.hashes[1] = PointHash(vertices[tris[j + 1]]);
                triangle.hashes[2] = PointHash(vertices[tris[j + 2]]);
                region.triangles.Add(triangle);
                tri2region.Add(triangle, region);
            }
            regions.Add(region);
        }

        for (int i = 0; i < regions.Count; ++i)
        {
            for (int j = i + 1; j < regions.Count; ++j)
            {

            }
        }

        // find adjacents
    }
}