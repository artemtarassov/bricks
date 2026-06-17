using System.Collections.Generic;
using UnityEngine;

public class BrickExplosionSteps : MonoBehaviour
{
    private const bool PlayOnStart = false;
    public static readonly int ExplosionStepCount = 10;

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

    public List<Transform> bricks = new List<Transform>();

    private readonly List<BrickAnimationState> brickStates = new List<BrickAnimationState>();
    private readonly List<BrickAnimationState> sortedBrickStates = new List<BrickAnimationState>();

    private float elapsedTime;
    private bool isPlaying;
    private bool isPrepared;
    private int currentExplosionStep;
    private Vector3 explosionCenter;

    public void Setup(List<Transform> bricks)
    {
        this.bricks = bricks;
        CacheBricks();
    }

    private void Start()
    {
        if (PlayOnStart)
        {
            Play();
            NextExplosionStep();
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        var hasActiveBricks = false;
        foreach (var brick in brickStates)
        {
            if (brick.transform == null || !brick.hasStarted)
            {
                continue;
            }

            var brickElapsedTime = elapsedTime - brick.startTime;
            var normalizedTime = Mathf.Clamp01(brickElapsedTime / brick.flyDuration);
            if (normalizedTime < 1f)
            {
                hasActiveBricks = true;
            }

            var currentTime = Mathf.Min(brickElapsedTime, brick.flyDuration);
            var displacement = brick.initialVelocity * currentTime;
            displacement.y += 0.5f * Gravity * currentTime * currentTime;

            brick.transform.position = brick.startPosition + displacement;
            brick.transform.rotation = brick.startRotation * Quaternion.Euler(brick.angularVelocity * currentTime);
            SetWorldScale(brick.transform, brick.startWorldScale * GetScaleMultiplier(normalizedTime));
        }

        if (!hasActiveBricks)
        {
            isPlaying = false;
        }
    }

    [ContextMenu("Prepare Brick Explosion Steps")]
    public void Play()
    {
        EnsureBrickCache();
        if (brickStates.Count == 0)
        {
            return;
        }

        ResetBricks();
        PrepareAnimationState();
        elapsedTime = 0f;
        isPlaying = false;
        isPrepared = true;
        currentExplosionStep = 0;
    }

    [ContextMenu("Next Brick Explosion Step")]
    public void NextExplosionStep()
    {
        if (!isPrepared)
        {
            Play();
        }

        if (!isPrepared || currentExplosionStep >= ExplosionStepCount)
        {
            return;
        }

        var stepStartIndex = GetStepStartIndex(currentExplosionStep);
        var stepBrickCount = GetStepBrickCount(currentExplosionStep);
        var stepEndIndex = Mathf.Min(stepStartIndex + stepBrickCount, sortedBrickStates.Count);
        var stepStartTime = elapsedTime;

        for (var i = stepStartIndex; i < stepEndIndex; i++)
        {
            var brick = sortedBrickStates[i];
            if (brick.transform == null)
            {
                continue;
            }

            brick.startTime = stepStartTime;
            brick.hasStarted = true;
        }

        currentExplosionStep++;
        if (stepEndIndex > stepStartIndex)
        {
            isPlaying = true;
        }
    }

    public bool ExplosionStepsCompleted()
    {
        return currentExplosionStep >= ExplosionStepCount;
    }

    [ContextMenu("Reset Brick Explosion Steps")]
    public void ResetExplosion()
    {
        isPlaying = false;
        isPrepared = false;
        elapsedTime = 0f;
        currentExplosionStep = 0;
        EnsureBrickCache();
        ResetBricks();
    }

    private void EnsureBrickCache()
    {
        if (CacheMatchesInput())
        {
            return;
        }

        CacheBricks();
    }

    private void CacheBricks()
    {
        brickStates.Clear();
        sortedBrickStates.Clear();

        foreach (var brick in bricks)
        {
            brickStates.Add(new BrickAnimationState
            {
                transform = brick,
                startPosition = brick.position,
                startRotation = brick.rotation,
                startWorldScale = brick.lossyScale
            });
        }

        sortedBrickStates.AddRange(brickStates);
        explosionCenter = GetAveragePosition();
    }

    private bool CacheMatchesInput()
    {
        var uniqueInputBricks = new HashSet<Transform>();
        foreach (var brick in bricks)
        {
            if (brick == null)
            {
                continue;
            }

            uniqueInputBricks.Add(brick);
        }

        if (uniqueInputBricks.Count != brickStates.Count)
        {
            return false;
        }

        foreach (var brickState in brickStates)
        {
            if (!uniqueInputBricks.Contains(brickState.transform))
            {
                return false;
            }
        }

        return true;
    }

    private void PrepareAnimationState()
    {
        explosionCenter = GetAveragePosition();

        foreach (var brick in brickStates)
        {
            brick.hasStarted = false;
            brick.startTime = 0f;

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
        foreach (var brick in brickStates)
        {
            if (brick.transform == null)
            {
                continue;
            }

            brick.hasStarted = false;
            brick.startTime = 0f;
            brick.transform.position = brick.startPosition;
            brick.transform.rotation = brick.startRotation;
            SetWorldScale(brick.transform, brick.startWorldScale);
        }
    }

    private int GetStepStartIndex(int stepIndex)
    {
        var baseBrickCount = sortedBrickStates.Count / ExplosionStepCount;
        var extraBricks = sortedBrickStates.Count % ExplosionStepCount;
        return stepIndex * baseBrickCount + Mathf.Min(stepIndex, extraBricks);
    }

    private int GetStepBrickCount(int stepIndex)
    {
        var baseBrickCount = sortedBrickStates.Count / ExplosionStepCount;
        var extraBricks = sortedBrickStates.Count % ExplosionStepCount;
        return baseBrickCount + (stepIndex < extraBricks ? 1 : 0);
    }

    private Vector3 GetAveragePosition()
    {
        if (brickStates.Count == 0)
        {
            return Vector3.zero;
        }

        var sum = Vector3.zero;
        foreach (var brick in brickStates)
        {
            sum += brick.startPosition;
        }

        return sum / brickStates.Count;
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
        public float startTime;
        public bool hasStarted;
    }
}
