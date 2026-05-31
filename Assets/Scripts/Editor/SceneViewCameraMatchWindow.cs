using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneViewCameraMatchWindow : EditorWindow
{
    private const double AutoUpdateIntervalSeconds = 0.2d;

    private enum RotationSnapMode
    {
        Off,
        Degrees10,
        Degrees22_5
    }

    private bool _autoUpdateEnabled;
    private bool _snapPositionEnabled;
    private double _nextAutoUpdateTime;
    private RotationSnapMode _rotationSnapMode = RotationSnapMode.Degrees22_5;

    [MenuItem("Tools/Match Main Camera To Scene View")]
    private static void ShowWindow()
    {
        GetWindow<SceneViewCameraMatchWindow>("Match Camera");
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        _autoUpdateEnabled = false;
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Syncs the Main Camera and Scene view to each other while keeping the camera settings you chose to preserve.",
            MessageType.Info);

        Camera mainCamera = Camera.main;
        SceneView sceneView = GetSceneView();

        if (mainCamera == null)
        {
            EditorGUILayout.HelpBox("No camera tagged MainCamera was found in the open scene.", MessageType.Warning);
        }

        if (sceneView == null || sceneView.camera == null)
        {
            EditorGUILayout.HelpBox("Open a Scene view so the tool has a camera to match.", MessageType.Warning);
        }

        using (new EditorGUI.DisabledScope(mainCamera == null || sceneView == null || sceneView.camera == null))
        {
            if (GUILayout.Button("Match Main Camera To Scene View", GUILayout.Height(32f)))
            {
                MatchMainCameraToSceneView(mainCamera, sceneView, true);
            }

            if (GUILayout.Button("Match Scene View To Main Camera", GUILayout.Height(32f)))
            {
                MatchSceneViewToMainCamera(mainCamera, sceneView, true);
            }
        }

        bool autoUpdateEnabled = EditorGUILayout.ToggleLeft("Auto update every 200ms", _autoUpdateEnabled);
        if (autoUpdateEnabled != _autoUpdateEnabled)
        {
            _autoUpdateEnabled = autoUpdateEnabled;
            _nextAutoUpdateTime = EditorApplication.timeSinceStartup + AutoUpdateIntervalSeconds;
        }

        EditorGUILayout.Space();
        _snapPositionEnabled = EditorGUILayout.ToggleLeft("Snap position to 1 decimal place", _snapPositionEnabled);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rotation Snapping", EditorStyles.boldLabel);
        DrawRotationSnapOption("Off", RotationSnapMode.Off);
        DrawRotationSnapOption("10.0", RotationSnapMode.Degrees10);
        DrawRotationSnapOption("22.5", RotationSnapMode.Degrees22_5);
    }

    private static SceneView GetSceneView()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            return SceneView.lastActiveSceneView;
        }

        foreach (object sceneView in SceneView.sceneViews)
        {
            if (sceneView is SceneView view)
            {
                return view;
            }
        }

        return null;
    }

    private void OnEditorUpdate()
    {
        if (!_autoUpdateEnabled || EditorApplication.timeSinceStartup < _nextAutoUpdateTime)
        {
            return;
        }

        _nextAutoUpdateTime = EditorApplication.timeSinceStartup + AutoUpdateIntervalSeconds;

        Camera mainCamera = Camera.main;
        SceneView sceneView = GetSceneView();
        MatchMainCameraToSceneView(mainCamera, sceneView, false);
    }

    private void DrawRotationSnapOption(string label, RotationSnapMode mode)
    {
        bool isSelected = _rotationSnapMode == mode;
        bool newValue = EditorGUILayout.ToggleLeft(label, isSelected);
        if (newValue)
        {
            _rotationSnapMode = mode;
        }
        else if (isSelected)
        {
            _rotationSnapMode = RotationSnapMode.Off;
        }
    }

    private void MatchMainCameraToSceneView(Camera mainCamera, SceneView sceneView, bool showDialogs)
    {
        if (mainCamera == null)
        {
            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Main Camera Not Found",
                    "No camera tagged MainCamera was found in the open scene.",
                    "OK");
            }
            return;
        }

        if (sceneView == null || sceneView.camera == null)
        {
            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Scene View Not Found",
                    "Open a Scene view so the tool has a camera to match.",
                    "OK");
            }
            return;
        }

        Camera sceneCamera = sceneView.camera;
        Transform mainTransform = mainCamera.transform;
        Vector3 snappedPosition = SnapPosition(sceneCamera.transform.position);
        Quaternion snappedRotation = SnapRotation(sceneCamera.transform.rotation, GetRotationSnapStep());

        Undo.RegisterCompleteObjectUndo(new Object[] { mainCamera, mainTransform }, "Match Main Camera To Scene View");

        mainTransform.SetPositionAndRotation(
            snappedPosition,
            snappedRotation);

        mainCamera.orthographic = sceneCamera.orthographic;

        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = sceneCamera.orthographicSize;
        }

        EditorUtility.SetDirty(mainCamera);
        EditorUtility.SetDirty(mainTransform);

        Scene cameraScene = mainCamera.gameObject.scene;
        if (cameraScene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(cameraScene);
        }

        SceneView.RepaintAll();
    }

    private void MatchSceneViewToMainCamera(Camera mainCamera, SceneView sceneView, bool showDialogs)
    {
        if (mainCamera == null)
        {
            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Main Camera Not Found",
                    "No camera tagged MainCamera was found in the open scene.",
                    "OK");
            }
            return;
        }

        if (sceneView == null || sceneView.camera == null)
        {
            if (showDialogs)
            {
                EditorUtility.DisplayDialog(
                    "Scene View Not Found",
                    "Open a Scene view so the tool has a camera to match.",
                    "OK");
            }
            return;
        }

        Transform mainTransform = mainCamera.transform;
        float sceneDistance = Mathf.Max(0.0001f, sceneView.cameraDistance);
        Vector3 scenePivot = mainTransform.position + mainTransform.forward * sceneDistance;
        float sceneSize = mainCamera.orthographic ? mainCamera.orthographicSize : sceneView.size;

        sceneView.orthographic = mainCamera.orthographic;
        sceneView.LookAtDirect(scenePivot, mainTransform.rotation, sceneSize);
        sceneView.Repaint();
    }

    private Vector3 SnapPosition(Vector3 position)
    {
        if (!_snapPositionEnabled)
        {
            return position;
        }

        return new Vector3(
            Mathf.Round(position.x * 10f) / 10f,
            Mathf.Round(position.y * 10f) / 10f,
            Mathf.Round(position.z * 10f) / 10f);
    }

    private static Quaternion SnapRotation(Quaternion rotation, float snapStep)
    {
        Vector3 eulerAngles = rotation.eulerAngles;
        eulerAngles.z = 0f;

        if (snapStep <= 0f)
        {
            return Quaternion.Euler(eulerAngles);
        }

        eulerAngles.x = SnapAngle(eulerAngles.x, snapStep);
        eulerAngles.y = SnapAngle(eulerAngles.y, snapStep);
        return Quaternion.Euler(eulerAngles);
    }

    private static float SnapAngle(float angle, float snapStep)
    {
        return Mathf.Round(angle / snapStep) * snapStep;
    }

    private float GetRotationSnapStep()
    {
        switch (_rotationSnapMode)
        {
            case RotationSnapMode.Degrees10:
                return 10f;
            case RotationSnapMode.Degrees22_5:
                return 22.5f;
            default:
                return 0f;
        }
    }
}
