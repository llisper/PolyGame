using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

class TriangleResolver
{
    class AdjacentPoints
    {
        public Vector2 point;
        public List<Vector2> adjacents = new List<Vector2>();
    }

    PolyGraph graph;
    Dictionary<long, AdjacentPoints> adjacents = new Dictionary<long, AdjacentPoints>();

    public TriangleResolver(PolyGraph graph)
    {
        this.graph = graph;
    }

    public void AddLine(Vector2 p0, Vector2 p1)
    {
        AddPoint(p0, p1);
        AddPoint(p1, p0);
    }

    public void Resolve()
    {
        FindTriangles();
        CalculateTriangleAdjacents();
    }

    void AddPoint(Vector2 p0, Vector2 p1)
    {
        long p0h = graph.PointHash(p0);
        long p1h = graph.PointHash(p1);

        AdjacentPoints ap;
        if (!adjacents.TryGetValue(p0h, out ap))
        {
            ap = new AdjacentPoints() { point = p0 };
            adjacents.Add(p0h, ap);
        }

        if (-1 == ap.adjacents.FindIndex(v => graph.PointHash(v) == p1h))
            ap.adjacents.Add(p1);
    }

    void FindTriangles()
    {
        HashSet<long> finishedPoints = new HashSet<long>();
        foreach (var kv in adjacents)
        {
            long hashCode = kv.Key;
            var p0 = kv.Value.point;
            foreach (var p1 in kv.Value.adjacents)
            {
                long p1h = graph.PointHash(p1);
                if (finishedPoints.Contains(p1h))
                    continue;

                foreach (var p2 in adjacents[p1h].adjacents)
                {
                    long p2h = graph.PointHash(p2);
                    if (finishedPoints.Contains(p2h) && p2h != hashCode)
                        continue;

                    if (-1 != adjacents[p2h].adjacents.FindIndex(v => graph.PointHash(v) == hashCode))
                    {
                        float cross = Vector3.Cross(p1 - p0, p2 - p0).z;
                        if (cross == 0f)
                            throw new Exception("Cross Product is zero, we got some degenerated triangles");
                        if (cross < 0f)
                            graph.AddTriangle(p0, p1, p2);
                    }
                }
            }
            finishedPoints.Add(hashCode);
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

        for (int i = 0; i < graph.triangles.Count; ++i)
        {
            var tri = graph.triangles[i];
            var log = new StringBuilder();
            log.AppendFormat("{0}, adj:{1} => ", i, tri.adjacents.Count);
            for (int j = 0; j < tri.adjacents.Count; ++j)
            {
                log.Append(tri.adjacents[j]);
                if (j < tri.adjacents.Count - 1)
                    log.Append(',');
            }

            if (tri.adjacents.Count <= 3)
                Debug.Log(log);
            else
                Debug.LogError(log);
        }
    }
}
