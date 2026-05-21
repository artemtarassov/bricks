using System.Collections.Generic;
using UnityEngine;

public class ColoredMaterials : MonoBehaviour
{
    public static ColoredMaterials Instance { get; private set; }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (Instance == null)
        {
            Instance = this;
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

}
