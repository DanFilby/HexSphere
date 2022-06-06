using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexMarker : MonoBehaviour
{
    [Header("Refrences")]
    public List<GameObject> MarkerSpheres;
    private GameObject MarkerObj;

    private Mesh currentMesh;

    public Mesh CurrentMesh{ set { currentMesh = value; } }

    private void Start()
    {
        currentMesh = new Mesh();
    }

    public void PlaceMarkersAllVerts(int colour = 0)
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int index = 0;
        foreach (var vert in currentMesh.vertices)
        {
            GameObject g = Instantiate(MarkerSpheres[colour % MarkerSpheres.Count], vert, Quaternion.identity) ;
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {index++}";
        }
    }

    public void PlaceMarkers(int subDivides, int colour = 0)
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        for (int i = 0; i < 12 * (subDivides + 1); i++)
        {
            GameObject g = Instantiate(MarkerSpheres[colour % MarkerSpheres.Count], currentMesh.vertices[i], Quaternion.identity);
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {i}";
        }

    }

    public void PlaceMarkers(List<int> vertIds, int colour = 0, bool destroyOld = true)
    {
        if (destroyOld) { Destroy(MarkerObj); }
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int count = 0;
        foreach (int id in vertIds)
        {
            GameObject g = Instantiate(MarkerSpheres[colour % MarkerSpheres.Count], currentMesh.vertices[id], Quaternion.identity);
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {count++}";
        }
    }

    public void PlaceMarkers(List<List<int>> vertIds, int colour = 0)
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int count = 0;
        foreach(List<int> vertList in vertIds)
        {
            foreach (int id in vertList)
            {
                GameObject g = Instantiate(MarkerSpheres[colour % MarkerSpheres.Count], currentMesh.vertices[id], Quaternion.identity);
                g.transform.parent = MarkerObj.transform;
                g.name = $"Marker: {count++}";
            }
        }    
    }

    public void PlaceMarkers(List<Vector3> positions, int colour = 0, bool destroyOld = true)
    {
        if (destroyOld) { Destroy(MarkerObj); }
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int count = 0;
        foreach (var pos in positions)
        {
            GameObject g = Instantiate(MarkerSpheres[colour % MarkerSpheres.Count],pos , Quaternion.identity);
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {count++}";
        }
    }
    public void PlaceMarkers(List<List<Vector3>> positions, int colour = 0)
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int count = 0;
        foreach (List<Vector3> poses in positions)
        {
            foreach (Vector3 pos in poses)
            {
                GameObject g = Instantiate(MarkerSpheres[colour % MarkerSpheres.Count], pos, Quaternion.identity);
                g.transform.parent = MarkerObj.transform;
                g.name = $"Marker: {count++}";
            }
        }
    }

}
