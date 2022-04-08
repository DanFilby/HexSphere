using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [Header("Tile Settings")]
    public GameObject HexTile;
    public float TileRadius;

    [Header("Map Settings")]
    public int Width;
    public int Height;

    void Start()
    {
        CreateTileMap(new Vector3());
    }

    void Update()
    {
        
    }

    private void CreateTileMap(Vector3 centrePos)
    {
        //distance between tiles
        float spacingX = TileRadius * 2f;
        float spacingY = TileRadius * 1.5f;

        for (int i = -Width / 2; i < Width / 2; i++)
        {
            for (int j = -Height / 2; j < Height / 2; j++)
            {
                //If the row is odd then each tile shifts up one radius
                Vector3 tilePos = new Vector3(i * spacingX + ((j % 2 != 0) ? TileRadius : 0), 0, j * spacingY) - centrePos;
                GameObject g = Instantiate(HexTile, tilePos, Quaternion.identity, transform);
                g.GetComponent<HexTile>().ID = i * Width + j;
            }
        }
    }

}
