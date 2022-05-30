using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCreator : MonoBehaviour
{
    Mesh sphere;

    [Range(0,6)]
    public int divides;
    private int prevDivides;    //keep track when to build new mesh

    [Header("Hex Tiling")]
    public bool GenerateHexes;
    public HexSphere hexSphere;
    HexSpherePointMap pointMap;
    private int divideCount = 1;

    [Header("Splitting")]
    public bool Split;
    public Material Mat;

    [Header("Vertex Debugging")]
    private VertexMarker vertexMarker;
    private int testSubdividesTracker = 0;

    private void Start()
    {
        pointMap = new HexSpherePointMap(divides);
        vertexMarker = GetComponent<VertexMarker>();

        BuildMesh(10);

        //track subdivides for change at runtime
        prevDivides = divides;

        //spliting the mesh
        if (Split) {
            SplitMesh();
        }

        if (GenerateHexes){
            CreateHexes();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            //gameObject.SetActive(false);
        }

    }

    private void Update()
    {
        if(divides != prevDivides)
        {
            BuildMesh(10);
            prevDivides = divides;
        }

        //places markers at the given subdivide level
        if (Input.GetKeyDown(KeyCode.M))
        {
            vertexMarker.PlaceMarkers(pointMap.GetVerticesSubdivs(testSubdividesTracker));
            Debug.Log($"Marker Sub divides:{testSubdividesTracker}");
            testSubdividesTracker = (testSubdividesTracker + 1) % (divides + 1);
        }
    }

    private void CreateHexes()
    {
        List<List<int>> pentagons = pointMap.pentagonVertIds();
        List<List<Vector3>> pentagonVerts = new List<List<Vector3>>();

        //change the pentagon vert id to vector3s
        int listCount = 0;
        foreach(List<int> pentagon in pentagons)
        {
            pentagonVerts.Add(new List<Vector3>(pentagon.Count));
            foreach (var id in pentagon) 
            {
                pentagonVerts[listCount].Add(sphere.vertices[id]);
            }
            listCount++;
        }

        hexSphere.CreateHexSphere(pentagonVerts);

    }


    private void BuildMesh(float radius)
    {
        sphere = new Mesh();
        sphere.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        gameObject.GetComponent<MeshFilter>().mesh = sphere;

        //consts so each point is dist 1 from centre
        float X = 0.525731112119133606f * radius;
        float Z = 0.850650808352039932f * radius;

        //12 verts preset to make the basic ICOSAHEDRON
        List<Vector3> verts = new List<Vector3>
        {
            new Vector3(-X, 0.0f, Z), new Vector3(X, 0.0f, Z), new Vector3(-X, 0.0f, -Z),
            new Vector3(X, 0.0f, -Z), new Vector3(0.0f, Z, X), new Vector3(0.0f, Z, -X),
            new Vector3(0.0f, -Z, X), new Vector3(0.0f, -Z, -X), new Vector3(Z, X, 0.0f),
            new Vector3(-Z, X, 0.0f), new Vector3(Z, -X, 0.0f), new Vector3(-Z, -X, 0.0f)
        };

        //triangles
        List<int> triangles = new List<int>()
        {
            0,4,1, 0,9,4, 9,5,4, 4,5,8, 4,8,1,
            8,10,1, 8,3,10, 5,3,8, 5,2,3, 2,7,3,
            7,10,3, 7,6,10, 7,11,6, 11,0,6, 0,1,6,
            6,1,10, 9,0,11, 9,11,2, 9,2,5, 7,2,11
        };


        for (int i = 0; i < triangles.Count; i += 3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 1];
            triangles[i + 1] = temp;
        }

        for (int i = 0; i < verts.Count; i++)
        {
            pointMap.AddPoint(i, (0, 0), 0);
        }

        for (int i = 0; i < divides; i++)
        {
            SubDivide2(ref verts, ref triangles, radius);
            divideCount++;
        }

        sphere.vertices = verts.ToArray();
        sphere.triangles = triangles.ToArray();
        sphere.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = sphere;

        vertexMarker.CurrentMesh = sphere;

        Debug.Log(sphere.vertices.Length);
        //PlaceMarkers();
    }

    
   
    private void SubDivide(ref List<Vector3> _verts, ref List<int> _tris, float radius)
    {
        //copy lists
        List<Vector3> verts = new List<Vector3>(_verts);
        List<int> tris = new List<int>(_tris);

        _verts.Clear();
        _tris.Clear();

        int triCounter = 0;
        for (int i = 0; i < tris.Count; i += 3)
        {
            //verticies of current triangle
            Vector3 v1 = verts[tris[i]];
            Vector3 v2 = verts[tris[i + 1]];
            Vector3 v3 = verts[tris[i + 2]];

            //find mid points. normalized to push out for a sphere shape
            Vector3 m1 = (v1 + v2) / 2;
            m1 = m1.normalized * radius;
            Vector3 m2 = (v2 + v3) / 2;
            m2 = m2.normalized * radius;
            Vector3 m3 = (v1 + v3) / 2;
            m3 = m3.normalized * radius;

            //add all as new verticies
            _verts.Add(v1);
            _verts.Add(m1);
            _verts.Add(v2);
            _verts.Add(m2);
            _verts.Add(v3);
            _verts.Add(m3);

            //add the new triangles
            _tris.Add(triCounter);
            _tris.Add(triCounter + 1);
            _tris.Add(triCounter + 5);

            _tris.Add(triCounter + 1);
            _tris.Add(triCounter + 2);
            _tris.Add(triCounter + 3);

            _tris.Add(triCounter + 1);
            _tris.Add(triCounter + 3);
            _tris.Add(triCounter + 5);

            _tris.Add(triCounter + 5);
            _tris.Add(triCounter + 3);
            _tris.Add(triCounter + 4);

            triCounter += 6;
        }
    }

    private void SubDivide2(ref List<Vector3> _verts, ref List<int> _tris, float radius)
    {
        List<Vector3> vertsNew = new List<Vector3>(_verts);
        List<int> trisNew = new List<int>();

        //key: id of both original verts value: id of new mid point
        Dictionary<(int, int), int> midPoints = new Dictionary<(int, int), int>();

        int vertCount = vertsNew.Count;

        for (int i = 0; i < _tris.Count; i += 3)
        {
            //verticies of current triangle
            Vector3 v1 = _verts[_tris[i]];
            Vector3 v2 = _verts[_tris[i + 1]];
            Vector3 v3 = _verts[_tris[i + 2]];

            //ids of the triangle's 3 edges' points
            (int, int)[] keys = { (_tris[i], _tris[i + 1]), (_tris[i + 1], _tris[i + 2]), (_tris[i], _tris[i + 2]) };

            //if the midpoint hasn't been calculated, calculate and add to verts and the index to the midpoint dict
            if (!midPoints.ContainsKey(keys[0]))
            {
                Vector3 midPoint = ((v1 + v2) / 2).normalized * radius;
                int index = vertsNew.IndexOf(midPoint);
                if (index != -1)
                {
                    midPoints[keys[0]] = index;
                }
                else
                {
                    vertsNew.Add(midPoint);
                    pointMap.AddPoint(vertCount, keys[0], divideCount);
                    midPoints[keys[0]] = vertCount++;
                }         
            }
            if (!midPoints.ContainsKey(keys[1]))
            {
                Vector3 midPoint = ((v2 + v3) / 2).normalized * radius;
                int index = vertsNew.IndexOf(midPoint);
                if (index != -1)
                {
                    midPoints[keys[1]] = index;
                }
                else
                {
                    vertsNew.Add(midPoint);
                    pointMap.AddPoint(vertCount, keys[1], divideCount);
                    midPoints[keys[1]] = vertCount++;
                }            
            }
            if (!midPoints.ContainsKey(keys[2]))
            {
                Vector3 midPoint = ((v1 + v3) / 2).normalized * radius;
                int index = vertsNew.IndexOf(midPoint);
                if (index != -1)
                {
                    midPoints[keys[2]] = index;
                }
                else
                {
                    vertsNew.Add(midPoint);
                    pointMap.AddPoint(vertCount, keys[2], divideCount);
                    midPoints[keys[2]] = vertCount++;
                }       
            }

            //triangle between bot left and middle left and bottom middle
            trisNew.Add(_tris[i]);
            trisNew.Add(midPoints[keys[0]]);
            trisNew.Add(midPoints[keys[2]]);

            //triangle between top and two middle midpoints
            trisNew.Add(midPoints[keys[0]]);
            trisNew.Add(_tris[i+1]);
            trisNew.Add(midPoints[keys[1]]);

            //triangle between all midpoints
            trisNew.Add(midPoints[keys[0]]);
            trisNew.Add(midPoints[keys[1]]);
            trisNew.Add(midPoints[keys[2]]);
            
            //triangle middle bottom and middle right and bottom right
            trisNew.Add(midPoints[keys[2]]);
            trisNew.Add(midPoints[keys[1]]);
            trisNew.Add(_tris[i+2]);
        }
        _verts = vertsNew;
        _tris = trisNew;
    }

    private void SplitMesh()
    {
        GameObject g = new GameObject("Split-Sphere");
        SplitMesh splitScript = g.AddComponent<SplitMesh>();
        splitScript.Split(sphere, Mat, transform.position);
        gameObject.SetActive(false);
    }

}

class HexSpherePointMap
{
    public List<HexSpherePoint> points;
    private int totalSubDivides;


    public HexSpherePointMap(int totalSubDivs)
    {
        points = new List<HexSpherePoint>();
        totalSubDivides = totalSubDivs;
    }

    public void AddPoint(int vertId, (int,int) outerIds, int subdivides)
    {
        HexSpherePoint hexPoint = new HexSpherePoint(vertId, outerIds, subdivides);
        points.Add(hexPoint);
    }

    public List<List<int>> pentagonVertIds()
    {
        List<List<int>> pentagons = new List<List<int>>();

        for(int i = 0; i < 12; i++)
        {
            List<int> pentagon = new List<int>();
            pentagon.Add(i);
            pentagons.Add(pentagon);
        }

        foreach (var hexPoint in points)
        {
            if(hexPoint.subDivIterations == totalSubDivides)
            {
                if(hexPoint.outerPointsIds.Item1 < 12)
                {
                    pentagons[hexPoint.outerPointsIds.Item1].Add(hexPoint.vertId);
                }
                if (hexPoint.outerPointsIds.Item2 < 12)
                {
                    pentagons[hexPoint.outerPointsIds.Item2].Add(hexPoint.vertId);
                }
            }
        }
        return pentagons;
    }

    public List<int> GetVerticesSubdivs(int subDivides)
    {
        List<int> verticies = new List<int>();

        foreach(var hexPoint in points)
        {
            if(hexPoint.subDivIterations == subDivides)
            {
                verticies.Add(hexPoint.vertId);
            }
        }
        return verticies;
    }
}

struct HexSpherePoint
{
    public HexSpherePoint(int id, (int,int) outerIds, int subDivs )
    {
        vertId = id;
        outerPointsIds = outerIds;
        subDivIterations = subDivs;
    }

    public int vertId;                  //index of vert in mesh verticies list
    public (int, int) outerPointsIds;   //id's of this vertex outerpoints
    public int subDivIterations;        //sub division iterations
}