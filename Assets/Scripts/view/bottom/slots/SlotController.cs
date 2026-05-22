using System;
using System.Collections.Generic;
using UnityEngine;

public class SlotController : MonoBehaviour
{
    private List<SlotColumn> columns;
    private SlotColumn columnPrefab;

    private List<EmitterBrick> emitters;
    private EmitterBrick emitterPrefab;

    void Awake()
    {
        this.columns = new List<SlotColumn>();
        this.emitters = new List<EmitterBrick>();
        this.columnPrefab = this.gameObject.GetComponentInChildren<SlotColumn>(true);
        this.emitterPrefab = this.gameObject.GetComponentInChildren<EmitterBrick>(true);
    }

    void Start()
    {
        this.emitterPrefab.gameObject.SetActive(false);
        this.columnPrefab.gameObject.SetActive(false);

        SlotModel.Instance.OnEmitterChanged += OnEmitterChanged;
        SlotModel.Instance.OnColumnsChanged += OnColumnsChanged;
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter += OnBrickMovedFromColumnToEmitter;
        CityModel.Instance.OnCityElementUnlocked += OnCityElementUnlocked;

        if (CityModel.Instance.GetCurrentElement() != null)
        {
            this.Setup();
        }
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
            emitter.gameObject.SetActive(true);
            this.emitters.Add(emitter);
            ViewModel.Instance.Emitters.Add(emitter);
            return emitter;
        }
        return this.emitters[index];
    }



    void OnDestroy()
    {
        SlotModel.Instance.OnEmitterChanged -= OnEmitterChanged;
        SlotModel.Instance.OnColumnsChanged -= OnColumnsChanged;
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter -= OnBrickMovedFromColumnToEmitter;
        CityModel.Instance.OnCityElementUnlocked -= OnCityElementUnlocked;
    }

    private void OnCityElementUnlocked(CityElement ce)
    {
        this.Setup();
    }

    private void Setup()
    {
        this.OnEmitterChanged();
        this.OnColumnsChanged();
    }

    private void OnEmitterChanged(int index = -1)
    {
        var slotModel = SlotModel.Instance;
        if (index == -1)
        {
            for (int i = 0; i < slotModel.Emitters.Length; i++)
            {
                GetEmitterByIndex(i).Setup(slotModel.Emitters[i], false);
            }
            return;
        }
        GetEmitterByIndex(index).Setup(slotModel.Emitters[index], false);
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