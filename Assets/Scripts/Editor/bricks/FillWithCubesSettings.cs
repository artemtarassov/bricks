using UnityEngine;

internal sealed class FillWithCubesSettings
{
    public const string GeneratedGroupName = "__GeneratedBricks";
    public const string BrickPrefabPath = "Assets/Prefabs/Brick.prefab";
    public const string BrickMaterialPath = "Assets/Materials/BrickMat.mat";

    public float BrickSize = 0.3f;
    public float BrickGap = 0.01f;
    public bool IncludeInactiveObjects;
    public bool AddBrickColliders;

    public float SafeBrickSize => Mathf.Max(0.0001f, BrickSize);
    public float SafeBrickGap => Mathf.Max(0f, BrickGap);
}
