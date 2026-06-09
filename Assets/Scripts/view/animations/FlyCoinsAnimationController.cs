using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlyCoinsAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject flyCoinPrefab;
    [SerializeField] private Transform target;
    [SerializeField] private int coinObjectsPerFlight = 10;
    [SerializeField] private float duration = Durations.CoinFlyDuration;
    [SerializeField] private float delayBetweenCoins = 0.03f;
    [SerializeField] private float startSpreadRadius = 18f;
    [SerializeField] private float targetSpreadRadius = 10f;
    [SerializeField] private float curveOffset = 90f;
    [SerializeField] private float curveOffsetVariance = 35f;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float peakScale = 1.05f;
    [SerializeField] private float endScale = 0.8f;
    [SerializeField] private float minSpin = 360f;
    [SerializeField] private float maxSpin = 720f;

    private readonly Queue<GameObject> cachedCoins = new Queue<GameObject>();
    private readonly List<GameObject> createdCoins = new List<GameObject>();
    private int addCoinsPerArrive;

    private void Start()
    {
        if (ViewModel.Instance != null)
        {
            ViewModel.Instance.OnFlyCoin += OnFlyCoin;
        }
    }

    private void OnDestroy()
    {
        if (ViewModel.Instance != null)
        {
            ViewModel.Instance.OnFlyCoin -= OnFlyCoin;
        }

        DOTween.Kill(this);

        for (var i = 0; i < createdCoins.Count; i++)
        {
            var coin = createdCoins[i];
            if (coin == null)
            {
                continue;
            }

            coin.transform.DOKill();
        }

        cachedCoins.Clear();
        createdCoins.Clear();
    }

    private void OnFlyCoin(Vector3 startPos, int fromAmount, int toAmount)
    {
        if (target == null)
        {
            Debug.LogWarning("FlyCoinsAnimationController: target is not assigned.");
            return;
        }
        this.addCoinsPerArrive = (toAmount - fromAmount) / coinObjectsPerFlight;
        FlyCoins(startPos, target.position, coinObjectsPerFlight);
    }

    public void FlyCoins(Vector3 startPos, Vector3 targetPos, int coinsPerFlight)
    {
        if (flyCoinPrefab == null)
        {
            Debug.LogWarning("FlyCoinsAnimationController: flyCoinPrefab is not assigned.");
            return;
        }

        var planeZ = transform.position.z;
        var start2D = new Vector3(startPos.x, startPos.y, planeZ);
        var target2D = new Vector3(targetPos.x, targetPos.y, planeZ);

        for (var i = 0; i < coinsPerFlight; i++)
        {
            SpawnCoin(start2D, target2D, i);
        }
    }

    private void SpawnCoin(Vector3 startPos, Vector3 targetPos, int index)
    {
        //Debug.Log($"Spawning coin {index + 1}/{coinObjectsPerFlight} index {index}");
        var coin = GetCoin();
        var coinTransform = coin.transform;
        coinTransform.SetParent(transform, true);
        coinTransform.DOKill();

        var startOffset = (Vector3)(Random.insideUnitCircle * startSpreadRadius);
        var targetOffset = (Vector3)(Random.insideUnitCircle * targetSpreadRadius);
        var from = startPos + startOffset;
        var to = targetPos + targetOffset;
        var control = GetControlPoint(from, to, index);
        var spin = Random.Range(minSpin, maxSpin) * (Random.value > 0.5f ? 1f : -1f);
        var flyDuration = Mathf.Max(0.1f, duration + Random.Range(-0.08f, 0.08f));
        var startSize = startScale * Random.Range(0.9f, 1.1f);
        var peakSize = peakScale * Random.Range(0.95f, 1.1f);
        var finishSize = endScale * Random.Range(0.9f, 1.05f);

        coinTransform.position = from;
        coinTransform.rotation = Quaternion.identity;
        coinTransform.localScale = Vector3.one * startSize;
        coin.SetActive(false);

        var scaleSequence = DOTween.Sequence()
            .Append(coinTransform.DOScale(peakSize, flyDuration * 0.45f).SetEase(Ease.OutBack))
            .Append(coinTransform.DOScale(finishSize, flyDuration * 0.55f).SetEase(Ease.InQuad));

        var sequence = DOTween.Sequence(this).SetTarget(coinTransform);

        sequence.AppendInterval(index * delayBetweenCoins);
        sequence.AppendCallback(() =>
        {
            if (coin != null)
            {
                coin.SetActive(true);
            }
        });

        sequence.Append(
            DOTween.To(
                () => 0f,
                progress => { coinTransform.position = EvaluateQuadraticBezier(from, control, to, progress); },
                1f,
                flyDuration
            ).SetEase(Ease.InOutQuad).SetTarget(coinTransform)
        );
        sequence.Join(coinTransform.DORotate(new Vector3(0f, 0f, spin), flyDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        sequence.Join(scaleSequence);
        sequence.OnComplete(() =>
        {
            OnCoinArrived(index);
            ReleaseCoin(coin);
        });
        sequence.OnKill(() =>
        {
            if (coin != null && coin.activeSelf)
            {
                ReleaseCoin(coin);
            }
        });
    }

    private Vector3 GetControlPoint(Vector3 startPos, Vector3 targetPos, int index)
    {
        var direction = targetPos - startPos;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return startPos;
        }

        var perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;
        var side = index % 2 == 0 ? 1f : -1f;
        var curveAmount = (curveOffset + Random.Range(-curveOffsetVariance, curveOffsetVariance)) * side;
        var midpoint = Vector3.Lerp(startPos, targetPos, 0.5f);
        var forwardShift = direction.normalized * Random.Range(-0.15f, 0.15f) * direction.magnitude;
        return midpoint + forwardShift + perpendicular * curveAmount;
    }

    private static Vector3 EvaluateQuadraticBezier(Vector3 startPos, Vector3 controlPos, Vector3 targetPos, float t)
    {
        var oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * startPos
               + 2f * oneMinusT * t * controlPos
               + t * t * targetPos;
    }

    private void OnCoinArrived(int index)
    {
        new AddCoinsCmd(addCoinsPerArrive).Run();
    }

    private GameObject GetCoin()
    {
        while (cachedCoins.Count > 0)
        {
            var cachedCoin = cachedCoins.Dequeue();
            if (cachedCoin != null)
            {
                cachedCoin.transform.DOKill();
                return cachedCoin;
            }
        }

        var createdCoin = Instantiate(flyCoinPrefab, transform);
        createdCoin.SetActive(false);
        createdCoins.Add(createdCoin);
        return createdCoin;
    }

    private void ReleaseCoin(GameObject coin)
    {
        if (coin == null)
        {
            return;
        }

        coin.transform.DOKill();
        coin.transform.SetParent(transform, true);
        coin.SetActive(false);
        cachedCoins.Enqueue(coin);
    }
}
