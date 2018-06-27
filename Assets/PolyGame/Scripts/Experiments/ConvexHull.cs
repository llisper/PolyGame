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

	bool ran;
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
		convexHullVertices.Clear();
		vertices = vertices.Distinct().ToList();
		Calculate(vertices);
		ran = true;
	}

	[ContextMenu("Run")]
	void Run()
	{
		vertices.Clear();
		for (int i = 0; i < transform.childCount; ++i)
		{
			var child = transform.GetChild(i);
			var mesh = child.GetComponent<MeshFilter>().sharedMesh;
			vertices.AddRange(Array.ConvertAll(mesh.vertices, v => (Vector2)(v + child.localPosition)));
		}
		vertices = vertices.Distinct().ToList();
		convexHullVertices.Clear();
		Calculate(vertices);
		ran = true;
	}

	void Calculate(List<Vector2> vertices)
	{
		if (vertices.Count < 3)
			return;

		int lowest = FindLowestPoint(vertices);
		lowestPoint = vertices[lowest];
		vertices.RemoveAt(lowest);
		SortByAngle(vertices, lowestPoint);

		Stack<Vector2> stack = new Stack<Vector2>();
		stack.Push(lowestPoint);
		stack.Push(vertices[0]);

		for (int i = 1; i < vertices.Count; ++i)
		{
			var p2 = vertices[i];
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
		Debug.Log("Convex Hull Vertices Count: " + convexHullVertices.Count);
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
				return 0;
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
		if (!ran)
			return;

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(lowestPoint, 7f);

		Gizmos.color = Color.yellow;
		if (svIndex == 0)
		{
			for (int i = 0; i < vertices.Count; ++i)
				Gizmos.DrawSphere(vertices[i], 7f);
		}
		else
		{
			for (int i = 0; i < svIndex; ++i)
				Gizmos.DrawSphere(vertices[i], 7f);
		}

		if (convexHullVertices.Count > 1)
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < convexHullVertices.Count; ++i)
			{
				int p = i == 0 ? convexHullVertices.Count - 1 : i -1;
				Gizmos.DrawLine(convexHullVertices[p], convexHullVertices[i]);
			}
		}	
	}
}
