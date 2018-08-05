using UnityEngine;
using ResourceModule;

public class PuzzleBackground
{
    public static GameObject Create(PolyGraph graph, Bounds bounds, bool takingInitialSnapshot = false)
    {
        var prefab = PrefabLoader.Load(Prefabs.Background);
        var go = prefab.Instantiate<GameObject>();
        go.layer = takingInitialSnapshot ? Layers.Snapshot : Layers.Debris;

        bounds = CalculateBounds(bounds);
        go.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
        go.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, Config.Instance.zorder.background);

        var renderer = go.GetComponent<MeshRenderer>();
        Material mat;
        if (Application.isPlaying)
        {
            mat = renderer.material;
        }
        else
        {
            mat = GameObject.Instantiate(renderer.sharedMaterial);
            renderer.sharedMaterial = mat;
        }

        if (null != graph.background)
        {
            mat.EnableKeyword(ShaderFeatures._TEXTURE_BG);
            mat.SetTexture("_MainTex", graph.background);
        }
        else
        {
            if (takingInitialSnapshot)
                mat.EnableKeyword(ShaderFeatures._USE_CIRCLE_ALPHA);
            mat.SetColor("_Color", BackgroundColor(graph));
            mat.SetVector("_Bounds", new Vector4(bounds.extents.x, bounds.extents.y));
        }

        if (takingInitialSnapshot)
            mat.EnableKeyword(ShaderFeatures._GREYSCALE);

        return go;
    }

    public static Color AvarageColor(PolyGraph graph)
    {
        Vector4 c = Vector4.zero;
        int count = 0;
        for (int i = 0; i < graph.transform.childCount; ++i)
        {
            var child = graph.transform.GetChild(i);
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
            var material = child.GetComponent<MeshRenderer>().sharedMaterial;
            int[] tris = mesh.triangles;

            if (material.IsKeywordEnabled(ShaderFeatures._USE_VERT_COLOR))
            {
                Color[] colors = mesh.colors;
                for (int j = 0; j < tris.Length; j += 3, ++count)
                    c += (Vector4)colors[tris[j]];
            }
            else
            {
                Vector3[] verts = mesh.vertices;
                var texture = (Texture2D)material.mainTexture;
                for (int j = 0; j < tris.Length; j += 3, ++count)
                {
                    Vector2[] points = new Vector2[]
                    {
                        child.localPosition + verts[tris[j]],
                        child.localPosition + verts[tris[j + 1]],
                        child.localPosition + verts[tris[j + 2]]
                    };
                    Vector2 centroid = PolyGraph.GetCentroid(points);
                    c += (Vector4)texture.GetPixelBilinear(
                        centroid.x / graph.size.x,
                        centroid.y / graph.size.y);
                }
            }
        }
        c /= count;
        return c;
    }

    static Bounds CalculateBounds(Bounds bounds)
    {
        bounds.extents = bounds.extents * 2f - bounds.extents / Config.Instance.camera.minRangeScale;
        return bounds;
    }

    static Color BackgroundColor(PolyGraph graph)
    {
        ArtCollection.Item item;
        if (ArtCollection.Instance.itemMap.TryGetValue(graph.name, out item))
            return Utils.ColorFromString(item.bgColor);
        else
            return AvarageColor(graph);
    }
}
