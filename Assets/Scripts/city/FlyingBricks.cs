using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FlyingBricks
{
    private readonly GameObject flyingBrickPrefab;
    private readonly Transform parent;
    private readonly Queue<FlyingBrickCacheItem> flyingBrickCache = new Queue<FlyingBrickCacheItem>();
    private readonly List<FlyingBrickCacheItem> createdBricks = new List<FlyingBrickCacheItem>();

    private class FlyingBrickCacheItem
    {
        public GameObject gameObject;
        public Renderer renderer;
    }

    public FlyingBricks(GameObject flyingBrickPrefab, Transform parent)
    {
        this.flyingBrickPrefab = flyingBrickPrefab;
        this.parent = parent;
    }

    public void Fly(FlyBrickData data)
    {
        var t = Durations.FlyBrickDuration;
        var brick = GetFlyingBrick();
        var brickTransform = brick.gameObject.transform;
        brickTransform.SetParent(this.parent);
        brickTransform.position = data.from;
        brickTransform.rotation = Quaternion.identity;
        brick.renderer.material.color = ColoredMaterials.Instance.GetColorByColorIndex(data.colorIndex);
        brick.gameObject.SetActive(true);
        brickTransform.DOMove(data.targetBrick.position, t).SetEase(Ease.Linear).OnComplete(() =>
        {
            ReleaseFlyingBrick(brick);
        });
    }

    public void Dispose()
    {
        foreach (var brick in this.createdBricks)
        {
            if (brick == null || brick.gameObject == null)
            {
                continue;
            }

            brick.gameObject.transform.DOKill();
        }

        this.flyingBrickCache.Clear();
        this.createdBricks.Clear();
    }

    private FlyingBrickCacheItem GetFlyingBrick()
    {
        while (this.flyingBrickCache.Count > 0)
        {
            var cachedBrick = this.flyingBrickCache.Dequeue();
            if (cachedBrick != null && cachedBrick.gameObject != null)
            {
                cachedBrick.gameObject.transform.DOKill();
                return cachedBrick;
            }
        }

        var go = Object.Instantiate(this.flyingBrickPrefab, this.parent);
        go.SetActive(false);

        var brick = new FlyingBrickCacheItem
        {
            gameObject = go,
            renderer = go.GetComponent<Renderer>()
        };
        this.createdBricks.Add(brick);
        return brick;
    }

    private void ReleaseFlyingBrick(FlyingBrickCacheItem brick)
    {
        if (brick == null || brick.gameObject == null)
        {
            return;
        }

        brick.gameObject.transform.DOKill();
        brick.gameObject.SetActive(false);
        brick.gameObject.transform.SetParent(this.parent);
        this.flyingBrickCache.Enqueue(brick);
    }
}
