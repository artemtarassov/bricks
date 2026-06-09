using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

internal static class BrickSpacingFixer
{
    private const float OverlapTolerance = 0.001f;
    private const float GridPitch = 1f;
    private const float SnapTolerance = 0.0001f;
    private const float RotationMismatchWarningDegrees = 0.1f;

    public static void FixSpacing(GameObject rootObject)
    {
        if (rootObject == null)
        {
            Debug.LogWarning("Fix Brick Spacing requires a selected GameObject.");
            return;
        }

        List<Transform> bricks = CollectBrickChildren(rootObject.transform);
        if (bricks.Count == 0)
        {
            Debug.LogWarning($"Fix Brick Spacing found no children named {FillWithCubesSettings.BrickTagName} under {rootObject.name}.");
            return;
        }

        WarnAboutMixedBrickRotations(rootObject.name, bricks);

        Undo.SetCurrentGroupName("Fix Brick Spacing");
        int undoGroup = Undo.GetCurrentGroup();
        int movedCount = SnapBricksToGrid(bricks);
        List<Transform> duplicateBricks = RemoveOverlappingBricks(bricks);

        Undo.CollapseUndoOperations(undoGroup);

        if (movedCount == 0 && duplicateBricks.Count == 0)
        {
            Debug.Log($"Fix Brick Spacing found no changes to make under {rootObject.name}.");
            return;
        }

        EditorSceneManager.MarkSceneDirty(rootObject.scene);
        Debug.Log($"Fix Brick Spacing moved {movedCount} brick(s) and removed {duplicateBricks.Count} overlapping brick(s) under {rootObject.name}.");
    }

    private static int SnapBricksToGrid(List<Transform> bricks)
    {
        Transform referenceBrick = GetReferenceBrick(bricks);
        if (referenceBrick == null)
        {
            return 0;
        }

        Matrix4x4 referenceLocalToWorld = referenceBrick.localToWorldMatrix;
        Matrix4x4 referenceWorldToLocal = referenceBrick.worldToLocalMatrix;
        var coordinatesX = new float[bricks.Count];
        var coordinatesY = new float[bricks.Count];
        var coordinatesZ = new float[bricks.Count];

        for (int i = 0; i < bricks.Count; i++)
        {
            Transform brick = bricks[i];
            if (brick == null)
            {
                continue;
            }

            Vector3 localPosition = referenceWorldToLocal.MultiplyPoint3x4(brick.position);
            coordinatesX[i] = localPosition.x;
            coordinatesY[i] = localPosition.y;
            coordinatesZ[i] = localPosition.z;
        }

        float offsetX = EstimateOffset(coordinatesX, GridPitch);
        float offsetY = EstimateOffset(coordinatesY, GridPitch);
        float offsetZ = EstimateOffset(coordinatesZ, GridPitch);
        float snapToleranceSqr = SnapTolerance * SnapTolerance;
        int movedCount = 0;

        for (int i = 0; i < bricks.Count; i++)
        {
            Transform brick = bricks[i];
            if (brick == null)
            {
                continue;
            }

            Vector3 snappedLocalPosition = new Vector3(
                SnapCoordinate(coordinatesX[i], offsetX, GridPitch),
                SnapCoordinate(coordinatesY[i], offsetY, GridPitch),
                SnapCoordinate(coordinatesZ[i], offsetZ, GridPitch));
            Vector3 snappedWorldPosition = referenceLocalToWorld.MultiplyPoint3x4(snappedLocalPosition);

            if ((snappedWorldPosition - brick.position).sqrMagnitude <= snapToleranceSqr)
            {
                continue;
            }

            Undo.RecordObject(brick, "Fix Brick Spacing");
            brick.position = snappedWorldPosition;
            PrefabUtility.RecordPrefabInstancePropertyModifications(brick);
            EditorUtility.SetDirty(brick);
            movedCount++;
        }

        return movedCount;
    }

    private static List<Transform> RemoveOverlappingBricks(List<Transform> bricks)
    {
        float overlapToleranceSqr = OverlapTolerance * OverlapTolerance;
        var duplicateBricks = new List<Transform>();
        var removeBrick = new bool[bricks.Count];

        for (int i = 0; i < bricks.Count; i++)
        {
            if (removeBrick[i])
            {
                continue;
            }

            Transform brick = bricks[i];
            if (brick == null)
            {
                continue;
            }

            Vector3 brickPosition = brick.position;
            for (int j = i + 1; j < bricks.Count; j++)
            {
                if (removeBrick[j])
                {
                    continue;
                }

                Transform otherBrick = bricks[j];
                if (otherBrick == null)
                {
                    continue;
                }

                Vector3 delta = otherBrick.position - brickPosition;
                if (delta.sqrMagnitude <= overlapToleranceSqr)
                {
                    removeBrick[j] = true;
                    duplicateBricks.Add(otherBrick);
                }
            }
        }

        for (int i = 0; i < duplicateBricks.Count; i++)
        {
            if (duplicateBricks[i] != null)
            {
                Undo.DestroyObjectImmediate(duplicateBricks[i].gameObject);
            }
        }

        return duplicateBricks;
    }

    private static List<Transform> CollectBrickChildren(Transform root)
    {
        var bricks = new List<Transform>();
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            if (current != root && current.name == FillWithCubesSettings.BrickTagName)
            {
                bricks.Add(current);
            }
        }

        return bricks;
    }

    private static Transform GetReferenceBrick(List<Transform> bricks)
    {
        for (int i = 0; i < bricks.Count; i++)
        {
            if (bricks[i] != null)
            {
                return bricks[i];
            }
        }

        return null;
    }

    private static float EstimateOffset(float[] coordinates, float pitch)
    {
        if (coordinates.Length == 0)
        {
            return 0f;
        }

        float bestOffset = Mathf.Repeat(coordinates[0], pitch);
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < coordinates.Length; i++)
        {
            float candidateOffset = Mathf.Repeat(coordinates[i], pitch);
            float score = 0f;

            for (int j = 0; j < coordinates.Length; j++)
            {
                score += GetCircularDistance(Mathf.Repeat(coordinates[j], pitch), candidateOffset, pitch);
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestOffset = candidateOffset;
            }
        }

        float accumulatedDelta = 0f;
        for (int i = 0; i < coordinates.Length; i++)
        {
            accumulatedDelta += GetShortestSignedDistance(Mathf.Repeat(coordinates[i], pitch), bestOffset, pitch);
        }

        return Mathf.Repeat(bestOffset + accumulatedDelta / coordinates.Length, pitch);
    }

    private static float SnapCoordinate(float coordinate, float offset, float pitch)
    {
        return offset + Mathf.Round((coordinate - offset) / pitch) * pitch;
    }

    private static float GetCircularDistance(float a, float b, float period)
    {
        float delta = Mathf.Abs(a - b);
        return Mathf.Min(delta, period - delta);
    }

    private static float GetShortestSignedDistance(float value, float target, float period)
    {
        float delta = value - target;
        delta -= Mathf.Round(delta / period) * period;
        return delta;
    }

    private static void WarnAboutMixedBrickRotations(string rootName, List<Transform> bricks)
    {
        Transform referenceBrick = GetReferenceBrick(bricks);
        if (referenceBrick == null)
        {
            return;
        }

        Quaternion referenceRotation = referenceBrick.rotation;
        for (int i = 0; i < bricks.Count; i++)
        {
            Transform brick = bricks[i];
            if (brick != null && Quaternion.Angle(referenceRotation, brick.rotation) > RotationMismatchWarningDegrees)
            {
                Debug.LogWarning($"Fix Brick Spacing found mixed brick rotations under {rootName}. The spacing snap uses the first brick as the grid reference.");
                return;
            }
        }
    }
}
