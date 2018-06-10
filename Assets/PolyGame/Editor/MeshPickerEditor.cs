﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshPicker))]
class MeshPickerEditor : Editor
{
    void OnSceneGUI()
    {
        var script = ((MeshPicker)target);
        if (null != Camera.current && 
            Event.current.type == EventType.MouseDown &&
            Event.current.button == 1)
        {
            var pos = Event.current.mousePosition;
            pos.y = Screen.height - pos.y;
            var ray = Camera.current.ScreenPointToRay(pos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.parent == script.transform)
                    script.Toggle(hit.transform.GetComponent<MeshRenderer>());
            }
        }
    }
}