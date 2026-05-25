using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


public class CityElement : MonoBehaviour
{
    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;


    public string dataKey => this.transform.parent.name + "_" + this.name;

    [HideInInspector]
    public CityElementDataContainer dataContainer;

    public Dictionary<Transform, BrickState> brickState = new Dictionary<Transform, BrickState>();
    private Dictionary<Transform, ColorIndex> brickColors = new Dictionary<Transform, ColorIndex>();

    private BrickLayersContainer brickLayersContainer;

    void Awake()
    {
        // brickState = new Dictionary<Transform, BrickState>();
        //brickColors = new Dictionary<Transform, ColorIndex>();

        var bricks = this.FindAllBricks();
        Assert.IsTrue(bricks.Count > 0, "No bricks found in city element " + this.name);

        foreach (var brick in bricks)
        {
            this.brickState.Add(brick, BrickState.Transparent);
            this.brickColors.Add(brick, ColorIndex.Undefined);
        }
        this.brickLayersContainer = new BrickLayersContainer(bricks);

        this.EnableVisuals(false);
        this.EnableAllBricks(true);
        //this.cityElementColors = new CityElementColors(this.CountBricks());
    }

    public void Setup(CityElementDataContainer dataContainer, int additionalBricksOnEmptyElement = 0)
    {
        Debug.Log($"CityElement Setup: setting up city element {this.name} with data container {dataContainer.key}");
        this.dataContainer = dataContainer;

        var sortedBricks = this.brickLayersContainer.sortedBricks;

        pointer = 0;
        this.EnableAllBricks(true);
        this.EnableVisuals(false);

        var j = 0;
        foreach (var bd in dataContainer.brickDataList)
        {
            for (var i = 0; i < bd.amount && j < sortedBricks.Count; i++)
            {
                this.brickColors[sortedBricks[j]] = bd.color;
                j++;
            }
        }
        foreach (var b in sortedBricks)
        {
            SetBrickState(b, BrickState.Transparent);
        }
        this.ShowNextColoredBricks(additionalBricksOnEmptyElement);
    }

    public List<Transform> FindAllBricks()
    {
        return GetBricksContainer().GetComponentsInChildren<Transform>(true).ToList().FindAll(b => b.tag == "Brick");
    }

    public int CountBricks()
    {
        return this.brickState.Keys.Count;
    }

    public int CountTransparentBricks()
    {
        return this.brickState.Count(kv => kv.Value == BrickState.Transparent);
    }

    public int CountColoredBricks(ColorIndex colorIndex)
    {
        return this.brickState.Count(kv => kv.Value == BrickState.Colored && this.GetColorOfBrick(kv.Key) == colorIndex);
    }

    public ColorIndex GetColorOfBrick(Transform brick)
    {
        Assert.IsTrue(this.brickColors.ContainsKey(brick), "Brick transform not found in city element");
        return this.brickColors[brick];
    }

    public int CountColoredBricks()
    {
        return this.brickState.Count(kv => kv.Value == BrickState.Colored);
    }

    public int CountFlyingBricks()
    {
        return this.brickState.Count(kv => kv.Value == BrickState.Flying);
    }

    public bool AllFull()
    {
        return this.brickState.All(kv => kv.Value == BrickState.Full);
    }

    public HashSet<ColorIndex> GetBrickColors()
    {
        var result = new HashSet<ColorIndex>();
        foreach (var c in this.brickState)
        {
            if (c.Value == BrickState.Colored || c.Value == BrickState.Flying)
            {
                result.Add(this.GetColorOfBrick(c.Key));
            }
        }
        return result;
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
        return this.brickLayersContainer.sortedBricks.Find(b => this.brickState[b] == BrickState.Colored && this.GetColorOfBrick(b) == colorIndex);
    }

    public Transform GetNextTransparentBrick()
    {
        return this.brickLayersContainer.sortedBricks.Find(b => this.brickState[b] == BrickState.Transparent);
    }

    private int pointer = 0;
    public void ShowNextColoredBricks(int totalDifferentBrickColors)
    {
        for (var i = 0; i < totalDifferentBrickColors && pointer < this.dataContainer.brickDataList.Count; i++)
        {
            var next = this.dataContainer.brickDataList[pointer];
            this.SetNextBrickColor(next.amount);
            pointer++;
        }
    }

    private void SetNextBrickColor(int amount)
    {
        Assert.IsTrue(amount > 0, "invalid amount");
        for (var i = 0; i < amount; i++)
        {
            var nextTransparentBrick = this.GetNextTransparentBrick();
            this.SetBrickState(nextTransparentBrick, BrickState.Colored);
        }
    }

    public void SetBrickState(Transform t, BrickState state)
    {
        Assert.IsTrue(this.brickState.ContainsKey(t), "Brick transform not found in city element");
        this.brickState[t] = state;
        var mr = t.GetComponent<MeshRenderer>();
        switch (state)
        {
            case BrickState.Transparent:
                mr.material = GetMaterialByName("BrickMatTransparent");
                break;
            case BrickState.Flying:
                break;
            case BrickState.Full:
                mr.material = GetMaterialByName("BrickMatFull");
                break;
            case BrickState.Colored:
                mr.material = GetMaterialByColorIndex(GetColorOfBrick(t));
                break;
        }
    }

    public void EnableAllBricks(bool enable)
    {
        foreach (var brick in this.brickLayersContainer.sortedBricks)
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
        var bricks = this.brickLayersContainer.sortedBricks;
        var sum = Vector3.zero;
        foreach (var brick in bricks)
        {
            sum += brick.transform.position;
        }

        return sum / bricks.Count;
    }

}