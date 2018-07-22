using UnityEngine;
using System;
using System.Collections.Generic;
using Lean.Touch;

public class TouchLog : LogDefine<TouchLog> { }

public class PuzzleTouch : MonoBehaviour
{
    public static PuzzleTouch Instance;

    enum Phase
    {
        Picking,
        Update,
    }

    Phase phase = Phase.Picking;
    float holdTimer;
    Transform objPicked;
    LeanFinger mainFinger;

    public static Func<Transform, bool> onObjPicked;
    public static Action<Transform, Vector2> onObjMove;
    public static Action<Transform> onObjReleased;
    public static Action<Vector2> onFingerDrag;
    public static Action<float> onFingerPinched;

    public LeanFinger MainFinger { get { return mainFinger; } }

    void Start()
    {
        Instance = this;
        LeanTouch.OnGesture += OnGesture;
    }

    void OnDestroy()
    {
        LeanTouch.OnGesture -= OnGesture;
        Instance = null;
    }

    void OnGesture(List<LeanFinger> fingers)
    {
        if (AllFingersOff(fingers))
        {
            phase = Phase.Picking;
            holdTimer = 0f;
            mainFinger = null;
            if (null != objPicked)
            {
                TouchLog.Log("All fingers off, release " + objPicked);
                onObjReleased?.Invoke(objPicked);
                objPicked = null;
            }
            return;
        }

        if (null == mainFinger || !mainFinger.IsActive || !mainFinger.Set)
        {
            if (null != objPicked)
            {
                TouchLog.Log("Main finger off, release " + objPicked);
                onObjReleased?.Invoke(objPicked);
                objPicked = null;
            }
            mainFinger = fingers[0];
        }

        if (phase == Phase.Picking)
        {
            if (fingers.Count > 1 || FingerMoved(mainFinger))
            {
                TouchLog.Log("Main finger move or more than one finger touches, go to <Update>");
                phase = Phase.Update;
            }
            else if ((holdTimer += Time.deltaTime) >= TouchVars.holdThreshold.FloatValue)
            {
                TouchLog.Log("Exceed hold threshold, start picking");
                var ray = PuzzleCamera.Main.ScreenPointToRay(mainFinger.ScreenPosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Config.Instance.camera.distance, ~Layers.Debris))
                {
                    var xform = hit.transform;
                    TouchLog.Log("Picking " + xform);
                    if (null != onObjPicked && onObjPicked(xform))
                    {
                        objPicked = xform;
                        TouchLog.Log(objPicked + " picked");
                    }
                }
                TouchLog.Log("Go to <Update>");
                phase = Phase.Update;
            }
        }

        if (phase == Phase.Update)
        {
            if (null == objPicked || fingers.Count > 1)
            {
                Vector2 current = LeanGesture.GetScreenCenter(fingers); 
                Vector2 delta = current - LeanGesture.GetLastScreenCenter(fingers);
                onFingerDrag?.Invoke(delta);

                if (fingers.Count == 2)
                    onFingerPinched?.Invoke(LeanGesture.GetPinchRatio());
            }

            if (null != objPicked && fingers.Count == 1)
            {
                onObjMove?.Invoke(objPicked, mainFinger.ScreenPosition);
            }
        }
    }

    bool AllFingersOff(List<LeanFinger> fingers)
    {
        if (fingers.Count == 0)
            return true;

        for (int i = 0; i < fingers.Count; ++i)
        {
            if (fingers[i].Set)
                return false;
        }
        return true;
    }

    bool FingerMoved(LeanFinger finger)
    {
        float delta = finger.GetScreenDistance(finger.StartScreenPosition);
        return delta >= TouchVars.holdMoveThreshold.FloatValue;
    }
}
