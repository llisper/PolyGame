using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class RayPick
{
    static RayPick()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (null != Camera.current)
        {
            if (null != Event.current)
            {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log("hit: " + hit.transform.name);
                }
            }
        }
    }
}
