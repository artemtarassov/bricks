using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class SpriteHelper
{
    public static Sprite LoadSprite(string sprite)
    {
        var loadedSprite = Resources.Load<Sprite>(sprite);
        if (loadedSprite == null)
        {
            //Debug.LogWarning("SpriteHelper image not present " + sprite);
        }
        return loadedSprite;
    }



}