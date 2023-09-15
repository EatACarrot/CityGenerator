using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct segment : System.IComparable<segment>
{

    public Vector3 pos { get; set; }
    public List<int> connections { get; set; }
    public Vector3 dir { get; set; }
    public bool highway { get; set; }
    public int timestep { get; set; }

    public int CompareTo(segment otherSegment)
    {
        if (otherSegment.GetType() != typeof(segment))
            throw new System.ArgumentException("Object is not a segment");

        if (otherSegment.timestep >= 0)
            return this.timestep.CompareTo(otherSegment.timestep);
        else
            return 1;
    }
}

public class LSystem : MonoBehaviour
{
    private PriorityQueue<segment> queue;
    public List<segment> segments;
    [SerializeField]
    private Texture2D WaterBoarder;
    private Texture2D PopulationDensity;
    [Header("Generation controlls")]
    public float sizeOfCity = 100;
    public int iterations = 10;
    public Vector3 cityCenter;
    public int theSeed = 12;
    [Header("Global goals controlls")]
    public float coneLength = 3;
    public float coneAngle = 10;
    public int sampleSize = 3;
    public int maxSplits = 3;
    public float splitColour = 0.8f;
    public float maxSplitDiviation = 6f;
    public float chanceToSplit = 50;
    public float chanceToStreet = 50;
    public int waitForStreets = 5;
    [Header("Local goals controlls")]
    public float CCRadius = 10f;
    public GameObject dot;
    [Header("Draw controlls")]
    public Material lineMaterial;
    public bool StreetsGenerated = false;

    private int itterationCount;



    // Update is called once per frame
    public void Generate(Texture2D PopulationDensity)
    {
        this.PopulationDensity = PopulationDensity;
        segment startingPoint = new segment() { 
            pos = new Vector3(
                Random.Range(0f, sizeOfCity),
                0f,
                Random.Range(0f, sizeOfCity)
                ), 
            connections = new List<int>(), 
            highway = true ,
            timestep = 0, 
            dir = new Vector3(
                1 - (theSeed % 10 + 1) / 10,
                0f,
                (theSeed % 10 + 1) / 10)
            };
        segments = new List<segment>();
        queue = new PriorityQueue<segment>();
        queue.Enqueue(startingPoint);
        itterationCount = 1;

        NothingsAlgorithm(iterations);
        ConnectEverything();

        StreetsGenerated = true;
        foreach(segment point in segments)
        {
            foreach(int i in point.connections)
            {
                DrawALine(point.pos, segments[i].pos, (segments[i].highway ? Color.black : Color.white) , (segments[i].highway && point.highway ? 0.5f : 0.2f));
            }
        }
    }

    // i current 
    // segments[i].connections[j] is the one with needs to know
    // segments[segments[i].connections[j]].connections[k]
    private void ConnectEverything()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments[i].connections.Count; j++)
            {
                
                bool alreadyThere = false;
                for (int k = 0; k < segments[  segments[i].connections[j]  ].connections.Count; k++)
                {
                    if(segments[segments[i].connections[j]].connections[k] == i)
                    {
                        
                        alreadyThere = true;
                    }
                }
                if (!alreadyThere)
                {
                    segments[segments[i].connections[j]].connections.Add(i);
                }
               
            }
        }
    }

    public List<segment> GetMap()
    {
        if (StreetsGenerated)
            return segments;
        else
            return null;
    }
    public float GetSize()
    {
        return sizeOfCity;
    }

    private void NothingsAlgorithm(int iterations)
    {
        
        while (queue.Count > 0 && itterationCount < iterations)
        {
            segment thePop = queue.Dequeue();
            if (LocalConstraints(ref thePop))
            {
                fixConnections(thePop);
                segments.Add(thePop);
                foreach (segment point in GlobalGoals(thePop, segments.Count - 1))
                {
                    queue.Enqueue(point);
                }
            }
            itterationCount++;
        }
    }

    private void fixConnections(segment seg)
    {
        foreach (int i in seg.connections)
        {
            segments[i].connections.Add(segments.Count);
        }
    }

    private bool LocalConstraints(ref segment seg)
    {
        if (WaterCheck(ref seg))
        {
            Debug.LogWarning("deleted water");
            return false;
        }
        if (OutOfCityBoarders(seg.pos))
        {
            Debug.LogWarning("deleted out of borders");
            return false;
        }
        if (!seg.highway && PopulationDensity.GetPixelBilinear(seg.pos.x / sizeOfCity, seg.pos.z / sizeOfCity).grayscale > splitColour)
        {
            Debug.LogWarning("deleted too little people");
            return false;
        }

        if (seg.connections.Count > 0 && CheckForCrossings(ref seg))
            return false;
        
        return true;
    }

    private bool OutOfCityBoarders(Vector3 vec)
    {
        return (vec.x > sizeOfCity || vec.x < 0 || vec.z > sizeOfCity || vec.z < 0);
    }

    private bool WaterCheck(ref segment seg)
    {
        if (WaterBoarder.GetPixelBilinear(seg.pos.x / sizeOfCity, seg.pos.z / sizeOfCity).grayscale < 0.8f)
            return false;
        float sonarRange = 0.2f;
        float sonarAngle = 0f;
        while (true)
        {
            if (sonarAngle > 180f || sonarRange > coneLength) return true;
            Vector3 left = seg.pos + (Quaternion.AngleAxis(-sonarAngle, Vector3.up) * Vector3.left) * sonarRange;
            Vector3 right = seg.pos + (Quaternion.AngleAxis(sonarAngle, Vector3.up) * Vector3.left) * sonarRange;
            if(WaterBoarder.GetPixelBilinear(left.x / sizeOfCity, left.z / sizeOfCity).grayscale < 0.8f 
                && !OutOfCityBoarders(left) 
                && (seg.connections.Count > 0 ? (left - segments[seg.connections[0]].pos).magnitude : float.PositiveInfinity) > CCRadius * 2.5f)
            {
                seg.pos = left;
                return false;
            }
            if (WaterBoarder.GetPixelBilinear(right.x / sizeOfCity, right.z / sizeOfCity).grayscale < 0.8f 
                && !OutOfCityBoarders(right) 
                && (seg.connections.Count > 0 ? (right - segments[seg.connections[0]].pos).magnitude : float.PositiveInfinity) > CCRadius * 2.5f)
            {
                seg.pos = right;
                return false;
            }
            sonarAngle += 45f;
            sonarRange *= 2;
        }
    }

    private bool CheckForCrossings(ref segment seg)
    {
        float bestInt = float.PositiveInfinity;
        Vector3 bestCord = seg.pos;
        int a = -1, b = -1;
        

        for (int i = 1; i < segments.Count; i++)
        {

            foreach (int conectedI in segments[i].connections)
            {

                int sweepIterations = 120 / 35;
                float sweepAngle = 0;
                Vector3 intersection;
                Vector3 tryDiff = segments[i].pos - segments[conectedI].pos;

                for (int ex = 0; ex < sweepIterations; ex++)
                {
                    for (int extraCheck = 1; extraCheck <= 2; extraCheck++)
                    {
                        Vector3 halfBestCoord = segments[seg.connections[0]].pos + (seg.pos - segments[seg.connections[0]].pos).normalized * ((bestCord - segments[seg.connections[0]].pos).magnitude / extraCheck);
                        Vector3 left = halfBestCoord + (Quaternion.AngleAxis(-sweepAngle, Vector3.up) * (seg.pos - segments[seg.connections[0]].pos).normalized) * CCRadius * (2 / (extraCheck));
                        Vector3 right = halfBestCoord + (Quaternion.AngleAxis(sweepAngle, Vector3.up) * (seg.pos - segments[seg.connections[0]].pos).normalized) * CCRadius * (2 / (extraCheck));
                        Vector3 leftDiff = left - halfBestCoord;
                        Vector3 rightDiff = right - halfBestCoord;
                        #region written out intersection
                        if (LineLineIntersection(out intersection, halfBestCoord, leftDiff, segments[conectedI].pos, tryDiff))
                        {
                            float leftSqrMagnitude = leftDiff.sqrMagnitude;
                            float trySqrMagnitude = tryDiff.sqrMagnitude;

                            if (
                                (intersection - halfBestCoord).sqrMagnitude <= leftSqrMagnitude
                                && (intersection - left).sqrMagnitude <= leftSqrMagnitude
                                && (intersection - segments[conectedI].pos).sqrMagnitude <= trySqrMagnitude
                                && (intersection - segments[i].pos).sqrMagnitude <= trySqrMagnitude)
                            {

                                if (conectedI == seg.connections[0] || i == seg.connections[0]) return true;
                                else if(extraCheck > 1)
                                {
                                    return true;
                                }

                                if ((intersection - segments[seg.connections[0]].pos).magnitude < bestInt)
                                {
                                    bestInt = (intersection - segments[seg.connections[0]].pos).magnitude;
                                    bestCord = intersection;
                                    //Instantiate(dot, intersection, transform.rotation);
                                    a = conectedI;
                                    b = i;
                                }

                            }

                        }
                        if (LineLineIntersection(out intersection, halfBestCoord, rightDiff, segments[conectedI].pos, tryDiff))
                        {
                            float rightSqrMagnitude = rightDiff.sqrMagnitude;
                            float trySqrMagnitude = tryDiff.sqrMagnitude;

                            if (
                                (intersection - halfBestCoord).sqrMagnitude <= rightSqrMagnitude
                                && (intersection - right).sqrMagnitude <= rightSqrMagnitude
                                && (intersection - segments[conectedI].pos).sqrMagnitude <= trySqrMagnitude
                                && (intersection - segments[i].pos).sqrMagnitude <= trySqrMagnitude)
                            {

                                if (conectedI == seg.connections[0] || i == seg.connections[0]) return true;
                                else if (extraCheck > 1)
                                {
                                    return true;
                                }

                                if ((intersection - segments[seg.connections[0]].pos).magnitude < bestInt)
                                {
                                    bestInt = (intersection - segments[seg.connections[0]].pos).magnitude;
                                    bestCord = intersection;
                                    //Instantiate(dot, intersection, transform.rotation);
                                    a = conectedI;
                                    b = i;
                                }

                            }
                        }
                        #endregion
                        sweepAngle += 35;
                    }
                }
                if (i != seg.connections[0] && conectedI != seg.connections[0])
                {
                    Vector3 intersectionO;
                    Vector3 tryDiffO = segments[i].pos - segments[conectedI].pos;
                    Vector3 segDiff = bestCord - segments[seg.connections[0]].pos;
                    #region intersection
                    if (LineLineIntersection(out intersectionO, segments[seg.connections[0]].pos, segDiff, segments[conectedI].pos, tryDiffO))
                    {
                        float segSqrMagnitude = segDiff.sqrMagnitude;
                        float trySqrMagnitude = tryDiffO.sqrMagnitude;

                        if (
                            (intersectionO - segments[seg.connections[0]].pos).sqrMagnitude <= segSqrMagnitude
                            && (intersectionO - bestCord).sqrMagnitude <= segSqrMagnitude
                            && (intersectionO - segments[conectedI].pos).sqrMagnitude <= trySqrMagnitude
                            && (intersectionO - segments[i].pos).sqrMagnitude <= trySqrMagnitude)
                        {
                            if ((intersectionO - segments[seg.connections[0]].pos).magnitude < bestInt)
                            {
                                bestInt = (intersectionO - segments[seg.connections[0]].pos).magnitude;
                                bestCord = intersectionO;
                                //Instantiate(dot, intersectionO, transform.rotation);
                                a = conectedI;
                                b = i;
                            }
                        }
                    }
                    #endregion

                }
            }
        }
        seg.pos = bestCord;


        if (bestInt != float.PositiveInfinity)
        {
            if (CheckForIntersections(seg, a, b))
            {
                return true;
            }
            for (int i = 0; i < segments[a].connections.Count; i++)
            {
                if (segments[a].connections[i] == b)
                {
                    segments[a].connections.RemoveAt(i);
                }
            }
            for (int i = 0; i < segments[b].connections.Count; i++)
            {
                if (segments[b].connections[i] == a)
                {
                    segments[b].connections.RemoveAt(i);
                }
            }
            if(segments[a].highway && segments[b].highway)
            {
                seg.highway = true;
            }
            segments[a].connections.Add(segments.Count);
            segments[b].connections.Add(segments.Count);
            seg.connections.Add(a);
            seg.connections.Add(b);
            if (seg.highway)
            {
                return false;
            }
            segments.Add(seg);
            return true;
        }

        return false;

    }

    private bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    private bool CheckForIntersections(segment seg, int a, int b)
    {
        for(int i = 0; i < segments[a].connections.Count; i++)
        {
            if (seg.connections[0] == segments[a].connections[i] || seg.connections[0] == segments[segments[a].connections[i]].connections[0])
            {
                return true;
            }
        }
        for (int i = 0; i < segments[b].connections.Count; i++)
        {
            if (seg.connections[0] == segments[b].connections[i] || seg.connections[0] == segments[segments[b].connections[i]].connections[0])
            {
                return true;
            }
        }

        if ((segments[a].pos - seg.pos).magnitude < CCRadius/5 && seg.connections[0] != segments[a].connections[0] && seg.connections[0] != a)
        {
            segments[seg.connections[0]].connections.Add(a);
            segments[a].connections.Add(seg.connections[0]);
            return true;
        }
        else if((segments[b].pos - seg.pos).magnitude < CCRadius/5 && seg.connections[0] != segments[b].connections[0] && seg.connections[0] != b)
        {
            segments[seg.connections[0]].connections.Add(b);
            segments[b].connections.Add(seg.connections[0]);
            return true;
        }


        return false;
    }

    private IEnumerable<segment> GlobalGoals(segment thePop, int thePopIndex)
    {
        List<segment> ret = new List<segment>();
        bool reachedCenter = PopulationDensity.GetPixelBilinear(thePop.pos.x / sizeOfCity, thePop.pos.z / sizeOfCity).grayscale <= splitColour ? true : false;
        Vector3 currentPosition = thePop.pos;
        Vector3 direction = thePop.dir;
        

        Vector3 bestCord = currentPosition + Quaternion.AngleAxis(Random.Range(-6f, 6f), Vector3.up) * direction * coneLength;
        if (thePop.highway)
        {
            float bestScore = PopulationDensity.GetPixelBilinear(bestCord.x / sizeOfCity, bestCord.z / sizeOfCity).grayscale;
            for (int i = 1; i < sampleSize; i++)
            {
                Vector3 left = currentPosition + (Quaternion.AngleAxis(-coneAngle * i, Vector3.up) * direction) * coneLength;
                Vector3 right = currentPosition + (Quaternion.AngleAxis(coneAngle * i, Vector3.up) * direction) * coneLength;
                if (PopulationDensity.GetPixelBilinear(left.x / sizeOfCity, left.z / sizeOfCity).grayscale > bestScore)
                {
                    bestCord = left;
                    bestScore = PopulationDensity.GetPixelBilinear(left.x / sizeOfCity, left.z / sizeOfCity).grayscale * coneLength;
                }
                if (PopulationDensity.GetPixelBilinear(right.x / sizeOfCity, right.z / sizeOfCity).grayscale > bestScore)
                {
                    bestCord = right;
                    bestScore = PopulationDensity.GetPixelBilinear(right.x / sizeOfCity, right.z / sizeOfCity).grayscale;
                }

            }
        }
        ret.Add(new segment() { dir = Vector3.Normalize(bestCord - currentPosition), highway = thePop.highway, connections = new List<int>() { thePopIndex }, pos = bestCord , timestep = itterationCount + (thePop.highway ? 0 : waitForStreets) });

        if (reachedCenter)
        {
            
            if (thePop.highway && Random.Range(0f, 100f) < chanceToSplit)
            {
                int side = Random.Range(0f, 2f) > 1 ? -1 : 1;
                for (int i = 0; i < (int)Random.Range(0f, maxSplits); i++)
                {
                    Vector3 prevDirection = (Quaternion.AngleAxis(90 * side + (180 / (maxSplits - 1) * i) + Random.Range(0f, maxSplitDiviation), Vector3.up) * Vector3.Normalize(bestCord - currentPosition));
                    Vector3 previous = thePop.pos + prevDirection * coneLength;
                    ret.Add(new segment()
                    {
                        dir = prevDirection,
                        highway = thePop.highway,
                        connections = new List<int>() { thePopIndex },
                        pos = previous,
                        timestep = itterationCount
                    });
                }
            }
            else if (Random.Range(0f, 100f) <= chanceToStreet + Mathf.Clamp((1 - PopulationDensity.GetPixelBilinear(thePop.pos.x / sizeOfCity, thePop.pos.z / sizeOfCity).grayscale) * splitColour * 10, 1, 100 - chanceToStreet))
            {
                int side = Random.Range(0f, 2f) > 1 ? -1 : 1;
                for (int i = 0; i < (int)Random.Range(1f, maxSplits+0.4f); i++)
                {
               
                    
                    Vector3 prevDirection = (Quaternion.AngleAxis(90 * side + (180 / (maxSplits - 1) * i) + Random.Range(0f, maxSplitDiviation), Vector3.up) * Vector3.Normalize(bestCord - currentPosition));
                    Vector3 previous = thePop.pos + prevDirection * coneLength;
                    ret.Add(new segment()
                    {
                        dir = prevDirection,
                        highway = false,
                        connections = new List<int>() { thePopIndex },
                        pos = previous,
                        timestep = itterationCount + waitForStreets
                    });
                }
            }
        }
        return ret;
    }

    private void DrawALine(Vector3 start, Vector3 end, Color color, float sizeOfRoad)
    {
        GameObject line = new GameObject("line");
        line.transform.position = start;
        var lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = sizeOfRoad;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
