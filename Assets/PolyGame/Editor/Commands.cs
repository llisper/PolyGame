using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

abstract class Command 
{
    protected MeshModifier.Info info;

    public Command(MeshModifier.Info info) { this.info = info; }

    public abstract void Undo();
}

class DropCommand : Command
{
    List<MeshRenderer> renderers;

    public DropCommand(MeshModifier.Info info, List<MeshRenderer> renderers) 
        : base(info) 
    { 
        this.renderers = new List<MeshRenderer>(renderers);
    }

    public override void Undo()
    {
        renderers.ForEach(v => {
            v.gameObject.SetActive(true);
            v.gameObject.GetComponent<MeshRenderer>().sharedMaterial = info.originalMat;
        });
    }
}

class JoinCommand : Command
{
    GameObject newRegion;
    List<MeshRenderer> deactivatedRenderers;

    public JoinCommand(MeshModifier.Info info, GameObject newRegion, List<MeshRenderer> renderers) 
        : base(info) 
    {
        this.newRegion = newRegion;
        deactivatedRenderers = new List<MeshRenderer>(renderers);
    }

    public override void Undo()
    {
        var mesh = newRegion.GetComponent<MeshFilter>().sharedMesh;
        string meshPath = string.Format("Assets/{0}/{1}/Meshes/{2}.prefab", Paths.Artworks, info.editObj.name, mesh.name);
        GameObject.DestroyImmediate(newRegion);
        AssetDatabase.DeleteAsset(meshPath);
        deactivatedRenderers.ForEach(v => {
            v.gameObject.SetActive(true);
            v.gameObject.GetComponent<MeshRenderer>().sharedMaterial = info.originalMat;
        });
    }
}