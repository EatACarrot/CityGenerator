using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Block
{
    public List<int> nodes;
    public List<Vector3> cordinates;
    public float sizeOFpolygon;

    public override string ToString()
    {
        string s = "";
        for (int i = 0; i < cordinates.Count; i++)
        {
            s += " {" + cordinates[i].x + " ," + cordinates[i].y + " ," + cordinates[i].z + "}" + "\n";
        }
        return s;
    }
}


public class BuildingGen : MonoBehaviour
{
    public GameObject[] prefabs;
    private List<Block> blocks;
    private List<segment> segments;
    [Header("Building Controlls")]
    public int longestAcceptableCycle = 4;
    public float MinimalSizeForLots;
    public float sideWalkSize = 0.4f;
    public Material lineMaterialBlock;
    public Material lineMaterialBuilding;
    public Material BuilingMaterial;
    public GameObject cube;
    private Texture2D PopulationDensity;
    private float sizeOfCity;

    //demo
    List<Building> buildingsMeshes;
    GameObject buildingSum;


    public void Generate(List<segment> segs, Texture2D PopulationDensity, float sizeOfCity)
    {
        segments = segs;
        this.PopulationDensity = PopulationDensity;
        this.sizeOfCity = sizeOfCity;
        if (segments.Count > 0)
        {
            blocks = FindBlocks();
            
            BlockFiltering();
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i] = insetPolygon(blocks[i], sideWalkSize);
            }
            BlockFilteringNr2();
        }

    }

    private void BlockFilteringNr2()
    {
        List<int> rem = new List<int>();

        for (int i = 0; i < blocks.Count; i++)
        {
            //Debug.Log(blocks[i].sizeOFpolygon + "  < " + AreaOfPoligon(blocks[i].cordinates));
            if (blocks[i].sizeOFpolygon < AreaOfPoligon(blocks[i].cordinates))
            {
                rem.Add(i);
            }
        }
        for (int i = rem.Count - 1; i >= 0; i--)
        {
            blocks.RemoveAt(rem[i]);
        }
    }

    private Block insetPolygon(Block b, float insetDistance)
    {
        Block copyB = new Block();
        copyB.cordinates = new List<Vector3>();
        copyB.nodes = new List<int>();
        copyB.sizeOFpolygon = b.sizeOFpolygon;
        foreach(int i in b.nodes)
        {
            copyB.nodes.Add(i);
        }
        for (int i = 0; i < b.cordinates.Count; i++)
        {
            //Debug.LogError(i + " - i, COOr - " + b.cordinates[i].x + ", " + b.cordinates[i].y + "," + b.cordinates[i].z);
            Vector3 v;
            if (insetCorner(
                b.cordinates[(i - 1 < 0 ? b.cordinates.Count - 1 : i - 1)],
                b.cordinates[i],
                b.cordinates[(i + 1 > b.cordinates.Count - 1 ? 0 : i + 1)],
                out v,
                insetDistance)
            )
            {
                copyB.cordinates.Add(v);
            }
            
            //Debug.LogError(i + " - i, COOr - " + v.x + ", " + v.y + "," + v.z);
        }
        //Debug.Log(b.ToString());
        //Debug.Log(copyB.ToString());
        return copyB;
    }

    private bool insetCorner(
        Vector3 prev,//prev point x and z
        Vector3 current,//current point x and z
        Vector3 next,//next point x and z
        out Vector3 v,// storage for changed point
        float insetDistance)
    {
        Vector3 dir1 = Vector3.Cross((current - prev).normalized, Vector3.up);
        if (Vector3.Angle((current - prev), (next - current)) < 5 && ((current - prev).magnitude < 1 || (next - current).magnitude < 1)){
            v = Vector3.zero ;
            return false;
        }
        Vector3 dir2 = Vector3.Cross((next - current).normalized, Vector3.up);
        Vector3 prev1 = prev + dir1 * insetDistance;
        Vector3 current1 = current + dir1 * insetDistance;
        Vector3 current2 = current + dir2 * insetDistance;
        Vector3 next2 = next + dir2 * insetDistance;
        if (Vector3.Angle((current - prev), (next - current)) > 150 && (prev1 - next2).magnitude < 0.05 * Vector3.Angle((current - prev), (next - current)))
        {
            v = Vector3.zero;
            return false;
        }

            v = lineIntersection(prev1, current1, current2, next2) ;
        return true;
    }

    private Vector3 lineIntersection(
        Vector3 line1Point1,
        Vector3 line1Point2,
        Vector3 line2Point1,
        Vector3 line2Point2
        )
    {
        line1Point2 -= line1Point1;
        line2Point1 -= line1Point1;
        line2Point2 -= line1Point1;

        float distAB = Mathf.Sqrt(line1Point2.x * line1Point2.x + line1Point2.z * line1Point2.z);

        float theCos = line1Point2.x / distAB;
        float theSin = line1Point2.z / distAB;
        float newX = line2Point1.x * theCos + line2Point1.z * theSin;
        line2Point1.z = line2Point1.z * theCos - line2Point1.x * theSin; line2Point1.x = newX;
        newX = line2Point2.x * theCos + line2Point2.z * theSin;
        line2Point2.z = line2Point2.z * theCos - line2Point2.x * theSin; line2Point2.x = newX;

        float lineCrossX = line2Point2.x + (line2Point1.x - line2Point2.x) * line2Point2.z / (line2Point2.z - line2Point1.z);

        Vector3 v = new Vector3(line1Point1.x + lineCrossX * theCos, 0f, line1Point1.z + lineCrossX * theSin);
       
        return v;
    }


    private void BlockFiltering()
    {
        List<int> rem = new List<int>();
        
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].sizeOFpolygon < MinimalSizeForLots)
            {
                rem.Add(i);
            }
        }
        for (int i = rem.Count-1; i >= 0; i--)
        {
            blocks.RemoveAt(rem[i]);
        }
    }

    private float AreaOfPoligon(List<Vector3> vertices)
    {
        float temp = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            if(i != vertices.Count - 1)
            {
                float mulA = vertices[i].x * vertices[i+1].z;
                float mulB = vertices[i + 1].x * vertices[i].z;
                temp = temp + (mulA - mulB);
            }
            else
            {
                float mulA = vertices[i].x * vertices[0].z;
                float mulB = vertices[0].x * vertices[i].z;
                temp = temp + (mulA - mulB);
            }
        }
        temp *= 0.5f;
        return Mathf.Abs(temp);
    }
         
    private Vector3 moveToPoint(Vector3 point, Vector3 theMoving)
    {
        return theMoving + (point - theMoving).normalized * 0.2f;
    }

    public void DrawBuildings()
    {
        TicTacToe(blocks);
    }

    private List<Block> FindBlocks()
    {
        List<Block> possibleBlocks = new List<Block>();
        for (int i = 1; i < segments.Count; i++)
        {
            if (segments[i].connections.Count > 1)
            {
                foreach (int item in segments[i].connections)
                {
                    
                    List<int> array = new List<int>();
                    array.Add(i);
                    if (segments[item].connections.Count < 1) continue;
                    ConnectionCheck(array, item, 1, i, i);
                    if (array[0] != -1 && array.Count > 2)
                    {
                        uniqueArray(ref possibleBlocks, array);
                    }
                    
                }
            }
        }
        return possibleBlocks;
    }

    private string ArrayToString(IEnumerable<int> array)
    {
        string s = "[";
        foreach (int item in array)
        {
            s += item.ToString();
            s += ", ";
        }
        s += "]";
        return s;
    }

    private void uniqueArray(ref List<Block> list, List<int> array)
    {
        Block b = new Block();
        if (list.Count == 0)
        {
            b.nodes = array;
            list.Add(b);
            return;
        }
        foreach (Block item in list)
        {
            int matches = 0;
            foreach (int i in array)
            {
                foreach (int node in item.nodes)
                {
                    if (i == node)
                    {
                        matches++;
                    }
                }
            }
            if (matches == array.Count)
            {
                return;
            }
        }
        b.nodes = array;
        b.cordinates = new List<Vector3>();
        foreach (int item in b.nodes)
        {
            b.cordinates.Add(segments[item].pos);
        }
        b.sizeOFpolygon = AreaOfPoligon(b.cordinates);
        list.Add(b);
    }

    private bool ConnectionCheck(List<int> array, int conn, int deep, int search, int prevConn)
    {
        if (deep == longestAcceptableCycle)
        {
            array[0] = -1;
            return false;
        }
        else if(segments[conn].connections.Count < 2)
        {
            array[0] = -1;
            return false;
        }
        else
        {

            if (deep != 1) {
                foreach (int connectedI in segments[conn].connections)
                {
                    if (connectedI == search)
                    {
                        array.Add(conn);
                        return true;
                    }
                }
            }
            Vector3 vec2 = segments[conn].pos - segments[prevConn].pos;
            float maxD = 0f;
            int maxDI = -1;
            float minD = float.PositiveInfinity;
            int minDI = -1;
            foreach (int connectedI in segments[conn].connections)
            {

                if (prevConn != connectedI)
                {
                    Vector3 vec = segments[connectedI].pos - segments[conn].pos;
                    float angle = Vector3.Angle(vec2, vec);
                    bool direction = isLeft(segments[conn].pos, segments[prevConn].pos, segments[connectedI].pos);
                    if (angle > maxD && !direction)
                    {
                        maxD = angle;
                        maxDI = connectedI;
                    }
                    if(angle < minD && direction || angle < 1f)
                    {
                        minD = angle;
                        minDI = connectedI;
                    }
                }
            }
            array.Add(conn);
            if (maxDI > -1)
            {
                
                if (!ConnectionCheck(array, maxDI, deep + 1, search, conn))
                {
                    array.Remove(conn);
                    return false;
                }
                return true;
            }
            else if(minDI > -1)
            {
 
                if (!ConnectionCheck(array, minDI, deep + 1, search, conn))
                {
                    array.Remove(conn);
                    return false;
                }
                return true;
            }
        }
        array.Remove(conn);
        return false;
    }

    public bool isLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        return (b.x - a.x) * (c.z - a.z) >= (b.z - a.z) * (c.x -a.x);
    }

    private void DrawALine(Vector3 start, Vector3 end, Color color, float sizeOfRoad, Material lineMaterial)
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

    private void TicTacToe(List<Block> list)
    {
        float houseDim = 2f;
        List<List<Vector3>> Buildings = new List<List<Vector3>>();
        foreach(Block b in list)
        {
            List<Vector3> points = new List<Vector3>();
            List<List<int>> pointIs = new List<List<int>>();
            List<List<int>> buildingIs = new List<List<int>>();
            List<int> outerCircle = new List<int>();
            Vector3 top = Vector3.zero, bottom = Vector3.zero;
            Vector3[] slices = new Vector3[2];
            slices = InnitialSetUp(b.cordinates, out top.x, out bottom.x, out top.z, out bottom.z, ref points, ref pointIs);
            
            
            for(int i = 0; i< pointIs.Count; i++)
            {
                outerCircle.Add(i);
            }
            
            Slice(ref points, ref pointIs, ref outerCircle, top, bottom, slices, houseDim);

            
            
            
            LotConnectionChecck(ref buildingIs, points, pointIs);


            foreach (List<int> building in buildingIs)
            {
                List<Vector3> buildingV = new List<Vector3>();
                foreach (int vertex in building)
                {
                    buildingV.Add(points[vertex]);
                }
                if (AreaOfPoligon(buildingV) < 0.5f)
                {
                    foreach (int vertex in building)
                    {
                        outerCircle.Add(vertex);
                    }
                    continue;
                }
                foreach (int vertex in building)
                {
                    if (pointIs[vertex].Count <= 3)
                    {
                        Buildings.Add(buildingV);
                        break;
                    }
                }
            }
        }
        
        //demo
        buildingsMeshes = new List<Building>();
        buildingSum = new GameObject("city");
        for(int i = 0;  i < Buildings.Count; i++)
        {
            
                Building build = new Building(Buildings[i], (int)((0.5f - PopulationDensity.GetPixelBilinear(Buildings[i][0].x / sizeOfCity, Buildings[i][0].z / sizeOfCity).grayscale) * 10), Vector3.one, buildingSum, BuilingMaterial, prefabs);
                buildingsMeshes.Add(build);

        }
    }

    private void LotConnectionChecck(ref List<List<int>> buildingIs, List<Vector3> points, List<List<int>> pointIs)
    {
        
        for (int i = 1; i < points.Count; i++)
        {
            if (pointIs[i].Count > 1)
            {
                foreach (int item in pointIs[i])
                {

                    List<int> array = new List<int>();
                    array.Add(i);
                    if (pointIs[item].Count < 1) continue;
                    Connection(points,pointIs,array, item, 1, i, i);
                    if (array[0] != -1 && array.Count > 2)
                    {
                        uniqueBuilding(array, ref buildingIs);
                    }

                }
            }
        }
        
    }

    private void uniqueBuilding(List<int> array,ref List<List<int>> buildingIs)
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
                return;
            }
        }

        buildingIs.Add(array);
    }

    private bool Connection(List<Vector3> points, List<List<int>> pointIs, List<int> array, int conn, int deep, int search, int prevConn)
    {
        if (deep == 6)
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

            if (deep != 1)
            {
                foreach (int connectedI in pointIs[conn])
                {
                    if (connectedI == search)
                    {
                        array.Add(conn);
                        return true;
                    }
                }
            }
            Vector3 vec2 = points[conn] - points[prevConn];
            float maxD = 0f;
            int maxDI = -1;
            float minD = float.PositiveInfinity;
            int minDI = -1;
            foreach (int connectedI in pointIs[conn])
            {

                if (prevConn != connectedI)
                {
                    Vector3 vec = points[connectedI] - points[conn];
                    float angle = Vector3.Angle(vec2, vec);
                    bool direction = isLeft(points[conn], points[prevConn], points[connectedI]);
                    if (angle > maxD && !direction)
                    {
                        maxD = angle;
                        maxDI = connectedI;
                    }
                    if (angle < minD && direction || angle < 1f)
                    {
                        minD = angle;
                        minDI = connectedI;
                    }
                }
            }
            array.Add(conn);
            if (maxDI > -1)
            {

                if (!Connection(points, pointIs, array, maxDI, deep + 1, search, conn))
                {
                    array.Remove(conn);
                    return false;
                }
                return true;
            }
            else if (minDI > -1)
            {
                if (!Connection(points, pointIs, array, minDI, deep + 1, search, conn))
                {
                    array.Remove(conn);
                    return false;
                }
                return true;
            }
        }
        array.Remove(conn);
        return false;
    }
    private void Slice(ref List<Vector3> points, ref List<List<int>> pointIs, ref List<int> outerCircle, Vector3 top, Vector3 bottom, Vector3[] slices, float houseDim)
    {
        
       
        Vector3 vector = Vector3.zero;
        top.x += 1;
        top.z += 2;
        bottom.x -= 2;
        bottom.z -= 1;
        //slices[0] = Vector3.left;
        //slices[1] = Vector3.forward;
        slices[0] = slices[0].normalized;
        slices[1] = slices[1].normalized;
        if(Mathf.Abs(slices[0].z) > Mathf.Abs(slices[1].z))
        {
            slices[0] *= (slices[0].z > 0 ? 1 : -1) * (top - bottom).magnitude * 4;
            slices[1] *= (slices[1].x > 0 ? -1 : 1) * (top - bottom).magnitude * 4;
        }
        else
        {
            slices[0] *= (slices[0].x > 0 ? -1 : 1) * (top - bottom).magnitude * 4;
            slices[1] *= (slices[1].z > 0 ? 1 : -1) * (top - bottom).magnitude * 4;
            Vector3 temp = slices[0];
            slices[0] = slices[1];
            slices[1] = temp;
            
        }

        for (int axis = 0; axis < slices.Length; axis++)
        {
            bool quitWhile = false;
            int k = 0;
            
            while (!quitWhile || k < 10)
            {
                quitWhile = true;
                Vector3 segDiff = slices[axis];
                int extraPI = points.Count;
                List<List<int>> pointIsCopy = new List<List<int>>();
                List<Vector3> pointsCopy = new List<Vector3>();
                for (int i = 0; i < points.Count; i++)
                {
                    foreach (int conectedI in pointIs[i])
                    {
                        if (conectedI > i) {
                            Vector3 tryDiff = points[conectedI] - points[i];

                            Vector3 intersection;
                            #region intersection
                            if (LineLineIntersection(out intersection, (axis == 0 ? bottom : top), segDiff, points[i], tryDiff))
                            {
                                float segSqrMagnitude = segDiff.sqrMagnitude;
                                float trySqrMagnitude = tryDiff.sqrMagnitude;

                                if (
                                    (intersection - (axis == 0 ? bottom : top)).sqrMagnitude <= segSqrMagnitude
                                    && (intersection - (slices[axis] + (axis == 0 ? bottom : top))).sqrMagnitude <= segSqrMagnitude
                                    && (intersection - points[i]).sqrMagnitude <= trySqrMagnitude
                                    && (intersection - points[conectedI]).sqrMagnitude <= trySqrMagnitude)
                                {
                                    quitWhile = false;
                                    pointsCopy.Add(intersection);
                                    List<int> p = new List<int>();
                                    p.Add(i);
                                    p.Add(conectedI);
                                    pointIsCopy.Add(p);
                                }
                            }
                        }
                        #endregion
                    }
                }


                for( int l = 0; l <  pointIsCopy.Count; l++)
                {
                    points.Add(pointsCopy[l]);
                    pointIs.Add(pointIsCopy[l]);
                    pointIs[pointIsCopy[l][0]].Remove(pointIsCopy[l][1]);
                    pointIs[pointIsCopy[l][1]].Remove(pointIsCopy[l][0]);
                    pointIs[pointIsCopy[l][0]].Add(points.Count - 1);
                    pointIs[pointIsCopy[l][1]].Add(points.Count - 1);
                }


                if (axis == 0) { bottom.x += houseDim; }
                else { top.z -= houseDim; }

                for (int i = extraPI; i < points.Count; i++)
                {
                    float closest = float.PositiveInfinity;
                    Vector3 dir = Vector3.zero;
                    int extraJ = -1;
                    int extraJ2 = -1;
                    for (int j = extraPI; j < points.Count; j++)
                    {
                        if (j != i && closest > (points[i] - points[j]).magnitude)
                        {
                            closest = (points[i] - points[j]).magnitude;
                            dir = points[i] - points[j];
                            extraJ = j;
                        }
                    }
                    
                    closest = float.PositiveInfinity;
                    for (int j = extraPI; j < points.Count; j++)
                    {
                        if (j != i && Vector3.Angle((points[i] - points[j]) * -1, dir) < 90f && closest > (points[i] - points[j]).magnitude)
                        {
                            closest = (points[i] - points[j]).magnitude;
                            extraJ2 = j;
                        }
                    }
                    if (extraJ != -1)
                        pointIs[i].Add(extraJ);
                    if (extraJ2 != -1)
                        pointIs[i].Add(extraJ2);
                }
                k++;
            }
        }
    }

    private bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
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
    private Vector3[] InnitialSetUp(List<Vector3> list, out float XHigh, out float XLow, out float ZHigh, out float ZLow, ref List<Vector3> points, ref List<List<int>> pointIs)
    {
        Vector3[] vector3s = new Vector3[2];
        float longest = 0;
        XHigh = -1f; XLow = float.PositiveInfinity; ZHigh = -1f; ZLow = float.PositiveInfinity;
        for (int i = 0; i < list.Count; i++)
        {
            //Finding one of the slice directions
            if((list[(i+1 > list.Count - 1? 0 : i+1)] - list[i]).magnitude > longest)
            {
                longest = (list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i]).magnitude;
                vector3s[0] = list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i];
            }

            //Getting the Max values
            if (list[i].x > XHigh)
                XHigh = list[i].x;
            if (list[i].x < XLow)
                XLow = list[i].x;

            if (list[i].z > ZHigh)
                ZHigh = list[i].z;
            if (list[i].z < ZLow)
                ZLow = list[i].z;

            //creating a graph
            points.Add(list[i]);
            List<int> p = new List<int>();
            p.Add(i - 1 > -1 ? i -1 : list.Count - 1);
            p.Add(i + 1 > list.Count - 1 ? 0 : i + 1);
            pointIs.Add(p);
        }
        longest = 0f;

        //finding the other slice direction
        for (int i = 0; i < list.Count; i++)
        {

            if (121f > Vector3.Angle(vector3s[0] , list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i]) 
                && Vector3.Angle(vector3s[0], list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i]) > 59f 
                && (list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i]).magnitude > longest)
            {
                longest = (list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i]).magnitude;
                vector3s[1] = list[(i + 1 > list.Count - 1 ? 0 : i + 1)] - list[i];
            }
        }
        return vector3s;

    }

}
