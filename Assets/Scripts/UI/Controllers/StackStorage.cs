using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackStorage : MonoBehaviour
{
    public int[] items;
    public int[] amounts;

    public void SetAt(int idx, int item, int amount)
    {
        items[idx] = item;
        amounts[idx] = amount;
    }

    public int GetItemAt(int idx)
    {
        return items[idx];
    }

    public int GetAmountAt(int idx)
    {
        return amounts[idx];
    }
}
