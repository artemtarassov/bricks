using System.Collections.Generic;
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

        List<Transform> descendantTransforms = new List<Transform>();
        CollectDescendantTransforms(selectedObject.transform, descendantTransforms);
        if (descendantTransforms.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "No Children Found",
                "The selected GameObject needs at least one child.",
                "OK");
            return;
        }

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;

        foreach (Transform childTransform in descendantTransforms)
        {
            EncapsulateChildBounds(selectedObject.transform, childTransform, ref min, ref max);
        }

        Vector3 size = max - min;
        size.x = Mathf.Max(size.x, MinimumAxisSize);
        size.y = Mathf.Max(size.y, MinimumAxisSize);
        size.z = Mathf.Max(size.z, MinimumAxisSize);
        size += Vector3.one * EnclosingGap;
        Vector3 center = (min + max) * 0.5f;

        GameObject enclosingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enclosingObject.name = "__EnclosingGameObject";
        Undo.RegisterCreatedObjectUndo(enclosingObject, "Create Enclosing GameObject");
        enclosingObject.transform.SetParent(selectedObject.transform, false);
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

    private static void EncapsulateChildBounds(Transform selectedTransform, Transform childTransform, ref Vector3 min, ref Vector3 max)
    {
        foreach (Vector3 corner in UnitCubeCorners)
        {
            Vector3 worldCorner = childTransform.TransformPoint(corner);
            Vector3 localCorner = selectedTransform.InverseTransformPoint(worldCorner);
            min = Vector3.Min(min, localCorner);
            max = Vector3.Max(max, localCorner);
        }
    }

    private static void CollectDescendantTransforms(Transform parentTransform, List<Transform> descendants)
    {
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform child = parentTransform.GetChild(i);
            descendants.Add(child);
            CollectDescendantTransforms(child, descendants);
        }
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
