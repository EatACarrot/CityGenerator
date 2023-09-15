using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRenderer : MonoBehaviour
{
    List<Floor> f;
    public GameObject[] prefabs;
    public Material mat;
    public int floors;
    public void Start()
    {

        f = new List<Floor>();
        for(int i = 0; i < floors; i++)
        {
            f.Add(new Floor(this.gameObject, new List<Vector3>()
            {
                new Vector3(-1, 0 -1),
                new Vector3(1, 0 -1),
                new Vector3(2, 0 ,1),
                new Vector3(0.5f, 0 ,3),
                new Vector3(-1, 0 ,2),
            }, i, mat, prefabs));
        }
        
    }


}
