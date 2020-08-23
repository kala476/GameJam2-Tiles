using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solid : TileBehaviour
{
	public List<TileBehaviour> belongingObject = new List<TileBehaviour>();
	public GameObject centre;

	public override void SetColor(Color color)
	{
		for (int i = 0; i < belongingObject.Count; i++)
		{
			belongingObject[i].mesh.material.color = color;
		}
	}
}
