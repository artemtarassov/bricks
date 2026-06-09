using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RocketPathLauncher : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float duration = 3f;
    [SerializeField, Min(0)] private int chaosPoints = 5;
    [SerializeField, Min(0f)] private float chaosRadius = 4f;
    [SerializeField, Min(1f)] private float accelerationPower = 2.5f;

    [Header("Comedy Rotation")]
    [SerializeField, Min(0f)] private float wobbleAmount = 25f;
    [SerializeField, Min(0f)] private float rollSpeed = 720f;
    [SerializeField] private bool reduceWobbleNearTarget = true;

    [Header("Cleanup")]
    [SerializeField, Min(0.1f)] private float explosionLifetime = 5f;

    private GameObject rocketInstance;
    private List<Vector3> points;
    private Vector3 currentTargetPosition;
    private Action<Vector3> onHitTarget;

    private float time;
    private bool flying;

    private const float MinDuration = 0.01f;
    private const float MinVelocitySqrMagnitude = 0.0001f;

    private void Awake()
    {
        ValidateConfiguration();
    }

    private void OnValidate()
    {
        duration = Mathf.Max(MinDuration, duration);
        chaosPoints = Mathf.Max(0, chaosPoints);
        chaosRadius = Mathf.Max(0f, chaosRadius);
        accelerationPower = Mathf.Max(1f, accelerationPower);
        wobbleAmount = Mathf.Max(0f, wobbleAmount);
        rollSpeed = Mathf.Max(0f, rollSpeed);
        explosionLifetime = Mathf.Max(0.1f, explosionLifetime);
    }

    public void Launch(
        Vector3 startPosition,
        Vector3 targetPosition,
        Action<Vector3> onHit = null
    )
    {
        ValidateConfiguration();
        ValidatePosition(startPosition, nameof(startPosition));
        ValidatePosition(targetPosition, nameof(targetPosition));

        if (!IsConfigurationUsable())
            return;

        if (!IsValidPosition(startPosition) || !IsValidPosition(targetPosition))
            return;

        if (rocketInstance != null)
        {
            Destroy(rocketInstance);
            rocketInstance = null;
        }

        currentTargetPosition = targetPosition;
        onHitTarget = onHit;

        rocketInstance = Instantiate(
            rocketPrefab,
            startPosition,
            Quaternion.identity
        );

        GeneratePath(startPosition, targetPosition);

        Assert.IsNotNull(points);
        Assert.IsTrue(points.Count >= 4, "Generated path must contain at least 4 points.");

        time = 0f;
        flying = true;
    }

    private void Update()
    {
        if (!flying)
            return;

        Assert.IsNotNull(rocketInstance, "Rocket instance became null while flying.");
        Assert.IsNotNull(points, "Path points became null while flying.");
        Assert.IsTrue(points.Count >= 4, "Path must contain at least 4 points.");

        if (rocketInstance == null || points == null || points.Count < 4)
        {
            flying = false;
            return;
        }

        time += Time.deltaTime;

        float t = Mathf.Clamp01(time / duration);
        float moveT = Mathf.Pow(t, accelerationPower);

        Vector3 position = GetCatmullRomPosition(moveT);
        Vector3 nextPosition = GetCatmullRomPosition(Mathf.Clamp01(moveT + 0.01f));

        rocketInstance.transform.position = position;

        Vector3 velocity = nextPosition - position;

        if (velocity.sqrMagnitude > MinVelocitySqrMagnitude)
        {
            Quaternion lookRotation = Quaternion.LookRotation(velocity.normalized);

            float wobbleFactor = reduceWobbleNearTarget ? 1f - moveT : 1f;

            float wobbleX =
                Mathf.Sin(time * 12f) *
                wobbleAmount *
                wobbleFactor;

            float wobbleY =
                Mathf.Cos(time * 9f) *
                wobbleAmount *
                wobbleFactor;

            float roll = time * rollSpeed;

            rocketInstance.transform.rotation =
                lookRotation * Quaternion.Euler(wobbleX, wobbleY, roll);
        }

        if (t >= 1f)
            HitTarget();
    }

    private void HitTarget()
    {
        flying = false;

        Vector3 impactPosition = currentTargetPosition;

        try
        {
            onHitTarget?.Invoke(impactPosition);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                impactPosition,
                Quaternion.identity
            );

            Destroy(explosion, explosionLifetime);
        }

        if (rocketInstance != null)
        {
            Destroy(rocketInstance);
            rocketInstance = null;
        }

        onHitTarget = null;
    }

    private void GeneratePath(Vector3 start, Vector3 end)
    {
        ValidatePosition(start, nameof(start));
        ValidatePosition(end, nameof(end));

        points = new List<Vector3>();

        points.Add(start);
        points.Add(start);

        Vector3 direction = end - start;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = transform.forward;

            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector3.forward;
        }

        direction.Normalize();

        Vector3 side = Vector3.Cross(direction, Vector3.up);

        if (side.sqrMagnitude < 0.01f)
            side = Vector3.Cross(direction, Vector3.right);

        if (side.sqrMagnitude < 0.01f)
            side = Vector3.forward;

        side.Normalize();

        for (int i = 1; i <= chaosPoints; i++)
        {
            float f = i / (float)(chaosPoints + 1);
            Vector3 basePoint = Vector3.Lerp(start, end, f);

            float fade = Mathf.Sin(f * Mathf.PI);

            Vector3 randomOffset =
                side * UnityEngine.Random.Range(-chaosRadius, chaosRadius) +
                Vector3.up * UnityEngine.Random.Range(-chaosRadius, chaosRadius);

            points.Add(basePoint + randomOffset * fade);
        }

        points.Add(end);
        points.Add(end);
    }

    private Vector3 GetCatmullRomPosition(float t)
    {
        Assert.IsNotNull(points);
        Assert.IsTrue(points.Count >= 4);

        t = Mathf.Clamp01(t);

        int segmentCount = points.Count - 3;
        Assert.IsTrue(segmentCount > 0, "Path must contain at least one segment.");

        float scaledT = t * segmentCount;
        int index = Mathf.Min(Mathf.FloorToInt(scaledT), segmentCount - 1);
        float localT = scaledT - index;

        return CatmullRom(
            points[index],
            points[index + 1],
            points[index + 2],
            points[index + 3],
            localT
        );
    }

    private static Vector3 CatmullRom(
        Vector3 p0,
        Vector3 p1,
        Vector3 p2,
        Vector3 p3,
        float t
    )
    {
        t = Mathf.Clamp01(t);

        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private void ValidateConfiguration()
    {
        Assert.IsNotNull(rocketPrefab, "Rocket prefab is not assigned.");

        Assert.IsTrue(duration > 0f, "Duration must be greater than 0.");
        Assert.IsTrue(chaosPoints >= 0, "Chaos points must be 0 or greater.");
        Assert.IsTrue(chaosRadius >= 0f, "Chaos radius must be 0 or greater.");
        Assert.IsTrue(accelerationPower >= 1f, "Acceleration power must be at least 1.");
        Assert.IsTrue(wobbleAmount >= 0f, "Wobble amount must be 0 or greater.");
        Assert.IsTrue(rollSpeed >= 0f, "Roll speed must be 0 or greater.");
        Assert.IsTrue(explosionLifetime > 0f, "Explosion lifetime must be greater than 0.");
    }

    private bool IsConfigurationUsable()
    {
        return
            rocketPrefab != null &&
            duration > 0f &&
            chaosPoints >= 0 &&
            chaosRadius >= 0f &&
            accelerationPower >= 1f &&
            wobbleAmount >= 0f &&
            rollSpeed >= 0f &&
            explosionLifetime > 0f;
    }

    private static void ValidatePosition(Vector3 value, string name)
    {
        Assert.IsFalse(ContainsNaN(value), $"{name} contains NaN.");
        Assert.IsFalse(ContainsInfinity(value), $"{name} contains infinity.");
    }

    private static bool IsValidPosition(Vector3 value)
    {
        return !ContainsNaN(value) && !ContainsInfinity(value);
    }

    private static bool ContainsNaN(Vector3 value)
    {
        return
            float.IsNaN(value.x) ||
            float.IsNaN(value.y) ||
            float.IsNaN(value.z);
    }

    private static bool ContainsInfinity(Vector3 value)
    {
        return
            float.IsInfinity(value.x) ||
            float.IsInfinity(value.y) ||
            float.IsInfinity(value.z);
    }
}