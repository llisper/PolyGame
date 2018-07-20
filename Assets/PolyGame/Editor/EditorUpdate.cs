using UnityEngine;
using UnityEditor;
using System;
using System.Diagnostics;
using System.Collections.Generic;

[InitializeOnLoad]
static class EditorUpdate
{
    const int fixedDelta = 100;
    static Action<float> onUpdate;

    static EditorUpdate()
    {
        var stopwatch = Stopwatch.StartNew();
        long last = 0;
        EditorApplication.update += () =>
        {
            long now = stopwatch.ElapsedMilliseconds;
            long delta = now - last;
            if (delta >= fixedDelta)
            {
                onUpdate?.Invoke(fixedDelta / 1000f);
                last = now;
            }
        };
    }

    public static void Add(Action<float> action)
    {
        onUpdate += action;
    }

    public static void Remove(Action<float> action)
    {
        onUpdate -= action;
    }
}