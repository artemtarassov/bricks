using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class NightModeController : MonoBehaviour
{
    private Light directionalLight;
    void Start()
    {
        this.directionalLight = GetComponent<Light>();
        PlayerModel.Instance.OnPlayerDataChanged += OnPlayerDataChanged;
    }
    private void OnPlayerDataChanged()
    {
        var nightModeEnabled = PlayerModel.Instance.IsSettingEnabled(SettingsKey.NightMode);
        if (this.directionalLight.enabled == !nightModeEnabled)
        {
            return;
        }
        if (nightModeEnabled)
        {
            //change environment lighting to be darker and disable directional light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            //change environment lighting to be brighter and enable directional light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientLight = Color.white;
        }
        this.directionalLight.enabled = !nightModeEnabled;
    }
    void OnDestroy()
    {
        PlayerModel.Instance.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

}