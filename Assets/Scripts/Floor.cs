using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor 
{
    int level;
    Vector3 dir;
    List<Vector3> contours;
    GameObject floor;
    MeshFilter mc;
    MeshRenderer mr;
    Mesh floorMesh;
    Material wallPaint;
    List<Wall> walls;

    public Floor(GameObject folder, List<Vector3> contours, int level, Material mat, GameObject[] prefabs)
    {
        this.level = level;
        this.contours = contours;
        this.wallPaint = mat;
        floor = new GameObject("Floor" + (level + 1).ToString());
        floor.transform.SetParent(folder.transform);
        walls = new List<Wall>();
        for (int i = 0; i < contours.Count; i++)
        {
            walls.Add(new Wall(contours[i],contours[(i + 1 < contours.Count? i + 1: 0)], 1, floor, prefabs, level));
        }
        Generate();
    }

    private void Generate()
    {
        floorMesh = new Mesh();
        mc = floor.AddComponent<MeshFilter>();
        mr = floor.AddComponent<MeshRenderer>();
        int contourCount = contours.Count;

        contours = CalculateVertices(contours);

        floorMesh.vertices = contours.ToArray();
        floorMesh.triangles = CalculateTriangles(contours, contourCount);
        floorMesh.RecalculateNormals();
        mc.sharedMesh = floorMesh;
        mr.material = wallPaint;
    }

    private List<Vector3> CalculateVertices(List<Vector3> list)
    {
        List<Vector3> vertices = new List<Vector3>();
        for (int y = level; y <= level + 1; y++)
        {
            for (int xz = 0; xz < list.Count; xz++)
            {
                vertices.Add(new Vector3(list[xz].x, y * 1.1f, list[xz].z));
            }
        }
        for (int xz = 0; xz < list.Count; xz++)
        {
            vertices.Add(new Vector3(list[xz].x,    level * 1.1f,      list[xz].z));
            vertices.Add(new Vector3(list[xz].x,    (level + 1) * 1.1f,  list[xz].z));
            vertices.Add(new Vector3(list[(xz + 1 >= list.Count ? 0 : xz + 1)].x,       level * 1.1f,      list[(xz + 1 >= list.Count ? 0 : xz + 1)].z));
            vertices.Add(new Vector3(list[(xz + 1 >= list.Count ? 0 : xz + 1)].x,       (level + 1) * 1.1f,  list[(xz + 1 >= list.Count ? 0 : xz + 1)].z));
        }
        return vertices;
    }

    private int[] CalculateTriangles(List<Vector3> contours, int contourCount)
    {
        List<int> trises = new List<int>();
        foreach(int i in TriangulationToTriangles(Triangulate(contours, 0, contourCount - 1), contours, 0, contourCount - 1, true ))
        {
            trises.Add(i);
        }
        foreach (int i in TriangulationToTriangles(Triangulate(contours, contourCount, contourCount * 2 - 1), contours, contourCount, contourCount * 2 - 1, false))
        {
            trises.Add(i);
        }
        for (int i = contourCount*2; i < contours.Count; i+=4)
        {
            //triangle 1
            trises.Add(i);
            trises.Add(i + 1);
            trises.Add(i + 2);
            //triangle 2
            trises.Add(i + 1);
            trises.Add(i + 3);
            trises.Add(i + 2);
        }
        return trises.ToArray();

    }

    private List<List<int>> Triangulate(List<Vector3> contours, int startI, int endI)
    {
        List<List<int>> triangulations = new List<List<int>>();
        Vector2 low = new Vector2(float.PositiveInfinity, float.PositiveInfinity), high = new Vector2(-100, -100);
        //determin high low point
        float trueY = contours[startI].y;
        for (int i = startI; i <= endI; i++)
        {
            if (contours[i].x > high.x)
                high.x = contours[i].x;
            if (contours[i].x < low.x)
                low.x = contours[i].x;

            if (contours[i].z > high.y)
                high.y = contours[i].z;
            if (contours[i].z < low.y)
                low.y = contours[i].z;
        }
        // add super triangle
        contours.Add(new Vector3(low.x-1,         trueY,  low.y-1)); //bottom corner
        contours.Add(new Vector3(low.x,         trueY,  high.y * 10)); // top z
        contours.Add(new Vector3(high.x * 10,    trueY,  low.y));// top x
        List<int> superTri = new List<int>() { contours.Count - 1, contours.Count - 2, contours.Count - 3 };
        triangulations.Add(superTri);
        for (int i = startI; i <= endI; i++)
        {
            List<List<int>> badTriangles = new List<List<int>>();
            foreach (List<int> tri in triangulations)
            {
                Vector3 middle = CalculateMiddlePoint(contours[tri[0]], contours[tri[1]], contours[tri[2]]); 
                if ( ( middle - contours[tri[0]] ).magnitude >= (middle - contours[i]).magnitude)
                {
                    badTriangles.Add(tri);
                }
            }

            List<(int, int)> polygon = new List<(int, int)>();
            List<(int, int)> duplicates = new List<(int, int)>();
            for (int tri1 = 0; tri1 < badTriangles.Count; tri1++)
            {
                for (int point = 0; point < badTriangles[tri1].Count; point++)
                {
                    (int, int) edge = (badTriangles[tri1][point], badTriangles[tri1][(point + 1  == badTriangles[tri1].Count ? 0 : point + 1)]);
                    bool dp = false;
                    foreach((int,int) t in duplicates)
                    {
                        if (tupleEqasion(t, edge))
                        {
                            dp = true;
                        }
                    }
                    if (!dp)
                    {
                        for(int pI = 0;  pI < polygon.Count; pI++)
                        {
                            if (tupleEqasion(polygon[pI], edge))
                            {
                                dp = true;
                                duplicates.Add(edge);
                                polygon.RemoveAt(pI);
                            }
                        }
                    }
                    if (!dp)
                        polygon.Add(edge);
                }
            }
            foreach (List<int> tri in badTriangles)
            {
                triangulations.Remove(tri);
            }
            foreach ((int,int) edge in polygon)
            {
                List<int> newTri = new List<int>();
                newTri.Add(i);
                newTri.Add(edge.Item1);
                newTri.Add(edge.Item2);
                triangulations.Add(newTri);
            }

        }
        
        for (int i = 0; i < triangulations.Count; i++)
        {
            for (int j = 0; j < triangulations[i].Count; j++)
            {
                if (triangulations[i][j] == contours.Count - 1 || triangulations[i][j] == contours.Count - 2 || triangulations[i][j] == contours.Count - 3)
                {
                    triangulations.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
        
       
        contours.RemoveAt(contours.Count - 1);
        contours.RemoveAt(contours.Count - 1);
        contours.RemoveAt(contours.Count - 1);
       
        //temp conversion
        return triangulations;
    }

    private Vector3 CalculateMiddlePoint(Vector3 vector31, Vector3 vector32, Vector3 vector33)
    {
        Vector3 normal1 = Vector3.Cross((vector31 - vector32), Vector3.up);
        Vector3 normal2 = Vector3.Cross((vector32 - vector33), Vector3.up);

        Vector3 middlePoint1 = (vector31 + vector32) * 0.5f;
        Vector3 middlePoint2 = (vector33 + vector32) * 0.5f;
        normal1 = normal1 + middlePoint1;
        normal2 = normal2 + middlePoint2;

        Vector3 crossProduct1 = Vector3.Cross(new Vector3(normal1.x, normal1.z, 1), new Vector3(middlePoint1.x, middlePoint1.z,1)); 
        Vector3 crossProduct2 = Vector3.Cross(new Vector3(normal2.x, normal2.z, 1), new Vector3(middlePoint2.x, middlePoint2.z,1));
        float x0 = (crossProduct1.y * crossProduct2.z - crossProduct2.y * crossProduct1.z) / (crossProduct1.x * crossProduct2.y - crossProduct2.x * crossProduct1.y);
        float y0 = (crossProduct1.z * crossProduct2.x - crossProduct2.z * crossProduct1.x) / (crossProduct1.x * crossProduct2.y - crossProduct2.x * crossProduct1.y);

        return new Vector3(x0, vector31.y, y0);
    }

    public bool tupleEqasion((int, int) t1, (int, int) t2)
    {
        return (t1.Item1 == t2.Item1 && t1.Item2 == t2.Item2) || (t1.Item2 == t2.Item1 && t1.Item1 == t2.Item2);
    }
    
    public List<int> TriangulationToTriangles(List<List<int>> triangulation, List<Vector3> contours, int startI, int endI, bool Clockwise = true) 
    {
        List<List<int>> pointIs = new List<List<int>>();
        for (int i = startI; i <= endI; i++)
        {
            List<int> pointI = new List<int>();
            pointIs.Add(pointI);
        }
        foreach(List<int> tri in triangulation)
        {
            //point 0
            pointIs[tri[0] - startI].Add(tri[1] - startI);
            pointIs[tri[0] - startI].Add(tri[2] - startI);

            //point 1
            pointIs[tri[1] - startI].Add(tri[0] - startI);
            pointIs[tri[1] - startI].Add(tri[2] - startI);

            //point 2
            pointIs[tri[2] - startI].Add(tri[0] - startI);
            pointIs[tri[2] - startI].Add(tri[1] - startI);
        }
        List<List<int>> trisIs = new List<List<int>>();
        for (int i = 0; i < pointIs.Count; i++)
        {
            foreach (int item in pointIs[i])
            {
                List<int> array = new List<int>();
                array.Add(i);
                if (pointIs[item].Count < 1) continue;
                HelperFunctions.ConnectionGlobal(contours, pointIs, array, item, 1, i, i, startI, Clockwise);
                if (array[0] != -1 && array.Count > 2)
                {
                    HelperFunctions.UniqueArrayGlobal(array, ref trisIs);
                }

            }
        }

        List<int> finalTris = new List<int>();

        foreach (List<int> list in trisIs)
        {

            finalTris.Add((list[0] + startI));
            finalTris.Add((list[1] + startI));
            finalTris.Add((list[2] + startI));
        }
        return finalTris;
    }
}
