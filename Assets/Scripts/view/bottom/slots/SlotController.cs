using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class SlotController : MonoBehaviour
{
    private const float AnimateInOffsetY = 500f;

    [SerializeField] private GameObject content;
    [SerializeField] private Button addSpaceButton;

    private List<SlotColumn> columns;
    private SlotColumn columnPrefab;
    private List<EmitterBrick> emitters;
    private EmitterBrick emitterPrefab;
    private Vector3 startPos;

    private void Awake()
    {
        columns = new List<SlotColumn>();
        emitters = new List<EmitterBrick>();
        columnPrefab = GetComponentInChildren<SlotColumn>(true);
        emitterPrefab = GetComponentInChildren<EmitterBrick>(true);

        emitterPrefab.gameObject.SetActive(false);
        columnPrefab.gameObject.SetActive(false);
        addSpaceButton.gameObject.SetActive(false);

        startPos = content.transform.localPosition;
    }

    private void Start()
    {
        InitializeEmitters();
        SubscribeToEvents();
        addSpaceButton.onClick.AddListener(OnAddSpaceButtonClicked);
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeEmitters()
    {
        for (var i = 0; i < SlotModel.MaxEmitters; i++)
        {
            var emitter = GetEmitterByIndex(i);
            ViewModel.Instance.Emitters.Add(emitter.transform);
            emitter.gameObject.SetActive(false);
        }
    }

    private void SubscribeToEvents()
    {
        SlotModel.Instance.OnEmitterChanged += OnEmitterChanged;
        SlotModel.Instance.OnColumnsChanged += OnColumnsChanged;
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter += OnBrickMovedFromColumnToEmitter;
        SlotModel.Instance.OnEmitterDeath += OnEmitterDeath;
        SlotModel.Instance.OnRemovedFromColumn += OnRemovedFromColumn;
        ViewModel.Instance.OnBottomNavChange += OnBottomNavChange;
        CityModel.Instance.OnElementCompleted += OnCityElementCompleted;
    }

    private void UnsubscribeFromEvents()
    {
        SlotModel.Instance.OnEmitterChanged -= OnEmitterChanged;
        SlotModel.Instance.OnColumnsChanged -= OnColumnsChanged;
        SlotModel.Instance.OnBrickMovedFromColumnToEmitter -= OnBrickMovedFromColumnToEmitter;
        SlotModel.Instance.OnEmitterDeath -= OnEmitterDeath;
        SlotModel.Instance.OnRemovedFromColumn -= OnRemovedFromColumn;
        ViewModel.Instance.OnBottomNavChange -= OnBottomNavChange;
        CityModel.Instance.OnElementCompleted -= OnCityElementCompleted;
    }

    private void OnBottomNavChange(BottomNav _)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (ViewModel.Instance.CurrentBottomNav == BottomNav.Slots)
        {
            AnimateIn();
            return;
        }

        content.SetActive(false);
    }

    private void AnimateIn()
    {
        if (content.activeSelf)
        {
            return;
        }

        //Debug.Log("SlotController: AnimateIn called");

        content.SetActive(true);
        content.transform.localPosition = startPos - new Vector3(0f, AnimateInOffsetY, 0f);
        content.transform.DOLocalMove(startPos, Durations.NavTransition).SetEase(Ease.OutSine);
    }

    private void OnRemovedFromColumn(SlotElementData data)
    {
        foreach (var column in columns)
        {
            column.Remove(data);
        }
    }

    private void UpdateAddSpaceButtonVisibility()
    {
        var allUnlocked = SlotModel.Instance.Emitters.All(e => e.isUnlocked);
        var hasVideo = AdModel.Instance.IsAdReady(AdUnits.Rewarded);
        var hasIap = IAPModel.Instance.HasPriceForProduct(IAPModel.AdditionalSpace);
        addSpaceButton.gameObject.SetActive(!allUnlocked && (hasVideo || hasIap));
    }

    private void OnAddSpaceButtonClicked()
    {
        new ShowViewCmd(ViewName.AddSpaceView).Run();
    }

    private SlotColumn GetSlotColumnByIndex(int index)
    {
        while (columns.Count <= index)
        {
            var column = Instantiate(columnPrefab, columnPrefab.transform.parent);
            column.gameObject.SetActive(true);
            columns.Add(column);
        }

        return columns[index];
    }

    private EmitterBrick GetEmitterByIndex(int index)
    {
        while (emitters.Count <= index)
        {
            var emitter = Instantiate(emitterPrefab, emitterPrefab.transform.parent);
            emitters.Add(emitter);
        }

        return emitters[index];
    }

    private void OnEmitterChanged(EmitterSpace emitterSpace = null)
    {
        //Debug.Log("SlotController: OnEmitterChanged called es " + (emitterSpace != null ? emitterSpace.index.ToString() : "null"));

        if (emitterSpace == null)
        {
            RefreshAllEmitters();
            return;
        }

        RefreshEmitter(emitterSpace);
        UpdateAdditionalEmitterTimeout();
        UpdateAddSpaceButtonVisibility();
    }

    private void RefreshAllEmitters()
    {
        var slotModel = SlotModel.Instance;
        Assert.IsTrue(slotModel.Emitters.Count > 0, "SlotController: OnEmitterChanged: Emitters list should not be empty");

        var unlockedEmitters = slotModel.Emitters.FindAll(e => e.isUnlocked);
        var lockedEmitters = slotModel.Emitters.FindAll(e => !e.isUnlocked);

        //Debug.Log($"SlotController: OnEmitterChanged: updating all emitters. unlocked {unlockedEmitters.Count} locked {lockedEmitters.Count}");

        foreach (var unlockedEmitter in unlockedEmitters)
        {
            ShowEmitter(unlockedEmitter, ResolveEmitterColor(unlockedEmitter), false);
            //Debug.Log($"SlotController: OnEmitterChanged: updated unlocked emitter {unlockedEmitter.index} with color {(unlockedEmitter.brickData != null ? unlockedEmitter.brickData.color.ToString() : "null")}");
        }

        foreach (var lockedEmitter in lockedEmitters)
        {
            HideEmitter(lockedEmitter);
        }
    }

    private void RefreshEmitter(EmitterSpace emitterSpace)
    {
        if (!emitterSpace.isUnlocked)
        {
            HideEmitter(emitterSpace);
            return;
        }

        ShowEmitter(emitterSpace, ResolveEmitterColor(emitterSpace), emitterSpace.IsEmpty);
    }

    private void ShowEmitter(EmitterSpace emitterSpace, Color color, bool animate)
    {
        var emitterView = GetEmitterByIndex(emitterSpace.index);
        emitterView.gameObject.SetActive(true);
        emitterView.Setup(color, emitterSpace, animate);
    }

    private void HideEmitter(EmitterSpace emitterSpace)
    {
        var emitterView = GetEmitterByIndex(emitterSpace.index);
        emitterView.gameObject.SetActive(false);
        emitterView.RemoveTimeout();
    }

    private Color ResolveEmitterColor(EmitterSpace emitterSpace)
    {
        var cityElement = ModelUtils.GetCurrentElement();
        if (cityElement == null || emitterSpace.brickData == null || emitterSpace.brickData.coloredAmount <= 0)
        {
            return Color.white;
        }

        return cityElement.GetBrickColor(emitterSpace.brickData.color);
    }

    private void UpdateAdditionalEmitterTimeout()
    {
        var timeoutTimestamp = PlayerModel.Instance.playerData.additionalEmitterUnlockTimeoutTimestamp;
        if (timeoutTimestamp <= 0)
        {
            return;
        }

        var additionalEmitter = GetEmitterByIndex(SlotModel.AdditionalEmitterIndex);
        additionalEmitter.SetTimeout(timeoutTimestamp);
    }

    private void OnColumnsChanged()
    {
        var slotModel = SlotModel.Instance;
        var element = ModelUtils.GetCurrentElement();

        foreach (var column in slotModel.Columns)
        {
            GetSlotColumnByIndex(column.columnIndex).Setup(column, element.brickColors);
        }
    }

    private void OnBrickMovedFromColumnToEmitter(BrickData brickData, int emitterIndex)
    {
        foreach (var column in columns)
        {
            if (column.Remove(brickData) == null)
            {
                continue;
            }

            var color = ModelUtils.GetCurrentElement().brickColors[(int)brickData.color];
            var emitterSpace = SlotModel.Instance.Emitters.Find(e => e.index == emitterIndex);
            GetEmitterByIndex(emitterIndex).Setup(color, emitterSpace, true);
            break;
        }
    }

    private void OnEmitterDeath(EmitterSpace emitterSpace)
    {
        GetEmitterByIndex(emitterSpace.index).Setup(Color.white, emitterSpace, false);
    }

    private void OnCityElementCompleted(CityElement _)
    {
        var slotModel = SlotModel.Instance;
        foreach (var column in slotModel.Columns)
        {
            GetSlotColumnByIndex(column.columnIndex).OnElementCompleted();
        }
    }
}
