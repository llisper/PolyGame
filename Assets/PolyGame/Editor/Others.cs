using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

static class Others
{
    [MenuItem("Tools/Others/Clear Saves")]
    static void ClearSaves()
    {
        if (Directory.Exists(Paths.Saves))
        {
            Directory.Delete(Paths.Saves, true);
            Debug.Log("Clear Saves");
        }
    }

    [MenuItem("Tools/Others/Update Puzzle List")]
    static void UpdatePuzzleList()
    {
        var ui = (GameObject)GameObject.Instantiate(Resources.Load("UI/UI"));
        var menu = (GameObject)GameObject.Instantiate(Resources.Load("UI/MenuPanel"), ui.transform.Find("Canvas/Base"));

        string[] dirs = Directory.GetDirectories(Application.dataPath + '/' + Paths.AssetResArtworksNoPrefix);
        menu.GetComponent<MenuPanel>().options = new List<string>(Array.ConvertAll(dirs, v => Path.GetFileName(v)));

        string prefabPath = "Assets/Resources/UI/MenuPanel.prefab";
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, menu);
        PrefabUtility.ReplacePrefab(menu, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(ui);
    }

    [MenuItem("Tools/Others/Complete Initial Snapshots")]
    static void CompleteInitialSnapshots()
    {
        Game.CompleteInitialSnapshots();
    }

    [MenuItem("Tools/Others/Convert To Use Vectex Color")]
    static void ConvertToUseVectexColor()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { Paths.AssetResArtworks });
        for (int g = 0; g < guids.Length; ++g)
        {
            GameObject go = null;
            string path = AssetDatabase.GUIDToAssetPath(guids[g]);
            EditorUtility.DisplayProgressBar("ConvertToUseVectexColor", path, (float)g / guids.Length);
            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                go = GameObject.Instantiate<GameObject>(prefab);
                var mat = go.GetComponentInChildren<MeshRenderer>().sharedMaterial;
                var texture = mat.mainTexture as Texture2D;
                if (null != texture)
                {
                    foreach (var meshFilter in go.GetComponentsInChildren<MeshFilter>())
                    {
                        var mesh = meshFilter.sharedMesh;
                        Vector3[] verts = mesh.vertices;
                        int[] tris = mesh.triangles;
                        Vector2[] uv = mesh.uv;

                        Vector3[] newVerts = new Vector3[tris.Length];
                        int[] newTris = new int[tris.Length];
                        for (int i = 0; i < tris.Length; ++i)
                        {
                            newVerts[i] = verts[tris[i]];
                            newTris[i] = i;
                        }

                        Color[] colors = new Color[tris.Length];
                        for (int i = 0; i < tris.Length; i += 3)
                        {
                            Vector2 centroidUV = PolyGraph.GetCentroid(uv[tris[i]], uv[tris[i + 1]], uv[tris[i + 2]]);
                            Color c = texture.GetPixelBilinear(centroidUV.x, centroidUV.y);
                            colors[i] = colors[i + 1] = colors[i + 2] = c;
                        }

                        mesh.vertices = newVerts;
                        mesh.triangles = newTris;
                        mesh.uv = null;
                        mesh.colors = colors;
                    }

                    mat.mainTexture = null;
                    mat.EnableKeyword(ShaderFeatures._USE_VERT_COLOR);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error when processing " + path);
                Debug.LogException(e);
            }
            finally
            {
                if (null != go)
                    GameObject.DestroyImmediate(go);
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
