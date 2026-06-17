using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class UnityStyleCubeMeshCreator
{
    private const string DefaultMeshName = "UnityStyleRoundedCube";
    private const string DefaultMeshAssetPath = "Assets/UnityStyleRoundedCube.asset";
    private const int DefaultSegments = 6;
    private const float DefaultBevel = 0.15f;
    private const float DefaultCornerRadius = 0.15f;
    private const float DefaultNormalRadiusScale = 0.5f;
    private const string SegmentsEditorPrefKey = "UnityStyleCubeMeshCreator.Segments";
    private const string BevelEditorPrefKey = "UnityStyleCubeMeshCreator.Bevel";
    private const string CornerRadiusEditorPrefKey = "UnityStyleCubeMeshCreator.CornerRadius";
    private const string NormalRadiusScaleEditorPrefKey = "UnityStyleCubeMeshCreator.NormalRadiusScale";

    [MenuItem("Tools/Meshes/Unity Style Rounded Cube...", false, 209)]
    private static void ShowWindow()
    {
        UnityStyleCubeMeshCreatorWindow.ShowWindow();
    }

    [MenuItem("Assets/Create/Mesh/Unity Style Rounded Cube Mesh", false, 210)]
    private static void CreateMeshAsset()
    {
        string folderPath = GetSelectedFolderPath();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, DefaultMeshName + ".asset").Replace('\\', '/'));

        Mesh mesh = BuildMesh(DefaultMeshName, GetNormalRadiusScale());
        SaveMeshAsset(mesh, assetPath);

        Object savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        Selection.activeObject = savedMesh;
        EditorGUIUtility.PingObject(savedMesh);
    }

    [MenuItem("Tools/Meshes/Rebuild Unity Style Rounded Cube", false, 210)]
    private static void RebuildDefaultMeshAsset()
    {
        Mesh mesh = BuildMesh(DefaultMeshName, GetNormalRadiusScale());
        SaveMeshAsset(mesh, DefaultMeshAssetPath);

        Object savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(DefaultMeshAssetPath);
        Selection.activeObject = savedMesh;
        EditorGUIUtility.PingObject(savedMesh);
    }

    [MenuItem("GameObject/3D Object/Unity Style Rounded Cube", false, 10)]
    private static void CreateSceneObject(MenuCommand command)
    {
        GameObject gameObject = new GameObject(DefaultMeshName);
        Undo.RegisterCreatedObjectUndo(gameObject, "Create Unity Style Rounded Cube");

        Mesh mesh = BuildMesh(DefaultMeshName, GetNormalRadiusScale());

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

        GameObjectUtility.SetParentAndAlign(gameObject, command.context as GameObject);
        Selection.activeGameObject = gameObject;
    }

    internal static float GetNormalRadiusScale()
    {
        return EditorPrefs.GetFloat(NormalRadiusScaleEditorPrefKey, DefaultNormalRadiusScale);
    }

    internal static void SetNormalRadiusScale(float value)
    {
        EditorPrefs.SetFloat(NormalRadiusScaleEditorPrefKey, Mathf.Max(0f, value));
    }

    internal static int GetSegments()
    {
        return Mathf.Max(1, EditorPrefs.GetInt(SegmentsEditorPrefKey, DefaultSegments));
    }

    internal static void SetSegments(int value)
    {
        EditorPrefs.SetInt(SegmentsEditorPrefKey, Mathf.Max(1, value));
    }

    internal static float GetBevel()
    {
        return EditorPrefs.GetFloat(BevelEditorPrefKey, DefaultBevel);
    }

    internal static void SetBevel(float value)
    {
        EditorPrefs.SetFloat(BevelEditorPrefKey, Mathf.Max(0f, value));
    }

    internal static float GetCornerRadius()
    {
        return EditorPrefs.GetFloat(CornerRadiusEditorPrefKey, DefaultCornerRadius);
    }

    internal static void SetCornerRadius(float value)
    {
        EditorPrefs.SetFloat(CornerRadiusEditorPrefKey, Mathf.Max(0f, value));
    }

    private static Mesh BuildMesh(string meshName, float normalRadiusScale)
    {
        const float halfExtent = 0.5f;
        int segments = GetSegments();
        float bevel = Mathf.Clamp(GetBevel(), 0.001f, halfExtent - 0.001f);
        float radius = Mathf.Clamp(GetCornerRadius(), 0.001f, bevel);
        float normalRadius = Mathf.Clamp(radius * Mathf.Max(0f, normalRadiusScale), 0.001f, radius);
        float innerExtent = halfExtent - bevel;
        float normalInnerExtent = halfExtent - normalRadius;
        int samplesPerAxis = (segments + 1) * 2;
        int verticesPerFace = samplesPerAxis * samplesPerAxis;
        int quadsPerFace = (samplesPerAxis - 1) * (samplesPerAxis - 1);
        int totalVertexCount = verticesPerFace * 6;

        Vector3[] vertices = new Vector3[totalVertexCount];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        int[] triangles = new int[quadsPerFace * 6 * 6];

        int vertexOffset = 0;
        int triangleOffset = 0;

        AddFace(Vector3.forward, Vector3.right, Vector3.up, halfExtent, innerExtent, normalInnerExtent, radius, segments, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);
        AddFace(Vector3.back, Vector3.left, Vector3.up, halfExtent, innerExtent, normalInnerExtent, radius, segments, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);
        AddFace(Vector3.left, Vector3.forward, Vector3.up, halfExtent, innerExtent, normalInnerExtent, radius, segments, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);
        AddFace(Vector3.right, Vector3.back, Vector3.up, halfExtent, innerExtent, normalInnerExtent, radius, segments, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);
        AddFace(Vector3.up, Vector3.right, Vector3.back, halfExtent, innerExtent, normalInnerExtent, radius, segments, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);
        AddFace(Vector3.down, Vector3.right, Vector3.forward, halfExtent, innerExtent, normalInnerExtent, radius, segments, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);

        Mesh mesh = new Mesh
        {
            name = meshName
        };

        if (vertices.Length > ushort.MaxValue)
        {
            mesh.indexFormat = IndexFormat.UInt32;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.tangents = tangents;
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void AddFace(
        Vector3 faceNormal,
        Vector3 faceRight,
        Vector3 faceUp,
        float halfExtent,
        float innerExtent,
        float normalInnerExtent,
        float radius,
        int segments,
        ref int vertexOffset,
        ref int triangleOffset,
        Vector3[] vertices,
        Vector3[] normals,
        Vector2[] uvs,
        Vector4[] tangents,
        int[] triangles)
    {
        float[] xSamples = CreateFaceSamples(halfExtent, innerExtent, segments);
        float[] ySamples = CreateFaceSamples(halfExtent, innerExtent, segments);

        AddPatch(faceNormal, faceRight, faceUp, xSamples, ySamples, halfExtent, innerExtent, normalInnerExtent, radius, ref vertexOffset, ref triangleOffset, vertices, normals, uvs, tangents, triangles);
    }

    private static float[] CreateFaceSamples(float halfExtent, float innerExtent, int segments)
    {
        float[] samples = new float[(segments + 1) * 2];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            samples[i] = Mathf.Lerp(-halfExtent, -innerExtent, t);
            samples[i + segments + 1] = Mathf.Lerp(innerExtent, halfExtent, t);
        }

        return samples;
    }

    private static void AddPatch(
        Vector3 faceNormal,
        Vector3 faceRight,
        Vector3 faceUp,
        float[] xSamples,
        float[] ySamples,
        float halfExtent,
        float innerExtent,
        float normalInnerExtent,
        float radius,
        ref int vertexOffset,
        ref int triangleOffset,
        Vector3[] vertices,
        Vector3[] normals,
        Vector2[] uvs,
        Vector4[] tangents,
        int[] triangles)
    {
        int rowSize = xSamples.Length;
        Vector4 tangent = new Vector4(faceRight.x, faceRight.y, faceRight.z, 1f);

        for (int y = 0; y < ySamples.Length; y++)
        {
            for (int x = 0; x < xSamples.Length; x++)
            {
                float xOffset = xSamples[x];
                float yOffset = ySamples[y];

                Vector3 cubePoint = faceNormal * halfExtent + faceRight * xOffset + faceUp * yOffset;
                Vector3 innerPoint = new Vector3(
                    Mathf.Clamp(cubePoint.x, -innerExtent, innerExtent),
                    Mathf.Clamp(cubePoint.y, -innerExtent, innerExtent),
                    Mathf.Clamp(cubePoint.z, -innerExtent, innerExtent));
                Vector3 geometryNormal = (cubePoint - innerPoint).normalized;

                Vector3 normalInnerPoint = new Vector3(
                    Mathf.Clamp(cubePoint.x, -normalInnerExtent, normalInnerExtent),
                    Mathf.Clamp(cubePoint.y, -normalInnerExtent, normalInnerExtent),
                    Mathf.Clamp(cubePoint.z, -normalInnerExtent, normalInnerExtent));

                Vector3 normal = (cubePoint - normalInnerPoint).normalized;
                int index = vertexOffset + y * rowSize + x;

                vertices[index] = innerPoint + geometryNormal * radius;
                normals[index] = normal;
                uvs[index] = new Vector2(
                    Mathf.InverseLerp(-halfExtent, halfExtent, xOffset),
                    Mathf.InverseLerp(-halfExtent, halfExtent, yOffset));
                tangents[index] = tangent;
            }
        }

        for (int y = 0; y < ySamples.Length - 1; y++)
        {
            for (int x = 0; x < xSamples.Length - 1; x++)
            {
                int root = vertexOffset + y * rowSize + x;
                int nextRow = root + rowSize;

                triangles[triangleOffset++] = root;
                triangles[triangleOffset++] = root + 1;
                triangles[triangleOffset++] = nextRow;

                triangles[triangleOffset++] = root + 1;
                triangles[triangleOffset++] = nextRow + 1;
                triangles[triangleOffset++] = nextRow;
            }
        }

        vertexOffset += xSamples.Length * ySamples.Length;
    }

    private static void SaveMeshAsset(Mesh mesh, string assetPath)
    {
        Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        if (existingMesh == null)
        {
            AssetDatabase.CreateAsset(mesh, assetPath);
        }
        else
        {
            EditorUtility.CopySerialized(mesh, existingMesh);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existingMesh);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static string GetSelectedFolderPath()
    {
        string assetPath = "Assets";

        if (Selection.activeObject != null)
        {
            assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        }

        if (string.IsNullOrEmpty(assetPath))
        {
            return "Assets";
        }

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return assetPath;
        }

        string directory = Path.GetDirectoryName(assetPath);
        return string.IsNullOrEmpty(directory) ? "Assets" : directory.Replace('\\', '/');
    }
}

public sealed class UnityStyleCubeMeshCreatorWindow : EditorWindow
{
    private int _segments;
    private float _bevel;
    private float _cornerRadius;
    private float _normalRadiusScale;

    public static void ShowWindow()
    {
        UnityStyleCubeMeshCreatorWindow window = GetWindow<UnityStyleCubeMeshCreatorWindow>("Rounded Cube");
        window.minSize = new Vector2(320f, 130f);
        window.Show();
    }

    private void OnEnable()
    {
        _segments = UnityStyleCubeMeshCreator.GetSegments();
        _bevel = UnityStyleCubeMeshCreator.GetBevel();
        _cornerRadius = UnityStyleCubeMeshCreator.GetCornerRadius();
        _normalRadiusScale = UnityStyleCubeMeshCreator.GetNormalRadiusScale();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Unity Style Rounded Cube", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        _segments = EditorGUILayout.IntField("Segments", _segments);
        if (EditorGUI.EndChangeCheck())
        {
            _segments = Mathf.Max(1, _segments);
            UnityStyleCubeMeshCreator.SetSegments(_segments);
        }

        EditorGUI.BeginChangeCheck();
        _bevel = EditorGUILayout.FloatField("Bevel", _bevel);
        if (EditorGUI.EndChangeCheck())
        {
            _bevel = Mathf.Max(0f, _bevel);
            UnityStyleCubeMeshCreator.SetBevel(_bevel);
        }

        EditorGUI.BeginChangeCheck();
        _cornerRadius = EditorGUILayout.FloatField("Corner Radius", _cornerRadius);
        if (EditorGUI.EndChangeCheck())
        {
            _cornerRadius = Mathf.Max(0f, _cornerRadius);
            UnityStyleCubeMeshCreator.SetCornerRadius(_cornerRadius);
        }

        EditorGUI.BeginChangeCheck();
        _normalRadiusScale = EditorGUILayout.FloatField("Normal Radius Scale", _normalRadiusScale);
        if (EditorGUI.EndChangeCheck())
        {
            _normalRadiusScale = Mathf.Max(0f, _normalRadiusScale);
            UnityStyleCubeMeshCreator.SetNormalRadiusScale(_normalRadiusScale);
        }

        EditorGUILayout.HelpBox(
            "Segments changes topology density. Bevel controls how far the edge cut-in extends. Corner Radius controls how rounded that bevel is. Lower Normal Radius Scale makes the large face areas shade more like a flat cube. Defaults are 6, 0.15, 0.15, and 0.5.",
            MessageType.Info);

        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Reset Default"))
            {
                _segments = 6;
                _bevel = 0.15f;
                _cornerRadius = 0.15f;
                _normalRadiusScale = 0.5f;
                UnityStyleCubeMeshCreator.SetSegments(_segments);
                UnityStyleCubeMeshCreator.SetBevel(_bevel);
                UnityStyleCubeMeshCreator.SetCornerRadius(_cornerRadius);
                UnityStyleCubeMeshCreator.SetNormalRadiusScale(_normalRadiusScale);
                GUI.FocusControl(null);
            }

            if (GUILayout.Button("Rebuild Mesh"))
            {
                UnityStyleCubeMeshCreator.SetSegments(_segments);
                UnityStyleCubeMeshCreator.SetBevel(_bevel);
                UnityStyleCubeMeshCreator.SetCornerRadius(_cornerRadius);
                UnityStyleCubeMeshCreator.SetNormalRadiusScale(_normalRadiusScale);
                EditorApplication.delayCall += RebuildMesh;
            }
        }
    }

    private static void RebuildMesh()
    {
        EditorApplication.ExecuteMenuItem("Tools/Meshes/Rebuild Unity Style Rounded Cube");
    }
}
