using System.Collections;
using System.Collections.Generic;

public class RandHelper
{
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