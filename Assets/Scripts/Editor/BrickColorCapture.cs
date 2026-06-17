using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BrickColorCapture : EditorWindow
{
    [MenuItem("Tools/Brick Color Capture")]
    public static void ShowWindow()
    {
        GetWindow<BrickColorCapture>("Brick Color Capture");
    }

    [System.Serializable]
    class BrickColorData
    {
        public List<Color> colors;
        public string dataKey;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("capture one"))
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Enter play mode to capture brick colors");
                return;
            }
            var cityElement = Selection.gameObjects[0].GetComponent<CityElement>();
            if (cityElement == null)
            {
                Debug.LogError("Select a city element to capture brick colors");
                return;
            }
            var controller = PrepareController(cityElement);
            controller.StartCoroutine(CaptureBrickColor(cityElement, data =>
            {
                cityElement.brickColors = data.colors;
            }));
        }

        if (GUILayout.Button("capture all"))
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Enter play mode to capture brick colors");
                return;
            }
            var building = Selection.gameObjects[0].GetComponent<BuildingElement>();
            if (building == null)
            {
                Debug.LogError("Select a building element to capture brick colors");
                return;
            }
            var elements = building.GetElements();
            var controller = FindObjectOfType<SlotTextureCameraController>(true);
            controller.gameObject.SetActive(true);
            controller.StartCoroutine(CaptureAllBrickColors(elements));
        }

        if (GUILayout.Button("apply"))
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Exit play mode to apply brick colors");
                return;
            }
            var building = Selection.gameObjects[0].GetComponent<BuildingElement>();
            if (building == null)
            {
                Debug.LogError("Select a building element to apply brick colors");
                return;
            }
            var elements = building.GetElements();
            foreach (var element in elements)
            {
                var json = SessionState.GetString(element.dataKey, "");
                if (string.IsNullOrEmpty(json))
                {
                    continue;
                }
                element.brickColors = JsonUtility.FromJson<BrickColorData>(json).colors;
                SessionState.EraseString(element.dataKey);
                Debug.Log($"Applied colors for element {element.dataKey} colors count {element.brickColors.Count}");
            }
        }
    }

    private SlotTextureCameraController PrepareController(CityElement cityElement)
    {
        var brickLights = FindObjectOfType<BrickLightController2>(true);
        brickLights.ShowLightForCityElement(cityElement);

        var controller = FindObjectOfType<SlotTextureCameraController>(true);
        controller.gameObject.SetActive(true);
        return controller;
    }

    private IEnumerator CaptureBrickColor(CityElement cityElement, Action<BrickColorData> onComplete)
    {
        var controller = PrepareController(cityElement);
        List<Color> colors = null;
        yield return controller.StartCoroutine(controller.GetBrickColors(cityElement, result => colors = result));

        var d = new BrickColorData();
        d.colors = colors;
        d.dataKey = cityElement.dataKey;
        onComplete?.Invoke(d);
    }

    private IEnumerator CaptureAllBrickColors(IEnumerable<CityElement> elements)
    {
        foreach (var element in elements)
        {
            BrickColorData data = null;
            yield return CaptureBrickColor(element, result => data = result);
            var json = JsonUtility.ToJson(data);
            SessionState.SetString(element.dataKey, json);
        }
    }


}
