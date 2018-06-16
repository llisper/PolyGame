using UnityEngine;
using System.Collections.Generic;

public class PolyGraphBehaviour : MonoBehaviour
{
    public Vector2Int size;
    [HideInInspector]
    public List<Triangle> triangles = new List<Triangle>();
    [HideInInspector]
    public List<Region> regions = new List<Region>();

    public long PointHash(Vector2 p)
    {
        return PolyGraph.PointHash(p, size);
    }
}
