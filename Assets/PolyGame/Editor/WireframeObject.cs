using UnityEngine;
using UnityEditor;
using Battlehub.Wireframe;

class WireframeObject
{
    [MenuItem("Tools/WireframeTest")]
    static void Generate()
    {
        var target = Selection.activeGameObject;
        if (null == target)
            return;

        var go = Object.Instantiate<GameObject>(target);
        go.name = target.name + "_Wireframe";
        var meshFilters = go.GetComponentsInChildren<MeshFilter>();
        var mat = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;
        var newMat = Object.Instantiate(mat);
        newMat.name = mat.name;
        newMat.shader = Shader.Find("Battlehub/Wireframe");
        newMat.SetTexture("_MainTex", null);
        newMat.SetColor("_Color", Color.grey);

        foreach (var meshFilter in meshFilters)
        {
            var mesh = Object.Instantiate(meshFilter.sharedMesh);
            mesh.name = meshFilter.sharedMesh.name;
            Barycentric.CalculateBarycentric(mesh);
            meshFilter.sharedMesh = mesh;
            meshFilter.GetComponent<MeshRenderer>().sharedMaterial = newMat;
        }
    }
}
