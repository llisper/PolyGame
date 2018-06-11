using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

static class RegionCombiner
{
    public static void Combine(List<GameObject> gameObjects)
    {
        int index = gameObjects.ConvertAll(v => int.Parse(Regex.Match(v.name, @"tri_([0-9]+)").Groups[1].Value)).Max();
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

        var go = new GameObject(
            string.Format("tri_{0}(new)", index),
            typeof(MeshFilter),
            typeof(MeshRenderer), 
            typeof(MeshCollider));
        go.transform.parent = gameObjects[0].transform.parent;
        go.GetComponent<MeshFilter>().mesh = mesh;
        go.transform.position = centroid;
        go.GetComponent<MeshCollider>().sharedMesh = mesh;
        go.GetComponent<MeshRenderer>().sharedMaterial = gameObjects[0].GetComponent<MeshRenderer>().sharedMaterial;
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