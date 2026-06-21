using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlyingBricks2
{
    private const float InitialScaleMultiplier = 0.25f;
    private const float ApproachDurationRatio = 0.8f;
    private const float LandingDurationRatio = 0.2f;
    private const float TargetBounceDuration = 0.4f;
    private const float OccupiedPositionThreshold = 0.02f;
    private const float OccupiedPositionThresholdSqr = OccupiedPositionThreshold * OccupiedPositionThreshold;

    private static readonly Vector3[] CandidateApproachDirections =
    {
        //new Vector3(0f, 1f, 0f),
        new Vector3(1f, 0f, 0f),
        new Vector3(-1f, 0f, 0f),
        new Vector3(0f, 0f, 1f),
        new Vector3(0f, 0f, -1f)
    };

    private readonly GameObject flyingBrickPrefab;
    private readonly Transform parent;
    private readonly Queue<PooledFlyingBrick> availableBricks = new Queue<PooledFlyingBrick>();
    private readonly List<PooledFlyingBrick> createdBricks = new List<PooledFlyingBrick>();

    private class PooledFlyingBrick
    {
        public GameObject gameObject;
        public Renderer renderer;
        public Vector3 initialLocalScale;
    }

    public FlyingBricks2(GameObject flyingBrickPrefab, Transform parent)
    {
        this.flyingBrickPrefab = flyingBrickPrefab;
        this.parent = parent;
    }

    public void Fly(List<Transform> occupiedBricks, FlyBrickData data)
    {
        var targetBrick = data.targetBrick;
        var pooledBrick = AcquireFlyingBrick();
        var brickTransform = pooledBrick.gameObject.transform;
        var targetPosition = targetBrick.position;
        var targetScale = targetBrick.localScale;
        var approachOffset = CalculateApproachOffset(occupiedBricks, targetBrick);

        PrepareFlyingBrick(pooledBrick, brickTransform, data, targetBrick);
        AnimateFlight(pooledBrick, brickTransform, targetBrick, targetPosition, targetScale, approachOffset);
    }

    public void Dispose()
    {
        foreach (var brick in this.createdBricks)
        {
            if (brick == null || brick.gameObject == null)
            {
                continue;
            }

            brick.gameObject.transform.DOKill();
        }

        this.availableBricks.Clear();
        this.createdBricks.Clear();
    }

    private void PrepareFlyingBrick(
        PooledFlyingBrick pooledBrick,
        Transform brickTransform,
        FlyBrickData flyData,
        Transform targetBrick)
    {
        brickTransform.SetParent(this.parent);
        brickTransform.position = flyData.from;
        brickTransform.localScale = pooledBrick.initialLocalScale * InitialScaleMultiplier;
        brickTransform.rotation = targetBrick.rotation;

        var brickMaterial = ColoredMaterials.Instance.GetMaterialByColorIndex(flyData.colorIndex);
        pooledBrick.renderer.sharedMaterial = brickMaterial;
        pooledBrick.gameObject.SetActive(true);
    }

    private void AnimateFlight(
        PooledFlyingBrick pooledBrick,
        Transform brickTransform,
        Transform targetBrick,
        Vector3 targetPosition,
        Vector3 targetScale,
        Vector3 approachOffset)
    {
        //Time.timeScale = 0.1f;
        var flightDuration = Durations.FlyBrickDuration;
        var approachDuration = flightDuration * ApproachDurationRatio;
        var landingDuration = flightDuration * LandingDurationRatio;

        brickTransform.DOScale(targetScale, approachDuration).SetEase(Ease.Linear);
        brickTransform.DOMove(targetPosition + approachOffset, approachDuration).SetEase(Ease.InSine).OnComplete(() =>
        {
            brickTransform.DOMove(targetPosition, landingDuration).SetEase(Ease.OutSine).OnComplete(() =>
            {
                ReturnFlyingBrickToPool(pooledBrick);
            });

            targetBrick.DOMove(targetPosition - approachOffset, landingDuration).SetEase(Ease.OutSine).OnComplete(() =>
            {
                targetBrick.DOMove(targetPosition, TargetBounceDuration).SetEase(Ease.OutBack);
            });
        });
    }

    private Vector3 CalculateApproachOffset(List<Transform> occupiedBricks, Transform targetBrick)
    {
        var targetPosition = targetBrick.position;
        var mainCamera = Camera.main;
        var cameraPosition = mainCamera != null ? mainCamera.transform.position : targetPosition;
        var transparentMaterial = ColoredMaterials.Instance.GetMaterialByName("BrickMatTransparent");
        var upwardOffset = GetWorldAxisOffset(targetBrick, Vector3.up);

        if (!IsOccupiedPosition(occupiedBricks, targetPosition + upwardOffset, transparentMaterial))
        {
            return upwardOffset;
        }

        var sortedDirections = new List<Vector3>(CandidateApproachDirections);

        sortedDirections.Sort((firstDirection, secondDirection) =>
        {
            var firstCandidatePosition = targetPosition + GetWorldAxisOffset(targetBrick, firstDirection);
            var secondCandidatePosition = targetPosition + GetWorldAxisOffset(targetBrick, secondDirection);
            var firstDistance = Vector3.Distance(cameraPosition, firstCandidatePosition);
            var secondDistance = Vector3.Distance(cameraPosition, secondCandidatePosition);
            return firstDistance.CompareTo(secondDistance);
        });

        foreach (var direction in sortedDirections)
        {
            var worldOffset = GetWorldAxisOffset(targetBrick, direction);
            var candidatePosition = targetPosition + worldOffset;
            if (!IsOccupiedPosition(occupiedBricks, candidatePosition, transparentMaterial))
            {
                return worldOffset;
            }
        }

        Debug.Log("FlyingBricks2: all candidate approach positions are occupied, defaulting to upward direction");
        return upwardOffset;
    }

    private bool IsOccupiedPosition(List<Transform> occupiedBricks, Vector3 candidatePosition, Material transparentMaterial)
    {
        foreach (var brick in occupiedBricks)
        {
            if (brick == null || !brick.gameObject.activeSelf)
            {
                continue;
            }

            var renderer = brick.GetComponentInChildren<Renderer>();
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }
            if (renderer.sharedMaterial == transparentMaterial)
            {
                continue;
            }

            var distanceToCandidateSqr = (brick.position - candidatePosition).sqrMagnitude;
            if (distanceToCandidateSqr < OccupiedPositionThresholdSqr)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector3 GetWorldAxisOffset(Transform targetBrick, Vector3 worldDirection)
    {
        var worldScale = targetBrick.lossyScale;
        return new Vector3(
            worldDirection.x * Mathf.Abs(worldScale.x),
            worldDirection.y * Mathf.Abs(worldScale.y),
            worldDirection.z * Mathf.Abs(worldScale.z));
    }


    private PooledFlyingBrick AcquireFlyingBrick()
    {
        while (this.availableBricks.Count > 0)
        {
            var cachedBrick = this.availableBricks.Dequeue();
            if (cachedBrick != null && cachedBrick.gameObject != null)
            {
                cachedBrick.gameObject.transform.DOKill();
                return cachedBrick;
            }
        }

        var gameObject = Object.Instantiate(this.flyingBrickPrefab, this.parent);
        gameObject.SetActive(false);

        var pooledBrick = new PooledFlyingBrick
        {
            gameObject = gameObject,
            renderer = gameObject.GetComponent<Renderer>(),
            initialLocalScale = gameObject.transform.localScale
        };

        this.createdBricks.Add(pooledBrick);
        return pooledBrick;
    }

    private void ReturnFlyingBrickToPool(PooledFlyingBrick brick)
    {
        if (brick == null || brick.gameObject == null)
        {
            return;
        }

        brick.gameObject.transform.DOKill();
        brick.gameObject.SetActive(false);
        brick.gameObject.transform.SetParent(this.parent);
        this.availableBricks.Enqueue(brick);
    }
}
