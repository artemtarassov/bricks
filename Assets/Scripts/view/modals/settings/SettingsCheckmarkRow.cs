using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsCheckmarkRow : MonoBehaviour
{
    [SerializeField] private SettingsKey key = SettingsKey.Undefined;
    [SerializeField] private GameObject checkmark;
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }
    
    void OnClick()
    {
        new SwitchSettingsCmd().Run(key);
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var isEnabled = PlayerModel.Instance.IsSettingEnabled(key);
        checkmark.SetActive(isEnabled);
    }
}