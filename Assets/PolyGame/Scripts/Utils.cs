using UnityEngine;
using UnityEngine.Rendering;
using System.Text;

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
}

public class Config
{
    public const float CameraDistance = 1001f;
    public static Vector2Int SnapshotSize = new Vector2Int(256, 256);
}

public class ShaderFeatures
{
    public const string _USE_VERT_COLOR = "_USE_VERT_COLOR";
    public const string _GREYSCALE = "_GREYSCALE";
}

public class Prefabs
{
    public const string PuzzleCamera = "Prefabs/PuzzleCamera";
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
