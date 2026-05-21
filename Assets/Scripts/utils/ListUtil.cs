using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class ListUtil
{
    public static void Shuffle<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public static T Rand<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return default(T);
        }
        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }

    public static void Swap<T>(this List<T> list, int indexA, int indexB)
    {
        Assert.IsTrue(indexA >= 0 && indexA < list.Count, $"indexA {indexA} is out of range for list of size {list.Count}");
        Assert.IsTrue(indexB >= 0 && indexB < list.Count, $"indexB {indexB} is out of range for list of size {list.Count}");
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
    }
}
