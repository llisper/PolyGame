using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using ResourceModule;
using ResourceModule.Hotfix;

public static class GenerateFileManifest
{
    public static void Generate(BuildTarget buildTarget)
    {
        string path = PathRouter.ProductPath + buildTarget;
        FileManifest manifest = new FileManifest();
        foreach (string file in Directory.EnumerateFiles(
            path,
            "*.*",
            SearchOption.AllDirectories))
        {
            if (Path.GetExtension(file) == ".manifest")
                continue;

            string name = PathRouter.NormalizePath(file)
                .Remove(0, path.Length + 1);
            byte[] bytes = File.ReadAllBytes(file);
            string md5 = FileManager.GetMd5Hash(bytes);
            manifest.files.Add(new FileEntry()
            {
                name = name,
                md5 = md5,
                size = bytes.Length
            });
        }

        File.WriteAllText(
            path + '/' + PathRouter.FileManifest,
            JsonUtility.ToJson(manifest, true),
            Encoding.UTF8);

        Debug.Log("Generate File Manifest for " + buildTarget);
    }
}