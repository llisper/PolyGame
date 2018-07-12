﻿using UnityEngine;

public class Paths
{
    public const string Artworks = "Artworks";
    public const string Configs = "Configs";
    public const string AssetArtworks = "Assets/PolyGame/Art/Artworks";
    public const string AssetArtworksNoPrefix = "PolyGame/Art/Artworks";
    public const string AssetResArtworks = "Assets/Resources/Artworks";
    public const string AssetResArtworksNoPrefix = "Resources/Artworks";
    public const string PolyWireframeMat = "Assets/PolyGame/Art/Materials/PolyWireframe.mat";
    public const string PolyGraphMat = "Assets/PolyGame/Art/Materials/PolyGraph.mat";

    public static string Saves { get { return Application.persistentDataPath + "/Saves"; } }

    public static string ToAssetPath(string absPath)
    {
        return absPath.Replace(Application.dataPath, "Assets");
    }

    public static string SnapshotRes(string name)
    {
        return string.Format(
            "{0}/{1}/{2}/{3}",
            Application.dataPath,
            Paths.AssetArtworksNoPrefix,
            name,
            PuzzleSnapshot.FileName);
    }

    public static string SnapshotSave(string name)
    {
        return string.Format("{0}/{1}/{2}", Saves, name, PuzzleSnapshot.FileName);
    }
}
