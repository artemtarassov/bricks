using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public static class GameObjectHelper
{
    public static void ActivateDelayed(float delay, MonoBehaviour d)
    {
        DOVirtual.DelayedCall(delay, () => { d.gameObject.SetActive(true); }, false);
    }

    public static void DeactivateDelayed(float delay, MonoBehaviour d)
    {
        DOVirtual.DelayedCall(delay, () => { d.gameObject.SetActive(false); }, false);
    }

    public static void ActivateDelayed(float delay, GameObject d)
    {
        DOVirtual.DelayedCall(delay, () => { d.SetActive(true); }, false);
    }

    public static void DeactivateDelayed(float delay, GameObject d)
    {
        DOVirtual.DelayedCall(delay, () => { d.SetActive(false); }, false);
    }

}