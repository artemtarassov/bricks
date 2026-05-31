using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal class FillWithBricks2
{
    public void Run(GameObject targetObject, FillWithCubesSettings settings)
    {
        if (targetObject == null || settings == null)
        {
            return;
        }

        var selectedCityElement = targetObject.GetComponent<CityElement>();
        if (selectedCityElement == null)
        {
            Debug.LogWarning($"FillWithBricks2 requires a CityElement on {targetObject.name}.");
            return;
        }

        if (!TryGetCombinedColliderBounds(selectedCityElement.gameObject, settings.IncludeInactiveObjects, out Bounds combinedColliderBounds))
        {
            Debug.LogWarning($"FillWithBricks2 could not find any valid colliders under {selectedCityElement.name}.");
            return;
        }

        Vector3 size = combinedColliderBounds.size;
        Debug.Log($"FillWithBricks2 measured {selectedCityElement.name} collider size: x={size.x:F4}, y={size.y:F4}, z={size.z:F4}");
        Collider[] sourceColliders = GetSourceColliders(selectedCityElement.gameObject, settings.IncludeInactiveObjects);
        Transform generatedRoot = CreateCubes(selectedCityElement.gameObject, size, combinedColliderBounds.center, settings);
        RemoveBricksNotTouchingSourceColliders(generatedRoot, sourceColliders);
    }

    private Transform CreateCubes(GameObject targetObject, Vector3 withinSize, Vector3 center, FillWithCubesSettings settings)
    {
        ClearGeneratedRoot(targetObject);

        Transform targetTransform = targetObject.transform;
        Transform generatedRoot = CreateGeneratedRoot(targetTransform);
        Vector3 localBrickSize = BrickVolumeUtility.GetLocalBrickSize(targetTransform, settings.SafeBrickSize);
        Vector3 localGap = BrickVolumeUtility.GetLocalGap(targetTransform, settings.SafeBrickGap);
        Vector3 localPitch = localBrickSize + localGap;
        Vector3Int counts = BrickVolumeUtility.GetGridCounts(new Bounds(center, withinSize), localBrickSize, localPitch);
        Vector3 start = BrickVolumeUtility.GetGridStart(new Bounds(center, withinSize), counts, localBrickSize, localPitch);
        Material brickMaterial = AssetDatabase.LoadAssetAtPath<Material>(FillWithCubesSettings.BrickMaterialPath);

        for (int x = 0; x < counts.x; x++)
        {
            for (int y = 0; y < counts.y; y++)
            {
                for (int z = 0; z < counts.z; z++)
                {
                    Vector3 localPosition = start + new Vector3(
                        x * localPitch.x,
                        y * localPitch.y,
                        z * localPitch.z);
                    GameObject brick = CreateBrickObject();
                    brick.transform.SetParent(generatedRoot, false);
                    brick.name = FillWithCubesSettings.BrickTagName;
                    brick.tag = FillWithCubesSettings.BrickTagName;
                    int brickLayer = LayerMask.NameToLayer(FillWithCubesSettings.GeneratedGroupLayerName);
                    if (brickLayer >= 0)
                    {
                        brick.layer = brickLayer;
                    }

                    brick.transform.localPosition = localPosition;
                    brick.transform.localRotation = Quaternion.identity;
                    brick.transform.localScale = localBrickSize;

                    if (!settings.AddBrickColliders)
                    {
                        RemoveBrickColliders(brick);
                    }

                    ApplyBrickMaterial(brick, brickMaterial);
                }
            }
        }

        EditorUtility.SetDirty(targetObject);
        return generatedRoot;
    }

    private static bool TryGetCombinedColliderBounds(GameObject targetObject, bool includeInactiveObjects, out Bounds combinedBounds)
    {
        combinedBounds = default;
        if (targetObject == null)
        {
            return false;
        }

        Collider[] colliders = targetObject.GetComponentsInChildren<Collider>(includeInactiveObjects);
        bool hasBounds = false;

        foreach (Collider collider in colliders)
        {
            if (ShouldSkipCollider(collider))
            {
                continue;
            }

            Bounds localColliderBounds = GetBoundsInLocalSpace(targetObject.transform, collider.bounds);
            if (!hasBounds)
            {
                combinedBounds = localColliderBounds;
                hasBounds = true;
                continue;
            }

            combinedBounds.Encapsulate(localColliderBounds.min);
            combinedBounds.Encapsulate(localColliderBounds.max);
        }

        return hasBounds;
    }

    private static bool ShouldSkipCollider(Collider collider)
    {
        if (collider == null)
        {
            return true;
        }

        Transform current = collider.transform;
        while (current != null)
        {
            if (current.name == FillWithCubesSettings.GeneratedGroupName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static Collider[] GetSourceColliders(GameObject targetObject, bool includeInactiveObjects)
    {
        Collider[] allColliders = targetObject.GetComponentsInChildren<Collider>(includeInactiveObjects);
        var sourceColliders = new System.Collections.Generic.List<Collider>(allColliders.Length);

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider collider = allColliders[i];
            if (!ShouldSkipCollider(collider))
            {
                sourceColliders.Add(collider);
            }
        }

        return sourceColliders.ToArray();
    }

    private static Bounds GetBoundsInLocalSpace(Transform targetTransform, Bounds worldBounds)
    {
        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;

        Vector3[] worldCorners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z),
        };

        Vector3 firstCorner = targetTransform.InverseTransformPoint(worldCorners[0]);
        Bounds localBounds = new Bounds(firstCorner, Vector3.zero);

        for (int i = 1; i < worldCorners.Length; i++)
        {
            localBounds.Encapsulate(targetTransform.InverseTransformPoint(worldCorners[i]));
        }

        return localBounds;
    }

    private static void ClearGeneratedRoot(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        Transform[] transforms = targetObject.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            if (current != null && current.name == FillWithCubesSettings.GeneratedGroupName)
            {
                Object.DestroyImmediate(current.gameObject);
            }
        }
    }

    private static Transform CreateGeneratedRoot(Transform parent)
    {
        GameObject root = new GameObject(FillWithCubesSettings.GeneratedGroupName);
        int generatedLayer = LayerMask.NameToLayer(FillWithCubesSettings.GeneratedGroupLayerName);
        if (generatedLayer >= 0)
        {
            root.layer = generatedLayer;
        }

        root.transform.SetParent(parent, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root.transform;
    }

    private static GameObject CreateBrickObject()
    {
        GameObject brickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FillWithCubesSettings.BrickPrefabPath);
        if (brickPrefab != null)
        {
            GameObject brick = (GameObject)PrefabUtility.InstantiatePrefab(brickPrefab);
            if (brick != null)
            {
                return brick;
            }

            return Object.Instantiate(brickPrefab);
        }

        return GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    private static void RemoveBrickColliders(GameObject brick)
    {
        Collider[] colliders = brick.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static void ApplyBrickMaterial(GameObject brick, Material brickMaterial)
    {
        if (brickMaterial == null)
        {
            return;
        }

        Renderer[] renderers = brick.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sharedMaterial = brickMaterial;
            }
        }
    }

    private static void RemoveBricksNotTouchingSourceColliders(Transform generatedRoot, Collider[] sourceColliders)
    {
        if (generatedRoot == null || sourceColliders == null || sourceColliders.Length == 0)
        {
            return;
        }

        Physics.SyncTransforms();
        var sourceColliderSet = new HashSet<Collider>(sourceColliders);

        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Transform brickTransform = generatedRoot.GetChild(i);
            if (brickTransform == null)
            {
                continue;
            }

            BoxCollider probeCollider = GetOrCreateProbeBoxCollider(brickTransform.gameObject, out bool destroyProbeCollider);
            bool touchesSourceCollider = probeCollider != null && DoesBrickTouchAnySourceCollider(probeCollider, sourceColliderSet);

            if (destroyProbeCollider && probeCollider != null)
            {
                Object.DestroyImmediate(probeCollider);
            }

            if (!touchesSourceCollider)
            {
                Object.DestroyImmediate(brickTransform.gameObject);
            }
        }
    }

    private static BoxCollider GetOrCreateProbeBoxCollider(GameObject brick, out bool destroyProbeCollider)
    {
        destroyProbeCollider = false;
        if (brick == null)
        {
            return null;
        }

        BoxCollider existingBoxCollider = brick.GetComponent<BoxCollider>();
        if (existingBoxCollider != null)
        {
            return existingBoxCollider;
        }

        destroyProbeCollider = true;
        return brick.AddComponent<BoxCollider>();
    }

    private static bool DoesBrickTouchAnySourceCollider(BoxCollider brickCollider, HashSet<Collider> sourceColliderSet)
    {
        Vector3 center = brickCollider.transform.TransformPoint(brickCollider.center);
        Vector3 halfExtents = Vector3.Scale(brickCollider.size * 0.5f, BrickVolumeUtility.GetAxisScale(brickCollider.transform));
        Collider[] overlaps = Physics.OverlapBox(
            center,
            halfExtents,
            brickCollider.transform.rotation,
            ~0,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider overlap = overlaps[i];
            if (overlap != null && sourceColliderSet.Contains(overlap))
            {
                return true;
            }
        }

        return false;
    }
}
