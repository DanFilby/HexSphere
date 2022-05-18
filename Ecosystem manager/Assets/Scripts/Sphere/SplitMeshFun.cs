using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitMeshFun : MonoBehaviour
{
    public bool Animate;

    private List<Transform> pieces;
    void Start()
    {
        MeshFilter[] filters = transform.GetComponentsInChildren<MeshFilter>();

        pieces = new List<Transform>(); 
        foreach(MeshFilter f in filters)
        {
            pieces.Add(f.transform);
        }

        StartCoroutine(ManipulatePieces());
    }


    IEnumerator ManipulatePieces()
    {
        WaitForSeconds timeStep = new WaitForSeconds(0.02f);

        while (true)
        {
            Transform piece = pieces[Random.Range(0,pieces.Count)];

            Vector3[] verts = piece.GetComponent<MeshFilter>().mesh.vertices;

            Vector3 move = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]) * 0.1f;

            if (Animate) { StartCoroutine(MovePiece(piece, move, 2.0f)); }

            yield return timeStep;
        }    
    }

    IEnumerator MovePiece(Transform t, Vector3 move, float time)
    {
        WaitForSeconds delay = new WaitForSeconds(time / 100.0f);
        for (int i = 0; i < 100; i++)
        {
            t.Translate(move / 100.0f);

            yield return delay;
        }

        for (int i = 0; i < 100; i++)
        {
            t.Translate(-move / 100.0f);

            yield return delay;
        }
    }

}
