using UnityEngine;
using UnityEngine.Assertions;

public class BrickLightController : MonoBehaviour
{
    private const float DistanceFromBrickFace = 6f;
    private const float DebugSphereRadius = 0.35f;
    private static readonly Bounds UnitCubeBounds = new Bounds(Vector3.zero, Vector3.one);
    private static readonly Vector2 FacePlaneOffset = new Vector2(0, 0);
    private static readonly FaceBasis[] FaceBases =
    {
        new FaceBasis(Vector3.right, Vector3.forward, Vector3.up),
        new FaceBasis(Vector3.left, Vector3.back, Vector3.up),
        new FaceBasis(Vector3.up, Vector3.right, Vector3.back),
        new FaceBasis(Vector3.down, Vector3.right, Vector3.forward),
        new FaceBasis(Vector3.forward, Vector3.right, Vector3.up),
        new FaceBasis(Vector3.back, Vector3.left, Vector3.up)
    };

    private struct FaceBasis
    {
        public readonly Vector3 normal;
        public readonly Vector3 right;
        public readonly Vector3 up;

        public FaceBasis(Vector3 normal, Vector3 right, Vector3 up)
        {
            this.normal = normal;
            this.right = right;
            this.up = up;
        }
    }

    [SerializeField] private Vector3 additionalOffsetRotationEuler = new Vector3(20, 20, 20);

    private Light lightComponent;//point light.
    private Transform debugBrick;
    private Vector3 debugCameraPosition;
    private Bounds debugBrickBounds;

    void Start()
    {
        this.lightComponent = this.GetComponentInChildren<Light>(true);
        Assert.IsNotNull(this.lightComponent, $"BrickLightController Start: failed to find Light component on {this.name}");
        Assert.IsTrue(this.lightComponent.type == LightType.Point, $"BrickLightController Start: expected Light component to be of type Point on {this.name}");
        this.lightComponent.gameObject.SetActive(false);
        Assert.IsNotNull(this.lightComponent, $"BrickLightController Start: failed to find Light component on {this.name}");
        CamModel.Instance.OnMoveCameraToCityElement += ShowLightForCityElement;
        CityModel.Instance.OnEnableDifferentColors += ShowLightForCityElement;//update light position when new colors are enabled, as it may change which brick is lit.
    }

    private void ShowLightForCityElement(CityElement cityElement)
    {
        RefreshLightForCityElement(cityElement);
    }

    private void RefreshLightForCityElement(CityElement cityElement)
    {
        this.lightComponent.gameObject.SetActive(true);
        var mainCamera = Camera.main;
        var camPos = mainCamera != null ? mainCamera.transform.position : cityElement.camPos;
        var additionalOffsetRotation = Quaternion.Euler(this.additionalOffsetRotationEuler);
        var sortedBricks = cityElement.GetBrickLayersContainer().sortedBricks;
        Assert.IsTrue(sortedBricks.Count > 0, $"BrickLightController ShowLightForCityElement: no bricks found for city element {cityElement.name}");

        //var brick = sortedBricks.FindLast((b) => b.gameObject.activeSelf);

        var brick = cityElement.GetColoredBricks().FindLast((b) => b.brickTransform.gameObject.activeSelf).brickTransform;
        Assert.IsNotNull(brick, $"BrickLightController ShowLightForCityElement: no active brick found for city element {cityElement.name}");

        Assert.AreNotEqual(camPos, Vector3.zero, $"BrickLightController ShowLightForCityElement: camPos should not be Vector3.zero for city element {cityElement.name}");

        var targetPos = GetClosestFaceTargetPosition(brick, UnitCubeBounds.center, UnitCubeBounds.extents, camPos, additionalOffsetRotation);
        //CacheDebugVisualization(brick, UnitCubeBounds, camPos);
        this.lightComponent.transform.position = targetPos;
    }

    private static Vector3 GetClosestFaceTargetPosition(Transform brick, Vector3 boundsCenter, Vector3 boundsExtents, Vector3 camPos, Quaternion additionalOffsetRotation)
    {
        var bestDistanceSqr = float.PositiveInfinity;
        var bestTargetPos = brick.position;
        var minimumY = brick.position.y;

        for (var i = 0; i < FaceBases.Length; i++)
        {
            GetFaceCandidateWorld(brick, boundsCenter, boundsExtents, FaceBases[i], additionalOffsetRotation, out _, out var candidateWorld);
            if (!IsCandidateAllowed(candidateWorld, minimumY))
            {
                continue;
            }

            var candidateDistanceSqr = (candidateWorld - camPos).sqrMagnitude;
            if (candidateDistanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = candidateDistanceSqr;
                bestTargetPos = candidateWorld;
            }
        }

        return bestTargetPos;
    }

    private void CacheDebugVisualization(Transform brick, Bounds meshBounds, Vector3 camPos)
    {
        this.debugBrick = brick;
        this.debugBrickBounds = meshBounds;
        this.debugCameraPosition = camPos;
    }

    private static void GetFaceCandidateWorld(
        Transform brick,
        Vector3 boundsCenter,
        Vector3 boundsExtents,
        FaceBasis face,
        Quaternion additionalOffsetRotation,
        out Vector3 faceCenterWorld,
        out Vector3 candidateWorld)
    {
        var rotatedNormal = additionalOffsetRotation * face.normal;
        var rotatedRight = additionalOffsetRotation * face.right;
        var rotatedUp = additionalOffsetRotation * face.up;
        var faceCenterLocal = boundsCenter + Vector3.Scale(rotatedNormal, boundsExtents);
        faceCenterWorld = brick.TransformPoint(faceCenterLocal);
        var faceRightWorld = brick.TransformDirection(rotatedRight).normalized;
        var faceUpWorld = brick.TransformDirection(rotatedUp).normalized;
        var faceNormalWorld = brick.TransformDirection(rotatedNormal).normalized;
        candidateWorld =
            faceCenterWorld +
            faceRightWorld * FacePlaneOffset.x +
            faceUpWorld * FacePlaneOffset.y +
            faceNormalWorld * DistanceFromBrickFace;
    }

    private void OnDrawGizmos()
    {
        if (this.debugBrick == null)
        {
            return;
        }

        var mainCamera = Camera.main;
        var camPos = mainCamera != null ? mainCamera.transform.position : this.debugCameraPosition;
        var additionalOffsetRotation = Quaternion.Euler(this.additionalOffsetRotationEuler);
        var bestIndex = GetClosestFaceIndex(this.debugBrick, this.debugBrickBounds.center, this.debugBrickBounds.extents, camPos, additionalOffsetRotation);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(camPos, DebugSphereRadius);

        var previousMatrix = Gizmos.matrix;
        Gizmos.matrix = this.debugBrick.localToWorldMatrix;
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        Gizmos.DrawWireCube(this.debugBrickBounds.center, this.debugBrickBounds.size);
        Gizmos.matrix = previousMatrix;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(this.debugBrick.position, DebugSphereRadius);

        for (var i = 0; i < FaceBases.Length; i++)
        {
            GetFaceCandidateWorld(this.debugBrick, this.debugBrickBounds.center, this.debugBrickBounds.extents, FaceBases[i], additionalOffsetRotation, out var faceCenterWorld, out var candidateWorld);
            var isAllowed = IsCandidateAllowed(candidateWorld, this.debugBrick.position.y);
            var isBest = isAllowed && i == bestIndex;

            Gizmos.color = isBest ? Color.green : (isAllowed ? Color.red : Color.gray);
            Gizmos.DrawSphere(candidateWorld, DebugSphereRadius);
            Gizmos.DrawLine(faceCenterWorld, candidateWorld);

            Gizmos.color = isBest
                ? new Color(0.4f, 1f, 0.4f, 1f)
                : (isAllowed ? new Color(1f, 0.5f, 0.5f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f));
            Gizmos.DrawLine(camPos, candidateWorld);
        }
    }

    private static int GetClosestFaceIndex(Transform brick, Vector3 boundsCenter, Vector3 boundsExtents, Vector3 camPos, Quaternion additionalOffsetRotation)
    {
        var bestDistanceSqr = float.PositiveInfinity;
        var bestIndex = -1;
        var minimumY = brick.position.y;

        for (var i = 0; i < FaceBases.Length; i++)
        {
            GetFaceCandidateWorld(brick, boundsCenter, boundsExtents, FaceBases[i], additionalOffsetRotation, out _, out var candidateWorld);
            if (!IsCandidateAllowed(candidateWorld, minimumY))
            {
                continue;
            }

            var candidateDistanceSqr = (candidateWorld - camPos).sqrMagnitude;
            if (candidateDistanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = candidateDistanceSqr;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private static bool IsCandidateAllowed(Vector3 candidateWorld, float minimumY)
    {
        return candidateWorld.y >= minimumY;
    }

    void OnDestroy()
    {
        CamModel.Instance.OnMoveCameraToCityElement -= ShowLightForCityElement;
        CityModel.Instance.OnEnableDifferentColors -= ShowLightForCityElement;
    }

}
