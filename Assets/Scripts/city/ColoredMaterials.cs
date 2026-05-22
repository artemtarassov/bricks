using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ColoredMaterials : MonoBehaviour
{
    public static ColoredMaterials Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (Instance == null)
        {
            Instance = this;
            SetMaterialColors();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    [SerializeField]
    public List<Material> materials;

    public Material GetMaterialByColorIndex(ColorIndex colorIndex)
    {
        return GetMaterialByName($"BrickMatColored_{(int)colorIndex}");
    }

    public Color GetColorByColorIndex(ColorIndex colorIndex)
    {
        var material = GetMaterialByColorIndex(colorIndex);
        return material.color;
    }


    public Material GetMaterialByName(string name)
    {
        var material = this.materials.Find(m => m.name == name);
        if (material == null)
        {
            throw new System.Exception($"Material with name {name} not found");
        }
        return material;
    }


    private void SetMaterialColors()
    {
        List<Color> colors = new()
        {
            /*
            new Color32(215,  25,  28, 255), // #D7191C Red
            new Color32(245, 124,   0, 255), // #F57C00 Orange
            new Color32(244, 208,  63, 255), // #F4D03F Yellow
            new Color32( 26, 150,  65, 255), // #1A9641 Green
            new Color32( 31,  93, 204, 255), // #1F5DCC Blue
            new Color32(123,  44, 191, 255), // #7B2CBF Purple
            new Color32(107, 114, 128, 255)  // #6B7280 Slate Gray*/

                new Color32(255,  45,  45, 255), // #FF2D2D Bright Red

            new Color32(255, 140,   0, 255), // #FF8C00 Bright Orange

            new Color32(255, 220,   0, 255), // #FFDC00 Bright Yellow

            new Color32(  0, 210,  90, 255), // #00D25A Bright Green

            new Color32(  0, 120, 255, 255), // #0078FF Bright Blue

            new Color32(155,  65, 255, 255), // #9B41FF Bright Purple

            new Color32( 90, 220, 220, 255)  // #5ADCDC Bright Teal
        };
        GetMaterialByColorIndex(ColorIndex.C0).color = colors[0];
        GetMaterialByColorIndex(ColorIndex.C1).color = colors[1];
        GetMaterialByColorIndex(ColorIndex.C2).color = colors[2];
        GetMaterialByColorIndex(ColorIndex.C3).color = colors[3];
        GetMaterialByColorIndex(ColorIndex.C4).color = colors[4];
        GetMaterialByColorIndex(ColorIndex.C5).color = colors[5];
        GetMaterialByColorIndex(ColorIndex.C6).color = colors[6];
    }

}
