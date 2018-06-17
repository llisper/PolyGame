using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Battlehub.Wireframe;

public class DrawWireframe : MonoBehaviour
{
	public GameObject target;

    void Awake()
    {
		var meshFilters = target.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < combine.Length; ++i)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        var mesh = new Mesh();
        mesh.name = target.name + "_onemesh";
        mesh.CombineMeshes(combine);
		Barycentric.CalculateBarycentric(mesh);

		GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
