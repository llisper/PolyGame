using UnityEngine;
using ResourceModule;

public class PuzzleBackground
{
    public static GameObject Create(PolyGraph graph, Bounds bounds)
    {
        var prefab = PrefabLoader.Load(Prefabs.Background);
        var go = prefab.Instantiate<GameObject>();

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
        mat.SetColor("_Color", BackgroundColor(graph));
        mat.SetVector("_Bounds", new Vector4(bounds.extents.x, bounds.extents.y));

        return go;
    }

    public static Color AvarageColor(PolyGraph graph)
    {
        Vector4 c = Vector4.zero;
        int count = 0;
        foreach (var meshFilter in graph.GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = meshFilter.sharedMesh;
            int[] tris = mesh.triangles;
            Color[] colors = mesh.colors;
            for (int i = 0; i < tris.Length; i += 3, ++count)
                c += (Vector4)colors[tris[i]];
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
