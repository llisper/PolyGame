using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text;
using System.Collections.Generic;

static class RegionCombiner
{
    public static GameObject Combine(PolyGraph graph, List<GameObject> gameObjects)
    {
        if (!ConnectedCheck(graph, gameObjects))
            return null;

        int index = NextIndex(graph.transform);
        CombineInstance[] combine = new CombineInstance[gameObjects.Count];
        for (int i = 0; i < combine.Length; ++i)
        {
            var meshFilter = gameObjects[i].GetComponent<MeshFilter>();
            combine[i].mesh = meshFilter.sharedMesh;
            combine[i].transform = meshFilter.transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.name = "mesh_" + index;
        mesh.CombineMeshes(combine);
        Vector3 centroid;
        mesh.vertices = Centralize(mesh.vertices, out centroid);
        mesh.RecalculateBounds();

        MeshUtility.Optimize(mesh);
        string savePath = string.Format("{0}/{1}/Meshes/{2}.prefab", Paths.AssetArtworks, graph.name, mesh.name);
        AssetDatabase.CreateAsset(mesh, savePath);
        AssetDatabase.SaveAssets();

        var go = new GameObject(
            index.ToString(),
            typeof(MeshFilter),
            typeof(MeshRenderer), 
            typeof(MeshCollider));
        Utils.SetupMeshRenderer(go);
        go.tag = Tags.Debris;
        go.layer = Layers.Debris;
        go.transform.parent = graph.transform;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.localPosition = centroid;
        go.GetComponent<MeshCollider>().sharedMesh = mesh;
        return go;
    }

    public static int NextIndex(Transform root)
    {
        int max = 0;
        for (int i = 0; i < root.childCount; ++i)
        {
            int index = int.Parse(root.GetChild(i).name);
            if (index > max)
                max = index;
        }
        return max + 1;
    }

    static Vector3[] Centralize(Vector3[] vertices, out Vector3 centroid)
    {
        centroid = new Vector3();
        for (int i = 0; i < vertices.Length; ++i)
            centroid += vertices[i];
        centroid /= vertices.Length;
        for (int i = 0; i < vertices.Length; ++i)
            vertices[i] -= centroid;
        return vertices;
    }

    static bool ConnectedCheck(PolyGraph graph, List<GameObject> gameObjects)
    {
        var xforms = gameObjects.ConvertAll(v => v.transform).ToArray();
        var resolver = new RegionResolver(graph);
        resolver.Collect(xforms);
        resolver.CalculateTriangleAdjacents();
        resolver.CalculateRegionAdjacents();

        List<int> regionIndexs = Enumerable.Range(0, resolver.regions.Count).ToList();
        List<List<string>> connectedRegions = new List<List<string>>();
        while (regionIndexs.Count > 0)
        {
            List<string> connected = new List<string>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(regionIndexs[0]);
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                regionIndexs.Remove(i);
                var region = resolver.regions[i];
                connected.Add(region.name);
                foreach (int adj in region.adjacents)
                {
                    if (regionIndexs.Contains(adj) && !queue.Contains(adj))
                        queue.Enqueue(adj);
                }
            }
            connectedRegions.Add(connected);
        }

        if (connectedRegions.Count > 1)
        {
            var log = new StringBuilder("Selected regions are not all connected, connected regions are:\n");
            foreach (var names in connectedRegions)
                log.AppendFormat("{{ {0} }}\n", string.Join(", ", names));
            Debug.LogError(log);
            return false;
        }
        else
        {
            return true;
        }
    }
}