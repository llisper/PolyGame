using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

class RegionBreaker
{
    public class Triangle
    {
        public int[] vertices;
        public long[] hashes;
        public List<int> adjacents;
    }

    [MenuItem("[PolyGame]/Break Disconnected Regions")]
    static void BreakDisconnectedRegions()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
        for (int g = 0; g < guids.Length; ++g)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            if (-1 != path.IndexOf('_'))
                continue;

            EditorUtility.DisplayProgressBar("Break Disconnected Regions", path, (float)g / guids.Length);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var go = GameObject.Instantiate(prefab);
                go.name = prefab.name;
                var graph = go.GetComponent<PolyGraph>();
                if (Resolve(graph))
                {
                    PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
                }
                GameObject.DestroyImmediate(go);
            }
            catch (Exception e)
            {
                Debug.LogError(path);
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        AssetDatabase.SaveAssets();
    }

    static bool Resolve(PolyGraph graph)
    {
        int nextIndex = 0;
        var xforms = new Transform[graph.transform.childCount];
        for (int i = 0; i < xforms.Length; ++i)
        {
            var xform = graph.transform.GetChild(i);
            xforms[i] = xform;
            int index = int.Parse(xform.name);
            if (index > nextIndex)
                nextIndex = index;
        }
        ++nextIndex;

        bool modified = false;
        for (int i = 0; i < xforms.Length; ++i)
            modified |= ResolveRegion(graph, xforms[i], ref nextIndex);
        return modified;
    }

    static bool ResolveRegion(PolyGraph graph, Transform xform, ref int nextIndex)
    {
        List<Triangle> triangles = new List<Triangle>();
        List<List<int>> regions = new List<List<int>>();

        var mesh = xform.GetComponent<MeshFilter>().sharedMesh;
        int[] tris = mesh.triangles;
        Color[] colors = mesh.colors;
        Vector2[] uv = mesh.uv;
        Vector3[] verts = Array.ConvertAll(mesh.vertices, v => v + xform.localPosition);

        for (int i = 0; i < tris.Length; i += 3)
        {
            var triangle = new Triangle()
            {
                vertices = new int[] { tris[i], tris[i + 1], tris[i + 2] },
                hashes = new long[] { graph.PointHash(verts[tris[i]]), graph.PointHash(verts[tris[i + 1]]), graph.PointHash(verts[tris[i + 2]]) },
                adjacents = new List<int>()
            };
            triangles.Add(triangle);
        }
        CalculateTriangleAdjacents(triangles);


        List<int> triIndex = Enumerable.Range(0, triangles.Count).ToList();
        while (triIndex.Count > 0)
        {
            List<int> region = new List<int>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(triIndex[0]);
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                triIndex.Remove(i);
                region.Add(i);

                var triangle = triangles[i];
                foreach (int adj in triangle.adjacents)
                {
                    if (triIndex.Contains(adj) && !queue.Contains(adj))
                        queue.Enqueue(adj);
                }
            }
            regions.Add(region);
        }

        if (regions.Count > 1)
        {
            Debug.LogFormat("<color=yellow>{0}: breaking region {1}</color>", graph.name, xform.name);
            var mat = xform.GetComponent<MeshRenderer>().sharedMaterial;
            foreach (var region in regions)
            {
                Debug.LogFormat("<color=green>{0}: create new region {1}</color>", graph.name, nextIndex);
                NewRegion(region, triangles, graph, mat, verts, colors, uv, nextIndex++);
            }

            GameObject.DestroyImmediate(xform.gameObject);
            string meshPath = string.Format("{0}/{1}/Meshes/{2}.prefab", Paths.AssetArtworks, graph.name, mesh.name);
            AssetDatabase.DeleteAsset(meshPath);
            return true;
        }
        else
        {
            return false;
        }
    }

    static void CalculateTriangleAdjacents(List<Triangle> triangles)
    {
        for (int i = 0; i < triangles.Count; ++i)
        {
            var triA = triangles[i];
            for (int j = i + 1; j < triangles.Count && triA.adjacents.Count < 3; ++j)
            {
                var triB = triangles[j];
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
    
    static void NewRegion(
        List<int> region,
        List<Triangle> triangles,
        PolyGraph graph,
        Material mat,
        Vector3[] verts,
        Color[] colors,
        Vector2[] uv,
        int index)
    {
        Vector3[] newVerts = new Vector3[region.Count * 3];
        Color[] newColors = new Color[region.Count * 3];
        for (int i = 0; i < region.Count; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                int k = triangles[region[i]].vertices[j];
                newVerts[i * 3 + j] = verts[k];
                newColors[i * 3 + j] = colors[k];
            }
        }
        Vector3 centroid = PolyGraph.GetCentroid(newVerts);
        int[] newTris = new int[newVerts.Length];
        for (int i = 0; i < newVerts.Length; ++i)
        {
            newVerts[i] -= centroid;
            newTris[i] = i;
        }

        var mesh = new Mesh();
        mesh.name = "mesh_" + index;
        mesh.vertices = newVerts;
        mesh.triangles = newTris;
        mesh.colors = newColors;
        mesh.uv = uv;

        MeshUtility.Optimize(mesh);
        string savePath = string.Format("{0}/{1}/Meshes/{2}.prefab", Paths.AssetArtworks, graph.name, mesh.name);
        AssetDatabase.CreateAsset(mesh, savePath);
        AssetDatabase.SaveAssets();

        GameObject triObj = new GameObject(
            index.ToString(),
            typeof(MeshFilter),
            typeof(MeshRenderer));
        Utils.SetupMeshRenderer(triObj);
        triObj.tag = Tags.Debris;
        triObj.layer = Layers.Debris;
        triObj.transform.SetParent(graph.transform);
        triObj.transform.localPosition = centroid;
        triObj.GetComponent<MeshFilter>().mesh = mesh;
        triObj.GetComponent<MeshRenderer>().sharedMaterial = mat;
        triObj.AddComponent<BoxCollider>();
    }
}