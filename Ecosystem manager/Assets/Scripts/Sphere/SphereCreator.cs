using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCreator : MonoBehaviour
{
    Mesh sphere;

    [Range(0,6)]
    public int divides;         //times to subdivide the triangles
    private int prevDivides;    //keep track when to build new mesh

    [Header("Hex Tiling")]
    public bool GenerateHexes;  
    public HexSphere hexSphere;     //script to split up the mesh into hex tiles
    HexSpherePointMap pointMap;     //keeps track of the verticies
    private int divideCount = 1;    //used when add verts to point map
    private List<int> originalTris; //sent to hex script

    [Header("Splitting")]
    public bool Split;      //when the mesh is split by each triangle 
    public Material Mat;

    [Header("Vertex Debugging")]
    private VertexMarker vertexMarker;
    private int debugSubdivideTracker = 0;

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

        //create hex tiling
        if (GenerateHexes){
            CreateHexes();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

    }

    private void Update()
    {
        //if changed divides in runtime
        if(divides != prevDivides)
        {
            BuildMesh(10);
            prevDivides = divides;
        }

        //places markers at the given subdivide level
        if (Input.GetKeyDown(KeyCode.M))
        {
            vertexMarker.CurrentMesh = sphere;
            vertexMarker.PlaceMarkers(pointMap.GetVerticesSubdivs(debugSubdivideTracker));
            Debug.Log($"Marker Sub divides:{debugSubdivideTracker}");
            debugSubdivideTracker = (debugSubdivideTracker + 1) % (divides + 1);
        }

        //shows all verts
        if (Input.GetKeyDown(KeyCode.N))
        {
            vertexMarker.CurrentMesh = sphere;
            vertexMarker.PlaceMarkersAllVerts();
        }
    }

    /// <summary>
    /// uses the sphere mesh to create a hex tiled version in a different object
    /// </summary>
    private void CreateHexes()
    {
        //send the info to the hex tiling scripts
        hexSphere.CreateHexSphere(sphere, pointMap, originalTris);
    }

    /// <summary>
    /// Create a 'spherical' mesh by subdividing a icosahedron
    /// </summary>
    private void BuildMesh(float radius)
    {
        //create mesh obj and increase maxium allowed verts
        sphere = new Mesh();
        sphere.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        gameObject.GetComponent<MeshFilter>().mesh = sphere;

        //used to offset each point equally from the centre
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


        List<int> triangles = new List<int>()
        {
            0,4,1, 0,9,4, 9,5,4, 4,5,8, 4,8,1,
            8,10,1, 8,3,10, 5,3,8, 5,2,3, 2,7,3,
            7,10,3, 7,6,10, 7,11,6, 11,0,6, 0,1,6,
            6,1,10, 9,0,11, 9,11,2, 9,2,5, 7,2,11
        };

        //used later in tiling script
        originalTris = new List<int>(triangles);

        //filps the tris so mesh renders front face
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

        //divide the tris 
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
    }

    private void SubDivide2(ref List<Vector3> _verts, ref List<int> _tris, float radius)
    {
        List<Vector3> vertsNew = new List<Vector3>(_verts);
        List<int> trisNew = new List<int>();

        //key: id of both original verts | value: id of new mid point
        Dictionary<(int, int), int> midPoints = new Dictionary<(int, int), int>();

        int vertCount = vertsNew.Count;

        for (int i = 0; i < _tris.Count; i += 3)
        {
            //verticies of current triangle
            Vector3 v1 = _verts[_tris[i]];
            Vector3 v2 = _verts[_tris[i + 1]];
            Vector3 v3 = _verts[_tris[i + 2]];

            //ids of the triangle's 3 sides points
            (int, int)[] keys = { (_tris[i], _tris[i + 1]), (_tris[i + 1], _tris[i + 2]), (_tris[i], _tris[i + 2]) };

            //if the midpoint hasn't been calculated, calculate and add to verts and the index to the midpoint dict
            if (!midPoints.ContainsKey(keys[0]))
            {
                Vector3 midPoint = ((v1 + v2) / 2).normalized * radius;

                //check if the point is already in the new verts list
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

    //original sub divide doesn't remove duplicates or add to point map 
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

    private void SplitMesh()
    {
        GameObject g = new GameObject("Split-Sphere");
        SplitMesh splitScript = g.AddComponent<SplitMesh>();
        splitScript.Split(sphere, Mat, transform.position);
        gameObject.SetActive(false);
    }

}

public class HexSpherePointMap
{
    public List<HexSpherePoint> points;
    public int totalSubDivides;


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

    public List<int> GetVerticesOuterPoint(int outerPointId)
    {
        List<int> resultVerts = new List<int>();

        foreach (var hexPoint in points)
        {
            if (hexPoint.outerPointsIds.Item1 == outerPointId)
            {
                resultVerts.Add(hexPoint.vertId);
            }
            if (hexPoint.outerPointsIds.Item2 == outerPointId)
            {
                resultVerts.Add(hexPoint.vertId);
            }
        }

        return resultVerts;

    }

    public List<int> GetVerticesOuterPoints(int id1, int id2)
    {
        List<int> resultVerts = new List<int>();

        foreach (var hexPoint in points)
        {
            if (hexPoint.outerPointsIds.Item1 == id1 && hexPoint.outerPointsIds.Item2 == id2)
            {
                resultVerts.Add(hexPoint.vertId);
            }
            if (hexPoint.outerPointsIds.Item1 == id2 && hexPoint.outerPointsIds.Item2 == id1)
            {
                resultVerts.Add(hexPoint.vertId);
            }
        }

        return resultVerts;
    }
}

public struct HexSpherePoint
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