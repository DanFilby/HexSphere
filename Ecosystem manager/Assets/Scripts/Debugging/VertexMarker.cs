using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexMarker : MonoBehaviour
{
    [Header("Refrences")]
    public GameObject MarkerSphere;
    private GameObject MarkerObj;

    private Mesh currentMesh;

    public Mesh CurrentMesh{ set { currentMesh = value; } }

    private void Start()
    {
        currentMesh = new Mesh();
    }

    public void PlaceMarkersAllVerts()
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int index = 0;
        foreach (var vert in currentMesh.vertices)
        {
            GameObject g = Instantiate(MarkerSphere, vert, Quaternion.identity);
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {index++}";
        }
    }

    public void PlaceMarkers(int subDivides)
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        for (int i = 0; i < 12 * (subDivides + 1); i++)
        {
            GameObject g = Instantiate(MarkerSphere, currentMesh.vertices[i], Quaternion.identity);
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {i}";
        }

    }

    public void PlaceMarkers(List<int> vertIds)
    {
        Destroy(MarkerObj);
        MarkerObj = new GameObject("Marker Parent Obj");
        MarkerObj.transform.parent = transform;

        int count = 0;
        foreach (int id in vertIds)
        {
            GameObject g = Instantiate(MarkerSphere, currentMesh.vertices[id], Quaternion.identity);
            g.transform.parent = MarkerObj.transform;
            g.name = $"Marker: {count++}";
        }
    }

}
