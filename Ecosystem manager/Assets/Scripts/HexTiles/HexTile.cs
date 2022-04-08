using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{

    public int ID;

    private Material _material;

    private void Start()
    {
        _material = GetComponentInChildren<Renderer>().material;

    }


    public void ToggleSelected(bool state)
    {
        _material.SetInt("Boolean_c3fb7131a360480597363249bbefe04a", (state)? 1 : 0);
    }

    public void ToggleHovered(bool state)
    {
        _material.SetInt("Boolean_b8403d294b0c48b598709335acf203f7", (state) ? 1 : 0);
    }


    public override bool Equals(object other)
    {
        if(other.GetType() != typeof(HexTile)) { return false; }
        return this.ID == (other as HexTile).ID;
    }

}
