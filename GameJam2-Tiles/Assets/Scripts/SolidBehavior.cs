using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidBehavior : MonoBehaviour
{
	public List<TileBehaviour> belongingObject = new List<TileBehaviour>();
	public GameObject center;

	public void SetColor(Color color)
	{
		belongingObject.ForEach(x => x.SetColor(color));
	}
}
