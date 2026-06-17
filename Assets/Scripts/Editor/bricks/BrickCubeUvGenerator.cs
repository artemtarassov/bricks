using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class BrickCubeUvGenerator
{
    private const string SourceModelPath = "Assets/FBX/rounded_cube_1x1x1.fbx";
    private const string OutputMeshPath = "Assets/FBX/dice_cube_uv.asset";
    private const string BrickPrefabPath = "Assets/Prefabs/Brick.prefab";
    private const string FlyingPrefabPath = "Assets/Prefabs/Flying.prefab";

    [MenuItem("Tools/Bricks/Rebuild Brick Cube UV Mesh")]
    public static void GenerateFromMenu()
    {
        Generate();
    }

    public static void Generate()
    {
        var sourceMesh = LoadSourceMesh(SourceModelPath);
        if (sourceMesh == null)
        {
            throw new InvalidOperationException($"Could not find a mesh inside {SourceModelPath}.");
        }

        var generatedMesh = CloneMesh(sourceMesh);
        var savedMesh = SaveMeshAsset(generatedMesh, OutputMeshPath);

        UpdatePrefabMesh(BrickPrefabPath, savedMesh);
        UpdatePrefabMesh(FlyingPrefabPath, savedMesh);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generated cube UV mesh at {OutputMeshPath} from {SourceModelPath}.");
    }

    private static Mesh LoadSourceMesh(string assetPath)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        Mesh bestMesh = null;

        foreach (var asset in assets)
        {
            if (asset is not Mesh mesh || mesh.vertexCount == 0)
            {
                continue;
            }

            if (bestMesh == null || mesh.vertexCount > bestMesh.vertexCount)
            {
                bestMesh = mesh;
            }
        }

        return bestMesh;
    }

    private static Mesh CloneMesh(Mesh sourceMesh)
    {
        var mesh = new Mesh
        {
            name = "dice_cube_uv",
            indexFormat = sourceMesh.vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
        };

        EditorUtility.CopySerialized(sourceMesh, mesh);
        mesh.name = "dice_cube_uv";
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh SaveMeshAsset(Mesh generatedMesh, string assetPath)
    {
        var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        if (existingMesh == null)
        {
            AssetDatabase.CreateAsset(generatedMesh, assetPath);
            return generatedMesh;
        }

        EditorUtility.CopySerialized(generatedMesh, existingMesh);
        UnityEngine.Object.DestroyImmediate(generatedMesh);
        EditorUtility.SetDirty(existingMesh);
        return existingMesh;
    }

    private static void UpdatePrefabMesh(string prefabPath, Mesh mesh)
    {
        var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        try
        {
            var meshFilter = prefabRoot.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                throw new InvalidOperationException($"Prefab {prefabPath} does not contain a MeshFilter on the root object.");
            }

            meshFilter.sharedMesh = mesh;
            EditorUtility.SetDirty(meshFilter);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
