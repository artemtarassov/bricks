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
        return;
        var colors = new List<Color>()
        {
            new Color(0.000f, 0.482f, 1.000f), // #007BFF Bright Blue
            new Color(1.000f, 0.302f, 0.302f), // #FF4D4D Hot Coral
            new Color(1.000f, 0.839f, 0.039f), // #FFD60A Vivid Yellow
            new Color(0.000f, 0.835f, 1.000f), // #00D5FF Aqua Cyan
            new Color(0.608f, 0.365f, 0.898f), // #9B5DE5 Electric Purple
            new Color(0.494f, 0.851f, 0.341f), // #7ED957 Lime Green
            new Color(1.000f, 0.176f, 0.667f), // #FF2DAA Hot Pink
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
