using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class FillWithCubesGenerator
{
    private const string UndoLabel = "Add Bricks";
    private const float PlacementPrecision = 1000f;
    private const float TouchingEpsilon = 0.0001f;
    private const int VisibilityRayRows = 180;
    private const float VisibilityRayDistancePadding = 0.05f;

    public static void AddBricks(GameObject targetObject, FillWithCubesSettings settings)
    {
        if (targetObject == null || settings == null)
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(UndoLabel);

        ClearBricks(targetObject);

        Transform generatedRoot = CreateGeneratedRoot(targetObject.transform);
        Collider[] colliders = targetObject.GetComponentsInChildren<Collider>(settings.IncludeInactiveObjects);
        var mirroredParents = new Dictionary<Transform, Transform>();
        var occupiedPlacements = new HashSet<BrickPlacementKey>();

        foreach (Collider collider in colliders)
        {
            if (ShouldSkipCollider(collider))
            {
                continue;
            }

            if (collider is BoxCollider boxCollider)
            {
                AddBricksForBounds(settings, targetObject.transform, generatedRoot, mirroredParents, occupiedPlacements, boxCollider, new Bounds(boxCollider.center, boxCollider.size));
                continue;
            }

            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                AddBricksForMesh(settings, targetObject.transform, generatedRoot, mirroredParents, occupiedPlacements, meshCollider);
            }
        }

        if (generatedRoot.childCount == 0)
        {
            Undo.DestroyObjectImmediate(generatedRoot.gameObject);
        }
        else
        {
            CleanupTouchingBricks(targetObject);
        }

        EditorUtility.SetDirty(targetObject);
    }

    public static void CleanupTouchingBricks(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        List<GameObject> generatedBricks = GetGeneratedBricks(targetObject);
        var occupiedCells = new Dictionary<Vector3Int, List<BrickBounds>>();
        var bricksToRemove = new HashSet<GameObject>();

        foreach (GameObject generatedBrick in generatedBricks)
        {
            if (generatedBrick == null || bricksToRemove.Contains(generatedBrick))
            {
                continue;
            }

            if (!TryGetBrickBounds(generatedBrick, out Bounds worldBounds))
            {
                continue;
            }

            Vector3Int minCell = WorldToCell(worldBounds.min);
            Vector3Int maxCell = WorldToCell(worldBounds.max);
            bool touchesExistingBrick = false;

            for (int x = minCell.x; x <= maxCell.x && !touchesExistingBrick; x++)
            {
                for (int y = minCell.y; y <= maxCell.y && !touchesExistingBrick; y++)
                {
                    for (int z = minCell.z; z <= maxCell.z; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (!occupiedCells.TryGetValue(cell, out List<BrickBounds> candidates))
                        {
                            continue;
                        }

                        foreach (BrickBounds candidate in candidates)
                        {
                            if (bricksToRemove.Contains(candidate.GameObject))
                            {
                                continue;
                            }

                            if (!DoBoundsTouchOrOverlap(worldBounds, candidate.WorldBounds))
                            {
                                continue;
                            }

                            bricksToRemove.Add(generatedBrick);
                            touchesExistingBrick = true;
                            break;
                        }

                        if (touchesExistingBrick)
                        {
                            break;
                        }
                    }
                }
            }

            if (touchesExistingBrick)
            {
                continue;
            }

            BrickBounds brickBounds = new BrickBounds(generatedBrick, worldBounds);
            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    for (int z = minCell.z; z <= maxCell.z; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (!occupiedCells.TryGetValue(cell, out List<BrickBounds> candidates))
                        {
                            candidates = new List<BrickBounds>();
                            occupiedCells.Add(cell, candidates);
                        }

                        candidates.Add(brickBounds);
                    }
                }
            }
        }

        foreach (GameObject brick in bricksToRemove)
        {
            Undo.DestroyObjectImmediate(brick);
        }

        CleanupEmptyGeneratedHierarchy(targetObject);
        EditorUtility.SetDirty(targetObject);
    }

    public static void CleanupNonVisibleBricks(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        CityElement cityElement = targetObject.GetComponent<CityElement>();
        if (cityElement == null)
        {
            return;
        }

        List<GameObject> generatedBricks = GetGeneratedBricks(targetObject);
        if (generatedBricks.Count == 0)
        {
            return;
        }

        int isolationLayer = FindUnusedLayer();
        if (isolationLayer < 0)
        {
            Debug.LogError("FillWithCubes could not find an unused layer for visibility cleanup.");
            return;
        }

        var visibleBricks = new HashSet<GameObject>();
        var colliderOwners = new Dictionary<Collider, GameObject>();
        var brickColliders = new Dictionary<GameObject, List<Collider>>();
        var brickBounds = new Dictionary<GameObject, Bounds>();
        var temporaryColliders = new List<Collider>();
        var layerStates = new List<VisibilityLayerState>();
        var activationStates = new List<ActivationState>();
        Camera visibilityCamera = null;
        GameObject cameraObject = null;

        try
        {
            TemporarilyActivateHierarchy(targetObject.transform, activationStates);

            cameraObject = EditorUtility.CreateGameObjectWithHideFlags(
                "FillWithCubes Visibility Camera",
                HideFlags.HideAndDontSave,
                typeof(Camera));
            visibilityCamera = cameraObject.GetComponent<Camera>();
            ApplyCameraSettings(visibilityCamera, CameraSettings.From(Camera.main), cityElement.camPos, cityElement.camRot);

            for (int i = 0; i < generatedBricks.Count; i++)
            {
                GameObject brick = generatedBricks[i];
                if (brick == null)
                {
                    continue;
                }

                Renderer[] renderers = brick.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    continue;
                }

                if (!TryGetBrickBounds(brick, out Bounds worldBounds))
                {
                    continue;
                }

                brickBounds[brick] = worldBounds;
                CaptureHierarchyLayers(brick.transform, isolationLayer, layerStates);
                RegisterBrickColliders(brick, colliderOwners, brickColliders, temporaryColliders);
            }

            int layerMask = 1 << isolationLayer;
            CollectVisibleBricksFromViewportRays(visibilityCamera, layerMask, colliderOwners, visibleBricks);
            CollectVisibleBricksFromSampleRays(visibilityCamera, layerMask, brickBounds, brickColliders, colliderOwners, visibleBricks);
        }
        finally
        {
            foreach (Collider collider in temporaryColliders)
            {
                if (collider != null)
                {
                    Object.DestroyImmediate(collider);
                }
            }

            foreach (VisibilityLayerState layerState in layerStates)
            {
                if (layerState.Transform != null)
                {
                    layerState.Transform.gameObject.layer = layerState.OriginalLayer;
                }
            }

            if (cameraObject != null)
            {
                Object.DestroyImmediate(cameraObject);
            }

            RestoreActivationStates(activationStates);
        }

        int removedCount = 0;
        foreach (GameObject brick in generatedBricks)
        {
            if (brick == null || visibleBricks.Contains(brick))
            {
                continue;
            }

            Undo.DestroyObjectImmediate(brick);
            removedCount++;
        }

        CleanupEmptyGeneratedHierarchy(targetObject);
        EditorUtility.SetDirty(targetObject);

        Debug.Log($"Removed {removedCount} non-visible bricks from {targetObject.name}.");
    }

    public static void ClearBricks(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        var uniqueContainers = new HashSet<GameObject>();
        Transform[] transforms = targetObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform current in transforms)
        {
            if (current != null && current.name == FillWithCubesSettings.GeneratedGroupName)
            {
                uniqueContainers.Add(current.gameObject);
            }
        }

        foreach (GameObject container in uniqueContainers)
        {
            Undo.DestroyObjectImmediate(container);
        }
    }

    private static int AddBricksForMesh(
        FillWithCubesSettings settings,
        Transform targetRoot,
        Transform generatedRoot,
        Dictionary<Transform, Transform> mirroredParents,
        HashSet<BrickPlacementKey> occupiedPlacements,
        MeshCollider meshCollider)
    {
        Bounds localBounds = meshCollider.sharedMesh.bounds;
        Vector3 localBrickSize = BrickVolumeUtility.GetLocalBrickSize(meshCollider.transform, settings.SafeBrickSize);
        Vector3 localGap = BrickVolumeUtility.GetLocalGap(meshCollider.transform, settings.SafeBrickGap);
        Vector3 localPitch = localBrickSize + localGap;
        Vector3Int counts = BrickVolumeUtility.GetGridCounts(localBounds, localBrickSize, localPitch);
        Vector3 start = BrickVolumeUtility.GetGridStart(localBounds, counts, localBrickSize, localPitch);
        Transform brickGroup = null;
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

                    if (!BrickVolumeUtility.DoesBrickIntersectMesh(meshCollider, localCenter, localBrickSize))
                    {
                        continue;
                    }

                    Quaternion worldRotation = meshCollider.transform.rotation;
                    Vector3 worldPosition = meshCollider.transform.TransformPoint(localCenter);
                    BrickPlacementKey placementKey = BrickPlacementKey.From(worldPosition, worldRotation);
                    if (!occupiedPlacements.Add(placementKey))
                    {
                        continue;
                    }

                    if (brickGroup == null)
                    {
                        Transform mirroredParent = GetOrCreateMirroredTransform(
                            targetRoot,
                            generatedRoot,
                            meshCollider.transform,
                            mirroredParents);
                        brickGroup = CreateColliderGroup(mirroredParent, meshCollider);
                    }

                    CreateBrick(settings, brickGroup, localCenter, localBrickSize);
                    bricksCreated++;
                }
            }
        }

        return bricksCreated > 0 ? 1 : 0;
    }

    private static int AddBricksForBounds(
        FillWithCubesSettings settings,
        Transform targetRoot,
        Transform generatedRoot,
        Dictionary<Transform, Transform> mirroredParents,
        HashSet<BrickPlacementKey> occupiedPlacements,
        Collider sourceCollider,
        Bounds localBounds)
    {
        Vector3 localBrickSize = BrickVolumeUtility.GetLocalBrickSize(sourceCollider.transform, settings.SafeBrickSize);
        Vector3 localGap = BrickVolumeUtility.GetLocalGap(sourceCollider.transform, settings.SafeBrickGap);
        Vector3 localPitch = localBrickSize + localGap;
        Vector3Int counts = BrickVolumeUtility.GetGridCounts(localBounds, localBrickSize, localPitch);
        Vector3 start = BrickVolumeUtility.GetGridStart(localBounds, counts, localBrickSize, localPitch);
        Transform brickGroup = null;
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

                    Quaternion worldRotation = sourceCollider.transform.rotation;
                    Vector3 worldPosition = sourceCollider.transform.TransformPoint(localCenter);
                    BrickPlacementKey placementKey = BrickPlacementKey.From(worldPosition, worldRotation);
                    if (!occupiedPlacements.Add(placementKey))
                    {
                        continue;
                    }

                    if (brickGroup == null)
                    {
                        Transform mirroredParent = GetOrCreateMirroredTransform(
                            targetRoot,
                            generatedRoot,
                            sourceCollider.transform,
                            mirroredParents);
                        brickGroup = CreateColliderGroup(mirroredParent, sourceCollider);
                    }

                    CreateBrick(settings, brickGroup, localCenter, localBrickSize);
                    bricksCreated++;
                }
            }
        }

        return bricksCreated > 0 ? 1 : 0;
    }

    private static void CreateBrick(FillWithCubesSettings settings, Transform parent, Vector3 localPosition, Vector3 localScale)
    {
        GameObject brick = CreateBrickObject(settings);
        Material brickMaterial = AssetDatabase.LoadAssetAtPath<Material>(FillWithCubesSettings.BrickMaterialPath);
        Undo.SetTransformParent(brick.transform, parent, UndoLabel);
        brick.name = "Brick";
        brick.transform.localPosition = localPosition;
        brick.transform.localRotation = Quaternion.identity;
        brick.transform.localScale = localScale;

        if (!settings.AddBrickColliders)
        {
            RemoveBrickColliders(brick);
        }

        if (brickMaterial != null)
        {
            Renderer[] renderers = brick.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial = brickMaterial;
            }
        }
    }

    private static GameObject CreateBrickObject(FillWithCubesSettings settings)
    {
        GameObject brick;
        GameObject brickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FillWithCubesSettings.BrickPrefabPath);

        if (brickPrefab != null)
        {
            brick = (GameObject)PrefabUtility.InstantiatePrefab(brickPrefab);
            if (brick == null)
            {
                brick = Object.Instantiate(brickPrefab);
            }
        }
        else
        {
            brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        Undo.RegisterCreatedObjectUndo(brick, UndoLabel);
        return brick;
    }

    private static Transform CreateGeneratedRoot(Transform parent)
    {
        GameObject root = new GameObject(FillWithCubesSettings.GeneratedGroupName);
        Undo.RegisterCreatedObjectUndo(root, UndoLabel);
        Undo.SetTransformParent(root.transform, parent, UndoLabel);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root.transform;
    }

    private static Transform GetOrCreateMirroredTransform(
        Transform targetRoot,
        Transform generatedRoot,
        Transform sourceTransform,
        Dictionary<Transform, Transform> mirroredParents)
    {
        if (sourceTransform == targetRoot)
        {
            return generatedRoot;
        }

        if (mirroredParents.TryGetValue(sourceTransform, out Transform existingMirror))
        {
            return existingMirror;
        }

        Transform mirroredParent = GetOrCreateMirroredTransform(
            targetRoot,
            generatedRoot,
            sourceTransform.parent,
            mirroredParents);

        GameObject mirrorObject = new GameObject(sourceTransform.name);
        Undo.RegisterCreatedObjectUndo(mirrorObject, UndoLabel);
        Undo.SetTransformParent(mirrorObject.transform, mirroredParent, UndoLabel);
        mirrorObject.transform.localPosition = sourceTransform.localPosition;
        mirrorObject.transform.localRotation = sourceTransform.localRotation;
        mirrorObject.transform.localScale = sourceTransform.localScale;

        mirroredParents[sourceTransform] = mirrorObject.transform;
        return mirrorObject.transform;
    }

    private static Transform CreateColliderGroup(Transform parent, Collider collider)
    {
        GameObject group = new GameObject(FillWithCubesSettings.GeneratedGroupName + "_" + collider.name + "_" + collider.GetType().Name);
        Undo.RegisterCreatedObjectUndo(group, UndoLabel);
        Undo.SetTransformParent(group.transform, parent, UndoLabel);
        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        group.transform.localScale = Vector3.one;
        return group.transform;
    }

    private static void RemoveBrickColliders(GameObject brick)
    {
        Collider[] colliders = brick.GetComponentsInChildren<Collider>(true);
        foreach (Collider brickCollider in colliders)
        {
            Undo.DestroyObjectImmediate(brickCollider);
        }
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

    private static void CleanupEmptyGeneratedHierarchy(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return;
        }

        Transform[] transforms = targetObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform transform in transforms.OrderByDescending(GetTransformDepth))
        {
            if (transform == null || transform == targetObject.transform)
            {
                continue;
            }

            if (!IsUnderGeneratedRoot(transform))
            {
                continue;
            }

            if (transform.childCount > 0)
            {
                continue;
            }

            if (transform.GetComponents<Component>().Length > 1)
            {
                continue;
            }

            Undo.DestroyObjectImmediate(transform.gameObject);
        }
    }

    private static List<GameObject> GetGeneratedBricks(GameObject targetObject)
    {
        var bricks = new List<GameObject>();
        Transform[] transforms = targetObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform root in transforms)
        {
            if (root == null || root.name != FillWithCubesSettings.GeneratedGroupName)
            {
                continue;
            }

            Transform[] descendants = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform current in descendants)
            {
                if (current == root)
                {
                    continue;
                }

                if (!current.name.StartsWith(FillWithCubesSettings.GeneratedGroupName + "_"))
                {
                    continue;
                }

                for (int i = 0; i < current.childCount; i++)
                {
                    bricks.Add(current.GetChild(i).gameObject);
                }
            }
        }

        return bricks;
    }

    private static bool IsUnderGeneratedRoot(Transform transform)
    {
        Transform current = transform;
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

    private static void CaptureHierarchyLayers(Transform root, int isolationLayer, List<VisibilityLayerState> layerStates)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform transform in transforms)
        {
            if (transform == null)
            {
                continue;
            }

            layerStates.Add(new VisibilityLayerState(transform, transform.gameObject.layer));
            transform.gameObject.layer = isolationLayer;
        }
    }

    private static void TemporarilyActivateHierarchy(Transform target, List<ActivationState> activationStates)
    {
        Transform current = target;
        while (current != null)
        {
            activationStates.Add(new ActivationState(current.gameObject, current.gameObject.activeSelf));
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private static void RestoreActivationStates(List<ActivationState> activationStates)
    {
        for (int i = 0; i < activationStates.Count; i++)
        {
            ActivationState state = activationStates[i];
            if (state.GameObject != null && state.GameObject.activeSelf != state.WasActiveSelf)
            {
                state.GameObject.SetActive(state.WasActiveSelf);
            }
        }
    }

    private static void RegisterBrickColliders(
        GameObject brick,
        Dictionary<Collider, GameObject> colliderOwners,
        Dictionary<GameObject, List<Collider>> brickColliders,
        List<Collider> temporaryColliders)
    {
        if (!brickColliders.TryGetValue(brick, out List<Collider> colliders))
        {
            colliders = new List<Collider>();
            brickColliders.Add(brick, colliders);
        }

        Collider[] existingColliders = brick.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in existingColliders)
        {
            RegisterBrickCollider(brick, collider, colliderOwners, colliders);
        }

        if (colliders.Count > 0)
        {
            return;
        }

        Renderer[] renderers = brick.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Collider collider = CreateTemporaryColliderForRenderer(renderer);
            if (collider == null)
            {
                continue;
            }

            temporaryColliders.Add(collider);
            RegisterBrickCollider(brick, collider, colliderOwners, colliders);
        }
    }

    private static void RegisterBrickCollider(
        GameObject brick,
        Collider collider,
        Dictionary<Collider, GameObject> colliderOwners,
        List<Collider> colliders)
    {
        if (collider == null || colliderOwners.ContainsKey(collider))
        {
            return;
        }

        colliderOwners.Add(collider, brick);
        colliders.Add(collider);
    }

    private static Collider CreateTemporaryColliderForRenderer(Renderer renderer)
    {
        if (renderer.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
        {
            MeshCollider meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = false;
            return meshCollider;
        }

        BoxCollider boxCollider = renderer.gameObject.AddComponent<BoxCollider>();
        Vector3 localCenter = renderer.transform.InverseTransformPoint(renderer.bounds.center);
        Vector3 localSize = renderer.transform.InverseTransformVector(renderer.bounds.size);
        boxCollider.center = localCenter;
        boxCollider.size = new Vector3(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z));
        return boxCollider;
    }

    private static void CollectVisibleBricksFromViewportRays(
        Camera camera,
        int layerMask,
        Dictionary<Collider, GameObject> colliderOwners,
        HashSet<GameObject> visibleBricks)
    {
        int rayColumns = Mathf.Max(1, Mathf.RoundToInt(VisibilityRayRows * camera.aspect));
        float maxDistance = camera.farClipPlane;

        for (int row = 0; row < VisibilityRayRows; row++)
        {
            float v = (row + 0.5f) / VisibilityRayRows;
            for (int column = 0; column < rayColumns; column++)
            {
                float u = (column + 0.5f) / rayColumns;
                Ray ray = camera.ViewportPointToRay(new Vector3(u, v, 0f));
                if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (colliderOwners.TryGetValue(hit.collider, out GameObject brick))
                {
                    visibleBricks.Add(brick);
                }
            }
        }
    }

    private static void CollectVisibleBricksFromSampleRays(
        Camera camera,
        int layerMask,
        Dictionary<GameObject, Bounds> brickBounds,
        Dictionary<GameObject, List<Collider>> brickColliders,
        Dictionary<Collider, GameObject> colliderOwners,
        HashSet<GameObject> visibleBricks)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        Vector3 cameraPosition = camera.transform.position;

        foreach (KeyValuePair<GameObject, Bounds> brickEntry in brickBounds)
        {
            GameObject brick = brickEntry.Key;
            if (brick == null || visibleBricks.Contains(brick))
            {
                continue;
            }

            Bounds bounds = brickEntry.Value;
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
            {
                continue;
            }

            if (!brickColliders.TryGetValue(brick, out List<Collider> colliders) || colliders.Count == 0)
            {
                continue;
            }

            bool isVisible = false;
            foreach (Collider collider in colliders)
            {
                if (collider == null)
                {
                    continue;
                }

                foreach (Vector3 samplePoint in GetVisibilitySamplePoints(collider, cameraPosition))
                {
                    if (!IsSamplePointVisible(camera, layerMask, colliderOwners, brick, samplePoint))
                    {
                        continue;
                    }

                    visibleBricks.Add(brick);
                    isVisible = true;
                    break;
                }

                if (isVisible)
                {
                    break;
                }
            }
        }
    }

    private static IEnumerable<Vector3> GetVisibilitySamplePoints(Collider collider, Vector3 cameraPosition)
    {
        Bounds bounds = collider.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3 center = bounds.center;

        yield return bounds.ClosestPoint(cameraPosition);
        yield return center;

        yield return new Vector3(min.x, min.y, min.z);
        yield return new Vector3(min.x, min.y, max.z);
        yield return new Vector3(min.x, max.y, min.z);
        yield return new Vector3(min.x, max.y, max.z);
        yield return new Vector3(max.x, min.y, min.z);
        yield return new Vector3(max.x, min.y, max.z);
        yield return new Vector3(max.x, max.y, min.z);
        yield return new Vector3(max.x, max.y, max.z);

        yield return new Vector3(min.x, center.y, center.z);
        yield return new Vector3(max.x, center.y, center.z);
        yield return new Vector3(center.x, min.y, center.z);
        yield return new Vector3(center.x, max.y, center.z);
        yield return new Vector3(center.x, center.y, min.z);
        yield return new Vector3(center.x, center.y, max.z);
    }

    private static bool IsSamplePointVisible(
        Camera camera,
        int layerMask,
        Dictionary<Collider, GameObject> colliderOwners,
        GameObject targetBrick,
        Vector3 samplePoint)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(samplePoint);
        if (viewportPoint.z <= 0f ||
            viewportPoint.x < 0f ||
            viewportPoint.x > 1f ||
            viewportPoint.y < 0f ||
            viewportPoint.y > 1f)
        {
            return false;
        }

        Vector3 origin = camera.transform.position;
        Vector3 direction = samplePoint - origin;
        float distance = direction.magnitude;
        if (distance <= Mathf.Epsilon)
        {
            return true;
        }

        direction /= distance;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, distance + VisibilityRayDistancePadding, layerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return colliderOwners.TryGetValue(hit.collider, out GameObject hitBrick) && hitBrick == targetBrick;
    }

    private static int GetTransformDepth(Transform transform)
    {
        int depth = 0;
        Transform current = transform;
        while (current != null)
        {
            depth++;
            current = current.parent;
        }

        return depth;
    }

    private readonly struct BrickPlacementKey
    {
        private readonly int posX;
        private readonly int posY;
        private readonly int posZ;
        private readonly int rotX;
        private readonly int rotY;
        private readonly int rotZ;
        private readonly int rotW;

        private BrickPlacementKey(int posX, int posY, int posZ, int rotX, int rotY, int rotZ, int rotW)
        {
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            this.rotW = rotW;
        }

        public static BrickPlacementKey From(Vector3 position, Quaternion rotation)
        {
            return new BrickPlacementKey(
                Quantize(position.x),
                Quantize(position.y),
                Quantize(position.z),
                Quantize(rotation.x),
                Quantize(rotation.y),
                Quantize(rotation.z),
                Quantize(rotation.w));
        }

        private static int Quantize(float value)
        {
            return Mathf.RoundToInt(value * PlacementPrecision);
        }
    }

    private readonly struct BrickBounds
    {
        public readonly GameObject GameObject;
        public readonly Bounds WorldBounds;

        public BrickBounds(GameObject gameObject, Bounds worldBounds)
        {
            GameObject = gameObject;
            WorldBounds = worldBounds;
        }
    }

    private static bool TryGetBrickBounds(GameObject brick, out Bounds worldBounds)
    {
        Renderer[] renderers = brick.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            worldBounds = default;
            return false;
        }

        worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            worldBounds.Encapsulate(renderers[i].bounds);
        }

        return true;
    }

    private static Vector3Int WorldToCell(Vector3 worldPoint)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPoint.x / TouchingCellSize),
            Mathf.FloorToInt(worldPoint.y / TouchingCellSize),
            Mathf.FloorToInt(worldPoint.z / TouchingCellSize));
    }

    private static bool DoBoundsTouchOrOverlap(Bounds a, Bounds b)
    {
        return a.min.x <= b.max.x + TouchingEpsilon &&
            a.max.x >= b.min.x - TouchingEpsilon &&
            a.min.y <= b.max.y + TouchingEpsilon &&
            a.max.y >= b.min.y - TouchingEpsilon &&
            a.min.z <= b.max.z + TouchingEpsilon &&
            a.max.z >= b.min.z - TouchingEpsilon;
    }

    private static int FindUnusedLayer()
    {
        var usedLayers = new bool[32];
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (GameObject gameObject in allObjects)
        {
            if (gameObject != null)
            {
                usedLayers[gameObject.layer] = true;
            }
        }

        for (int layer = 31; layer >= 8; layer--)
        {
            if (!usedLayers[layer])
            {
                return layer;
            }
        }

        return -1;
    }

    private static void ApplyCameraSettings(Camera camera, CameraSettings settings, Vector3 position, Vector3 rotationEuler)
    {
        camera.enabled = false;
        camera.cameraType = CameraType.Preview;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.useOcclusionCulling = false;
        camera.orthographic = settings.Orthographic;
        camera.orthographicSize = settings.OrthographicSize;
        camera.fieldOfView = settings.FieldOfView;
        camera.aspect = settings.Aspect;
        camera.nearClipPlane = settings.NearClipPlane;
        camera.farClipPlane = settings.FarClipPlane;
        camera.transform.position = position;
        camera.transform.rotation = Quaternion.Euler(rotationEuler);
    }

    private readonly struct VisibilityLayerState
    {
        public readonly Transform Transform;
        public readonly int OriginalLayer;

        public VisibilityLayerState(Transform transform, int originalLayer)
        {
            Transform = transform;
            OriginalLayer = originalLayer;
        }
    }

    private readonly struct ActivationState
    {
        public readonly GameObject GameObject;
        public readonly bool WasActiveSelf;

        public ActivationState(GameObject gameObject, bool wasActiveSelf)
        {
            GameObject = gameObject;
            WasActiveSelf = wasActiveSelf;
        }
    }

    private readonly struct CameraSettings
    {
        public readonly bool Orthographic;
        public readonly float OrthographicSize;
        public readonly float FieldOfView;
        public readonly float Aspect;
        public readonly float NearClipPlane;
        public readonly float FarClipPlane;

        private CameraSettings(bool orthographic, float orthographicSize, float fieldOfView, float aspect, float nearClipPlane, float farClipPlane)
        {
            Orthographic = orthographic;
            OrthographicSize = orthographicSize;
            FieldOfView = fieldOfView;
            Aspect = aspect;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
        }

        public static CameraSettings From(Camera camera)
        {
            if (camera == null)
            {
                return new CameraSettings(false, 5f, 60f, 16f / 9f, 0.3f, 1000f);
            }

            return new CameraSettings(
                camera.orthographic,
                camera.orthographicSize,
                camera.fieldOfView,
                camera.aspect,
                camera.nearClipPlane,
                camera.farClipPlane);
        }
    }

    private static float TouchingCellSize => 0.5f;
}
