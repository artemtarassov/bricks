using System.Collections.Generic;
using UnityEngine;

public class BrickTopDownExplosion : MonoBehaviour
{
    private const string BrickNamePrefix = "Brick";
    private const bool PlayOnStart = true;

    private const float BaseFlyDuration = 1.2f;
    private const float FlyDurationRandomness = 0.2f;
    private const float Gravity = -12f;
    private const float HorizontalDistanceMin = 1.2f;
    private const float HorizontalDistanceMax = 3.4f;
    private const float VerticalDropMin = 1.5f;
    private const float VerticalDropMax = 4.5f;
    private const float UpwardBurstVelocityMin = 1.2f;
    private const float UpwardBurstVelocityMax = 3.5f;
    private const float DirectionRandomness = 0.45f;
    private const float RotationSpeedMin = 180f;
    private const float RotationSpeedMax = 540f;
    private const float ScaleDownStartNormalized = 0.55f;
    private const float LayerHeightTolerance = 0.1f;
    private const float LayerDelayStep = 0.08f;
    private const float LayerDelayRandomness = 0.03f;

    private readonly List<BrickAnimationState> bricks = new List<BrickAnimationState>();

    private float elapsedTime;
    private bool isPlaying;
    private Vector3 explosionCenter;

    private void Awake()
    {
        CacheBricks();
    }

    private void Start()
    {
        if (PlayOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        var hasPendingOrActiveBricks = false;
        foreach (var brick in bricks)
        {
            if (brick.transform == null)
            {
                continue;
            }

            var brickElapsedTime = elapsedTime - brick.startDelay;
            if (brickElapsedTime <= 0f)
            {
                hasPendingOrActiveBricks = true;
                continue;
            }

            var normalizedTime = Mathf.Clamp01(brickElapsedTime / brick.flyDuration);
            if (normalizedTime < 1f)
            {
                hasPendingOrActiveBricks = true;
            }

            var currentTime = Mathf.Min(brickElapsedTime, brick.flyDuration);
            var displacement = brick.initialVelocity * currentTime;
            displacement.y += 0.5f * Gravity * currentTime * currentTime;

            brick.transform.position = brick.startPosition + displacement;
            brick.transform.rotation = brick.startRotation * Quaternion.Euler(brick.angularVelocity * currentTime);
            SetWorldScale(brick.transform, brick.startWorldScale * GetScaleMultiplier(normalizedTime));
        }

        if (!hasPendingOrActiveBricks)
        {
            isPlaying = false;
        }
    }

    [ContextMenu("Play Brick Top Down Explosion")]
    public void Play()
    {
        if (bricks.Count == 0)
        {
            CacheBricks();
        }

        if (bricks.Count == 0)
        {
            return;
        }

        ResetBricks();
        PrepareAnimationState();
        elapsedTime = 0f;
        isPlaying = true;
    }

    [ContextMenu("Reset Brick Top Down Explosion")]
    public void ResetExplosion()
    {
        isPlaying = false;
        elapsedTime = 0f;
        ResetBricks();
    }

    private void CacheBricks()
    {
        bricks.Clear();

        var transforms = GetComponentsInChildren<Transform>(true);
        foreach (var child in transforms)
        {
            if (child == transform || !child.name.StartsWith(BrickNamePrefix))
            {
                continue;
            }

            bricks.Add(new BrickAnimationState
            {
                transform = child,
                startPosition = child.position,
                startRotation = child.rotation,
                startWorldScale = child.lossyScale
            });
        }

        explosionCenter = GetAveragePosition();
    }

    private void PrepareAnimationState()
    {
        explosionCenter = GetAveragePosition();

        var sortedBricks = new List<BrickAnimationState>(bricks);
        sortedBricks.Sort((a, b) => b.startPosition.y.CompareTo(a.startPosition.y));

        var currentLayerY = float.NaN;
        var currentLayerIndex = -1;
        foreach (var brick in sortedBricks)
        {
            if (currentLayerIndex < 0 || Mathf.Abs(brick.startPosition.y - currentLayerY) > LayerHeightTolerance)
            {
                currentLayerIndex++;
                currentLayerY = brick.startPosition.y;
            }

            brick.startDelay = currentLayerIndex * LayerDelayStep + Random.Range(0f, LayerDelayRandomness);
        }

        foreach (var brick in bricks)
        {
            var outward = brick.startPosition - explosionCenter;
            var flatOutward = new Vector3(outward.x, 0f, outward.z);
            if (flatOutward.sqrMagnitude < 0.001f)
            {
                flatOutward = Random.insideUnitSphere;
                flatOutward.y = 0f;
            }

            flatOutward.Normalize();
            var randomOffset = Random.insideUnitSphere * DirectionRandomness;
            randomOffset.y = 0f;
            var direction = (flatOutward + randomOffset).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            brick.flyDuration = BaseFlyDuration * Random.Range(1f - FlyDurationRandomness, 1f + FlyDurationRandomness);

            var horizontalDistance = Random.Range(HorizontalDistanceMin, HorizontalDistanceMax);
            var landingOffset = direction * horizontalDistance;
            landingOffset.y = Random.Range(-VerticalDropMax, -VerticalDropMin);

            brick.initialVelocity = CalculateInitialVelocity(landingOffset, brick.flyDuration);
            brick.initialVelocity.y += Random.Range(UpwardBurstVelocityMin, UpwardBurstVelocityMax);
            brick.angularVelocity = new Vector3(
                RandomSignedRange(RotationSpeedMin, RotationSpeedMax),
                RandomSignedRange(RotationSpeedMin, RotationSpeedMax),
                RandomSignedRange(RotationSpeedMin, RotationSpeedMax)
            );
        }
    }

    private void ResetBricks()
    {
        foreach (var brick in bricks)
        {
            if (brick.transform == null)
            {
                continue;
            }

            brick.transform.position = brick.startPosition;
            brick.transform.rotation = brick.startRotation;
            SetWorldScale(brick.transform, brick.startWorldScale);
        }
    }

    private Vector3 GetAveragePosition()
    {
        if (bricks.Count == 0)
        {
            return Vector3.zero;
        }

        var sum = Vector3.zero;
        foreach (var brick in bricks)
        {
            sum += brick.startPosition;
        }

        return sum / bricks.Count;
    }

    private Vector3 CalculateInitialVelocity(Vector3 displacement, float duration)
    {
        var velocity = displacement / duration;
        velocity.y -= 0.5f * Gravity * duration;
        return velocity;
    }

    private float GetScaleMultiplier(float normalizedTime)
    {
        if (normalizedTime <= ScaleDownStartNormalized)
        {
            return 1f;
        }

        var scaleDownProgress = Mathf.InverseLerp(ScaleDownStartNormalized, 1f, normalizedTime);
        return 1f - Mathf.SmoothStep(0f, 1f, scaleDownProgress);
    }

    private float RandomSignedRange(float min, float max)
    {
        var value = Random.Range(min, max);
        return Random.value < 0.5f ? -value : value;
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        var parent = target.parent;
        if (parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        var parentScale = parent.lossyScale;
        target.localScale = new Vector3(
            SafeDivide(worldScale.x, parentScale.x),
            SafeDivide(worldScale.y, parentScale.y),
            SafeDivide(worldScale.z, parentScale.z)
        );
    }

    private float SafeDivide(float value, float divisor)
    {
        if (Mathf.Abs(divisor) < 0.0001f)
        {
            return 0f;
        }

        return value / divisor;
    }

    private class BrickAnimationState
    {
        public Transform transform;
        public Vector3 startPosition;
        public Quaternion startRotation;
        public Vector3 startWorldScale;
        public Vector3 initialVelocity;
        public Vector3 angularVelocity;
        public float flyDuration;
        public float startDelay;
    }
}
