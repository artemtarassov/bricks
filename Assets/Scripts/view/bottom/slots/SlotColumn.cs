using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
public class SlotColumn : MonoBehaviour
{
    private List<SlotElement> slotElements = new List<SlotElement>();

    [SerializeField]
    private SlotElement slotBrickPrefab;


    [SerializeField]
    private GameObject checkmark;

    [SerializeField]
    private GameObject blinkingArrow;

    [SerializeField]
    private GameObject topSlot;

    private SlotColumnData columnData;

    void Awake()
    {
        this.slotBrickPrefab.gameObject.SetActive(false);
        this.GetComponent<HoldButton>().OnClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        var activeSlotElement = this.slotElements.Find(e => e.gameObject.activeSelf);
        if (activeSlotElement != null)
            new SelectColumnCmd(this.columnData.columnIndex, activeSlotElement).Run();
        this.blinkingArrow.SetActive(false);
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

    private bool IsVisible(SlotElementData e)
    {
        return e.IsVisible();
    }

    public void Setup(SlotColumnData sc, List<Color> brickColors)
    {
        this.checkmark.gameObject.SetActive(false);
        this.blinkingArrow.gameObject.SetActive(false);
        this.topSlot.gameObject.SetActive(false);

        this.columnData = sc;
        var nextIndex = 0;
        var visibleList = sc.list.FindAll((e) => IsVisible(e));

        foreach (var data in visibleList)
        {
            var e = CreateSlotElementByIndex(nextIndex);
            e.Setup(nextIndex, data, brickColors);
            e.gameObject.name = "SlotElement_" + nextIndex;
            e.gameObject.SetActive(true);
            e.transform.DOKill();
            e.transform.localPosition = GetSlotElementPosition(nextIndex);
            nextIndex++;
        }
        var destroyList = new List<SlotElement>();
        for (int i = nextIndex; i < this.slotElements.Count; i++)
        {
            destroyList.Add(this.slotElements[i]);
        }
        foreach (var e in destroyList)
        {
            this.slotElements.Remove(e);
            Destroy(e.gameObject);
        }
        this.UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        for (var i = 0; i < this.slotElements.Count; i++)
        {
            var e = this.slotElements[i];
            e.UpdateIndex(i);
            e.gameObject.SetActive(i < 4);
        }
        var hasElements = this.slotElements.Any((e) => e.gameObject.activeSelf);

        this.ShowGameObject(this.checkmark, !hasElements);
        this.ShowGameObject(this.topSlot, hasElements);
    }

    public void OnElementCompleted()
    {
        var hasElements = this.slotElements.Any((e) => e.gameObject.activeSelf);
        if (hasElements)
        {
            var lastElementIsExplosion = this.slotElements[0].slotElementData.type == SlotElementType.FinalExplosion;
            this.blinkingArrow.SetActive(lastElementIsExplosion);
        }
        else
        {
            this.blinkingArrow.SetActive(false);
        }
    }

    private void ShowGameObject(GameObject t, bool show)
    {
        if (show == t.activeSelf)
        {
            return;
        }
        if (show)
        {
            t.SetActive(true);
            t.transform.DOKill();
            t.transform.localScale = Vector3.zero;
            t.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutSine);
        }
        else
        {
            t.SetActive(true);
            t.transform.localScale = Vector3.one;
            t.transform.DOKill();
            t.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                t.SetActive(false);
            });
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
        return this.slotElements.Find(e => e.slotElementData.BrickData == bd);
    }
    private SlotElement GetSlotElement(SlotElementData sed)
    {
        return this.slotElements.Find(e => e.slotElementData == sed);
    }

    public SlotElement Remove(SlotElementData sed)
    {
        var slotElement = GetSlotElement(sed);
        if (slotElement == null)
        {
            return null;
        }
        return this.Remove(slotElement);
    }


    public SlotElement Remove(BrickData bd)
    {
        var slotElement = GetSlotElementByBrickData(bd);
        if (slotElement == null)
        {
            return null;
        }
        return this.Remove(slotElement);
    }

    private SlotElement Remove(SlotElement slotElement)
    {
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