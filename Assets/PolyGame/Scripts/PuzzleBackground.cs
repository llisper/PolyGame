using UnityEngine;

public class PuzzleBackground
{
    public static GameObject Create(PolyGraph graph, Bounds backgroundBounds)
    {
        var prefab = Resources.Load<GameObject>(Prefabs.Background);
        var go = GameObject.Instantiate(prefab);

        var bounds = backgroundBounds;
        go.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);
        go.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, Config.Instance.zorder.background);

        var renderer = go.GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", AvarageColor(graph));
        mpb.SetVector("_Bounds", new Vector4(bounds.extents.x, bounds.extents.y));
        renderer.SetPropertyBlock(mpb);

        return go;
    }
    
    static Color AvarageColor(PolyGraph graph)
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
}
