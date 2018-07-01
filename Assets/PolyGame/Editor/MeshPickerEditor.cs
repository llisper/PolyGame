using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshPicker))]
class MeshPickerEditor : Editor
{
    void OnSceneGUI()
    {
        var script = ((MeshPicker)target);
        if (null != Camera.current)
        {
            if (Event.current.type == EventType.MouseDown ||
                Event.current.type == EventType.MouseMove)
            {
                script.SetHoveringRegion(null);
                var pos = Event.current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(pos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.transform.parent == script.transform)
                    {
                        if (Event.current.type == EventType.MouseDown &&
                            Event.current.button == 1)
                        {
                            script.Toggle(hit.transform.GetComponent<MeshRenderer>());
                            MeshModifier.DoRepaint();
                            Event.current.Use();
                        }
                        script.SetHoveringRegion(hit.transform.name);
                    }
                }
            }
        }
    }
}