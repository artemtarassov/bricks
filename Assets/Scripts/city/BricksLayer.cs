using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BricksLayer : MonoBehaviour
{
    public static readonly string BrickTag = "Brick";


    [SerializeField]
    private bool xSortAscending = true;

    [SerializeField]
    private bool zSortAscending = true;

    public List<Transform> GetSortedBricks()
    {
        return SortBricks(CollectBrickChildren());
    }

    private List<Transform> CollectBrickChildren()
    {
        var collectedBricks = new List<Transform>();
        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.CompareTag(BrickTag))
            {
                collectedBricks.Add(child);
            }
        }

        return collectedBricks;
    }

    private List<Transform> SortBricks(List<Transform> bricks)
    {
  

        if (xSortAscending && zSortAscending)
        {
            return bricks.OrderBy(b => RoundPos(b.localPosition.y)).ThenBy(b => RoundPos(b.localPosition.x)).ThenBy(b => RoundPos(b.localPosition.z)).ToList();
        }

        if (xSortAscending && !zSortAscending)
        {
            return bricks.OrderBy(b => RoundPos(b.localPosition.y)).ThenBy(b => RoundPos(b.localPosition.x)).ThenByDescending(b => RoundPos(b.localPosition.z)).ToList();
        }

        if (!xSortAscending && zSortAscending)
        {
            return bricks.OrderBy(b => RoundPos(b.localPosition.y)).ThenByDescending(b => RoundPos(b.localPosition.x)).ThenBy(b => RoundPos(b.localPosition.z)).ToList();
        }

        return bricks.OrderBy(b => RoundPos(b.localPosition.y)).ThenByDescending(b => RoundPos(b.localPosition.x)).ThenByDescending(b => RoundPos(b.localPosition.z)).ToList();
    }

    private static int RoundPos(float p)
    {
        return Mathf.RoundToInt(p * 10);
    }
}
