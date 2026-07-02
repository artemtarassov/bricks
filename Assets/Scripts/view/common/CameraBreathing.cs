using UnityEngine;

[DefaultExecutionOrder(1000)]
public class CameraBreathing : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetTransform;

    [Header("Breathing")]
    [SerializeField, Min(1f)] private float breathsPerMinute = 8f;
    [SerializeField, Range(0f, 1f)] private float intensity = 1f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Position Offset")]
    [SerializeField] private float verticalAmplitude = 0.035f;
    [SerializeField] private float forwardAmplitude = 0.02f;
    [SerializeField] private float sidewaysAmplitude = 0.01f;

    [Header("Rotation Offset")]
    [SerializeField] private float pitchAmplitude = 0.6f;
    [SerializeField] private float yawAmplitude = 0.25f;
    [SerializeField] private float rollAmplitude = 0.35f;

    [Header("Motion Feel")]
    [SerializeField, Range(0f, 1f)] private float inhaleExhaleBias = 0.3f;
    [SerializeField, Min(0f)] private float blendSpeed = 4f;
    [SerializeField] private bool randomizeStartPhase = true;

    private Transform resolvedTarget;
    private Vector3 lastAppliedPositionOffset;
    private Quaternion lastAppliedRotationOffset = Quaternion.identity;
    private float currentIntensity = 1f;
    private float phase;
    private bool hasAppliedOffset;

    private void Awake()
    {
        ResolveTarget();
        if (randomizeStartPhase)
        {
            phase = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    private void OnEnable()
    {
        ResolveTarget();
        currentIntensity = intensity;
    }

    private void OnDisable()
    {
        RemovePreviousOffset();
    }

    private void LateUpdate()
    {
        if (!ResolveTarget())
        {
            return;
        }

        RemovePreviousOffset();

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (deltaTime <= 0f)
        {
            return;
        }

        currentIntensity = Mathf.MoveTowards(currentIntensity, intensity, blendSpeed * deltaTime);
        phase += deltaTime * breathsPerMinute * Mathf.PI * 2f / 60f;

        float primaryWave = Mathf.Sin(phase);
        float secondaryWave = Mathf.Sin((phase * 2f) - (Mathf.PI * 0.35f));
        float breathWave = Mathf.Clamp(primaryWave + (secondaryWave * inhaleExhaleBias), -1f, 1f);
        float swayWave = Mathf.Sin(phase + Mathf.PI * 0.5f);
        float rollWave = Mathf.Sin((phase * 0.5f) + Mathf.PI * 0.15f);

        lastAppliedPositionOffset =
            (Vector3.up * (breathWave * verticalAmplitude)) +
            (Vector3.forward * (breathWave * forwardAmplitude)) +
            (Vector3.right * (swayWave * sidewaysAmplitude));
        lastAppliedPositionOffset *= currentIntensity;

        Vector3 rotationEuler = new Vector3(
            breathWave * pitchAmplitude,
            swayWave * yawAmplitude,
            rollWave * rollAmplitude) * currentIntensity;
        lastAppliedRotationOffset = Quaternion.Euler(rotationEuler);

        resolvedTarget.position += resolvedTarget.rotation * lastAppliedPositionOffset;
        resolvedTarget.rotation = resolvedTarget.rotation * lastAppliedRotationOffset;
        hasAppliedOffset = true;
    }

    private bool ResolveTarget()
    {
        if (targetTransform != null)
        {
            resolvedTarget = targetTransform;
            return true;
        }

        if (resolvedTarget != null)
        {
            return true;
        }

        if (TryGetComponent(out Camera ownCamera) && ownCamera != null)
        {
            resolvedTarget = transform;
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            resolvedTarget = mainCamera.transform;
            return true;
        }

        return false;
    }

    private void RemovePreviousOffset()
    {
        if (!hasAppliedOffset || resolvedTarget == null)
        {
            return;
        }

        resolvedTarget.rotation = resolvedTarget.rotation * Quaternion.Inverse(lastAppliedRotationOffset);
        resolvedTarget.position -= resolvedTarget.rotation * lastAppliedPositionOffset;
        hasAppliedOffset = false;
        lastAppliedPositionOffset = Vector3.zero;
        lastAppliedRotationOffset = Quaternion.identity;
    }
}
