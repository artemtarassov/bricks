using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BrickLayersContainer
{
    public readonly List<BricksLayer> layers = new List<BricksLayer>();
    public readonly List<Transform> sortedBricks = new List<Transform>();
    public BrickLayersContainer(List<Transform> bricks)
    {
        //vertical.
        var sortedByY = bricks.OrderBy(b => b.position.y).ToList();
        var currentY = float.MinValue;
        var currentLayer = new BricksLayer() { layerIndex = 0, bricks = new List<Transform>(), yPos = 0 };
        foreach (var brick in sortedByY)
        {
            if (currentY == float.MinValue)
            {
                currentY = brick.position.y;
                currentLayer.yPos = currentY;
            }
            if (Mathf.Abs(brick.position.y - currentY) > 0.1f)
            {
                this.layers.Add(currentLayer);
                currentLayer = new BricksLayer() { layerIndex = currentLayer.layerIndex + 1, bricks = new List<Transform>(), yPos = brick.position.y };
                currentY = brick.position.y;
            }
            currentLayer.bricks.Add(brick);
        }
        if (currentLayer.bricks.Count > 0)
        {
            //sort bricks by x and z position
            currentLayer.bricks = currentLayer.bricks.OrderBy(b => b.position.x).ThenBy(b => b.position.z).ToList();
            this.layers.Add(currentLayer);
        }

        this.layers.Sort((a, b) => a.yPos.CompareTo(b.yPos));

        foreach (var layer in this.layers)
        {
            foreach (var brick in layer.bricks)
            {
                this.sortedBricks.Add(brick);
            }
        }
    }

}