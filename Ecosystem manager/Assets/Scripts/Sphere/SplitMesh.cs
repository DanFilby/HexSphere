using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitMesh : MonoBehaviour
{
    public bool AnimateSplit;

    private Mesh mesh;
    private Material meshMat;


    public void Split(Mesh _mesh, Material _mat, Vector3 worldPos)
    {
        transform.position = worldPos;

        mesh = _mesh;
        meshMat = _mat;

        CreateSplitobj();
    }

    private void CreateSplitobj()
    {
        Mesh[] splitSphere = MeshSplit();

        int count = 0;

        foreach (Mesh m in splitSphere)
        {
            GameObject splitPiece = new GameObject($"Piece-{++count}");
            splitPiece.AddComponent<MeshFilter>().mesh = m;
            splitPiece.AddComponent<MeshRenderer>().material = meshMat;
            splitPiece.transform.parent = transform;
        }

        gameObject.AddComponent<SplitMeshFun>();
        gameObject.AddComponent<WorldSpinner>();
    }

    private Mesh[] MeshSplit()
    {
        Mesh[] meshes = new Mesh[mesh.triangles.Length / 3];

        int indexM = 0;

        //loop through each triangle and add their vertices to a new mesh
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            meshes[indexM] = new Mesh();
            meshes[indexM].vertices = new Vector3[] { mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]], mesh.vertices[mesh.triangles[i + 2]] };
            meshes[indexM].triangles = new int[] { 0, 1, 2 };
            meshes[indexM].RecalculateNormals();
            indexM++;
        }

        return meshes;
    }

}
