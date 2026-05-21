using UnityEngine;

internal static class BrickVolumeUtility
{
    private static readonly Vector3[] InsideTestDirections =
    {
        Vector3.right,
        Vector3.up,
        Vector3.forward,
        new Vector3(1f, 1f, 1f).normalized,
        new Vector3(1f, 0.37f, 0.73f).normalized,
    };

    private static readonly Vector3[] CornerSigns =
    {
        new Vector3(-1f, -1f, -1f),
        new Vector3(-1f, -1f, 1f),
        new Vector3(-1f, 1f, -1f),
        new Vector3(-1f, 1f, 1f),
        new Vector3(1f, -1f, -1f),
        new Vector3(1f, -1f, 1f),
        new Vector3(1f, 1f, -1f),
        new Vector3(1f, 1f, 1f),
    };

    private static readonly Vector3[] FaceDirections =
    {
        Vector3.right,
        Vector3.left,
        Vector3.up,
        Vector3.down,
        Vector3.forward,
        Vector3.back,
    };

    public static Vector3 GetAxisScale(Transform transform)
    {
        Vector3 lossyScale = transform.lossyScale;
        return new Vector3(
            Mathf.Max(Mathf.Abs(lossyScale.x), 0.0001f),
            Mathf.Max(Mathf.Abs(lossyScale.y), 0.0001f),
            Mathf.Max(Mathf.Abs(lossyScale.z), 0.0001f));
    }

    public static Vector3 GetLocalBrickSize(Transform transform, float worldBrickSize)
    {
        Vector3 axisScale = GetAxisScale(transform);
        return new Vector3(
            worldBrickSize / axisScale.x,
            worldBrickSize / axisScale.y,
            worldBrickSize / axisScale.z);
    }

    public static Vector3 GetLocalGap(Transform transform, float worldGap)
    {
        Vector3 axisScale = GetAxisScale(transform);
        return new Vector3(
            worldGap / axisScale.x,
            worldGap / axisScale.y,
            worldGap / axisScale.z);
    }

    public static Vector3 GetCompensatedLocalScale(Transform parent, float worldBrickSize)
    {
        Vector3 axisScale = GetAxisScale(parent);
        return new Vector3(
            worldBrickSize / axisScale.x,
            worldBrickSize / axisScale.y,
            worldBrickSize / axisScale.z);
    }

    public static Vector3Int GetGridCounts(Bounds bounds, Vector3 localBrickSize, Vector3 localPitch)
    {
        return new Vector3Int(
            GetBrickCount(bounds.size.x, localBrickSize.x, localPitch.x),
            GetBrickCount(bounds.size.y, localBrickSize.y, localPitch.y),
            GetBrickCount(bounds.size.z, localBrickSize.z, localPitch.z));
    }

    public static Vector3 GetGridStart(Bounds bounds, Vector3Int counts, Vector3 localBrickSize, Vector3 localPitch)
    {
        Vector3 occupiedSize = new Vector3(
            (counts.x - 1) * localPitch.x + localBrickSize.x,
            (counts.y - 1) * localPitch.y + localBrickSize.y,
            (counts.z - 1) * localPitch.z + localBrickSize.z);

        return bounds.center - occupiedSize * 0.5f + localBrickSize * 0.5f;
    }

    public static bool DoesBrickIntersectMesh(MeshCollider meshCollider, Vector3 localCenter, Vector3 localBrickSize)
    {
        Vector3 halfSize = localBrickSize * 0.5f;
        Mesh mesh = meshCollider.sharedMesh;
        if (mesh == null)
        {
            return false;
        }

        if (IsPointInsideClosedMesh(mesh, localCenter, localBrickSize))
        {
            return true;
        }

        foreach (Vector3 sign in CornerSigns)
        {
            Vector3 localCorner = localCenter + Vector3.Scale(halfSize, sign);
            if (IsPointInsideClosedMesh(mesh, localCorner, localBrickSize))
            {
                return true;
            }
        }

        foreach (Vector3 direction in FaceDirections)
        {
            Vector3 localFaceCenter = localCenter + Vector3.Scale(halfSize, direction);
            if (IsPointInsideClosedMesh(mesh, localFaceCenter, localBrickSize))
            {
                return true;
            }
        }

        return false;
    }

    private static int GetBrickCount(float axisSize, float brickSize, float pitch)
    {
        if (axisSize <= brickSize)
        {
            return 1;
        }

        return Mathf.Max(1, Mathf.CeilToInt((axisSize - brickSize) / pitch) + 1);
    }

    private static bool IsPointInsideClosedMesh(Mesh mesh, Vector3 point, Vector3 localBrickSize)
    {
        Bounds bounds = mesh.bounds;
        float boundsPadding = Mathf.Max(localBrickSize.x, Mathf.Max(localBrickSize.y, localBrickSize.z)) * 0.5f;
        bounds.Expand(boundsPadding * 2f);
        if (!bounds.Contains(point))
        {
            return false;
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        float epsilon = Mathf.Max(0.00001f, localBrickSize.magnitude * 0.0005f);
        int insideVotes = 0;

        foreach (Vector3 direction in InsideTestDirections)
        {
            int hitCount = CountUniqueIntersections(point, direction, vertices, triangles, epsilon);
            if ((hitCount & 1) == 1)
            {
                insideVotes++;
            }
        }

        return insideVotes > InsideTestDirections.Length / 2;
    }

    private static int CountUniqueIntersections(Vector3 rayOrigin, Vector3 rayDirection, Vector3[] vertices, int[] triangles, float epsilon)
    {
        float[] hitDistances = new float[triangles.Length / 3];
        int hitDistanceCount = 0;

        for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex += 3)
        {
            Vector3 a = vertices[triangles[triangleIndex]];
            Vector3 b = vertices[triangles[triangleIndex + 1]];
            Vector3 c = vertices[triangles[triangleIndex + 2]];

            if (!TryIntersectTriangle(rayOrigin, rayDirection, a, b, c, epsilon, out float distance))
            {
                continue;
            }

            hitDistances[hitDistanceCount] = distance;
            hitDistanceCount++;
        }

        if (hitDistanceCount == 0)
        {
            return 0;
        }

        System.Array.Sort(hitDistances, 0, hitDistanceCount);

        int uniqueCount = 1;
        float lastDistance = hitDistances[0];
        for (int i = 1; i < hitDistanceCount; i++)
        {
            if (Mathf.Abs(hitDistances[i] - lastDistance) <= epsilon * 10f)
            {
                continue;
            }

            uniqueCount++;
            lastDistance = hitDistances[i];
        }

        return uniqueCount;
    }

    private static bool TryIntersectTriangle(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        float epsilon,
        out float distance)
    {
        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;
        Vector3 p = Vector3.Cross(rayDirection, edge2);
        float determinant = Vector3.Dot(edge1, p);

        if (Mathf.Abs(determinant) < epsilon)
        {
            distance = 0f;
            return false;
        }

        float inverseDeterminant = 1f / determinant;
        Vector3 t = rayOrigin - a;
        float u = Vector3.Dot(t, p) * inverseDeterminant;
        if (u < -epsilon || u > 1f + epsilon)
        {
            distance = 0f;
            return false;
        }

        Vector3 q = Vector3.Cross(t, edge1);
        float v = Vector3.Dot(rayDirection, q) * inverseDeterminant;
        if (v < -epsilon || u + v > 1f + epsilon)
        {
            distance = 0f;
            return false;
        }

        distance = Vector3.Dot(edge2, q) * inverseDeterminant;
        return distance > epsilon;
    }
}
