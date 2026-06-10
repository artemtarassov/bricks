using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BricksMatrix
{
    private List<List<List<Transform>>> matrix = new List<List<List<Transform>>>();
    public BricksMatrix(CityElement cityElement)
    {
        var sortedBricks = cityElement.GetBrickLayersContainer().sortedBricks;
        //tbd.
        var firstBrick = sortedBricks[0];
        Transform closestBrick = null;
        var closestDistance = float.MaxValue;
        for (var i = 1; i < sortedBricks.Count; i++)
        {
            var brick = sortedBricks[i];
            var distance = Vector3.Distance(brick.position, firstBrick.position);
            if (distance < closestDistance)
            {
                closestBrick = brick;
                closestDistance = distance;
            }
        }

        var lowestX = sortedBricks.Min(b => b.position.x);
        var lowestY = sortedBricks.Min(b => b.position.y);
        var lowestZ = sortedBricks.Min(b => b.position.z);


        foreach (var brick in sortedBricks)
        {
            var pos = brick.position;
            var x = Mathf.RoundToInt((pos.x - lowestX) / closestDistance);
            var y = Mathf.RoundToInt((pos.y - lowestY) / closestDistance);
            var z = Mathf.RoundToInt((pos.z - lowestZ) / closestDistance);
            SetBrick(x, y, z, brick);
        }

    }

    public void SetBrick(int x, int y, int z, Transform brick)
    {
        while (matrix.Count <= x)
        {
            matrix.Add(new List<List<Transform>>());
        }
        var layer = matrix[x];
        while (layer.Count <= y)
        {
            layer.Add(new List<Transform>());
        }
        var row = layer[y];
        while (row.Count <= z)
        {
            row.Add(null);
        }
        row[z] = brick;
    }

    public int[] GetBrickMatrixPos(Transform brick)
    {
        for (int x = 0; x < matrix.Count; x++)
        {
            var layer = matrix[x];
            for (int y = 0; y < layer.Count; y++)
            {
                var row = layer[y];
                for (int z = 0; z < row.Count; z++)
                {
                    if (row[z] == brick)
                    {
                        return new int[] { x, y, z };
                    }
                }
            }
        }
        return null;
    }

    public Transform GetBrick(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0)
        {
            return null;
        }

        if (x >= matrix.Count)
        {
            return null;
        }

        var layer = matrix[x];
        if (y >= layer.Count)
        {
            return null;
        }

        var row = layer[y];
        if (z >= row.Count)
        {
            return null;
        }
        return row[z];
    }

}