using UnityEngine;

public class TileActivation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        if (other.tag.Equals("Tile"))
        {
            TileBehaviour tile = other.GetComponent<TileBehaviour>();

            if (tile)
            {
                RegisterTile(tile);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Tile"))
        {
            TileBehaviour tile = other.GetComponent<TileBehaviour>();

            if (tile)
            {
                UnRegisterTile(tile);
            }
        }
    }

    void RegisterTile(TileBehaviour tile)
    {
        bool value = true;

        tile.state.Set(TileStateDescriptor.InRange);
        //if (tile.solid != null)
        //{
        //    tile.solid.tileState = TileState.inRangeIdle;
        //    tile.solid.isInPlayerRadius = value;
        //}

        if (Settings.instance.conserveVelocity)
            tile.RestoreVelocity();

        tile.state.isInPlayerRadius = value;
    }

    void UnRegisterTile(TileBehaviour tile)
    {
        tile.state.isInPlayerRadius = false;
        tile.state.Set(TileStateDescriptor.OutOfRange);

        //if (tile.solid != null)
        //{
        //    tile.solid.tileState = TileState.outOfRange;
        //    //tile.solid.isInPlayerRadius = value;
        //}

        tile.SaveVelocity();
    }
}
