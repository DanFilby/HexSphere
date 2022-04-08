using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TileSelector : MonoBehaviour
{
    [Header("References")]
    public Camera MainCamera;

    //layer of the base of tiles and the main tile layer. base has the mesh collider for mouse accuracy and other s box collider
    private LayerMask _tileBaseLayer = 1 << 6;
    private LayerMask _tileLayer = 1 << 7;

    //tiles that the mouse hovers over
    private HexTile _hoverTile = null;
    private HexTile _prevHoverTile = null;

    //tiles that are selected
    private List<HexTile> _selectedTiles = new List<HexTile>();

    //when selecting multiple tiles. use this point as corner of square of tiles
    private Vector3 _initalClickPos;

    void Update()
    {
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out RaycastHit hit,50f,_tileBaseLayer))
        {
            //get tile
            Transform tileHit = hit.transform.parent;
            HexTile currentTile = tileHit.GetComponent<HexTile>();

            //update tile hovered over
            UpdateHoverTile(currentTile);

            if (Input.GetMouseButtonDown(0))
            {
                SelectTile();
               _initalClickPos = currentTile.GetComponentInParent<Transform>().position;
            }
            if (Input.GetMouseButtonUp(0))
            {
                SquareSelect(currentTile.GetComponentInParent<Transform>().position);
            }
        }
    }

    private void UpdateHoverTile(HexTile hoveredTile)
    {
        //if the current hovered tile is empty or it hasn't changed skip
        if (_hoverTile != null && _hoverTile == hoveredTile) return;

        //update current tile
        _hoverTile = hoveredTile;
        _hoverTile?.ToggleHovered(true);

        //dehovers preious tile. (checks if not null with '?')
        _prevHoverTile?.ToggleHovered(false);

        //update previous hover tile
        _prevHoverTile = _hoverTile;
    }

    private void SelectTile()
    {
        //deselect all selected tiles
        _selectedTiles.ForEach(i => i.ToggleSelected(false));
        _selectedTiles.Clear();

        //update the current selected
        _hoverTile.ToggleSelected(true);
        _selectedTiles.Add(_hoverTile);
    }

    private void SquareSelect(Vector3 finalClickPos)
    {
        Vector3 halfDif = (finalClickPos - _initalClickPos) / 2f;

        //check within the box created for all tiles.
        Collider[] colliders = Physics.OverlapBox(_initalClickPos + halfDif, new Vector3(Mathf.Abs(halfDif.x), 1, Mathf.Abs(halfDif.z)), Quaternion.identity, _tileLayer);

        foreach(Collider c in colliders)
        {
            _selectedTiles.Add(c.GetComponentInChildren<HexTile>());
        }

        _selectedTiles.ForEach(i => i.ToggleSelected(true));
    }

}
