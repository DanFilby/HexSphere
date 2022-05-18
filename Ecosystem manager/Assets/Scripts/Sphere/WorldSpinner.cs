using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpinner : MonoBehaviour
{
    public float speed = 750;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), -Input.GetAxis("Mouse X"), 0) * Time.deltaTime * speed);

        }
    }
}
