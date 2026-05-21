using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotColumn : MonoBehaviour
{
    private List<SlotElement> bricks;
    private SlotElement slotBrickPrefab;

    public SlotElementDataList columnData;

    void Awake()
    {
        this.bricks = new List<SlotElement>();
        this.slotBrickPrefab = this.gameObject.GetComponentInChildren<SlotElement>(true);
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        new SelectColumnCmd(this.columnData.list[0]).Run(); 
    }

    private SlotElement GetSlotBrickByIndex(int index)
    {
        if (index >= this.bricks.Count)
        {
            var brick = Instantiate(this.slotBrickPrefab, this.slotBrickPrefab.transform.parent);
            brick.gameObject.SetActive(true);
            this.bricks.Add(brick);
            return brick;
        }
        return this.bricks[index];
    }

    public void Setup(SlotElementDataList sc)
    {
        this.columnData = sc;

        for (int i = 0; i < bricks.Count; i++)
        {
            var brick = GetSlotBrickByIndex(i);
            brick.Setup(null);
        }

        for (int i = 0; i < sc.list.Count; i++)
        {
            var brickData = sc.list[i];
            var brick = GetSlotBrickByIndex(i);
            brick.Setup(brickData);
        }
        this.slotBrickPrefab.gameObject.SetActive(false);
    }
}