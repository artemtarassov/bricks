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

    void Start()
    {
        this.emitterPrefab.gameObject.SetActive(false);
        this.columnPrefab.gameObject.SetActive(false);

        SlotModel.Instance.OnEmitterChanged += OnEmitterChanged;
        SlotModel.Instance.OnSlotsChanged += OnSlotsChanged;
        CityModel.Instance.OnCityElementUnlocked += OnCityElementUnlocked;

        if (CityModel.Instance.GetCurrentElement() != null)
        {
            this.Setup();
        }
    }

    void OnDestroy()
    {
        SlotModel.Instance.OnEmitterChanged -= OnEmitterChanged;
        SlotModel.Instance.OnSlotsChanged -= OnSlotsChanged;
        CityModel.Instance.OnCityElementUnlocked -= OnCityElementUnlocked;
    }

    private void OnCityElementUnlocked(CityElement ce)
    {
        this.Setup();
    }

    private void Setup()
    {
        this.OnEmitterChanged(null);
        this.OnSlotsChanged(null);
    }

    private void OnEmitterChanged(BrickData bd)
    {
        var slotModel = SlotModel.Instance;
        var emitters = slotModel.Emitters;
        for (var i = 0; i < emitters.Length; i++)
        {
            var emitter = GetEmitterByIndex(i);
            emitter.Setup(emitters[i]);
        }
    }

    private void OnSlotsChanged(BrickData bd)
    {
        var slotModel = SlotModel.Instance;
        var columns = slotModel.Columns;
        for (var i = 0; i < columns.Count && i < 5; i++)
        {
            var column = GetSlotColumnByIndex(i);
            column.Setup(columns[i]);
        }
    }
}