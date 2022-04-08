using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCreator : MonoBehaviour
{
    Mesh sphere;

    Vector3[] vertices;
    Vector3[] normals;

    private void Start()
    {
        sphere = new Mesh();
        sphere.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        BuildMesh(ref sphere, 10);
        gameObject.GetComponent<MeshFilter>().mesh = sphere;

        vertices = sphere.vertices;
        normals = sphere.normals;
    }

    private void BuildMesh(ref Mesh mesh, float radius)
    {
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

        for (int i = 0; i < 4; i++)
        {
            SubDivide(ref verts, ref triangles, radius);

        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        Debug.Log(mesh.vertices.Length);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }

    }


}
