using UnityEngine;
using UnityEditor;

class TexturePostProcessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Trilinear;
    }
}