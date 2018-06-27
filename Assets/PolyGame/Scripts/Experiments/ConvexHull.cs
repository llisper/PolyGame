using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ConvexHull : MonoBehaviour 
{
	public Vector2Int area;
	public int points;
    public int runStep = 1;
    public float drawRadius = 5f;

	Vector2 lowestPoint;
	List<Vector2> vertices = new List<Vector2>();
	List<Vector2> convexHullVertices = new List<Vector2>();

    [ContextMenu("Random Points")]
	void RandomPoints()
	{
		vertices.Clear();
		for (int i = 0; i < points; ++i)
		{
			var point = new Vector2(
				UnityEngine.Random.Range(0, area.x + 1),
				UnityEngine.Random.Range(0, area.y + 1));
			vertices.Add(point);
		}
		vertices = vertices.Distinct().ToList();
	}

	[ContextMenu("Read Mesh")]
	void ReadMesh()
	{
		vertices.Clear();
		for (int i = 0; i < transform.childCount; ++i)
		{
			var child = transform.GetChild(i);
			var mesh = child.GetComponent<MeshFilter>().sharedMesh;
			vertices.AddRange(Array.ConvertAll(mesh.vertices, v => (Vector2)(v + child.localPosition)));
		}
		vertices = vertices.Distinct().ToList();
	}

    int steps;
    bool calculating;
    Vector2 p0, p1, p2;
    Coroutine calcRoutine;
    
    [ContextMenu("CalculateStepThrough")]
    void CalculateStepThrough()
    {
        if (null != calcRoutine)
        {
            StopCoroutine(calcRoutine);
            calcRoutine = null;
        }

        steps = 0;
		convexHullVertices.Clear();
        calculating = true;
        calcRoutine = StartCoroutine(Calculating(new List<Vector2>(vertices)));
    }

    IEnumerator Calculating(List<Vector2> verts)
    {
        if (vertices.Count < 3)
            yield break;

		int lowest = FindLowestPoint(verts);
		lowestPoint = verts[lowest];
		verts.RemoveAt(lowest);
		SortByAngle(verts, lowestPoint);

        convexHullVertices.Add(lowestPoint);
        convexHullVertices.Add(verts[0]);

        int inspectStep = steps;
		for (int i = 1; i < verts.Count; ++i)
		{
			p2 = verts[i];
			while (convexHullVertices.Count > 1)
			{
                p1 = convexHullVertices[convexHullVertices.Count - 1];
				p0 = convexHullVertices[convexHullVertices.Count - 2];

				var p01 = p1 - p0;
				var p02 = p2 - p0;
				float cross = p01.x * p02.y - p01.y * p02.x;

                Debug.LogFormat(
                    "step:{6} p0:{0}, p1:{1}, p2:{2}, p01:{3}, p02:{4}, corss:{5}", 
                    p0, p1, p2, p01, p02, cross, steps);

                while (steps >= inspectStep)
                {
                    if (Input.GetMouseButtonUp(0))
                        inspectStep += runStep;
                    yield return null;
                }
                ++steps;

                if (cross < 0)
                    convexHullVertices.RemoveAt(convexHullVertices.Count - 1);
                else
                    break;
			}
            convexHullVertices.Add(p2);
		}

        calculating = false;
        calcRoutine = null;
    }

    [ContextMenu("Calculate")]
	void Calculate()
	{
        if (null != calcRoutine)
        {
            calculating = false;
            StopCoroutine(calcRoutine);
            calcRoutine = null;
        }

		convexHullVertices.Clear();
        var verts = new List<Vector2>(vertices);
		if (verts.Count < 3)
			return;

		int lowest = FindLowestPoint(verts);
		lowestPoint = verts[lowest];
		verts.RemoveAt(lowest);
		SortByAngle(verts, lowestPoint);

		Stack<Vector2> stack = new Stack<Vector2>();
		stack.Push(lowestPoint);
		stack.Push(verts[0]);

		for (int i = 1; i < verts.Count; ++i)
		{
			var p2 = verts[i];
			while (stack.Count > 1)
			{
				var p1 = stack.Pop();
				var p0 = stack.Peek();

				var p01 = p1 - p0;
				var p02 = p2 - p0;
				float cross = p01.x * p02.y - p01.y * p02.x;

				if (cross > 0)
				{
					stack.Push(p1);
					break;
				}
			}
			stack.Push(p2);
		}

		convexHullVertices.AddRange(stack);
	}

	int FindLowestPoint(List<Vector2> vertices)
	{
		int lowest = 0;
		var current = vertices[0];
		for (int i = 1; i < vertices.Count; ++i)
		{
			var v = vertices[i];
			if (v.y < current.y || (v.y == current.y && v.x < current.x))
			{
				lowest = i;
				current = v;
			}
		}
		return lowest;
	}

	void SortByAngle(List<Vector2> vertices, Vector2 lowestPoint)
	{
		vertices.Sort((x, y) => 
		{ 
			float dx = Vector2.Dot((x - lowestPoint).normalized, Vector2.right);
			float dy = Vector2.Dot((y - lowestPoint).normalized, Vector2.right);
            if (dx > dy)
                return -1;
            else if (dx < dy)
                return 1;
            else
                return x.x < y.x ? -1 : (x.x > y.x ? 1 : 0);
		});
	}

	int svIndex = 0;

	[ContextMenu("Show Sorted Vertices")]
	void ShowSortedVertices()
	{
		StartCoroutine(ShowingSortedVertices());
	}

	IEnumerator ShowingSortedVertices()
	{
		var wait = new WaitForSeconds(0.5f);
		for (svIndex = 0; svIndex < vertices.Count; ++svIndex)
			yield return wait;
	}

	void OnDrawGizmosSelected()
	{
        if (calculating)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(p0, drawRadius + 2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(p1, drawRadius + 2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(p2, drawRadius + 2f);
        }

		Gizmos.color = Color.yellow;
		if (svIndex == 0)
		{
			for (int i = 0; i < vertices.Count; ++i)
				Gizmos.DrawSphere(vertices[i], drawRadius);
		}
		else
		{
			for (int i = 0; i < svIndex; ++i)
				Gizmos.DrawSphere(vertices[i], drawRadius);
		}

		if (convexHullVertices.Count > 1)
		{
			Gizmos.color = Color.green;
			for (int i = 1; i < convexHullVertices.Count; ++i)
				Gizmos.DrawLine(convexHullVertices[i - 1], convexHullVertices[i]);
            if (!calculating)
				Gizmos.DrawLine(convexHullVertices[0], convexHullVertices[convexHullVertices.Count - 1]);
		}	
	}
}
