using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

static class RegionCombiner
{
    public static GameObject Combine(GameObject root, List<GameObject> gameObjects)
    {
        int index = NextIndex(root);
        CombineInstance[] combine = new CombineInstance[gameObjects.Count];
        for (int i = 0; i < combine.Length; ++i)
        {
            var meshFilter = gameObjects[i].GetComponent<MeshFilter>();
            combine[i].mesh = meshFilter.sharedMesh;
            combine[i].transform = meshFilter.transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.name = "tri_" + index + "_mesh";
        mesh.CombineMeshes(combine);
        Vector3 centroid;
        mesh.vertices = Centralize(mesh.vertices, out centroid);
        mesh.RecalculateBounds();

        MeshUtility.Optimize(mesh);
        string savePath = string.Format("Assets/{0}/{1}/Meshes/{2}.mat", Paths.Artworks, root.name, mesh.name);
        AssetDatabase.CreateAsset(mesh, savePath);
        AssetDatabase.SaveAssets();

        var go = new GameObject(
            "tri_" + index,
            typeof(MeshFilter),
            typeof(MeshRenderer), 
            typeof(MeshCollider));
        go.transform.parent = root.transform;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.position = centroid;
        go.GetComponent<MeshCollider>().sharedMesh = mesh;
        return go;
    }

    static int NextIndex(GameObject root)
    {
        int max = 0;
        for (int i = 0; i < root.transform.childCount; ++i)
        {
            int index = int.Parse(Regex.Match(root.transform.GetChild(i).name, @"tri_([0-9]+)").Groups[1].Value);
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
}