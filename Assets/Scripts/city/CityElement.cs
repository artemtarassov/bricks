using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

class BrickInfo
{
    public BrickState state;
    public ColorIndex color;
}

public class CityElement : MonoBehaviour
{
    [SerializeField] public Vector3 camPos;
    [SerializeField] public Vector3 camRot;


    public string dataKey => this.transform.parent.name + "_" + this.name;

    [HideInInspector]
    public CityElementDataContainer dataContainer;
    private Dictionary<Transform, BrickInfo> brickInfo = new Dictionary<Transform, BrickInfo>();
    private BrickLayersContainer brickLayersContainer;

    private BrickTopDownExplosion explosion;

    void Awake()
    {
        var bricks = this.FindAllBricks();
        this.brickLayersContainer = new BrickLayersContainer(bricks);
        foreach (var brick in bricks)
        {
            this.brickInfo[brick] = new BrickInfo()
            {
                state = BrickState.Transparent,
                color = ColorIndex.Undefined
            };
        }
    }

    public void Setup(CityElementDataContainer dataContainer)
    {
        Debug.Log($"CityElement Setup: setting up city element {this.name} with data container {dataContainer.dataKey}");
        this.dataContainer = dataContainer;
        this.EnableAllBricks(true);
        this.EnableVisuals(false);
        this.SetBrickColors();
        this.ShowBrickStates();
    }

    public List<Transform> FindAllBricks()
    {
        return GetBricksContainer().GetComponentsInChildren<Transform>(true).ToList().FindAll(b => b.tag == "Brick");
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

    private List<Transform> SortedBricks => this.brickLayersContainer.sortedBricks;

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

    public void ShowBrickStates()
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
            if (this.explosion == null)
            {
                this.explosion = this.gameObject.AddComponent<BrickTopDownExplosion>();
            }
            else
            {
                this.explosion.Play();
            }
            this.EnableVisuals(true);
        }
    }


    public Transform GetColoredBrick(ColorIndex colorIndex)
    {
        var elementBrickData = dataContainer.brickDataList.Find(b => b.color == colorIndex && b.coloredAmount > 0);
        if (elementBrickData == null)
        {
            return null;
        }
        return GetBrick(elementBrickData, elementBrickData.GetBrickIndex(BrickState.Colored));
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


    /*public void ShowNextColoredBricks(int totalDifferentBrickColors)
    {
        var lastColoredBrickIndex = this.brickLayersContainer.sortedBricks.FindLastIndex(b => this.brickState[b] == BrickState.Colored);
        var pointer = lastColoredBrickIndex + 1;
        for (var i = 0; i < totalDifferentBrickColors && lastColoredBrickIndex < this.dataContainer.brickDataList.Count; i++)
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
    }*/

    private void ShowBrickState(Transform t, BrickState state)
    {
        if (this.brickInfo.ContainsKey(t) && this.brickInfo[t].state == state)
        {
            return;
        }
        this.brickInfo[t].state = state;
        var mr = t.GetComponent<MeshRenderer>();
        switch (state)
        {
            case BrickState.Transparent:
                mr.material = GetMaterialByName("BrickMatTransparent");
                break;
            case BrickState.Emitting:
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
        foreach (var brick in this.SortedBricks)
        {
            brick.gameObject.SetActive(enable);
        }
        if (enable && this.explosion != null)
        {
            this.explosion.ResetExplosion();
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
        var sum = Vector3.zero;
        foreach (var brick in SortedBricks)
        {
            sum += brick.transform.position;
        }

        return sum / SortedBricks.Count;
    }

}