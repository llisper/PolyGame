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
            var current = Event.current;
            if (current.type == EventType.MouseDown ||
                current.type == EventType.MouseMove)
            {
                script.SetHoveringRegion(null);
                var pos = current.mousePosition;
                var ray = HandleUtility.GUIPointToWorldRay(pos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.transform.parent == script.transform)
                    {
                        if (current.type == EventType.MouseDown &&
                            current.button == 1)
                        {
                            script.Toggle(hit.transform.GetComponent<MeshRenderer>());
                            MeshModifier.DoRepaint();
                            current.Use();
                        }
                        script.SetHoveringRegion(hit.transform.name);
                    }
                }
            }
            MeshModifier.ShortcutCheck(true);
        }
    }
}