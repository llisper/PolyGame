using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

class RegionBreaker
{
    static List<Triangle> triangles = new List<Triangle>();
    static List<Region> regions = new List<Region>();

    [MenuItem("Tools/Break Disconnected Regions")]
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
                var graph = go.GetComponent<PolyGraph>();
                // PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
                GameObject.DestroyImmediate(go);
                break;
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

    // static void Collect(Transform[] xforms)
    // {
    //     for (int i = 0; i < xforms.Length; ++i)
    //     {
    //         var child = xforms[i];
    //         var region = new Region();
    //         region.name = child.name;
    //         regions.Add(region);

    //         var mesh = child.GetComponent<MeshFilter>().sharedMesh;
    //         int[] tris = mesh.triangles;
    //         Vector3[] vertices = Array.ConvertAll(mesh.vertices, v => v + child.localPosition);
    //         for (int j = 0; j < tris.Length; j += 3)
    //         {
    //             Vector2 p0 = vertices[tris[j]];
    //             Vector2 p1 = vertices[tris[j + 1]];
    //             Vector2 p2 = vertices[tris[j + 2]];
    //             var triangle = new Triangle()
    //             {
    //                 region = regions.Count - 1,
    //                 vertices = new Vector2Int[]
    //                 {
    //                     new Vector2Int((int)p0.x, (int)p0.y),
    //                     new Vector2Int((int)p1.x, (int)p1.y),
    //                     new Vector2Int((int)p2.x, (int)p2.y)
    //                 },
    //                 hashes = new long[] { graph.PointHash(p0), graph.PointHash(p1), graph.PointHash(p2) }
    //             };
    //             triangles.Add(triangle);
    //             region.triangles.Add(triangles.Count - 1);
    //             tri2region.Add(triangle, region);
    //         }
    //     }
    // }

}