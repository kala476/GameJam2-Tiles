using System.Collections;
using System.Collections.Generic;
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
        
        //visuals
		if (tile.mesh != null)
        {
            tile.SetColor(value == true ? Settings.instance.activeTileColor : Settings.instance.inactiveTileColor);

        }
        
    }
}
