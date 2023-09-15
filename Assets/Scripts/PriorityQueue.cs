using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T> where T : System.IComparable<T>
{
    public void Clear()
    {
        mA.Clear();
    }

    public bool Empty
    {
        get { return mA.Count == 0; }
    }

    public int Count
    {
        get { return mA.Count; }
    }

    public void Enqueue(T item)
    {
        int n = mA.Count; mA.Add(item);
        while (n != 0)
        {
            int p = n / 2;
            if (mA[n].CompareTo(mA[p]) >= 0) break;
            T tmp = mA[n]; mA[n] = mA[p]; mA[p] = tmp;
            n = p;
        }
    }

    public T Dequeue()
    {
        T val = mA[0];
        int nMax = mA.Count - 1;
        mA[0] = mA[nMax]; mA.RemoveAt(nMax);

        int p = 0;
        while (true)
        {
            int c = p * 2; if (c >= nMax) break;
            if (c + 1 < nMax && mA[c + 1].CompareTo(mA[c]) < 0) c++;

            if (mA[p].CompareTo(mA[c]) <= 0) break;

            T tmp = mA[p]; mA[p] = mA[c]; mA[c] = tmp;
            p = c;
        }
        return val;
    }


    List<T> mA = new List<T>();
}
