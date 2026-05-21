using UnityEngine;
using UnityEngine.UI;

public class ColoredButton : MonoBehaviour
{
    public void SetColor(ColorIndex colorIndex)
    {
        var color = ColoredMaterials.Instance.GetColorByColorIndex(colorIndex);
        var img = this.GetComponent<Image>();
        img.color = color;
    }

}
