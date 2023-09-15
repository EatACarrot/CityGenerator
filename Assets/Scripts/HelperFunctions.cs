using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperFunctions 
{
    public static bool ConnectionGlobal(List<Vector3> points, List<List<int>> pointIs, List<int> array, int conn, int deep, int search, int prevConn = -1 , int indexOffset = 0, bool dir = true)
    {
        if (deep == 3)
        {
            array[0] = -1;
            return false;
        }
        else if (pointIs[conn].Count < 2)
        {
            array[0] = -1;
            return false;
        }
        else
        {

            if (deep > 1)
            {
                foreach (int connectedI in pointIs[conn])
                {
                    if (connectedI == search)
                    {
                        array.Add(conn);
                        //Debug.LogError("aaaaaa " + deep);
                        return true;
                    }
                }
            }
            Vector3 vec2 = points[conn + indexOffset] - (prevConn == -1 ?new Vector3(points[conn + indexOffset].x, points[conn + indexOffset].y, points[conn + indexOffset].z - 1f)  : points[prevConn + indexOffset]);
            float maxD = 0f;
            int maxDI = -1;
            float minD = float.PositiveInfinity;
            int minDI = -1;
            foreach (int connectedI in pointIs[conn])
            {
                //Debug.LogWarning("before " + "\n" + ArrayToString(array) + "\n" + " conn -" + conn + " deep - " + deep);

                if (prevConn != connectedI)
                {
                    Vector3 vec = points[connectedI + indexOffset] - points[conn + indexOffset];
                    float angle = Vector3.Angle(vec2, vec);
                    bool direction = isLeft(points[conn + indexOffset], (prevConn == -1 ? new Vector3(points[conn + indexOffset].x, points[conn + indexOffset].y, points[conn + indexOffset].z - 1f) : points[prevConn + indexOffset]), points[connectedI + indexOffset]);
                    if (((angle > maxD && !direction) && dir) || ((angle > maxD && direction) && !dir))
                    {
                        maxD = angle;
                        maxDI = connectedI;
                    }
                    /*if (((angle < minD && direction || angle < 1f) && dir) || ((angle < minD && !direction || angle < 1f) && !dir))
                    {
                        minD = angle;
                        minDI = connectedI;
                    }*/
                    //Debug.LogWarning("before " + "\n" + ArrayToString(array) + "\n" + " conn -" + conn + " deep - " + deep);
                }
            }
            array.Add(conn);
            if (maxDI > -1)
            {

                if (!ConnectionGlobal(points, pointIs, array, maxDI, deep + 1, search, conn, indexOffset))
                {
                    array.Remove(conn);
                    return false;
                }
                return true;
            }
            /*else if (minDI > -1)
            {
                if (!ConnectionGlobal(points, pointIs, array, minDI, deep + 1, search, conn, indexOffset))
                {
                    array.Remove(conn);
                    return false;
                }
                return true;
            }*/
            //Debug.LogWarning("after " + "\n" + ArrayToString(array) + "\n" + " conn -" + conn +" deep - " + deep);
        }
        array.Remove(conn);
        return false;
    }

    public static bool isLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        return (b.x - a.x) * (c.z - a.z) >= (b.z - a.z) * (c.x - a.x);
    }

    public static void UniqueArrayGlobal(List<int> array, ref List<List<int>> buildingIs)
    {
        if (buildingIs.Count == 0)
        {
            buildingIs.Add(array);
            return;
        }

        foreach (List<int> item in buildingIs)
        {
            int matches = 0;
            foreach (int i in array)
            {
                foreach (int node in item)
                {
                    if (i == node)
                    {
                        matches++;
                    }
                }
            }
            if (matches == array.Count)
            {
                //Debug.LogWarning(ArrayToString(item.nodes) + "- i " + ArrayToString(array));
                return;
            }
        }

        buildingIs.Add(array);
    }
}
