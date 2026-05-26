using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlotColumn : MonoBehaviour
{
    private List<SlotElement> slotElements;
    private SlotElement slotBrickPrefab;

    [HideInInspector]
    public SlotElementDataList columnData;

    void Awake()
    {
        this.slotElements = new List<SlotElement>();
        this.slotBrickPrefab = this.gameObject.GetComponentInChildren<SlotElement>(true);
        this.slotBrickPrefab.gameObject.SetActive(false);
        this.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        var nextVisibleSlotElement = this.slotElements.Find(e => e.gameObject.activeSelf).slotElementData;
        new SelectColumnCmd(nextVisibleSlotElement).Run();
    }

    private SlotElement CreateSlotElementByIndex(int index)
    {
        if (index >= this.slotElements.Count)
        {
            var e = Instantiate(this.slotBrickPrefab, this.slotBrickPrefab.transform.parent);
            this.slotElements.Add(e);
            return e;
        }
        return this.slotElements[index];
    }

    public void Setup(SlotElementDataList sc)
    {
        this.columnData = sc;
        for (int i = 0; i < sc.list.Count; i++)
        {
            var e = CreateSlotElementByIndex(i);
            e.Setup(sc.list[i]);
            e.gameObject.name = "SlotElement_" + i;
            e.gameObject.SetActive(true);
            e.transform.DOKill();
            e.transform.localPosition = GetSlotElementPosition(i);
        }
        this.UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        for (var i = 0; i < this.slotElements.Count; i++)
        {
            var e = this.slotElements[i];
            e.gameObject.SetActive(i < 4);
        }
    }

    private Vector3 GetSlotElementPosition(int index)
    {
        var initialY = -20;
        var elementHeight = 150;
        var elementGap = 30;
        var y = -elementHeight / 2 - index * (elementHeight + elementGap) + initialY;
        return new Vector3(0, y, 0);
    }

    private SlotElement GetSlotElementByBrickData(BrickData bd)
    {
        return this.slotElements.Find(e => e.brickData == bd);
    }

    public SlotElement Remove(BrickData bd)
    {
        var slotElement = GetSlotElementByBrickData(bd);
        if (slotElement == null)
        {
            return null;
        }
        this.slotElements.Remove(slotElement);
        slotElement.transform.DOKill();
        slotElement.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InFlash).OnComplete(() => Destroy(slotElement.gameObject));
        UpdateVisibility();

        var t = Durations.SlotElementMove;
        for (var i = 0; i < this.slotElements.Count; i++)
        {
            var e = this.slotElements[i];
            var newPos = GetSlotElementPosition(i);
            e.transform.DOKill();
            if (e.gameObject.activeSelf)
                e.transform.DOLocalMove(newPos, t).SetEase(Ease.OutQuad).SetDelay(0.2f);
            else
                e.transform.localPosition = newPos;
        }
        return slotElement;
    }
}