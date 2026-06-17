using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TakeScreenshot : EditorWindow
{
    private const BindingFlags InstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags StaticBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private const string WindowTitle = "Screenshot Taker";
    private const string ScreenshotButtonLabel = "take screenshot 6.5";
    private const string ScreenshotSizeName = "6.5";
    private const int ScreenshotWidth = 1242;
    private const int ScreenshotHeight = 2688;
    private const string ScreenshotFolderName = "Screenshots";

    [MenuItem("Tools/Screenshot Taker")]
    public static void ShowWindow()
    {
        GetWindow<TakeScreenshot>(WindowTitle);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("6.5 portrait screenshot", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"{ScreenshotWidth} x {ScreenshotHeight}");
        EditorGUILayout.Space();

        if (GUILayout.Button(ScreenshotButtonLabel, GUILayout.Height(36f)))
        {
            StartScreenshotCapture();
        }
    }

    private static void StartScreenshotCapture()
    {
        try
        {
            EnsureGameViewSize();

            string outputPath = GetOutputPath();
            Debug.Log($"Preparing 6.5 screenshot at {outputPath}");

            EditorApplication.delayCall += () =>
            {
                FocusAndRefreshGameView();
                EditorApplication.delayCall += () => CaptureScreenshot(outputPath);
            };
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to prepare screenshot capture: {exception}");
        }
    }

    private static void CaptureScreenshot(string outputPath)
    {
        try
        {
            FocusAndRefreshGameView();
            ScreenCapture.CaptureScreenshot(outputPath);
            Debug.Log($"Screenshot saved to {outputPath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to capture screenshot: {exception}");
        }
    }

    private static string GetOutputPath()
    {
        DirectoryInfo projectDirectory = Directory.GetParent(Application.dataPath)
            ?? throw new InvalidOperationException("Could not resolve the Unity project root.");
        string projectRoot = projectDirectory.FullName;
        string screenshotDirectory = Path.Combine(projectRoot, ScreenshotFolderName);
        Directory.CreateDirectory(screenshotDirectory);

        string fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        return Path.Combine(screenshotDirectory, fileName);
    }

    private static void EnsureGameViewSize()
    {
        object group = GetCurrentGameViewSizeGroup();
        int sizeIndex = FindGameViewSizeIndex(group, ScreenshotWidth, ScreenshotHeight, ScreenshotSizeName);

        if (sizeIndex < 0)
        {
            AddCustomGameViewSize(group, ScreenshotWidth, ScreenshotHeight, ScreenshotSizeName);
            sizeIndex = FindGameViewSizeIndex(group, ScreenshotWidth, ScreenshotHeight, ScreenshotSizeName);
        }

        if (sizeIndex < 0)
        {
            throw new InvalidOperationException("Could not find or create the 6.5 Game View size.");
        }

        SelectGameViewSize(sizeIndex);
    }

    private static object GetCurrentGameViewSizeGroup()
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type gameViewSizesType = editorAssembly.GetType("UnityEditor.GameViewSizes")
            ?? throw new InvalidOperationException("UnityEditor.GameViewSizes type not found.");

        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
        PropertyInfo instanceProperty = singletonType.GetProperty(
            "instance",
            StaticBindings)
            ?? throw new InvalidOperationException("GameViewSizes singleton instance not found.");

        object instance = instanceProperty.GetValue(null, null)
            ?? throw new InvalidOperationException("GameViewSizes singleton instance is null.");

        PropertyInfo currentGroupProperty = gameViewSizesType.GetProperty(
            "currentGroup",
            InstanceBindings);

        if (currentGroupProperty != null)
        {
            return currentGroupProperty.GetValue(instance, null)
                ?? throw new InvalidOperationException("Current Game View group is null.");
        }

        MethodInfo getCurrentGroupTypeMethod = gameViewSizesType.GetMethod(
            "GetCurrentGroupType",
            InstanceBindings)
            ?? throw new InvalidOperationException("GetCurrentGroupType method not found.");

        object currentGroupType = getCurrentGroupTypeMethod.Invoke(instance, null)
            ?? throw new InvalidOperationException("Current Game View group type is null.");

        MethodInfo getGroupMethod = gameViewSizesType.GetMethod(
            "GetGroup",
            InstanceBindings)
            ?? throw new InvalidOperationException("GetGroup method not found.");

        return getGroupMethod.Invoke(instance, new[] { currentGroupType })
            ?? throw new InvalidOperationException("Current Game View group is null.");
    }

    private static int FindGameViewSizeIndex(object group, int width, int height, string sizeName)
    {
        Type groupType = group.GetType();
        MethodInfo getBuiltinCountMethod = groupType.GetMethod("GetBuiltinCount", InstanceBindings)
            ?? throw new InvalidOperationException("GetBuiltinCount method not found.");
        MethodInfo getCustomCountMethod = groupType.GetMethod("GetCustomCount", InstanceBindings)
            ?? throw new InvalidOperationException("GetCustomCount method not found.");
        MethodInfo getGameViewSizeMethod = groupType.GetMethod("GetGameViewSize", InstanceBindings)
            ?? throw new InvalidOperationException("GetGameViewSize method not found.");

        int totalCount = (int)getBuiltinCountMethod.Invoke(group, null) + (int)getCustomCountMethod.Invoke(group, null);
        for (int index = 0; index < totalCount; index++)
        {
            object size = getGameViewSizeMethod.Invoke(group, new object[] { index });
            if (size == null)
            {
                continue;
            }

            int currentWidth = ReadIntMember(size, "width");
            int currentHeight = ReadIntMember(size, "height");
            string currentName = ReadStringMember(size, "baseText");
            if (string.IsNullOrEmpty(currentName))
            {
                currentName = ReadStringMember(size, "displayText");
            }

            if (currentWidth == width && currentHeight == height)
            {
                return index;
            }

            if (!string.IsNullOrEmpty(currentName) && currentName == sizeName)
            {
                return index;
            }
        }

        return -1;
    }

    private static void AddCustomGameViewSize(object group, int width, int height, string sizeName)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize")
            ?? throw new InvalidOperationException("UnityEditor.GameViewSize type not found.");
        Type gameViewSizeTypeEnum = editorAssembly.GetType("UnityEditor.GameViewSizeType")
            ?? throw new InvalidOperationException("UnityEditor.GameViewSizeType type not found.");

        object fixedResolution = Enum.Parse(gameViewSizeTypeEnum, "FixedResolution");
        ConstructorInfo constructor = gameViewSizeType.GetConstructor(
            InstanceBindings,
            null,
            new[] { gameViewSizeTypeEnum, typeof(int), typeof(int), typeof(string) },
            null)
            ?? throw new InvalidOperationException("GameViewSize constructor not found.");

        object newSize = constructor.Invoke(new object[] { fixedResolution, width, height, sizeName });
        MethodInfo addCustomSizeMethod = group.GetType().GetMethod("AddCustomSize", InstanceBindings)
            ?? throw new InvalidOperationException("AddCustomSize method not found.");

        addCustomSizeMethod.Invoke(group, new[] { newSize });
    }

    private static void SelectGameViewSize(int sizeIndex)
    {
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView")
            ?? throw new InvalidOperationException("UnityEditor.GameView type not found.");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);

        MethodInfo sizeSelectionCallback = gameViewType.GetMethod(
            "SizeSelectionCallback",
            InstanceBindings);

        if (sizeSelectionCallback != null)
        {
            object[] arguments = sizeSelectionCallback.GetParameters().Length == 1
                ? new object[] { sizeIndex }
                : new object[] { sizeIndex, null };
            sizeSelectionCallback.Invoke(gameView, arguments);
        }
        else
        {
            PropertyInfo selectedSizeIndexProperty = gameViewType.GetProperty(
                "selectedSizeIndex",
                InstanceBindings)
                ?? throw new InvalidOperationException("selectedSizeIndex property not found.");
            selectedSizeIndexProperty.SetValue(gameView, sizeIndex, null);
        }

        gameView.Focus();
        gameView.Repaint();
    }

    private static void FocusAndRefreshGameView()
    {
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType == null)
        {
            return;
        }

        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        gameView.Focus();
        gameView.Repaint();
    }

    private static int ReadIntMember(object instance, string memberName)
    {
        Type type = instance.GetType();

        PropertyInfo property = type.GetProperty(memberName, InstanceBindings);
        if (property != null)
        {
            object value = property.GetValue(instance, null);
            if (value != null)
            {
                return Convert.ToInt32(value);
            }
        }

        FieldInfo field = type.GetField(memberName, InstanceBindings);
        if (field != null)
        {
            object value = field.GetValue(instance);
            if (value != null)
            {
                return Convert.ToInt32(value);
            }
        }

        return 0;
    }

    private static string ReadStringMember(object instance, string memberName)
    {
        Type type = instance.GetType();

        PropertyInfo property = type.GetProperty(memberName, InstanceBindings);
        if (property != null)
        {
            return property.GetValue(instance, null) as string ?? string.Empty;
        }

        FieldInfo field = type.GetField(memberName, InstanceBindings);
        if (field != null)
        {
            return field.GetValue(instance) as string ?? string.Empty;
        }

        return string.Empty;
    }
}
