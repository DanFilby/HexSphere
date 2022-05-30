using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSphere : MonoBehaviour
{
    public Material mat;

    [Header("debugging")]
    public VertexMarker marker;

    //called from sphere creator which gives all verts 
    public void CreateHexSphere(List<List<Vector3>> pentagons)
    {
        CreatePentagons(pentagons);

    }

    private void CreatePentagons(List<List<Vector3>> pentagons)
    {
        GameObject parentObj = new GameObject("Pentagons");
        parentObj.transform.parent = transform;

        List<List<Vector3>> clockWiseVerts = new List<List<Vector3>>();

        //reorder the verts to clockwise order starting at the first outer point
        foreach (var pentagon in pentagons)
        {
            Dictionary<float, int> vertAngles = new Dictionary<float, int>();

            //the second point is used as start point so find a vector parrallel to check
            //if of points are right or left of it
            Vector3 perpPivot = Vector3.Cross(pentagon[1] - pentagon[0], pentagon[0]);

            for (int i = 2; i <= 5; i++)
            { 
                //check if right or left of the starting point either 1 / -1
                int rightFromPivot = (Vector3.Dot(pentagon[i], perpPivot)) > 0 ? 1 : -1;

                float angleFromPivot = Vector3.Angle(pentagon[1], pentagon[i]) * rightFromPivot;

                vertAngles.Add(angleFromPivot, i);
            }

            List<Vector3> reordered = new List<Vector3>();
            reordered.Add(pentagon[0]); //add pentagon centre and pivot point
            reordered.Add(pentagon[1]);

            List<float> positiveAngles = new List<float>();
            List<float> negativeAnlges = new List<float>();

            foreach(float angle in vertAngles.Keys)
            {
                if(angle > 0) { positiveAngles.Add(angle); }
                else
                {
                    negativeAnlges.Add(angle);
                }
            }

            positiveAngles.Sort();
            negativeAnlges.Sort();

            reordered.Add(pentagon[vertAngles[negativeAnlges[1]]]);
            reordered.Add(pentagon[vertAngles[negativeAnlges[0]]]);
            reordered.Add(pentagon[vertAngles[positiveAngles[1]]]);
            reordered.Add(pentagon[vertAngles[positiveAngles[0]]]);


            clockWiseVerts.Add(reordered);
        }


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


    private void CreateMeshObj(Mesh _mesh, Transform _parent, Material _material, string _objName)
    {
        GameObject g = new GameObject(_objName);
        g.AddComponent<MeshFilter>().mesh = _mesh;
        g.AddComponent<MeshRenderer>().sharedMaterial = _material;
        g.transform.parent = _parent;
    }


}
