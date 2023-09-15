using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall
{
    private GameObject[] prefabs;
    Vector3 point1;
    Vector3 point2;
    float placeMentInterval = 0.2f;
    GameObject wallobject;
    int level;

    public Wall(Vector3 point1, Vector3 point2, float pInterval, GameObject folder, GameObject[] prefabs, int level)
    {
        this.point1 = point1;
        this.point2 = point2;
        this.prefabs = prefabs;
        this.placeMentInterval = pInterval;
        wallobject = new GameObject("wall");
        wallobject.transform.SetParent(folder.transform);
        this.prefabs = prefabs;
        this.level = level;
        Generate();
    }

    private void Generate()
    {
       
        Vector3 dir = Vector3.Cross((point2 - point1), Vector3.up);
        float angle = Vector3.Angle(dir, Vector3.right) * (HelperFunctions.isLeft(Vector3.zero, Vector3.right, dir)? 1 :-1 );
        Vector3 Direction = new Vector3(0f, -angle, 0f);
       
        Ledge(Direction);
        Details(Direction);

    }

    private void Details(Vector3 Direction)
    {
        float size = (point2 - point1).magnitude;
        int repetitions = (int)(size / 0.7f);
        for (int i = 1; i < repetitions; i++)
        {
            int option = (int)UnityEngine.Random.Range(1, 3);
            if (option == 3)
                continue;
            if (level > 0)
                option = 2;
            Vector3 location = point1 + (point2 - point1).normalized * 0.7f * i;// new Vector3(point1.x, point1.y + 0.5f, point1.z);
            location.y = level;
            location.y += (option == 2? 0.7f : 0.4f);
            GameObject stuff = UnityEngine.MonoBehaviour.Instantiate(prefabs[option], location, Quaternion.Euler(Direction));
            stuff.transform.SetParent(wallobject.transform);
        }
    }

    private void Ledge(Vector3 Direction)
    {
        Vector3 middle = (point2 + point1) * 0.5f;
        middle.y = level;
        float size = (point2 - point1).magnitude;
        GameObject stuff = UnityEngine.MonoBehaviour.Instantiate(prefabs[0], new Vector3(middle.x, (middle.y + 1) * 1.1f, middle.z), Quaternion.Euler(Direction));
        stuff.transform.localScale = new Vector3(stuff.transform.localScale.x, stuff.transform.localScale.y, stuff.transform.localScale.z * size);
        stuff.transform.SetParent(wallobject.transform);
    }
}
