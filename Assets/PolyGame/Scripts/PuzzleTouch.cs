﻿using UnityEngine;
using System;
using System.Collections.Generic;
using Lean.Touch;

public class PuzzleTouch : MonoBehaviour
{
    public static PuzzleTouch Instance;

    enum Phase
    {
        Picking,
        Update,
    }

    public float holdThreshold = 0.25f;
    public float holdMoveThreshold = 15f;

    Phase phase = Phase.Picking;
    float holdTimer;
    Transform objPicked;
    LeanFinger mainFinger;

    public static Action<Transform> onObjPicked;
    public static Action<Transform> onObjReleased;
    public static Action<Vector2, Vector2, Transform> onFingerDrag;
    public static Action<float> onFingerPinched;

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
                if (null != onObjReleased)
                    onObjReleased(objPicked);
                objPicked = null;
            }
            return;
        }

        if (null == mainFinger || !mainFinger.IsActive || !mainFinger.Set)
        {
            if (null != objPicked)
            {
                if (null != onObjReleased)
                    onObjReleased(objPicked);
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
            else if ((holdTimer += Time.deltaTime) >= holdThreshold)
            {
                var ray = PuzzleCamera.Main.ScreenPointToRay(mainFinger.ScreenPosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Utils.CameraDistance, ~Layers.Debris))
                {
                    objPicked = hit.transform;
                    if (null != onObjPicked)
                        onObjPicked(objPicked);
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
                if (null != onFingerDrag)
                    onFingerDrag(current, delta, null);

                if (fingers.Count == 2)
                {
                    if (null != onFingerPinched)
                        onFingerPinched(LeanGesture.GetPinchRatio());
                }
            }

            if (null != objPicked && fingers.Count == 1)
            {
                Vector2 delta = mainFinger.ScreenPosition - mainFinger.LastScreenPosition;
                if (null != onFingerDrag)
                    onFingerDrag(mainFinger.ScreenPosition, delta, objPicked);
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
        return finger.GetScreenDistance(finger.StartScreenPosition) >= holdMoveThreshold;
    }
}
