using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Text;
using System.Diagnostics;

public class Utils
{
    public static void Destroy(UnityEngine.Object obj)
    {
        #if UNITY_EDITOR
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(obj);
        else
            UnityEngine.Object.DestroyImmediate(obj);
        #else
        UnityEngine.Object.Destroy(obj);
        #endif // UNITY_EDITOR
    }

    public static string Utf16ToUtf8(string utf16String)
    {
        // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
        byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);
        byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

        // Return UTF8 bytes as ANSI string
        return Encoding.UTF8.GetString(utf8Bytes);
    }

    public static byte[] RemoveBOM(byte[] bytes)
    {
        byte[] ret = bytes;
        if (bytes.Length >= 3 &&
            bytes[0] == 0xef &&
            bytes[1] == 0xbb &&
            bytes[2] == 0xbf)
        {
            ret = new byte[bytes.Length - 3];
            Array.Copy(bytes, 3, ret, 0, bytes.Length - 3);
        }
        return ret;
    }

    public static string ColorToString(Color color)
    {
        Color32 c32 = color;
        if (c32.a != 255)
            return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}", c32.r, c32.g, c32.b, c32.a);
        else
            return string.Format("{0:x2}{1:x2}{2:x2}", c32.r, c32.g, c32.b);
    }

    public static Color ColorFromString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return Color.black;

        byte[] c = new byte[4] { 0, 0, 0, 255 };
        for (int i = 0; i < str.Length - 1; i += 2)
            c[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
        return new Color32(c[0], c[1], c[2], c[3]);
    }

    public static void SetupMeshRenderer(GameObject go)
    {
        var renderer = go.GetComponent<MeshRenderer>();
        if (null != renderer)
            SetupMeshRenderer(renderer);
    }

    public static void SetupMeshRenderer(MeshRenderer renderer)
    {
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.allowOcclusionWhenDynamic = false;
    }

    public static Bounds CalculateBounds(Vector2 pos, float aspect, float orthographicSize)
    {
        Vector2 orthoSize = new Vector2(aspect * orthographicSize, orthographicSize);
        return new Bounds(pos, orthoSize * 2);
    }
}

public class TimeCount : IDisposable
{
    string desc;
    Stopwatch stopwatch;

    public TimeCount(string desc)
    {
        this.desc = desc;
        stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        stopwatch.Stop();
        UnityEngine.Debug.LogFormat(
            "--- {0}: {1:f3}s",
            desc, stopwatch.ElapsedMilliseconds / 1000.0);
    }

    public static TimeCount Start(string desc)
    {
        return new TimeCount(desc);
    }

    public static void Measure(Action action)
    {
        using (new TimeCount(action.Method.Name))
            action();
    }
}

public class ShaderFeatures
{
    public const string _USE_VERT_COLOR = "_USE_VERT_COLOR";
    public const string _GREYSCALE = "_GREYSCALE";
    public const string _USE_CIRCLE_ALPHA = "_USE_CIRCLE_ALPHA";
}

public class Prefabs
{
    public const string PuzzleCamera = "Prefabs/PuzzleCamera";
    public const string Background = "Prefabs/Background";

    public const string UI = "UI/UI";
    public const string PuzzleGroupView = "UI/Widgets/PuzzleGroupView";
    public const string PuzzleItem = "UI/Widgets/PuzzleItem";
}

public class Tags
{
    public const string Debris = "Debris";
}

public class Layers
{
    public const int Debris = 8;
    public const int Snapshot = 9;
}
