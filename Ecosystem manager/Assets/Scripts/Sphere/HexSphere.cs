using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexSphere : MonoBehaviour
{
    public Material mat;

    private Mesh sphere;
    private List<int> originalTris;
    HexSpherePointMap pointMap;

    private float distBtwPts;

    [Header("debugging")]
    public VertexMarker marker;

    //called from sphere creator which gives all verts 
    public void CreateHexSphere(Mesh _sphere, HexSpherePointMap _pointMap, List<int> _originalTris)
    {
        sphere = _sphere;
        originalTris = _originalTris;
        pointMap = _pointMap;

        CreatePentagons();
        CreateHexagons();

    }

    /// <summary>
    /// finds hexes surrounding the pentagons
    /// </summary>
    /// <returns></returns>
    private List<int> FindHexCentres()
    {
        List<int> hexagonCentreIds = new List<int>();

        for (int i = 0; i < originalTris.Count; i += 3)
        {
            //original tri points
            int v1 = originalTris[i];
            int v2 = originalTris[i + 1];
            int v3 = originalTris[i + 2];

            //midpoints
            int m1 = pointMap.GetVerticesOuterPoints(v1, v2)[0];
            int m2 = pointMap.GetVerticesOuterPoints(v1, v3)[0];
            int m3 = pointMap.GetVerticesOuterPoints(v2, v3)[0];

            //returns the hex at the bottom of each triangle
            hexagonCentreIds.Add(HexCentreHelper(v1, m1, m2));
            hexagonCentreIds.Add(HexCentreHelper(v2, m1, m3));
            hexagonCentreIds.Add(HexCentreHelper(v3, m2, m3));
        }

        //remove duplicates
        hexagonCentreIds = hexagonCentreIds.Distinct().ToList();

        //marker.PlaceMarkers(hexagonCentreIds, 1, false);

        return hexagonCentreIds;
    }

    /// <summary>
    /// Finds the point directly above the triangle of the given vertex. the hexagon centre
    /// see one note for diagram
    /// </summary>
    private int HexCentreHelper(int v1, int m1, int m2)
    {
        int m3 = pointMap.GetVerticesOuterPoints(v1, m1)[0];
        int m4 = pointMap.GetVerticesOuterPoints(v1, m2)[0];

        int hexCentre = pointMap.GetVerticesOuterPoints(m3, m4)[0];

        return hexCentre;
    }


    private void CreateHexagons()
    {
        List<int> hexagonCentreIds = FindHexCentres();

        List<List<Vector3>> hexagonVerts = new List<List<Vector3>>();

        foreach(var hexCentre in hexagonCentreIds)
        {
            List<Vector3> hexagon = Hexagon(hexCentre);
            hexagonVerts.Add(hexagon);

        }

        hexagonVerts = ReorderClockwise(hexagonVerts);

        GameObject parentObj = new GameObject("Hexagons");
        parentObj.transform.parent = transform;

        //creates a mesh for each pentagon adding to new game object
        foreach (var hexagon in hexagonVerts)
        {
            Mesh mesh = new Mesh();

            //the pentagon has 6 verts (1 in middle)
            Vector3[] verts = { hexagon[0], hexagon[1], hexagon[2], hexagon[3], hexagon[4], hexagon[5], hexagon[6] };
            mesh.vertices = verts;

            int[] tris = { 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 1, 0 };

            mesh.triangles = tris;
            mesh.RecalculateNormals();

            CreateMeshObj(mesh, parentObj.transform, mat, "pentagon");
        }
    }

    /// <summary>
    /// returns all points on the original triangles edges
    /// </summary>
    private List<int> FindTriEdgePoints()
    {
        List<int> hexagonCentreIds = new List<int>();

        for (int i = 0; i < originalTris.Count; i += 3)
        {
            int v1 = originalTris[i];
            int v2 = originalTris[i + 1];
            int v3 = originalTris[i + 2];

            hexagonCentreIds.AddRange(MidPoints(v1, v2, pointMap.totalSubDivides - 1));
            hexagonCentreIds.AddRange(MidPoints(v1, v3, pointMap.totalSubDivides - 1));
            hexagonCentreIds.AddRange(MidPoints(v2, v3, pointMap.totalSubDivides - 1));

            hexagonCentreIds = hexagonCentreIds.Distinct().ToList();
        }

        return hexagonCentreIds;
    }

    //recursively find all midpoints between 2 outer points including the subdivided points
    private List<int> MidPoints(int p1, int p2, int subdivides)
    {
        List<int> points = new List<int>();

        int midpoint = pointMap.GetVerticesOuterPoints(p1, p2)[0];
        points.Add(midpoint);

        subdivides--;

        if(subdivides > 0)
        {
            points.AddRange(MidPoints(p1, midpoint, subdivides));
            points.AddRange(MidPoints(p2, midpoint, subdivides));
        }


        return points;
    }

    /// <summary>
    /// finds the surrounding points to each hexagon centre
    /// </summary>
    private List<Vector3> Hexagon(int centreId)
    {
        Vector3 centre = sphere.vertices[centreId];
        List<Vector3> hexagon = new List<Vector3>();
        hexagon.Add(centre);

        foreach(Vector3 vert in sphere.vertices)
        {
            float dist = Vector3.Distance(centre, vert);
            if(dist < 0.1f) { continue; }   //don't re add centre
            if (dist < distBtwPts * 1.2f)
            {
                hexagon.Add(vert);
            }
        }
        return hexagon;
    }

    /// <summary>
    /// Creates pentagon objects at each of the oringal edges
    /// </summary>
    private void CreatePentagons()
    {
        List<List<int>> pentagons = pointMap.pentagonVertIds();
        List<List<Vector3>> pentagonVerts = new List<List<Vector3>>();

        //change the pentagon vert id to vector3s
        int listCount = 0;
        foreach (List<int> pentagon in pentagons)
        {
            pentagonVerts.Add(new List<Vector3>(pentagon.Count));
            foreach (var id in pentagon)
            {
                pentagonVerts[listCount].Add(sphere.vertices[id]);
            }
            listCount++;
        }

        //used for finding nieghbouring points
        distBtwPts = Vector3.Distance(pentagonVerts[0][0], pentagonVerts[0][1]);

        GameObject parentObj = new GameObject("Pentagons");
        parentObj.transform.parent = transform;

        //reorder to clockwise
        List<List<Vector3>> clockWiseVerts = ReorderClockwise(pentagonVerts);

        //creates a mesh for each pentagon adding to new game object
        foreach (var pentagon in clockWiseVerts)
        {
            Mesh mesh = new Mesh();


            //the pentagon has 6 verts (1 in middle)
            Vector3[] verts = { pentagon[0], pentagon[1], pentagon[2], pentagon[3], pentagon[4], pentagon[5] };
            mesh.vertices = verts;

            int[] tris = { 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 1, 0 };
            mesh.triangles = tris;
            mesh.RecalculateNormals();

            CreateMeshObj(mesh, parentObj.transform, mat, "pentagon");
        }

    }

    //reorder each hex to clockwise order starting at the first outer point
    private List<List<Vector3>> ReorderClockwise(List<List<Vector3>> hexVerts)
    {
        List<List<Vector3>> clockWiseVerts = new List<List<Vector3>>();

        foreach (var hex in hexVerts)
        {
            Dictionary<float, int> vertAngles = new Dictionary<float, int>();

            //the second point is used as start point so find a vector parrallel to check
            //if of points are right or left of it
            Vector3 perpPivot = Vector3.Cross(hex[1] - hex[0], hex[0]);

            for (int i = 2; i < hex.Count; i++)
            {
                //check if right or left of the starting point either 1 / -1
                int rightFromPivot = (Vector3.Dot(hex[i], perpPivot)) > 0 ? 1 : -1;

                float angleFromPivot = Vector3.Angle(hex[1], hex[i]) * rightFromPivot;

                vertAngles.Add(angleFromPivot, i);
            }

            List<Vector3> reordered = new List<Vector3>();
            reordered.Add(hex[0]); //add centre and pivot point
            reordered.Add(hex[1]);

            //add negative values in descending order then positive in ascending

            List<float> positiveAngles = new List<float>();
            List<float> negativeAnlges = new List<float>();

            foreach (float angle in vertAngles.Keys)
            {
                if (angle > 0) { positiveAngles.Add(angle); }
                else
                {
                    negativeAnlges.Add(angle);
                }
            }

            positiveAngles.Sort();
            negativeAnlges.Sort();

            for (int i = negativeAnlges.Count - 1; i >= 0; i--)
            {
                reordered.Add(hex[vertAngles[negativeAnlges[i]]]);
            }

            for (int i = positiveAngles.Count - 1; i >= 0; i--)
            {
                reordered.Add(hex[vertAngles[positiveAngles[i]]]);
            }

            clockWiseVerts.Add(reordered);
        }
        return clockWiseVerts;
    }

    /// <summary>
    /// creates a new gameobject holding the given mesh 
    /// </summary>
    private void CreateMeshObj(Mesh _mesh, Transform _parent, Material _material, string _objName)
    {
        GameObject g = new GameObject(_objName);
        g.AddComponent<MeshFilter>().mesh = _mesh;
        g.AddComponent<MeshRenderer>().sharedMaterial = _material;
        g.transform.parent = _parent;
    }


}
