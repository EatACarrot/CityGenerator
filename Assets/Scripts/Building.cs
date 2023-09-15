using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building 
{
    int height;
    List<Vector3> boarders;
    Vector3 direction;
    List<Floor> floors;
    GameObject bld;
    Material buildingMat;
    

    public Building(List<Vector3> boarders, int height, Vector3 streetDirection, GameObject folder, Material materialB, GameObject[] prefabs)
    {
        this.height = height;
        this.direction = streetDirection;
        this.boarders = boarders;
        this.floors = new List<Floor>();
        this.bld = new GameObject("building");
        this.bld.transform.SetParent(folder.transform);
        buildingMat = materialB;
        for (int i = 0; i < height; i++)
        {
            floors.Add(new Floor(bld, boarders, i, buildingMat, prefabs));
        }
    }
}
