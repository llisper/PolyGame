using UnityEngine;

public class Paths
{
    public const string Artworks = "Artworks";
    public const string Configs = "Configs";
    public const string AssetArtworks = "Assets/PolyGame/Art/Artworks";
    public const string AssetArtworksNoPrefix = "PolyGame/Art/Artworks";
    public const string AssetResArtworks = "Assets/Res/Artworks";
    public const string AssetResArtworksNoPrefix = "Res/Artworks";
    public const string PolyWireframeMat = "Assets/PolyGame/Art/Materials/PolyWireframe.mat";
    public const string PolyGraphMat = "Assets/PolyGame/Art/Materials/PolyGraph.mat";

    public const string Snapshot = "snapshot";
    public const string SnapshotFile = "snapshot.png";
    public const string Wireframe = "wireframe";

    public static string Saves { get { return Application.persistentDataPath + "/Saves"; } }
    public static string SnapshotMark { get { return Saves + "/SnapshotMark"; } }

    public static string ToAssetPath(string absPath)
    {
        return absPath.Replace(Application.dataPath, "Assets");
    }

    public static string SnapshotSave(string name)
    {
        return string.Format("{0}/{1}/{2}", Saves, name, SnapshotFile);
    }
}
