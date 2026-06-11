using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    private List<SlotColumn> columns;
    private SlotColumn columnPrefab;

    private List<EmitterBrick> emitters;
    private EmitterBrick emitterPrefab;

    [SerializeField]
    private Button addSpaceButton;

    private Vector3 startPos;

    void Awake()
    {
        this.columns = new List<SlotColumn>();
        this.emitters = new List<EmitterBrick>();
        this.columnPrefab = this.gameObject.GetComponentInChildren<SlotColumn>(true);
        this.emitterPrefab = this.gameObject.GetComponentInChildren<EmitterBrick>(true);
    }

    void Start()
    {
        this.startPos = this.content.transform.localPosition;
        this.emitterPrefab.gameObject.SetActive(false);
        this.columnPrefab.gameObject.SetActive(false);


        for (var i = 0; i < SlotModel.MaxEmitters; i++)
        {
            var eb = GetEmitterByIndex(i);
            ViewModel.Instance.Emitters.Add(eb.transform);
            eb.gameObject.SetActive(false);
        }

        SlotModel.Instance.OnEmitterChanged += OnEmitterChanged;
        SlotModel.Instance.OnColumnsChanged += OnColumnsChanged;
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter += OnBrickMovedFromColumnToEmitter;
        SlotModel.Instance.OnRemovedFromColumn += OnRemovedFromColumn;
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;

        this.addSpaceButton.onClick.AddListener(OnAddSpaceButtonClicked);

        this.UpdateVisibility();
    }

    private void OnBottomNavChange(BottomNav nav)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        var nav = ViewModel.Instance.CurrentBottomNav;
        if (nav == BottomNav.Slots)
        {
            this.AnimateIn();
        }
        else
        {
            this.content.SetActive(false);
        }
    }

    private void AnimateIn()
    {
        if (this.content.activeSelf)
        {
            return;
        }
        this.content.SetActive(true);
        this.content.transform.localPosition = this.startPos - new Vector3(0, 500, 0);
        this.content.transform.DOLocalMove(this.startPos, Durations.NavTransition).SetEase(Ease.OutSine);
    }

  

    private void OnRemovedFromColumn(SlotElementData data)
    {
        foreach (var column in this.columns)
        {
            column.Remove(data);
        }
    }

    private void UpdateAddSpaceButtonVisibility()
    {
        this.addSpaceButton.gameObject.SetActive(PlayerModel.Instance.playerData.additionalEmitterUnlockTimeoutTimestamp == 0);
    }

    private void OnAddSpaceButtonClicked()
    {
        new ShowViewCmd(ViewName.AddSpaceView).Run();
    }

    private SlotColumn GetSlotColumnByIndex(int index)
    {
        if (index >= this.columns.Count)
        {
            var column = Instantiate(this.columnPrefab, this.columnPrefab.transform.parent);
            column.gameObject.SetActive(true);
            this.columns.Add(column);
            return column;
        }
        return this.columns[index];
    }

    private EmitterBrick GetEmitterByIndex(int index)
    {
        if (index >= this.emitters.Count)
        {
            var emitter = Instantiate(this.emitterPrefab, this.emitterPrefab.transform.parent);
            this.emitters.Add(emitter);
            return emitter;
        }
        return this.emitters[index];
    }



    void OnDestroy()
    {
        SlotModel.Instance.OnEmitterChanged -= OnEmitterChanged;
        SlotModel.Instance.OnColumnsChanged -= OnColumnsChanged;
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter -= OnBrickMovedFromColumnToEmitter;
        SlotModel.Instance.OnRemovedFromColumn -= OnRemovedFromColumn;
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
    }

    private void OnEmitterChanged(EmitterSpace es = null)
    {
        //Debug.Log("SlotController: OnEmitterChanged called");
        var slotModel = SlotModel.Instance;

        if (es == null)
        {
            var unlockedEmitters = slotModel.Emitters.FindAll(e => e.isUnlocked);
            var lockedEmitters = slotModel.Emitters.FindAll(e => !e.isUnlocked);
            foreach (var e in unlockedEmitters)
            {
                var eb = GetEmitterByIndex(e.index);
                eb.gameObject.SetActive(true);
                eb.Setup(e.brickData, false);
            }
            foreach (var e in lockedEmitters)
            {
                var eb = GetEmitterByIndex(e.index);
                eb.gameObject.SetActive(false);
                eb.RemoveTimeout();
            }
            return;
        }
        {
            var animate = es.IsEmpty;
            var eb = GetEmitterByIndex(es.index);
            if (es.isUnlocked)
            {
                eb.gameObject.SetActive(true);
                eb.Setup(es.brickData, animate);
            }
            else
            {
                eb.gameObject.SetActive(false);
                eb.RemoveTimeout();
            }

        }

        var curTimestamp = TimeUtils.GetUnixTimestamp();
        var playerData = PlayerModel.Instance.playerData;
        var additionalEmitterUnlockTimeoutTimestamp = playerData.additionalEmitterUnlockTimeoutTimestamp;
        if (additionalEmitterUnlockTimeoutTimestamp > 0)
        {
            var additionalEmitter = GetEmitterByIndex(SlotModel.AdditionalEmitterIndex);
            additionalEmitter.SetTimeout(additionalEmitterUnlockTimeoutTimestamp);
        }
        this.UpdateAddSpaceButtonVisibility();

    }

    private void OnColumnsChanged()
    {
        Debug.Log("SlotController: OnColumnsChanged called");
        var slotModel = SlotModel.Instance;
        foreach (var c in slotModel.Columns)
        {
            Debug.Log($"SlotController Setting up slot column {c.columnIndex} with {c.list.Count} elements");
            GetSlotColumnByIndex(c.columnIndex).Setup(c);
        }
    }
    private void OnBrickMovedFromColumnToEmitter(BrickData bd, int emitterIndex)
    {
        foreach (var column in this.columns)
        {
            var removedSlot = column.Remove(bd);
            if (removedSlot != null)
            {
                GetEmitterByIndex(emitterIndex).Setup(bd, true);
                break;
            }
        }
    }
}