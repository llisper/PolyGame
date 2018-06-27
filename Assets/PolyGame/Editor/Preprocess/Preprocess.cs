using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public static class Preprocess
{
    static GameObject mainObj;

    public interface Importer
    {
        string Name { get; }
        Mesh[] Meshes { get; }
        Material Material { get; }
        GameObject GameObject { get; }

        void Import();
    }

    public static void Process(string name)
    {
        Clear(name);
        CreateFolders(name);
        var importer = CreateImporter(name);
        importer.Import();        
        Save(importer);
    }

    static Importer CreateImporter(string name)
    {
        string path = string.Format("{0}/{1}/{2}/{2}", 
            Application.dataPath, 
            Paths.AssetArtworksNoPrefix, 
            name);

        if (File.Exists(path + PixelGraphImporter.Suffix))
            return new PixelGraphImporter(name);
        else if (File.Exists(path + VectorGraphImporter.Suffix))
            return new VectorGraphImporter(name);
        return null;
    }

    static void Clear(string name)
    {
        string parent = string.Format("{0}/{1}/", Paths.AssetArtworks, name);
        DeleteFolder(parent + "Materials");
        DeleteFolder(parent + "Meshes");
        DeleteFolder(Paths.AssetResArtworks + '/' + name);
        AssetDatabase.Refresh();
    }

    static void DeleteFolder(string name)
    {
        if (Directory.Exists(name))
            Directory.Delete(name, true);
    }

    static void CreateFolders(string name)
    {
        string parent = string.Format("{0}/{1}", Paths.AssetArtworks, name);
        AssetDatabase.CreateFolder(parent, "Materials");
        AssetDatabase.CreateFolder(parent, "Meshes");
        AssetDatabase.CreateFolder(Paths.AssetResArtworks, name);
    }

    static void Save(Importer importer)
    {
        foreach (var mesh in importer.Meshes)
        {
            MeshUtility.Optimize(mesh);
            string meshPath = string.Format("{0}/{1}/Meshes/{2}.prefab", Paths.AssetArtworks, importer.Name, mesh.name);
            AssetDatabase.CreateAsset(mesh, meshPath);
        }

        string matPath = string.Format("{0}/{1}/Materials/{2}.mat", Paths.AssetArtworks, importer.Name, importer.Material.name);
        AssetDatabase.CreateAsset(importer.Material, matPath);

        string prefabPath = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, importer.Name);
        UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, importer.GameObject);
        PrefabUtility.ReplacePrefab(importer.GameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(importer.GameObject);

        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Preprocess/Complete Initial Snapshots")]
    static void CompleteInitialSnapshots()
    {
        Game.CompleteInitialSnapshots();
    }
}
