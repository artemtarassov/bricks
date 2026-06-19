using UnityEngine;
using UnityEngine.UI;

public class UIBrick : MonoBehaviour
{
    [SerializeField] private Image bg;
    [SerializeField] private Image rune;
    [SerializeField] private Image gloss;

    [SerializeField] private Sprite[] sprites;

    public void SetColor(Color clr, ColorIndex colorIndex)
    {
        var brighterBy = 1.0f;
        this.bg.color = new Color(clr.r * brighterBy, clr.g * brighterBy, clr.b * brighterBy);
        this.rune.sprite = sprites[(int)colorIndex];
    }

    public void ShowGloss(bool show)
    {
        this.gloss.gameObject.SetActive(show);
    }


}