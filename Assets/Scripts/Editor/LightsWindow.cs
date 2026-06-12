using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LightData
{
    public string scenePath;
    public string globalObjectId;
    public string dataKey;
    public string objectPath;
    public Vector3 rotation;
}

[System.Serializable]
public class LightDataCache
{
    public List<LightData> items = new List<LightData>();
}

[InitializeOnLoad]
public class LightsWindow : EditorWindow
{
    private const string LightDataSessionKey = "LightsWindow.CachedLightData";

    static LightsWindow()
    {

    }

    [MenuItem("Tools/Lights Window")]
    public static void ShowWindow()
    {
        GetWindow<LightsWindow>("LightsWindow");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Copy light positions"))
        {
            CopyLightPositions();
        }

        if (GUILayout.Button("Restore light rotations"))
        {
            RestoreCachedLightRotations();
            //mark scene dirty to ensure changes are saved. 
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(currentScene);
            }
        }
        if (GUILayout.Button("rotation light to selected city element"))
        {
            var selectedCityElement = GetSelectedCityElement();
            if (selectedCityElement != null)
            {
                var pos = selectedCityElement.GetAveragePosition();
                var light = GetBrickLight();
                if (light != null)
                {
                    light.transform.position = selectedCityElement.camPos;
                    light.transform.LookAt(pos);
                    CopyLightPositions();
                }
                else
                {
                    Debug.LogWarning("No light found. Please add a directional light with 'BrickLight' in its name to the scene.");
                }
            }
            else
            {
                Debug.LogWarning("No city element selected. Please select a city element to rotate light to.");
            }
        }


    }

    private CityElement GetSelectedCityElement()
    {
        if (Selection.gameObjects.Length > 0)
        {
            var selectedCityElement = Selection.gameObjects[0].GetComponent<CityElement>();
            if (selectedCityElement == null)
            {
                var allCityElements1 = Object.FindObjectsOfType<CityElement>(true);
                var lastActive1 = allCityElements1.LastOrDefault(ce => ce.gameObject.activeInHierarchy);
                if (lastActive1 != null)
                {
                    return lastActive1;
                }
            }
            else
            {
                return selectedCityElement;
            }
        }


        var allCityElements = Object.FindObjectsOfType<CityElement>(true);
        var lastActive = allCityElements.LastOrDefault(ce => ce.gameObject.activeInHierarchy);
        if (lastActive != null)
        {
            return lastActive;
        }
        return null;
    }

    private void CopyLightPositions()
    {
        var selectedCityElement = GetSelectedCityElement();
        if (selectedCityElement != null)
        {
            var light = GetBrickLight();
            if (light != null)
            {
                var rotation = light.transform.eulerAngles;
                if (EditorApplication.isPlaying)
                {
                    selectedCityElement.lightRot = rotation;
                    CacheLightRotation(selectedCityElement, rotation);
                    Debug.Log($"Cached light rotation for {selectedCityElement.name}. It will be restored after exiting Play mode.");
                    return;
                }

                ApplyLightRotation(selectedCityElement, rotation, "Copy Light Rotation");
            }
            else
            {
                Debug.LogWarning("No light found. Please add a directional light with 'BrickLight' in its name to the scene.");
            }
        }
        else
        {
            Debug.LogWarning("No city element selected. Please select a city element to copy light position to.");
        }
    }


    private Light GetBrickLight()
    {
        var list = Object.FindObjectsOfType<Light>(true).ToList().FindAll(l => l.type == LightType.Directional && l.gameObject.name.Contains("BricksLight"));
        return list.FirstOrDefault();
    }


    private static void CacheLightRotation(CityElement cityElement, Vector3 rotation)
    {
        var cache = LoadCache();
        var scenePath = cityElement.gameObject.scene.path;
        var globalObjectId = GetGlobalObjectId(cityElement);
        var dataKey = cityElement.dataKey;
        var objectPath = GetHierarchyPath(cityElement.transform);
        var existingItem = cache.items.FirstOrDefault(item =>
            (!string.IsNullOrEmpty(globalObjectId) && item.globalObjectId == globalObjectId)
            || (item.scenePath == scenePath && item.dataKey == dataKey && !string.IsNullOrEmpty(dataKey)));

        if (existingItem == null)
        {
            existingItem = new LightData
            {
                scenePath = scenePath,
                globalObjectId = globalObjectId,
                dataKey = dataKey,
                objectPath = objectPath
            };
            cache.items.Add(existingItem);
        }

        existingItem.scenePath = scenePath;
        existingItem.globalObjectId = globalObjectId;
        existingItem.dataKey = dataKey;
        existingItem.objectPath = objectPath;
        existingItem.rotation = rotation;
        SaveCache(cache);
    }

    private static void RestoreCachedLightRotations()
    {

        var cache = LoadCache();
        if (cache.items.Count == 0)
        {
            Debug.Log("No cached light rotations found.");
            return;
        }

        Debug.Log("Restoring cached light rotations... elements " + cache.items.Count);

        var restoredCount = 0;
        foreach (var item in cache.items)
        {
            if (!TryFindCityElement(item, out var cityElement))
            {
                Debug.LogWarning($"Could not restore cached light rotation for '{item.objectPath}' in scene '{item.scenePath}'.");
                continue;
            }

            ApplyLightRotation(cityElement, item.rotation, "Restore Cached Light Rotation");
            restoredCount++;
        }

        ClearCache();

        if (restoredCount > 0)
        {
            Debug.Log($"Restored cached light rotation for {restoredCount} CityElement object(s).");
        }
    }

    private static void ApplyLightRotation(CityElement cityElement, Vector3 rotation, string undoLabel)
    {
        Undo.RecordObject(cityElement, undoLabel);
        cityElement.lightRot = rotation;
        Debug.Log($"Applied light rotation to {cityElement.name}: {rotation}");
    }

    private static LightDataCache LoadCache()
    {
        var json = SessionState.GetString(LightDataSessionKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new LightDataCache();
        }

        return JsonUtility.FromJson<LightDataCache>(json) ?? new LightDataCache();
    }

    private static void SaveCache(LightDataCache cache)
    {
        Debug.Log($"Saving light rotation cache with {cache.items.Count} items.");
        SessionState.SetString(LightDataSessionKey, JsonUtility.ToJson(cache));
    }

    private static void ClearCache()
    {
        SessionState.SetString(LightDataSessionKey, string.Empty);
    }

    private static bool TryFindCityElement(LightData item, out CityElement cityElement)
    {
        cityElement = null;

        var scene = SceneManager.GetSceneByPath(item.scenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return false;
        }

        cityElement = FindCityElementByGlobalObjectId(item.globalObjectId);
        if (cityElement != null)
        {
            return true;
        }

        cityElement = FindCityElementByDataKey(scene, item.dataKey);
        if (cityElement != null)
        {
            return true;
        }

        var targetGameObject = FindGameObjectByPath(scene, item.objectPath);
        if (targetGameObject == null)
        {
            return false;
        }

        cityElement = targetGameObject.GetComponent<CityElement>();
        return cityElement != null;
    }

    private static CityElement FindCityElementByGlobalObjectId(string globalObjectIdString)
    {
        if (string.IsNullOrEmpty(globalObjectIdString))
        {
            return null;
        }

        if (!GlobalObjectId.TryParse(globalObjectIdString, out var globalObjectId))
        {
            return null;
        }

        return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId) as CityElement;
    }

    private static CityElement FindCityElementByDataKey(Scene scene, string dataKey)
    {
        if (string.IsNullOrEmpty(dataKey))
        {
            return null;
        }

        foreach (var rootObject in scene.GetRootGameObjects())
        {
            var cityElements = rootObject.GetComponentsInChildren<CityElement>(true);
            foreach (var cityElement in cityElements)
            {
                if (cityElement.dataKey == dataKey)
                {
                    return cityElement;
                }
            }
        }

        return null;
    }

    private static GameObject FindGameObjectByPath(Scene scene, string objectPath)
    {
        if (string.IsNullOrEmpty(objectPath))
        {
            return null;
        }

        var parts = objectPath.Split('/');
        if (parts.Length == 0)
        {
            return null;
        }

        Transform current = null;
        foreach (var rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name == parts[0])
            {
                current = rootObject.transform;
                break;
            }
        }

        if (current == null)
        {
            return null;
        }

        for (var i = 1; i < parts.Length; i++)
        {
            current = current.Find(parts[i]);
            if (current == null)
            {
                return null;
            }
        }

        return current.gameObject;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        var names = new List<string>();
        var current = transform;
        while (current != null)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return string.Join("/", names);
    }

    private static string GetGlobalObjectId(Object target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(target);
        if (globalObjectId.identifierType == 0)
        {
            return string.Empty;
        }

        return globalObjectId.ToString();
    }
}
