using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

class ImportPostProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;
        if (importer.assetPath.StartsWith(Paths.AssetArtworks))
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Trilinear;
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (path.StartsWith(Paths.AssetArtworks))
            {
                string filename = Path.GetFileName(path);
                if (Regex.IsMatch(filename, @"[A-Z]"))
                {
                    AssetDatabase.RenameAsset(path, filename.ToLower());
                    Debug.LogFormat(
                        "(Case-Check) rename {0} -> {1}/{2}",
                        path,
                        Path.GetDirectoryName(path),
                        filename.ToLower());
                }
            }
        }
    }
}
