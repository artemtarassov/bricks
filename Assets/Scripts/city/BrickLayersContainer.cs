using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrickLayersContainer
{
    public readonly List<Transform> sortedBricks = new List<Transform>();

    public BrickLayersContainer(Transform generatedBricksContainer)
    {
        var brickLayers = generatedBricksContainer.GetComponentsInChildren<BricksLayer>(true);
        foreach (var brickLayer in brickLayers)
        {
            sortedBricks.AddRange(brickLayer.GetSortedBricks());
        }
        var looseChildren = new List<Transform>();
        for (var i = 0; i < generatedBricksContainer.childCount; i++)
        {
            var child = generatedBricksContainer.GetChild(i);
            if (child.CompareTag(BricksLayer.BrickTag))
            {
                looseChildren.Add(child);
            }
        }
        if (looseChildren.Count > 0)
        {
            var sortedLooseBricks = looseChildren.OrderBy(b => RoundPos(b.localPosition.y)).ThenBy(b => RoundPos(b.localPosition.x)).ThenBy(b => RoundPos(b.localPosition.z));
            sortedBricks.AddRange(sortedLooseBricks);
        }
        foreach (var b in sortedBricks)
        {
           // b.transform.localScale = b.transform.localScale * 0.97f; //force scale update to fix any issues with lossy scale not being updated on child bricks
        }
    }

    private static int RoundPos(float p)
    {
        return Mathf.RoundToInt(p * 10);
    }

}
