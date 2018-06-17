using UnityEngine;
using System.Collections.Generic;
using Lean.Touch;

public class UseLeanTouch : MonoBehaviour
{


	void Start ()
    {
        LeanTouch.OnGesture += OnGesture;
        LeanTouch.OnFingerDown += OnFingerDown;
	}

    void OnFingerDown(LeanFinger finger)
    {
        var ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f))
            Debug.Log(hit.transform);
    }

    void OnGesture(List<LeanFinger> fingers)
    {
        fingerStatus.Clear();
        foreach (var finger in fingers)
        {
            fingerStatus.Add(string.Format("[{0}] Age:{1:f2}, Set:{2}, TapCount:{3}, Position:{4}, ScreenDelta:{5}, ScreenDistance:{6}", finger.Index, finger.Age, finger.Set, finger.TapCount, finger.ScreenPosition, finger.ScreenDelta, finger.GetScreenDistance(finger.StartScreenPosition)));
        }
    }

    GUIStyle style;
    List<string> fingerStatus = new List<string>();

    void OnGUI()
    {
        if (null == style)
        {
            style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.black;
        }

        foreach (var s in fingerStatus)
            GUILayout.Label(s, style);
    }
}
