using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
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
        SlotModel.Instance.OnEmitterDeath += OnEmitterDeath;
        SlotModel.Instance.OnRemovedFromColumn += OnRemovedFromColumn;
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
        CityModel.Instance.OnElementCompleted += OnCityElementCompleted;

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
        Debug.Log("SlotController: AnimateIn called");
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
        var allUnlocked = SlotModel.Instance.Emitters.All(e => e.isUnlocked);
        this.addSpaceButton.gameObject.SetActive(!allUnlocked);
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
        SlotModel.Instance.OnEmitterDeath -= OnEmitterDeath;
        SlotModel.Instance.OnRemovedFromColumn -= OnRemovedFromColumn;
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
        CityModel.Instance.OnElementCompleted -= OnCityElementCompleted;
    }

    private void OnEmitterChanged(EmitterSpace es = null)
    {
        Debug.Log("SlotController: OnEmitterChanged called es " + (es != null ? es.index.ToString() : "null"));
        var slotModel = SlotModel.Instance;
        var cityElement = ModelUtils.GetCurrentElement();
        var clr = Color.white;

        if (cityElement != null && es != null && es.brickData != null)
        {
            clr = cityElement.GetBrickColor(es.brickData.color);
        }

        if (es == null)
        {
            Assert.IsTrue(slotModel.Emitters.Count > 0, "SlotController: OnEmitterChanged: Emitters list should not be empty");
            var unlockedEmitters = slotModel.Emitters.FindAll(e => e.isUnlocked);
            var lockedEmitters = slotModel.Emitters.FindAll(e => !e.isUnlocked);

            Debug.Log($"SlotController: OnEmitterChanged: updating all emitters. unlocked {unlockedEmitters.Count} locked {lockedEmitters.Count}");



            foreach (var e in unlockedEmitters)
            {
                var eb = GetEmitterByIndex(e.index);
                eb.gameObject.SetActive(true);


                if (cityElement != null && e.brickData != null && e.brickData.coloredAmount > 0)
                {
                    var clr2 = cityElement.GetBrickColor(e.brickData.color);
                    eb.Setup(clr2, e, false);
                }
                else
                {
                    eb.Setup(clr, e, false);
                }

                Debug.Log($"SlotController: OnEmitterChanged: updated unlocked emitter {e.index} with color {(e.brickData != null ? e.brickData.color.ToString() : "null")}");
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
                eb.Setup(clr, es, animate);
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
        //Debug.Log("SlotController: OnColumnsChanged called");
        var slotModel = SlotModel.Instance;
        var element = ModelUtils.GetCurrentElement();
        foreach (var c in slotModel.Columns)
        {
            //Debug.Log($"SlotController Setting up slot column {c.columnIndex} with {c.list.Count} elements");
            GetSlotColumnByIndex(c.columnIndex).Setup(c, element.brickColors);
        }
    }
    private void OnBrickMovedFromColumnToEmitter(BrickData bd, int emitterIndex)
    {
        foreach (var column in this.columns)
        {
            var removedSlot = column.Remove(bd);
            if (removedSlot != null)
            {
                var clr = ModelUtils.GetCurrentElement().brickColors[(int)bd.color];
                var emitter = SlotModel.Instance.Emitters.Find(e => e.index == emitterIndex);
                GetEmitterByIndex(emitterIndex).Setup(clr, emitter, true);
                break;
            }
        }
    }

    private void OnEmitterDeath(EmitterSpace es)
    {
        GetEmitterByIndex(es.index).Setup(Color.white, es, false);
    }

    private void OnCityElementCompleted(CityElement cityElement)
    {
        var slotModel = SlotModel.Instance;
        foreach (var c in slotModel.Columns)
        {
            GetSlotColumnByIndex(c.columnIndex).OnElementCompleted();
        }
    }
}