using UnityEngine;
using System;
using System.Collections.Generic;

[Obsolete]
public class PolyGraphBehaviour : MonoBehaviour
{
    public Vector2Int size;
    public int subRegionCount = 14;

    // [HideInInspector]
    public List<Triangle> triangles = new List<Triangle>();
    // [HideInInspector]
    public List<Region> regions = new List<Region>();

    List<List<Region>> subRegions = new List<List<Region>>();
    Queue<Region> queue = new Queue<Region>();
    HashSet<Region> finished = new HashSet<Region>();

    public long PointHash(Vector2 p)
    {
        // return PolyGraph.PointHash(p, size);
        return 0;
    }

    void Awake()
    {
        // CalculateSubRegions();
        // GenerateSubRegionMeshes();
    }

    /*
    void CalculateSubRegions()
    {
        queue.Enqueue(regions[0]);
        var currentSubRegion = new List<Region>();
        while (queue.Count > 0)
        {
            var region = queue.Dequeue();
            if (currentSubRegion.Count >= subRegionCount)
            {
                subRegions.Add(currentSubRegion);
                currentSubRegion = new List<Region>();
                queue.Clear();
                queue.Enqueue(region);
            }
            else
            {
                finished.Add(region);
                currentSubRegion.Add(region);

                foreach (int i in region.adjacents)
                {
                    var next = regions[i];
                    if (!finished.Contains(next) && !queue.Contains(next))
                    {
                        queue.Enqueue(next);
                        finished.Add(next);
                    }
                }
            }
        }

        subRegions.Add(currentSubRegion);
        currentSubRegion = null;
    }

    void GenerateSubRegionMeshes()
    {
        var parent = new GameObject("Subregions");
        // parent.transform.SetParent(transform);

        for (int i = 0; i < subRegions.Count; ++i)
        {
            var go = new GameObject(i.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent.transform);
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> tris = new List<int>();
            int j = 0;
            foreach (var region in subRegions[i])
            {
                foreach (var tri in region.triangles)
                {
                    vertices.AddRange(Array.ConvertAll(triangles[tri].vertices, v => (Vector3)v));
                    tris.Add(j);
                    tris.Add(j + 1);
                    tris.Add(j + 2);
                    j += 3;
                }
            }
            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = tris.ToArray();
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
    */
}
