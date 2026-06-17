using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotTextureCameraController : MonoBehaviour
{
    public GameObject bricksContainer;
    private static readonly WaitForEndOfFrame WaitForEndOfFrameInstruction = new WaitForEndOfFrame();
    void Start()
    {

    }

    public IEnumerator GetBrickColors(CityElement currentElement, Action<List<Color>> onComplete)
    {
        var cam = this.GetComponentInChildren<Camera>();
        var anyBrick = currentElement.GetBrickLayersContainer().sortedBricks[0];

        this.transform.position = currentElement.camPos;
        this.transform.rotation = Quaternion.Euler(currentElement.camRot);
        this.bricksContainer.transform.rotation = anyBrick.rotation;
        this.bricksContainer.transform.position = anyBrick.position;

        var allColors = Enum.GetValues(typeof(ColorIndex));
        var brickColors = new List<Color>();
        foreach (ColorIndex color in allColors)
        {
            if (color == ColorIndex.Undefined)
            {
                continue;
            }
            var meshRenderers = this.bricksContainer.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in meshRenderers)
                mr.material = ColoredMaterials.Instance.GetMaterialByColorIndex(color);

            cam.Render();
            yield return WaitForEndOfFrameInstruction;
            var camOutputTexture = cam.targetTexture;
            var clr = GetAvgColor(camOutputTexture);
            brickColors.Add(clr);
        }

        onComplete?.Invoke(brickColors);
    }

    private static Color GetAvgColor(RenderTexture txt)
    {
        RenderTexture.active = txt;
        Texture2D tex = new Texture2D(txt.width, txt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, txt.width, txt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        var counter = 0;

        var blackPixel = new Color(0.2f, 0.2f, 0.2f); // multiply by 2 to compensate for possible darkening of colors due to lighting, add 0.1 to each channel to compensate for possible compression artifacts that can make pure black pixels slightly non-black
        //Debug.Log($"SlotTextureCameraController: GetAvgColor: topLeftPixel={blackPixel}");

        var r = 0.0f; var g = 0.0f; var b = 0.0f;
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                var pixel = tex.GetPixel(x, y);
                var isBlack = (pixel.r <= blackPixel.r && pixel.g <= blackPixel.g && pixel.b <= blackPixel.b);
                if (pixel.a < 0.1f || isBlack)
                {
                    continue;
                }
                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
                counter++;
            }
        }
        var result = new Color(r / counter, g / counter, b / counter, 1);
        return AdjustContrast(result, 1.1f);
    }


    public static Color AdjustContrast(Color color, float contrast)

    {

        // contrast = 1 means unchanged

        // contrast > 1 increases contrast

        // contrast < 1 decreases contrast

        float r = (color.r - 0.5f) * contrast + 0.5f;

        float g = (color.g - 0.5f) * contrast + 0.5f;

        float b = (color.b - 0.5f) * contrast + 0.5f;

        return new Color(

            Mathf.Clamp01(r),

            Mathf.Clamp01(g),

            Mathf.Clamp01(b),

            color.a

        );

    }

    void OnDestroy()
    {
        //ViewModel.Instance.OnUpdateSlotTextures -= OnUpdateSlotTextures;
    }

}
