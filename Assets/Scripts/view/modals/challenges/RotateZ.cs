using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class RotateZ : MonoBehaviour
{
    void Update()
    {
        this.transform.Rotate(0, 0, 0.3f);
    }

}