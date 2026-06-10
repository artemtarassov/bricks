using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;

public class FlyingBricksTest : MonoBehaviour
{
    [SerializeField] private GameObject brickPrefab;
    [SerializeField] private GameObject target;

    private FlyingBricks2 flyingBricks;


    void Start()
    {
        Time.timeScale = 0.1f;
        this.target.gameObject.SetActive(true);
        this.brickPrefab.SetActive(false);
        flyingBricks = new FlyingBricks2(this.brickPrefab, this.transform);

        DOTween.Sequence(this).AppendInterval(2f).AppendCallback(() =>
        {
            StartFly();
        }).SetLoops(-1);
        StartFly();
    }

    private void StartFly()
    {
        var startPos = new Vector3(0, 5, 0);
        flyingBricks.Fly(
            new List<Transform> { },
            new FlyBrickData
            {
                colorIndex = ColorIndex.C0,
                from = startPos,
                targetBrick = this.target.transform
            }
        );
    }
    void OnDestroy()
    {
        DOTween.Kill(this);
        flyingBricks.Dispose();
    }

}
