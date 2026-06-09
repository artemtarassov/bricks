using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal sealed class FillWithBricks3
{
    private const float PlacementPrecision = 1000f;
    private const float RotationMismatchWarningDegrees = 0.1f;

    public void Run(GameObject targetObject, FillWithCubesSettings settings)
    {
        if (targetObject == null || settings == null)
        {
            return;
        }

        var selectedCityElement = targetObject.GetComponent<CityElement>();
        if (selectedCityElement == null)
        {
            Debug.LogWarning($"FillWithBricks3 requires a CityElement on {targetObject.name}.");
            return;
        }

        Collider[] sourceColliders = GetSourceColliders(selectedCityElement.gameObject, settings.IncludeInactiveObjects);
        if (sourceColliders.Length == 0)
        {
            Debug.LogWarning($"FillWithBricks3 could not find any valid colliders under {selectedCityElement.name}.");
            return;
        }

        ClearGeneratedRoot(targetObject);

        Transform referenceTransform = GetReferenceTransform(sourceColliders);
        Transform generatedRoot = CreateGeneratedRoot(selectedCityElement.transform, referenceTransform);
        Vector3 rootLocalBrickSize = BrickVolumeUtility.GetLocalBrickSize(generatedRoot, settings.SafeBrickSize);
        Material brickMaterial = AssetDatabase.LoadAssetAtPath<Material>(FillWithCubesSettings.BrickMaterialPath);
        var occupiedPlacements = new HashSet<BrickPlacementKey>();
        int bricksCreated = 0;

        WarnAboutMixedColliderRotations(selectedCityElement.name, sourceColliders, generatedRoot.rotation);

        for (int i = 0; i < sourceColliders.Length; i++)
        {
            bricksCreated += AddBricksForCollider(
                sourceColliders[i],
                generatedRoot,
                rootLocalBrickSize,
                brickMaterial,
                settings,
                occupiedPlacements);
        }

        if (bricksCreated == 0)
        {
            Object.DestroyImmediate(generatedRoot.gameObject);
            Debug.LogWarning($"FillWithBricks3 did not create any bricks for {selectedCityElement.name}.");
            return;
        }

        RemoveBricksNotTouchingSourceColliders(generatedRoot, sourceColliders);
        if (generatedRoot.childCount == 0)
        {
            Object.DestroyImmediate(generatedRoot.gameObject);
        }

        EditorUtility.SetDirty(targetObject);
    }

    private static int AddBricksForCollider(
        Collider sourceCollider,
        Transform generatedRoot,
        Vector3 rootLocalBrickSize,
        Material brickMaterial,
        FillWithCubesSettings settings,
        HashSet<BrickPlacementKey> occupiedPlacements)
    {
        if (sourceCollider is BoxCollider boxCollider)
        {
            return AddBricksForBoxCollider(
                boxCollider,
                generatedRoot,
                rootLocalBrickSize,
                brickMaterial,
                settings,
                occupiedPlacements);
        }

        if (sourceCollider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            return AddBricksForMeshCollider(
                meshCollider,
                generatedRoot,
                rootLocalBrickSize,
                brickMaterial,
                settings,
                occupiedPlacements);
        }

        return 0;
    }

    private static int AddBricksForBoxCollider(
        BoxCollider boxCollider,
        Transform generatedRoot,
        Vector3 rootLocalBrickSize,
        Material brickMaterial,
        FillWithCubesSettings settings,
        HashSet<BrickPlacementKey> occupiedPlacements)
    {
        return AddBricksForBounds(
            boxCollider,
            new Bounds(boxCollider.center, boxCollider.size),
            generatedRoot,
            rootLocalBrickSize,
            brickMaterial,
            settings,
            occupiedPlacements,
            intersectsVolume: null);
    }

    private static int AddBricksForMeshCollider(
        MeshCollider meshCollider,
        Transform generatedRoot,
        Vector3 rootLocalBrickSize,
        Material brickMaterial,
        FillWithCubesSettings settings,
        HashSet<BrickPlacementKey> occupiedPlacements)
    {
        Vector3 localBrickSize = BrickVolumeUtility.GetLocalBrickSize(meshCollider.transform, settings.SafeBrickSize);
        return AddBricksForBounds(
            meshCollider,
            meshCollider.sharedMesh.bounds,
            generatedRoot,
            rootLocalBrickSize,
            brickMaterial,
            settings,
            occupiedPlacements,
            localCenter => BrickVolumeUtility.DoesBrickIntersectMesh(meshCollider, localCenter, localBrickSize));
    }

    private static int AddBricksForBounds(
        Collider sourceCollider,
        Bounds localBounds,
        Transform generatedRoot,
        Vector3 rootLocalBrickSize,
        Material brickMaterial,
        FillWithCubesSettings settings,
        HashSet<BrickPlacementKey> occupiedPlacements,
        System.Func<Vector3, bool> intersectsVolume)
    {
        Vector3 localBrickSize = BrickVolumeUtility.GetLocalBrickSize(sourceCollider.transform, settings.SafeBrickSize);
        Vector3 localGap = BrickVolumeUtility.GetLocalGap(sourceCollider.transform, settings.SafeBrickGap);
        Vector3 localPitch = localBrickSize + localGap;
        Vector3Int counts = BrickVolumeUtility.GetGridCounts(localBounds, localBrickSize, localPitch);
        Vector3 start = BrickVolumeUtility.GetGridStart(localBounds, counts, localBrickSize, localPitch);
        int bricksCreated = 0;

        for (int x = 0; x < counts.x; x++)
        {
            for (int y = 0; y < counts.y; y++)
            {
                for (int z = 0; z < counts.z; z++)
                {
                    Vector3 localCenter = start + new Vector3(
                        x * localPitch.x,
                        y * localPitch.y,
                        z * localPitch.z);

                    if (intersectsVolume != null && !intersectsVolume(localCenter))
                    {
                        continue;
                    }

                    Vector3 worldPosition = sourceCollider.transform.TransformPoint(localCenter);
                    BrickPlacementKey placementKey = BrickPlacementKey.From(worldPosition);
                    if (!occupiedPlacements.Add(placementKey))
                    {
                        continue;
                    }

                    Vector3 rootLocalPosition = generatedRoot.InverseTransformPoint(worldPosition);
                    CreateBrick(
                        generatedRoot,
                        rootLocalPosition,
                        rootLocalBrickSize,
                        brickMaterial,
                        settings);
                    bricksCreated++;
                }
            }
        }

        return bricksCreated;
    }

    private static Collider[] GetSourceColliders(GameObject targetObject, bool includeInactiveObjects)
    {
        Collider[] allColliders = targetObject.GetComponentsInChildren<Collider>(includeInactiveObjects);
        var sourceColliders = new List<Collider>(allColliders.Length);

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

    private static bool ShouldSkipCollider(Collider collider)
    {
        if (collider == null)
        {
            return true;
        }

        if (!collider.enabled)
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

    private static Transform CreateGeneratedRoot(Transform parent, Transform referenceTransform)
    {
        GameObject root = new GameObject(FillWithCubesSettings.GeneratedGroupName);
        int generatedLayer = LayerMask.NameToLayer(FillWithCubesSettings.GeneratedGroupLayerName);
        if (generatedLayer >= 0)
        {
            root.layer = generatedLayer;
        }

        root.transform.SetParent(parent, false);
        if (referenceTransform == null)
        {
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
        }
        else
        {
            root.transform.localPosition = parent.InverseTransformPoint(referenceTransform.position);
            root.transform.localRotation = Quaternion.Inverse(parent.rotation) * referenceTransform.rotation;
        }
        root.transform.localScale = Vector3.one;
        return root.transform;
    }

    private static void CreateBrick(
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material brickMaterial,
        FillWithCubesSettings settings)
    {
        GameObject brick = CreateBrickObject();
        brick.transform.SetParent(parent, false);
        brick.name = FillWithCubesSettings.BrickTagName;
        brick.tag = FillWithCubesSettings.BrickTagName;
        int brickLayer = LayerMask.NameToLayer(FillWithCubesSettings.GeneratedGroupLayerName);
        if (brickLayer >= 0)
        {
            brick.layer = brickLayer;
        }

        brick.transform.localPosition = localPosition;
        brick.transform.localRotation = Quaternion.identity;
        brick.transform.localScale = localScale;

        if (!settings.AddBrickColliders)
        {
            RemoveBrickColliders(brick);
        }

        ApplyBrickMaterial(brick, brickMaterial);
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

    private static Transform GetReferenceTransform(Collider[] sourceColliders)
    {
        Transform bestTransform = null;
        float bestVolume = float.NegativeInfinity;

        for (int i = 0; i < sourceColliders.Length; i++)
        {
            Collider collider = sourceColliders[i];
            if (collider == null)
            {
                continue;
            }

            float volume = GetColliderVolume(collider);
            if (volume > bestVolume)
            {
                bestVolume = volume;
                bestTransform = collider.transform;
            }
        }

        return bestTransform;
    }

    private static float GetColliderVolume(Collider collider)
    {
        if (collider is BoxCollider boxCollider)
        {
            Vector3 scaledSize = Vector3.Scale(boxCollider.size, BrickVolumeUtility.GetAxisScale(boxCollider.transform));
            return scaledSize.x * scaledSize.y * scaledSize.z;
        }

        if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            Vector3 scaledSize = Vector3.Scale(meshCollider.sharedMesh.bounds.size, BrickVolumeUtility.GetAxisScale(meshCollider.transform));
            return scaledSize.x * scaledSize.y * scaledSize.z;
        }

        Bounds bounds = collider.bounds;
        return bounds.size.x * bounds.size.y * bounds.size.z;
    }

    private static void WarnAboutMixedColliderRotations(string objectName, Collider[] sourceColliders, Quaternion rootRotation)
    {
        for (int i = 0; i < sourceColliders.Length; i++)
        {
            Collider collider = sourceColliders[i];
            if (collider == null)
            {
                continue;
            }

            if (Quaternion.Angle(collider.transform.rotation, rootRotation) > RotationMismatchWarningDegrees)
            {
                Debug.LogWarning(
                    $"FillWithBricks3 is rotating {FillWithCubesSettings.GeneratedGroupName} to one dominant collider for {objectName}. " +
                    $"At least one collider ({collider.name}) has a different rotation, so some bricks may not align perfectly.");
                return;
            }
        }
    }

    private readonly struct BrickPlacementKey
    {
        private readonly int posX;
        private readonly int posY;
        private readonly int posZ;

        private BrickPlacementKey(int posX, int posY, int posZ)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
        }

        public static BrickPlacementKey From(Vector3 position)
        {
            return new BrickPlacementKey(
                Quantize(position.x),
                Quantize(position.y),
                Quantize(position.z));
        }

        private static int Quantize(float value)
        {
            return Mathf.RoundToInt(value * PlacementPrecision);
        }
    }
}
