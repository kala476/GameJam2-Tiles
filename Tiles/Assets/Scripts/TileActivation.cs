using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileActivation : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag.Equals("Tile"))
        {
            TileBehaviour tile = other.GetComponent<TileBehaviour>();

            if (tile != null)
            {
                SetTileActive(tile, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Tile"))
        {
           TileBehaviour tile = other.GetComponent<TileBehaviour>();

            if (tile != null)
            {
                   SetTileActive(tile, false);
            }
        }
    }

    void SetTileActive(TileBehaviour tile, bool value)
    {
        tile.isInPlayerRadius = value;

        if (value == true) 
        {
            tile.tileState = TileBehaviour.TileState.inRangeIdle;
            if (tile.solid != null) 
            {
                tile.solid.tileState = TileBehaviour.TileState.inRangeIdle;
                tile.solid.isInPlayerRadius = value;
            }

            if (Settings.instance.conserveVelocity == true)
                tile.RestoreVelocity();
        }

        if (value == false) 
        {
            tile.tileState = TileBehaviour.TileState.outOfRange;

            if (tile.solid != null)
            {
                tile.solid.tileState = TileBehaviour.TileState.outOfRange;
                tile.solid.isInPlayerRadius = value;
            }

            tile.SaveVelocity();
        }
    }
}
