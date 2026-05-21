using UnityEngine;

public class EmitterBrick : MonoBehaviour
{
    public BrickData brickData;

    public void Setup(BrickData eb = null)
    {
        this.brickData = eb;
        this.GetComponentInChildren<UIBrick>().SetData(eb);
    }
}