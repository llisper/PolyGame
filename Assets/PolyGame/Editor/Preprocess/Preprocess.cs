﻿using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public static class Preprocess
{
    public interface Importer : IDisposable
    {
        string Name { get; }
        Mesh[] Meshes { get; }
        Material Material { get; }
        GameObject GameObject { get; }

        void Import();
    }

    public static void Process(string name)
    {
        Debug.LogFormat("--- Preprocess {0} Start ---", name);
        using (TimeCount.Start("Total"))
        {
            Clear(name);
            CreateFolders(name);
            using (var importer = CreateImporter(name))
            {
                Debug.Log(importer.GetType().Name);
                importer.Import();
                var graph = importer.GameObject.GetComponent<PolyGraph>();
                ProcessAfterImport(graph);
                Save(importer);
            }
        }
        Debug.LogFormat("--- Preprocess {0} Finished ---", name);
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
        else
            throw new Exception("No appropriate importer for " + path);
    }

    static void ProcessAfterImport(PolyGraph graph)
    {
        using (TimeCount.Start("Resolve Regions"))
            RegionResolver.Resolve(graph);
        using (TimeCount.Start("Create Wireframe"))
            WireframeCreator.Create(graph);
        SaveInitialSnapshot(graph);
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
        using (TimeCount.Start("Saving meshes"))
        {
            foreach (var mesh in importer.Meshes)
            {
                MeshUtility.Optimize(mesh);
                string meshPath = string.Format("{0}/{1}/Meshes/{2}.prefab", Paths.AssetArtworks, importer.Name, mesh.name);
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
        }

        using (TimeCount.Start("Saving material"))
        {
            string matPath = string.Format("{0}/{1}/Materials/{2}.mat", Paths.AssetArtworks, importer.Name, importer.Material.name);
            AssetDatabase.CreateAsset(importer.Material, matPath);
        }

        using (TimeCount.Start("Saving prefab"))
        {
            string prefabPath = string.Format("{0}/{1}/{1}.prefab", Paths.AssetResArtworks, importer.Name);
            UnityEngine.Object prefab = PrefabUtility.CreatePrefab(prefabPath, importer.GameObject);
            PrefabUtility.ReplacePrefab(importer.GameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }

        using (TimeCount.Start("Saving assets"))
            AssetDatabase.SaveAssets();
    }

    static void SaveInitialSnapshot(PolyGraph graph)
    {
        using (TimeCount.Start("Saving initial snapshot"))
        {
            string path = Paths.SnapshotRes(graph.name);
            PuzzleSnapshotOneOff.Take(graph, null, path);
            graph.initialSnapshot = AssetDatabase.LoadAssetAtPath<Texture2D>(Paths.ToAssetPath(path));
        }
    }
}
