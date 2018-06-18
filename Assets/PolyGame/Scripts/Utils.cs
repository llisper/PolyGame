using UnityEngine;
using UnityEngine.Rendering;

public class Utils
{
    public const float CameraDistance = 1001f;

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

public class Tags
{
    public const string Debris = "Debris";
}

public class Layers
{
    public const int Debris = 8;
}
