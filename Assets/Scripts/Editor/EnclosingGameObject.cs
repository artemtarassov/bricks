using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class EnclosingGameObject : EditorWindow
{
    private const float MinimumAxisSize = 0.01f;
    private const float EnclosingGap = 0.02f;
    private const string EnclosingMaterialPath = "Assets/Materials/Enclosing.mat";
    private const string EnclosingMeshPath = "Assets/FBX/open_left_right_shell_100x100x100.fbx";
    private static readonly int[][] ScalePermutations =
    {
        new[] { 0, 1, 2 },
        new[] { 0, 2, 1 },
        new[] { 1, 0, 2 },
        new[] { 1, 2, 0 },
        new[] { 2, 0, 1 },
        new[] { 2, 1, 0 }
    };
    private static readonly Vector3[] UnitCubeCorners =
    {
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, 0.5f)
    };

    [MenuItem("Tools/Enclosing GameObject")]
    public static void ShowWindow()
    {
        GetWindow<EnclosingGameObject>("Enclosing");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("create enclosing gameobject"))
        {
            CreateEnclosingGameObject();
        }

        if (GUILayout.Button("switch scale combination"))
        {
            SwitchScaleCombination();
        }
    }

    private static void CreateEnclosingGameObject()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog(
                "No Selection",
                "Select a GameObject to enclose first.",
                "OK");
            return;
        }

        Transform referenceTransform = GetReferenceTransform(selectedObject.transform);
        if (referenceTransform == null)
        {
            EditorUtility.DisplayDialog(
                "No Geometry Found",
                "The selected GameObject needs at least one valid collider, mesh, or renderer.",
                "OK");
            return;
        }

        if (!TryGetEnclosingBounds(selectedObject.transform, referenceTransform, out Vector3 center, out Vector3 size))
        {
            EditorUtility.DisplayDialog(
                "No Bounds Found",
                "The selected GameObject needs at least one valid collider, mesh, or renderer.",
                "OK");
            return;
        }

        GameObject enclosingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enclosingObject.name = "__EnclosingGameObject";
        Undo.RegisterCreatedObjectUndo(enclosingObject, "Create Enclosing GameObject");
        enclosingObject.transform.SetParent(referenceTransform, false);
        enclosingObject.transform.localPosition = center;
        enclosingObject.transform.localRotation = Quaternion.identity;
        enclosingObject.transform.localScale = size;
        enclosingObject.transform.SetParent(selectedObject.transform.parent, true);

        MeshFilter meshFilter = enclosingObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Mesh enclosingMesh = LoadEnclosingMesh();
            if (enclosingMesh != null)
            {
                Undo.RecordObject(meshFilter, "Assign Enclosing Mesh");
                meshFilter.sharedMesh = enclosingMesh;
            }
        }

        MeshRenderer meshRenderer = enclosingObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Undo.RecordObject(meshRenderer, "Configure Enclosing Renderer");
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            Material enclosingMaterial = AssetDatabase.LoadAssetAtPath<Material>(EnclosingMaterialPath);
            if (enclosingMaterial != null)
            {
                meshRenderer.sharedMaterial = enclosingMaterial;
            }
        }

        Collider collider = enclosingObject.GetComponent<Collider>();
        if (collider != null)
        {
            Undo.DestroyObjectImmediate(collider);
        }

        Selection.activeGameObject = enclosingObject;
        EditorGUIUtility.PingObject(enclosingObject);
    }

    private static void SwitchScaleCombination()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog(
                "No Selection",
                "Select a GameObject first.",
                "OK");
            return;
        }

        Transform selectedTransform = selectedObject.transform;
        string stateKey = "EnclosingGameObject.ScalePermutation." + selectedObject.GetInstanceID();
        string baseKey = stateKey + ".Base";
        Vector3 currentScale = selectedTransform.localScale;
        int currentPermutationIndex = SessionState.GetInt(stateKey, 0);
        Vector3 baseScale = StringToVector3(SessionState.GetString(baseKey, Vector3ToString(currentScale)));

        if (!ApproximatelyEqual(currentScale, ApplyPermutation(baseScale, currentPermutationIndex)))
        {
            baseScale = currentScale;
            currentPermutationIndex = 0;
        }

        int nextPermutationIndex = (currentPermutationIndex + 1) % ScalePermutations.Length;
        Vector3 nextScale = ApplyPermutation(baseScale, nextPermutationIndex);

        Undo.RecordObject(selectedTransform, "Switch Scale Combination");
        selectedTransform.localScale = nextScale;
        EditorUtility.SetDirty(selectedTransform);
        SessionState.SetInt(stateKey, nextPermutationIndex);
        SessionState.SetString(baseKey, Vector3ToString(baseScale));
    }

    private static bool TryGetEnclosingBounds(
        Transform selectedTransform,
        Transform referenceTransform,
        out Vector3 center,
        out Vector3 size)
    {
        center = Vector3.zero;
        size = Vector3.zero;

        bool hasBounds = false;
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        Collider[] colliders = selectedTransform.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (ShouldSkipTransform(selectedTransform, collider.transform))
            {
                continue;
            }

            if (collider is BoxCollider boxCollider)
            {
                EncapsulateBounds(referenceTransform, boxCollider.transform, new Bounds(boxCollider.center, boxCollider.size), ref min, ref max);
                hasBounds = true;
                continue;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                EncapsulateBounds(referenceTransform, meshCollider.transform, meshCollider.sharedMesh.bounds, ref min, ref max);
                hasBounds = true;
                continue;
            }
        }

        if (!hasBounds)
        {
            MeshFilter[] meshFilters = selectedTransform.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                if (meshFilter.sharedMesh == null || ShouldSkipTransform(selectedTransform, meshFilter.transform))
                {
                    continue;
                }

                EncapsulateBounds(referenceTransform, meshFilter.transform, meshFilter.sharedMesh.bounds, ref min, ref max);
                hasBounds = true;
            }
        }

        if (!hasBounds)
        {
            Renderer[] renderers = selectedTransform.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || ShouldSkipTransform(selectedTransform, renderer.transform))
                {
                    continue;
                }

                EncapsulateWorldBounds(referenceTransform, renderer.bounds, ref min, ref max);
                hasBounds = true;
            }
        }

        if (!hasBounds)
        {
            return false;
        }

        size = max - min;
        size.x = Mathf.Max(size.x, MinimumAxisSize);
        size.y = Mathf.Max(size.y, MinimumAxisSize);
        size.z = Mathf.Max(size.z, MinimumAxisSize);
        size += Vector3.one * EnclosingGap;
        center = (min + max) * 0.5f;
        return true;
    }

    private static Transform GetReferenceTransform(Transform selectedTransform)
    {
        Transform bestTransform = null;
        float bestVolume = float.NegativeInfinity;

        Collider[] colliders = selectedTransform.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (ShouldSkipTransform(selectedTransform, collider.transform))
            {
                continue;
            }

            if (collider is BoxCollider boxCollider)
            {
                float volume = GetScaledBoundsVolume(boxCollider.transform, new Bounds(boxCollider.center, boxCollider.size));
                if (volume > bestVolume)
                {
                    bestVolume = volume;
                    bestTransform = boxCollider.transform;
                }
                continue;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                float volume = GetScaledBoundsVolume(meshCollider.transform, meshCollider.sharedMesh.bounds);
                if (volume > bestVolume)
                {
                    bestVolume = volume;
                    bestTransform = meshCollider.transform;
                }
            }
        }

        MeshFilter[] meshFilters = selectedTransform.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter.sharedMesh == null || ShouldSkipTransform(selectedTransform, meshFilter.transform))
            {
                continue;
            }

            float volume = GetScaledBoundsVolume(meshFilter.transform, meshFilter.sharedMesh.bounds);
            if (volume > bestVolume)
            {
                bestVolume = volume;
                bestTransform = meshFilter.transform;
            }
        }

        Renderer[] renderers = selectedTransform.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || ShouldSkipTransform(selectedTransform, renderer.transform))
            {
                continue;
            }

            float volume = GetWorldBoundsVolume(renderer.bounds);
            if (volume > bestVolume)
            {
                bestVolume = volume;
                bestTransform = renderer.transform;
            }
        }

        return bestTransform;
    }

    private static void EncapsulateBounds(
        Transform referenceTransform,
        Transform sourceTransform,
        Bounds localBounds,
        ref Vector3 min,
        ref Vector3 max)
    {
        Vector3 boundsMin = localBounds.min;
        Vector3 boundsMax = localBounds.max;

        foreach (Vector3 corner in UnitCubeCorners)
        {
            Vector3 sourceLocalCorner = new Vector3(
                corner.x > 0f ? boundsMax.x : boundsMin.x,
                corner.y > 0f ? boundsMax.y : boundsMin.y,
                corner.z > 0f ? boundsMax.z : boundsMin.z);
            Vector3 worldCorner = sourceTransform.TransformPoint(sourceLocalCorner);
            Vector3 referenceLocalCorner = referenceTransform.InverseTransformPoint(worldCorner);
            min = Vector3.Min(min, referenceLocalCorner);
            max = Vector3.Max(max, referenceLocalCorner);
        }
    }

    private static void EncapsulateWorldBounds(Transform referenceTransform, Bounds worldBounds, ref Vector3 min, ref Vector3 max)
    {
        Vector3 boundsMin = worldBounds.min;
        Vector3 boundsMax = worldBounds.max;

        foreach (Vector3 corner in UnitCubeCorners)
        {
            Vector3 worldCorner = new Vector3(
                corner.x > 0f ? boundsMax.x : boundsMin.x,
                corner.y > 0f ? boundsMax.y : boundsMin.y,
                corner.z > 0f ? boundsMax.z : boundsMin.z);
            Vector3 referenceLocalCorner = referenceTransform.InverseTransformPoint(worldCorner);
            min = Vector3.Min(min, referenceLocalCorner);
            max = Vector3.Max(max, referenceLocalCorner);
        }
    }

    private static bool ShouldSkipTransform(Transform selectedRoot, Transform transform)
    {
        Transform current = transform;
        while (current != null && current != selectedRoot)
        {
            if (current.name.StartsWith("__"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static float GetScaledBoundsVolume(Transform transform, Bounds localBounds)
    {
        Vector3 axisScale = BrickVolumeUtility.GetAxisScale(transform);
        Vector3 scaledSize = Vector3.Scale(localBounds.size, axisScale);
        return scaledSize.x * scaledSize.y * scaledSize.z;
    }

    private static float GetWorldBoundsVolume(Bounds worldBounds)
    {
        Vector3 size = worldBounds.size;
        return size.x * size.y * size.z;
    }

    private static Mesh LoadEnclosingMesh()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(EnclosingMeshPath);
        foreach (Object asset in assets)
        {
            if (asset is Mesh mesh)
            {
                return mesh;
            }
        }

        return null;
    }

    private static Vector3 ApplyPermutation(Vector3 scale, int permutationIndex)
    {
        int[] permutation = ScalePermutations[permutationIndex];
        float[] axes = { scale.x, scale.y, scale.z };
        return new Vector3(axes[permutation[0]], axes[permutation[1]], axes[permutation[2]]);
    }

    private static bool ApproximatelyEqual(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x)
            && Mathf.Approximately(a.y, b.y)
            && Mathf.Approximately(a.z, b.z);
    }

    private static string Vector3ToString(Vector3 value)
    {
        return value.x + "|" + value.y + "|" + value.z;
    }

    private static Vector3 StringToVector3(string value)
    {
        string[] parts = value.Split('|');
        if (parts.Length != 3)
        {
            return Vector3.one;
        }

        if (!float.TryParse(parts[0], out float x)
            || !float.TryParse(parts[1], out float y)
            || !float.TryParse(parts[2], out float z))
        {
            return Vector3.one;
        }

        return new Vector3(x, y, z);
    }
}
