using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using ResourceModule;
using ResourceModule.Hotfix;

public static class GenerateVersion
{
    public static void Generate(Version.Config vconf, BuildTarget buildTarget)
    {
        string text = JsonUtility.ToJson(vconf, true);
        string outputPath = PathRouter.ProductPath + buildTarget;
        Directory.CreateDirectory(outputPath);
        File.WriteAllText(outputPath + '/' + PathRouter.Version, text, Encoding.UTF8);
        Debug.LogFormat("Generate version({0}) for {1}", vconf.name, buildTarget);
    }
}
