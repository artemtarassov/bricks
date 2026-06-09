using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class BrickCubeUvGenerator
{
    private const string SourceModelPath = "Assets/FBX/dice.fbx";
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

        var generatedMesh = BuildCubeProjectedMesh(sourceMesh);
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

    private static Mesh BuildCubeProjectedMesh(Mesh sourceMesh)
    {
        var sourceVertices = sourceMesh.vertices;
        var sourceNormals = sourceMesh.normals;
        var sourceTangents = sourceMesh.tangents;
        var sourceColors = sourceMesh.colors32;
        var bounds = sourceMesh.bounds;

        var vertices = new List<Vector3>(sourceMesh.triangles.Length);
        var normals = new List<Vector3>(sourceMesh.triangles.Length);
        var tangents = new List<Vector4>(sourceMesh.triangles.Length);
        var colors = new List<Color32>(sourceMesh.triangles.Length);
        var uvs = new List<Vector2>(sourceMesh.triangles.Length);
        var subMeshTriangles = new List<int>[sourceMesh.subMeshCount];

        for (var subMesh = 0; subMesh < sourceMesh.subMeshCount; subMesh++)
        {
            var sourceTriangles = sourceMesh.GetTriangles(subMesh);
            var newTriangles = new List<int>(sourceTriangles.Length);

            for (var triangleIndex = 0; triangleIndex < sourceTriangles.Length; triangleIndex += 3)
            {
                var i0 = sourceTriangles[triangleIndex];
                var i1 = sourceTriangles[triangleIndex + 1];
                var i2 = sourceTriangles[triangleIndex + 2];

                var projectionFace = GetProjectionFace(sourceVertices[i0], sourceVertices[i1], sourceVertices[i2]);

                // Duplicate triangle vertices so each triangle can keep its own cube-face projection.
                newTriangles.Add(AddVertex(i0, projectionFace, bounds, sourceVertices, sourceNormals, sourceTangents, sourceColors, vertices, normals, tangents, colors, uvs));
                newTriangles.Add(AddVertex(i1, projectionFace, bounds, sourceVertices, sourceNormals, sourceTangents, sourceColors, vertices, normals, tangents, colors, uvs));
                newTriangles.Add(AddVertex(i2, projectionFace, bounds, sourceVertices, sourceNormals, sourceTangents, sourceColors, vertices, normals, tangents, colors, uvs));
            }

            subMeshTriangles[subMesh] = newTriangles;
        }

        var mesh = new Mesh
        {
            name = $"{sourceMesh.name}_CubeUv",
            indexFormat = vertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
        };

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);

        mesh.subMeshCount = subMeshTriangles.Length;
        for (var subMesh = 0; subMesh < subMeshTriangles.Length; subMesh++)
        {
            mesh.SetTriangles(subMeshTriangles[subMesh], subMesh, false);
        }

        if (normals.Count == vertices.Count)
        {
            mesh.SetNormals(normals);
        }
        else
        {
            mesh.RecalculateNormals();
        }

        if (tangents.Count == vertices.Count)
        {
            mesh.SetTangents(tangents);
        }

        if (colors.Count == vertices.Count)
        {
            mesh.SetColors(colors);
        }

        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }

    private static int AddVertex(
        int sourceIndex,
        ProjectionFace projectionFace,
        Bounds bounds,
        IReadOnlyList<Vector3> sourceVertices,
        IReadOnlyList<Vector3> sourceNormals,
        IReadOnlyList<Vector4> sourceTangents,
        IReadOnlyList<Color32> sourceColors,
        ICollection<Vector3> vertices,
        ICollection<Vector3> normals,
        ICollection<Vector4> tangents,
        ICollection<Color32> colors,
        ICollection<Vector2> uvs)
    {
        var vertexIndex = vertices.Count;
        var position = sourceVertices[sourceIndex];

        vertices.Add(position);
        uvs.Add(ProjectToCubeFace(projectionFace, position, bounds));

        if (sourceNormals.Count == sourceVertices.Count)
        {
            normals.Add(sourceNormals[sourceIndex]);
        }

        if (sourceTangents.Count == sourceVertices.Count)
        {
            tangents.Add(sourceTangents[sourceIndex]);
        }

        if (sourceColors.Count == sourceVertices.Count)
        {
            colors.Add(sourceColors[sourceIndex]);
        }

        return vertexIndex;
    }

    private static ProjectionFace GetProjectionFace(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        var triangleNormal = Vector3.Cross(p1 - p0, p2 - p0);
        if (triangleNormal.sqrMagnitude <= Mathf.Epsilon)
        {
            triangleNormal = (p0 + p1 + p2) / 3f;
        }

        triangleNormal.Normalize();

        var absX = Mathf.Abs(triangleNormal.x);
        var absY = Mathf.Abs(triangleNormal.y);
        var absZ = Mathf.Abs(triangleNormal.z);

        if (absX >= absY && absX >= absZ)
        {
            return triangleNormal.x >= 0f ? ProjectionFace.PositiveX : ProjectionFace.NegativeX;
        }

        if (absY >= absX && absY >= absZ)
        {
            return triangleNormal.y >= 0f ? ProjectionFace.PositiveY : ProjectionFace.NegativeY;
        }

        return triangleNormal.z >= 0f ? ProjectionFace.PositiveZ : ProjectionFace.NegativeZ;
    }

    private static Vector2 ProjectToCubeFace(ProjectionFace projectionFace, Vector3 position, Bounds bounds)
    {
        var x = Mathf.InverseLerp(bounds.min.x, bounds.max.x, position.x);
        var y = Mathf.InverseLerp(bounds.min.y, bounds.max.y, position.y);
        var z = Mathf.InverseLerp(bounds.min.z, bounds.max.z, position.z);

        return projectionFace switch
        {
            ProjectionFace.PositiveX => new Vector2(1f - z, y),
            ProjectionFace.NegativeX => new Vector2(z, y),
            ProjectionFace.PositiveY => new Vector2(x, 1f - z),
            ProjectionFace.NegativeY => new Vector2(x, z),
            ProjectionFace.PositiveZ => new Vector2(x, y),
            ProjectionFace.NegativeZ => new Vector2(1f - x, y),
            _ => new Vector2(x, y)
        };
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

    private enum ProjectionFace
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }
}
