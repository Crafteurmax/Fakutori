using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LimitedSizeStack<T>
{
    private LinkedList<T> items = new LinkedList<T>();
    public List<T> Items => items.ToList();
    public int Capacity { get; }
    public LimitedSizeStack(int capacity)
    {
        Capacity = capacity;
    }

    public void Push(T item)
    {
        // if stack is full remove the bottom of the pile (reverse beacause LinkedList)
        if (items.Count == Capacity)
        {
            items.RemoveFirst();
            items.AddLast(item);
        }
        else
        {
            items.AddLast(new LinkedListNode<T>(item));
        }
    }

    public T Pop()
    {
        if (items.Count == 0)
        {
            //in history I use a struct so the default value returned is a struct with default value member
            return default;
        }
        var ls = items.Last;
        items.RemoveLast();
        return ls == null ? default : ls.Value;
    }

    public void Clear()
    {
        items.Clear();
    }

    public int Count()
    {
        return items.Count;
    }
}
