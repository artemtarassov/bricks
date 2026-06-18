using System.Collections;
using System.Collections.Generic;

public class RandHelper
{
    public enum RandPos
    {
        FirstThird,
        SecondThird,
        LastThird,
        FirstHalf,
        SecondHalf,
        Random
    }

    public static int GetRandIndex<T>(List<T> list, int minIndex, RandPos p)
    {
        if (list == null || list.Count == 0)
        {
            return 0;
        }

        int startIndex = System.Math.Max(0, System.Math.Min(minIndex, list.Count - 1));
        int len = list.Count - startIndex;

        if (len <= 1)
        {
            return startIndex;
        }

        switch (p)
        {
            case RandPos.FirstThird:
                return GetRandomIndexInRange(startIndex, startIndex, startIndex + len / 3, list.Count);
            case RandPos.SecondThird:
                return GetRandomIndexInRange(startIndex, startIndex + len / 3, startIndex + 2 * len / 3, list.Count);
            case RandPos.LastThird:
                return GetRandomIndexInRange(startIndex, startIndex + 2 * len / 3, list.Count, list.Count);
            case RandPos.FirstHalf:
                return GetRandomIndexInRange(startIndex, startIndex, startIndex + len / 2, list.Count);
            case RandPos.SecondHalf:
                return GetRandomIndexInRange(startIndex, startIndex + len / 2, list.Count, list.Count);
            case RandPos.Random:
                return GetRandomIndexInRange(startIndex, startIndex, list.Count, list.Count);
            default:
                return startIndex;
        }
    }

    private static int GetRandomIndexInRange(int fallbackIndex, int min, int max, int maxExclusive)
    {
        min = System.Math.Max(fallbackIndex, min);
        max = System.Math.Min(maxExclusive, max);

        if (min >= maxExclusive)
        {
            return maxExclusive - 1;
        }

        if (max <= min)
        {
            return min;
        }

        return GetRandomInt(min, max);
    }

    public static int GetRandomInt(int min, int max)
    {
        return new System.Random().Next(min, max);
    }

    public static T GetRandomElement<T>(IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new System.ArgumentException("List cannot be null or empty");
        }
        int index = GetRandomInt(0, list.Count);
        return list[index];
    }

    public static void Shuffle<T>(IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new System.ArgumentException("List cannot be null or empty");
        }
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = GetRandomInt(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

}
