using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CamAdjustWindow : EditorWindow
{
    private const float CameraDistanceToSelection = 18;

    private static Camera s_TargetCamera;
    private static Camera s_PreviewCamera;
    private static RenderTexture s_PreviewTexture;
    private static bool s_IsPreviewEnabled;

    [MenuItem("Tools/CamAdjust Window")]
    public static void ShowWindow()
    {
        GetWindow<CamAdjustWindow>("CamAdjust");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Enable Camera Preview in editor"))
        {
            EnableCameraPreview();
        }

        if (GUILayout.Button("Rotate Main Camera To Selection"))
        {
            RotateMainCameraToSelection();
        }

        if (GUILayout.Button("Copy camera position"))
        {
            CopyCameraPosition();
        }


        if (GUILayout.Button("Move Camera to selected CityElement"))
        {
            MoveCameraToSelectedCityElement();
        }

        if (s_IsPreviewEnabled && GUILayout.Button("Disable Camera Preview in editor"))
        {
            DisableCameraPreview();
        }

        if (!s_IsPreviewEnabled)
        {
            return;
        }

        if (s_TargetCamera == null)
        {
            EditorGUILayout.HelpBox("No Main Camera is available for preview.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Main Camera Preview", EditorStyles.boldLabel);

        float aspect = Mathf.Max(0.01f, s_TargetCamera.aspect);
        Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.ExpandWidth(true));
        DrawPreviewIntoRect(previewRect);
    }

    private void MoveCameraToSelectedCityElement()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            EditorUtility.DisplayDialog(
                "Main Camera Not Found",
                "No camera tagged MainCamera was found in the open scene.",
                "OK");
            return;
        }

        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No Selection",
                "Select a GameObject with a CityElement component first.",
                "OK");
            return;
        }

        GameObject selectedObject = Selection.gameObjects[0];
        CityElement cityElement = selectedObject.GetComponent<CityElement>();
        if (cityElement == null)
        {
            Debug.LogError("select city element first");
            return;
        }

        Undo.RecordObject(mainCamera.transform, "Move Camera To Selected CityElement");
        mainCamera.transform.position = cityElement.camPos;
        mainCamera.transform.rotation = Quaternion.Euler(cityElement.camRot);
        EditorUtility.SetDirty(mainCamera.transform);

        if (s_IsPreviewEnabled)
        {
            s_TargetCamera = mainCamera;
        }

        SceneView.RepaintAll();
    }

    private static void CopyCameraPosition()
    {
        GameObject selectedObject = Selection.gameObjects[0];
        var ce = selectedObject.GetComponent<CityElement>();
        if (ce != null)
        {
            var cam = Camera.main;
            ce.camPos = cam.transform.position;
            ce.camRot = cam.transform.eulerAngles;
            //save scene

        }
    }

    private static void EnableCameraPreview()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            EditorUtility.DisplayDialog(
                "Main Camera Not Found",
                "No camera tagged MainCamera was found in the open scene.",
                "OK");
            return;
        }

        s_TargetCamera = mainCamera;
        s_IsPreviewEnabled = true;

        Selection.activeGameObject = mainCamera.gameObject;
        EditorGUIUtility.PingObject(mainCamera.gameObject);
        GetWindow<CamAdjustWindow>("CamAdjust").Repaint();
    }

    private static void DisableCameraPreview()
    {
        s_IsPreviewEnabled = false;
        s_TargetCamera = null;

        CleanupPreviewResources();
    }

    private static void RotateMainCameraToSelection()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            EditorUtility.DisplayDialog(
                "Main Camera Not Found",
                "No camera tagged MainCamera was found in the open scene.",
                "OK");
            return;
        }

        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "No Selection",
                "Select a GameObject in the scene to rotate the Main Camera toward it.",
                "OK");
            return;
        }

        GameObject selectedObject = Selection.gameObjects[0];
        if (selectedObject.GetComponent<CityElement>() == null)
        {
            Debug.LogError("select city element first");
            return;
        }
        selectedObject.gameObject.SetActive(true);
        Vector3 targetPosition = CalculateAverageHierarchyPosition(selectedObject.transform);
        Vector3 cameraOffset = mainCamera.transform.position - targetPosition;
        Vector3 offsetDirection = cameraOffset.sqrMagnitude > 0.0001f
            ? cameraOffset.normalized
            : -mainCamera.transform.forward;

        Vector3 newCameraPosition = targetPosition + offsetDirection * CameraDistanceToSelection;
        Vector3 lookDirection = targetPosition - newCameraPosition;

        Undo.RecordObject(mainCamera.transform, "Rotate Main Camera To Selection");
        mainCamera.transform.position = newCameraPosition;
        mainCamera.transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        EditorUtility.SetDirty(mainCamera.transform);

        if (s_IsPreviewEnabled)
        {
            s_TargetCamera = mainCamera;
        }

        SceneView.RepaintAll();
        CopyCameraPosition();
    }

    private void Update()
    {
        if (s_IsPreviewEnabled)
        {
            Repaint();
        }
    }

    private static Camera GetPreviewCamera(Camera sourceCamera, Vector2 previewSize)
    {
        if (s_PreviewCamera == null)
        {
            GameObject previewObject = EditorUtility.CreateGameObjectWithHideFlags(
                "CamAdjust Preview Camera",
                HideFlags.HideAndDontSave,
                typeof(Camera));

            s_PreviewCamera = previewObject.GetComponent<Camera>();
            s_PreviewCamera.enabled = false;
            s_PreviewCamera.cameraType = CameraType.Preview;
        }

        EnsurePreviewTexture(previewSize);

        s_PreviewCamera.CopyFrom(sourceCamera);
        CopyUniversalCameraData(sourceCamera, s_PreviewCamera);
        s_PreviewCamera.enabled = false;
        s_PreviewCamera.cameraType = CameraType.Preview;
        s_PreviewCamera.targetTexture = s_PreviewTexture;

        return s_PreviewCamera;
    }

    private static void EnsurePreviewTexture(Vector2 previewSize)
    {
        int width = Mathf.Max(1, Mathf.CeilToInt(previewSize.x));
        int height = Mathf.Max(1, Mathf.CeilToInt(previewSize.y));

        if (s_PreviewTexture != null &&
            s_PreviewTexture.width == width &&
            s_PreviewTexture.height == height)
        {
            return;
        }

        if (s_PreviewTexture != null)
        {
            Object.DestroyImmediate(s_PreviewTexture);
        }

        s_PreviewTexture = new RenderTexture(width, height, 24)
        {
            name = "CamAdjust Preview Texture",
            hideFlags = HideFlags.HideAndDontSave
        };
    }

    private static void CopyUniversalCameraData(Camera sourceCamera, Camera previewCamera)
    {
        if (!sourceCamera.TryGetComponent(out UniversalAdditionalCameraData sourceData))
        {
            return;
        }

        UniversalAdditionalCameraData previewData = previewCamera.GetUniversalAdditionalCameraData();
        EditorUtility.CopySerialized(sourceData, previewData);
    }

    private static Vector3 CalculateAverageHierarchyPosition(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>();
        Vector3 totalPosition = Vector3.zero;

        foreach (Transform currentTransform in transforms)
        {
            totalPosition += currentTransform.position;
        }

        return totalPosition / transforms.Length;
    }

    private static void DrawPreviewIntoRect(Rect previewRect)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        Camera previewCamera = GetPreviewCamera(s_TargetCamera, previewRect.size);
        previewCamera.Render();

        EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));
        GUI.DrawTexture(previewRect, s_PreviewTexture, ScaleMode.ScaleToFit, false);

        Rect borderRect = new Rect(previewRect.x - 1f, previewRect.y - 1f, previewRect.width + 2f, previewRect.height + 2f);
        Handles.BeginGUI();
        Handles.color = new Color(0f, 0f, 0f, 0.4f);
        Handles.DrawAAPolyLine(
            2f,
            new Vector3(borderRect.xMin, borderRect.yMin),
            new Vector3(borderRect.xMax, borderRect.yMin),
            new Vector3(borderRect.xMax, borderRect.yMax),
            new Vector3(borderRect.xMin, borderRect.yMax),
            new Vector3(borderRect.xMin, borderRect.yMin));
        Handles.EndGUI();
    }

    private static void CleanupPreviewResources()
    {
        if (s_PreviewCamera != null)
        {
            Object.DestroyImmediate(s_PreviewCamera.gameObject);
            s_PreviewCamera = null;
        }

        if (s_PreviewTexture != null)
        {
            Object.DestroyImmediate(s_PreviewTexture);
            s_PreviewTexture = null;
        }
    }
}
