using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

class ImportPostProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Trilinear;
    }

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets, 
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            var match = Regex.Match(path, Paths.AssetArtworksNoPrefix + @"/(\w+)/\1\.txt");
            if (match.Success)
            {
                string flagPath = string.Format("{0}/{1}/{2}/(update)", Application.dataPath, Paths.AssetArtworksNoPrefix, match.Groups[1].Value);
                File.WriteAllText(flagPath, "");
            }
        }
    }
}