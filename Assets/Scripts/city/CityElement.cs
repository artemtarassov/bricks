using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

class BrickInfo
{
    public BrickState state;
    public ColorIndex color;
}

public struct ColoredBrickInfo
{
    public BrickData brickData;
    public Transform brickTransform;
    public int index;
}

public class CityElement : MonoBehaviour
{
    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;

    [SerializeField] public Transform __GeneratedBricks = null;
    [SerializeField] public Transform __EnclosingGameObject = null;

    public string dataKey => this.transform.parent.name + "_" + this.name;

    [HideInInspector]
    public CityElementDataContainer dataContainer;
    private Dictionary<Transform, BrickInfo> brickInfo = new Dictionary<Transform, BrickInfo>();
    private BrickLayersContainer brickLayersContainer;

    private BrickExplosion explosion;

    //private readonly string genereatedBrickGameObjectName = "__GeneratedBricks";

    void Awake()
    {
        Assert.IsNotNull(__GeneratedBricks, "Generated bricks container not assigned in inspector for " + this.gameObject.name);
        this.EnableBricks(false);
        this.EnableVisuals(true);
    }

    public BrickLayersContainer GetBrickLayersContainer()
    {
        if (brickLayersContainer != null)
        {
            return brickLayersContainer;
        }

        this.brickLayersContainer = new BrickLayersContainer(__GeneratedBricks.transform);
        foreach (var brick in this.brickLayersContainer.sortedBricks)
        {
            this.brickInfo[brick] = new BrickInfo()
            {
                state = BrickState.Undefined,
                color = ColorIndex.Undefined
            };
        }
        return brickLayersContainer;
    }

    public BricksMatrix matrix { get; private set; }

    public void Setup(CityElementDataContainer dataContainer)
    {
        //Debug.Log($"CityElement Setup: setting up city element {this.name} with data container {dataContainer.dataKey}");
        Assert.IsNotNull(dataContainer, "Data container should not be null when setting up city element " + this.gameObject.name);
        this.dataContainer = dataContainer;

        this.EnableBricks(true);
        this.EnableVisuals(false);
        this.SetBrickColors();
        this.ShowCurrentState();

        this.matrix = new BricksMatrix(this);
    }

    public List<Transform> FindAllBricks()
    {
        var list = __GeneratedBricks.GetComponentsInChildren<Transform>(true).ToList().FindAll(b => b.CompareTag("Brick"));
        Assert.IsTrue(list.Count > 0, "FindAllBricks found no bricks in city element " + this.gameObject.name);
        return list;
    }



    public ColorIndex GetColorOfBrick(Transform brick)
    {
        Assert.IsTrue(this.brickInfo.ContainsKey(brick), "Brick transform not found in city element");
        return this.brickInfo[brick].color;
    }

    public HashSet<ColorIndex> GetBrickColors()
    {
        return this.dataContainer.GetBrickColors();
    }



    private Material GetMaterialByName(string name)
    {
        return ColoredMaterials.Instance.GetMaterialByName(name);
    }

    private Material GetMaterialByColorIndex(ColorIndex colorIndex)
    {
        return ColoredMaterials.Instance.GetMaterialByColorIndex(colorIndex);
    }

    private List<Transform> SortedBricks => this.GetBrickLayersContainer().sortedBricks;

    private void SetBrickColors()
    {
        var j = 0;
        foreach (var bd in dataContainer.brickDataList)
        {
            for (var i = 0; i < bd.max && j < SortedBricks.Count; i++)
            {
                var t = SortedBricks[j];
                this.brickInfo[t].color = bd.color;
                j++;
            }
        }
    }

    public void ShowCurrentState()
    {
        Assert.IsNotNull(this.dataContainer, "Data container is null. Cannot show brick states.");
        Assert.IsTrue(this.dataContainer.brickDataList.Count > 0, "Brick data list is empty. Cannot show brick states.");
        Assert.IsTrue(this.SortedBricks.Count > 0, "No bricks found in city element. Cannot show brick states.");
        var j = 0;
        foreach (var bd in dataContainer.brickDataList)
        {
            for (var i = 0; i < bd.max && j < SortedBricks.Count; i++)
            {
                Assert.IsNotNull(SortedBricks[j], "Sorted brick is null. Cannot show brick state.");
                this.ShowBrickState(SortedBricks[j], bd.GetBrickState(i));
                j++;
            }
        }
        if (this.dataContainer.ElementCompleted() && !this.HasVisuals())
        {
            //explode
        }
    }

    public void Explode()
    {
        if (this.explosion == null)
        {
            this.explosion = this.gameObject.AddComponent<BrickExplosion>();
        }
        this.explosion.Play();
        this.EnableVisuals(true);
    }


    public ColoredBrickInfo GetFurthestColoredBrick(ColorIndex colorIndex)
    {
        var camPos = Camera.main.transform.position;
        var allColoredBricks = GetColoredBricks(colorIndex);
        var closestBricks = allColoredBricks.OrderBy(b => Vector3.Distance(b.brickTransform.position, camPos));
        if (closestBricks.Count() == 0)
        {
            return default(ColoredBrickInfo);
        }
        var furthestBrick = closestBricks.Last();
        return furthestBrick;
    }



    public List<ColoredBrickInfo> GetColoredBricks(ColorIndex colorIndex = ColorIndex.Undefined)
    {
        var coloredBricks = new List<ColoredBrickInfo>();
        foreach (var brickData in dataContainer.brickDataList)
        {
            if (brickData.coloredAmount > 0 && (colorIndex == ColorIndex.Undefined || brickData.color == colorIndex))
            {
                var indexList = brickData.GetBrickIndexList(BrickState.Colored);
                foreach (var index in indexList)
                {
                    var brickTransform = GetBrick(brickData, index);
                    coloredBricks.Add(new ColoredBrickInfo { brickData = brickData, brickTransform = brickTransform, index = index });
                }
            }
        }
        return coloredBricks;
    }

    public Transform GetBrick(BrickData find, int index)
    {
        Assert.IsTrue(find.max > index, "Index should be less than max amount of the brick data");
        var j = 0;
        foreach (var bd in dataContainer.brickDataList)
        {
            if (bd == find)
            {
                return SortedBricks[j + index];
            }
            else
            {
                j += bd.max;
            }
        }
        return null;
    }

    private void ShowBrickState(Transform t, BrickState state)
    {
        if (this.brickInfo.ContainsKey(t) && this.brickInfo[t].state == state)
        {
            return;
        }
        //Debug.Log("ShowBrickState " + state);
        this.brickInfo[t].state = state;
        var mr = t.GetComponent<MeshRenderer>();

        switch (state)
        {
            case BrickState.Undefined:
            case BrickState.Transparent:
                mr.enabled = false;
                break;
            case BrickState.SemiTransparent:
                mr.enabled = true;
                mr.material = GetMaterialByName("BrickMatTransparent");
                break;
            case BrickState.Emitting:
                break;
            case BrickState.Full:
                mr.enabled = true;
                mr.material = GetMaterialByName("BrickMatFull");
                break;
            case BrickState.Colored:
                mr.enabled = true;
                mr.material = GetMaterialByColorIndex(GetColorOfBrick(t));
                break;
        }
    }

    public void EnableBricks(bool enable)
    {
        __GeneratedBricks.gameObject.SetActive(enable);
        if (enable && this.explosion != null)
        {
            this.explosion.ResetExplosion();
        }
        if (enable && __EnclosingGameObject != null)
        {
            __EnclosingGameObject.gameObject.SetActive(true);
        }
    }

    public Transform GetChildByName(string name)
    {
        var n = this.transform.childCount;
        for (var i = 0; i < n; i++)
        {
            var child = this.transform.GetChild(i);
            if (child.name.StartsWith(name))
            {
                return child;
            }
        }
        return null;
    }

    public void EnableVisuals(bool enable)
    {
        var n = this.transform.childCount;
        for (var i = 0; i < n; i++)
        {
            var child = this.transform.GetChild(i);
            if (!child.name.StartsWith("__"))
            {
                child.gameObject.SetActive(enable);
            }
        }
        if (enable && __EnclosingGameObject != null)
        {
            __EnclosingGameObject.gameObject.SetActive(false);
        }
    }

    public bool HasVisuals()
    {
        for (var i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i);
            if (!child.name.StartsWith("__"))
            {
                return child.gameObject.activeSelf;
            }
        }
        return false;
    }


    public Vector3 GetAveragePosition()
    {
        if (this.SortedBricks.Count == 0)
        {
            return this.transform.position;
        }
        return GameObjectHelper.GetAveragePosition(this.SortedBricks);
    }


}