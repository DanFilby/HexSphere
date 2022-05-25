using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearestVertex : MonoBehaviour
{
    public Camera mainCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShootRay();
        }
    }

    private void ShootRay()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            MeshCollider sphereCollider = hit.collider as MeshCollider;

            FindClosestVertice(sphereCollider.sharedMesh, hit.point);
            
        }

    }

    private void FindClosestVertice(Mesh mesh, Vector3 point)
    {
        float closest = 100;
        int index = 0;
        int count = 0;
        foreach(Vector3 vert in mesh.vertices)
        {
            float dist = Vector3.Distance(point, vert);
            if (dist < closest)
            {
                index = count;
                closest = dist;
            }
            count++;
        }
        Debug.Log(index);
    }


}
