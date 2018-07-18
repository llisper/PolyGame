using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using ResourceModule;
using ResourceModule.Hotfix;

public static class GenerateVersion
{
    public static void Generate(string versionName, string cdn, BuildTarget buildTarget)
    {
        var ver = new Version.Config();
        ver.name = versionName;
        ver.cdn = cdn;
        string text = JsonUtility.ToJson(ver, true);

        string outputPath = PathRouter.ProductPath + buildTarget;
        Directory.CreateDirectory(outputPath);
        File.WriteAllText(outputPath + '/' + PathRouter.Version, text, Encoding.UTF8);
        Debug.LogFormat("Generate version({0}) for {1}", versionName, buildTarget);
    }
}
