using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


public class CityElement : MonoBehaviour
{
    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;


    public string dataKey => this.transform.parent.name + "_" + this.name;

    //public CityElementColors cityElementColors { get; private set; }

    [HideInInspector]
    public CityElementDataContainer dataContainer;

    public Dictionary<Transform, BrickState> allBricks = new Dictionary<Transform, BrickState>();
    public Dictionary<Transform, ColorIndex> brickColors = new Dictionary<Transform, ColorIndex>();

    private BrickLayersContainer brickLayersContainer;

    void Awake()
    {
        allBricks = new Dictionary<Transform, BrickState>();
        brickColors = new Dictionary<Transform, ColorIndex>();

        var bricks = this.GetBricks();
        Assert.IsTrue(bricks.Count > 0, "No bricks found in city element " + this.name);

        foreach (var brick in bricks)
        {
            this.allBricks.Add(brick, BrickState.Transparent);
            this.brickColors.Add(brick, ColorIndex.Undefined);
        }
        this.brickLayersContainer = new BrickLayersContainer(bricks);

        this.EnableVisuals(false);
        this.EnableAllBricks(true);
        //this.cityElementColors = new CityElementColors(this.CountBricks());
    }

    public List<Transform> GetBricks()
    {
        if (this.allBricks != null && this.allBricks.Keys.Count > 0)
        {
            return this.allBricks.Keys.ToList();
        }
        return GetBricksContainer().GetComponentsInChildren<Transform>(true).ToList().FindAll(b => b.tag == "Brick");
    }

    public int CountBricks()
    {
        return this.allBricks.Keys.Count;
    }

    public int CountTransparentBricks()
    {
        return this.allBricks.Count(kv => kv.Value == BrickState.Transparent);
    }

    public int CountColoredBricks(ColorIndex colorIndex)
    {
        return this.brickColors.Count(kv => kv.Value == colorIndex);
    }

    public int CountColoredBricks()
    {
        return this.allBricks.Count(kv => kv.Value == BrickState.Colored);
    }

    public int CountFlyingBricks()
    {
        return this.allBricks.Count(kv => kv.Value == BrickState.Flying);
    }

    public bool AllFull()
    {
        return this.allBricks.All(kv => kv.Value == BrickState.Full);
    }

    

    private Material GetMaterialByName(string name)
    {
        return ColoredMaterials.Instance.GetMaterialByName(name);
    }

    private Material GetMaterialByColorIndex(ColorIndex colorIndex)
    {
        return ColoredMaterials.Instance.GetMaterialByColorIndex(colorIndex);
    }


    public Transform GetNextColoredBrick(ColorIndex colorIndex)
    {
        foreach (var layer in brickLayersContainer.layers)
        {
            var b = layer.bricks.Find(b => this.brickColors[b] == colorIndex);
            if (b != null)
            {
                return b;
            }
        }
        return null;
    }

    public Transform GetNextTransparentBrick()
    {
        foreach (var layer in brickLayersContainer.layers)
        {
            var transparentBrick = layer.bricks.Find(b => this.allBricks[b] == BrickState.Transparent);
            if (transparentBrick != null)
            {
                return transparentBrick;
            }
        }
        return null;
    }


    private int pointer = 0;
    public void ShowNextColoredBricks(int totalDifferentBrickColors)
    {
        for (var i = 0; i < totalDifferentBrickColors && pointer < this.dataContainer.brickDataList.Count; i++)
        {
            var next = this.dataContainer.brickDataList[pointer];
            this.SetBrickColors(next);
            pointer++;
        }
    }

    private void SetBrickColors(BrickData bd)
    {
        Assert.IsTrue(bd.amount > 0, "invalid amout");
        for (var i = 0; i < bd.amount; i++)
        {
            var nextTransparentBrick = this.GetNextTransparentBrick();
            this.SetBrickState(nextTransparentBrick, BrickState.Colored, bd.color);
        }
    }

    public void SetBrickState(Transform t, BrickState state, ColorIndex colorIndex = ColorIndex.Undefined)
    {
        Assert.IsTrue(this.allBricks.ContainsKey(t), "Brick transform not found in city element");
        this.allBricks[t] = state;
        var mr = t.GetComponent<MeshRenderer>();
        switch (state)
        {
            case BrickState.Transparent:
                mr.material = GetMaterialByName("BrickMatTransparent");
                brickColors[t] = ColorIndex.Undefined;
                break;
            case BrickState.Flying:
                //mr.material = GetMaterialByName("BrickMatTransparent");
                brickColors[t] = ColorIndex.Undefined;
                break;
            case BrickState.Full:
                mr.material = GetMaterialByName("BrickMatFull");
                brickColors[t] = ColorIndex.Undefined;
                break;
            case BrickState.Colored:
                Assert.IsTrue(colorIndex >= 0, "Color index must be provided for colored bricks");
                mr.material = GetMaterialByColorIndex(colorIndex);
                brickColors[t] = colorIndex;
                break;
        }
    }

    public void EnableAllBricks(bool enable)
    {
        foreach (var brick in this.allBricks.Keys)
        {
            brick.gameObject.SetActive(enable);
        }
    }

    public void EnableVisuals(bool enable)
    {
        for (var i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i);
            if (!child.name.StartsWith("__"))
            {
                child.gameObject.SetActive(enable);
            }
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

    private Transform GetBricksContainer()
    {
        for (var i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i);
            if (child.name.StartsWith("__"))
            {
                return child;
            }
        }
        return null;
    }

    public Vector3 GetAveragePosition()
    {
        var bricks = GetBricks();
        var sum = Vector3.zero;
        foreach (var brick in bricks)
        {
            sum += brick.transform.position;
        }

        return sum / bricks.Count;
    }

}