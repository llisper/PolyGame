using UnityEngine;

public class Paths
{
    public const string Artworks = "Artworks";
    public const string AssetArtworks = "Assets/PolyGame/Art/Artworks";
    public const string AssetArtworksNoPrefix = "PolyGame/Art/Artworks";
    public const string AssetResArtworks = "Assets/Resources/Artworks";
    public const string AssetResArtworksNoPrefix = "Resources/Artworks";
    public const string PolyWireframe = "Assets/PolyGame/Art/Materials/PolyWireframe.mat";

    public static string Saves { get { return Application.persistentDataPath + "/Saves"; } }
}
