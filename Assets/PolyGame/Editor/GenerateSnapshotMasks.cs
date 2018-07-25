using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateSnapshotMasks : EditorWindow
{
    int size = 256;
    float angle = 85f;

    Texture2D[] textures = new Texture2D[6];

    [MenuItem("[PolyGame]/Generate Snapshot Masks")]
    static void Open()
    {
        GetWindow<GenerateSnapshotMasks>().Show();
    }

    void OnGUI()
    {
        EditorGUIUtility.labelWidth = 64f;
        size = EditorGUILayout.IntField("size", size, GUILayout.Width(150f));
        angle = EditorGUILayout.FloatField("angle", angle, GUILayout.Width(150f));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate", GUILayout.Width(100f)))
            Generate();
        if (null != textures[0] && GUILayout.Button("Apply", GUILayout.Width(100f)))
            Apply();
        EditorGUILayout.EndHorizontal();
        ShowTextures();
    }

    void OnDestroy()
    {
        DestroyTextures();
    }

    void Generate()
    {
        DestroyTextures();
        for (int i = 0; i < textures.Length; ++i)
        {
            Texture2D tex2d = new Texture2D(size, size, TextureFormat.RGB24, false);
            Vector2[] p = CalcPoints(i);
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    if (IsInside(x, y, p)) 
                        tex2d.SetPixel(x, y, Color.white);
                    else
                        tex2d.SetPixel(x, y, Color.black);
                }
            }
            tex2d.Apply();
            textures[i] = tex2d;
        }
    }

    Vector2[] CalcPoints(int type)
    {
        Vector2[] p = new Vector2[4];
        float len = size / Mathf.Tan(angle * Mathf.Deg2Rad);
        if (type == 0)
        {
            p[0] = new Vector2(0f, 0f);
            p[1] = new Vector2(0f, size);
            p[2] = new Vector2(size, size);
            p[3] = new Vector2((float)size - len, 0f);
        }
        else if (type == 1)
        {
            p[0] = new Vector2(0f, 0f);
            p[1] = new Vector2(len, size);
            p[2] = new Vector2(size, size);
            p[3] = new Vector2((float)size - len, 0f);
        }
        else if (type == 2)
        {
            p[0] = new Vector2(0f, 0f);
            p[1] = new Vector2(len, size);
            p[2] = new Vector2(size, size);
            p[3] = new Vector2(size, 0f);
        }
        else if (type == 3)
        {
            p[0] = new Vector2(0f, 0f);
            p[1] = new Vector2(0f, size);
            p[2] = new Vector2(size - len, size);
            p[3] = new Vector2(size, 0f);
        }
        else if (type == 4)
        {
            p[0] = new Vector2(len, 0f);
            p[1] = new Vector2(0f, size);
            p[2] = new Vector2(size - len, size);
            p[3] = new Vector2(size, 0f);
        }
        else if (type == 5)
        {
            p[0] = new Vector2(len, 0f);
            p[1] = new Vector2(0f, size);
            p[2] = new Vector2(size, size);
            p[3] = new Vector2(size, 0f);
        }
        return p;

    }

    bool IsInside(int x, int y, Vector2[] points)
    {
        Vector2 p = new Vector2(x, y);
        int len = points.Length;
        for (int i = 0; i < len; ++i)
        {
            Vector2 v1 = points[(i + 1) % len] - points[i];
            Vector2 v2 = p - points[i];
            if (Vector3.Cross(v1, v2).z > 0f)
                return false;
        }
        return true;
    }


    void ShowTextures()
    {
        if (null != textures[0])
        {
            float xStart = 5f;
            float xGap = 10f;
            float yStart = 77f;
            float yGap = 10f;
            float pSize = 96f;

            EditorGUILayout.LabelField("Preview:");
            for (int i = 0; i < textures.Length; ++i)
            {
                var tex2d = textures[i];
                if (null != tex2d)
                {
                    float x = xStart + (i % 3) * pSize + (i % 3) * xGap;
                    float y = yStart + (i / 3) * pSize + (i / 3) * yGap;
                    EditorGUI.DrawPreviewTexture(new Rect(x, y, pSize, pSize), tex2d);
                }
            }
        }
    }

    void DestroyTextures()
    {
        for (int i = 0; i < textures.Length; ++i)
        {
            if (null != textures[i])
            {
                DestroyImmediate(textures[i]);
                textures[i] = null;
            }
        }
    }

    void Apply()
    {
        for (int i = 0; i < textures.Length; ++i)
        {
            var tex2d = textures[i];
            if (null != tex2d)
            {
                byte[] bytes = tex2d.EncodeToPNG();
                string path = string.Format("{0}/SnapshotMask{1}.png", Paths.Masks, i);
                File.WriteAllBytes(path, bytes);
                Debug.Log("Write " + path);

                AssetDatabase.Refresh();

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.SingleChannel;
                importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Trilinear;
            }
        }
        Debug.Log("Done!");
    }
}