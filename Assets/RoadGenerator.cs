using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public int nodeN = 10;
    public Vector2 area;

    List<Vector3> highNodeLoc;
    List<List<int>> highNodeGraph;

    
    // Start is called before the first frame update
    void Start()
    {
        highNodeLoc = new List<Vector3>();
        highNodeGraph = new List<List<int>>();
        highNodeGraph.Add(new List<int>());
        for (int i = 0; i < nodeN; i++)
        {
            highNodeGraph[0].Add(i);
            highNodeLoc.Add(new Vector3(
                    Random.Range(0f,area.x),
                    0f,
                    Random.Range(0f,area.y)
                ));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
