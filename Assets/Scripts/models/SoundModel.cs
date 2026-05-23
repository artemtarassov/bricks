using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SoundModel
{
    public static SoundModel Instance;

    public Action<string> OnPlaySound;

    public void Play(string name)
    {
        this.OnPlaySound?.Invoke(name);
    }

}