using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XGD.TileQuest
{
	public class SolidBehavior : MonoBehaviour
	{
		public List<TileBehaviour> tiles = new List<TileBehaviour>();
		public Solid solid;

		public void SetColor(Color color)
		{
			tiles.ForEach(x => x.SetColor(color));
		}
	}

}