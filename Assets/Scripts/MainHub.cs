using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainHub : MonoBehaviour
{
    public LSystem mapGen;
    public BuildingGen buildGen;
    public List<segment> segments;
    [SerializeField]
    private Texture2D PopulationDensity;
    void Start()
    {
        mapGen.Generate(PopulationDensity);

       
        if (mapGen.StreetsGenerated)
        {
            buildGen.Generate(mapGen.GetMap(), PopulationDensity, mapGen.GetSize());
            buildGen.DrawBuildings();
        }
       
    }
}
