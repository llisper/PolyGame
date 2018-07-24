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
    RaycastHit[] hits = new RaycastHit[100];

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

    // GameObject debugObj;

    // void Update()
    // {
    //     if (null == debugObj)
    //     {
    //         debugObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //         debugObj.name = "__TouchDebug";
    //         debugObj.layer = Layers.Debris;
    //         debugObj.transform.SetParent(transform);
    //         debugObj.GetComponent<MeshRenderer>().material.color = Color.red;
    //     }

    //     if (null != mainFinger)
    //     {
    //         var worldPos = PuzzleCamera.Main.ScreenToWorldPoint(mainFinger.ScreenPosition);
    //         worldPos.z = -Config.Instance.camera.distance + 100;
    //         debugObj.transform.position = worldPos;
    //         debugObj.transform.localScale = Vector3.one * TouchVars.raycastRadius * 2;
    //         debugObj.SetActive(true);
    //     }
    //     else
    //     {
    //         debugObj.SetActive(false);
    //     }
    // }

    void OnGesture(List<LeanFinger> fingers)
    {
        if (AllFingersOff(fingers))
        {
            phase = Phase.Picking;
            holdTimer = 0f;
            mainFinger = null;
            if (null != objPicked)
            {
                onObjReleased?.Invoke(objPicked);
                objPicked = null;
            }
            return;
        }

        if (null == mainFinger || !mainFinger.IsActive || !mainFinger.Set)
        {
            if (null != objPicked)
            {
                onObjReleased?.Invoke(objPicked);
                objPicked = null;
            }
            mainFinger = fingers[0];
        }

        if (phase == Phase.Picking)
        {
            if (fingers.Count > 1 || FingerMoved(mainFinger))
            {
                phase = Phase.Update;
            }
            else if ((holdTimer += Time.deltaTime) >= TouchVars.holdThreshold.FloatValue)
            {
                var ray = PuzzleCamera.Main.ScreenPointToRay(mainFinger.ScreenPosition);
                int numOfHits = Physics.SphereCastNonAlloc(
                    ray,
                    TouchVars.raycastRadius,
                    hits,
                    Config.Instance.camera.distance,
                    ~Layers.Debris);

                if (numOfHits > 0)
                {
                    Vector3 origin = ray.origin;
                    origin.z = PuzzleCamera.Main.transform.position.z;
                    Transform selected = null;
                    float distance = float.MaxValue;
                    for (int i = 0; i < numOfHits; ++i)
                    {
                        var xform  = hits[i].transform;
                        if (null == selected)
                        {
                            float newDistance = Vector3.Distance(xform.position, origin);
                            if (newDistance < distance)
                            {
                                selected = xform;
                                distance = newDistance;
                            }
                        }
                    }

                    if (null != selected)
                    {
                        TouchLog.Log("Picking " + selected);
                        if (null != onObjPicked && onObjPicked(selected))
                        {
                            objPicked = selected;
                            TouchLog.Log(objPicked + " Picked");
                        }
                    }

                }
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
