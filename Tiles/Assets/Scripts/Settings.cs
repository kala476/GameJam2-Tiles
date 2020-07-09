using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings instance;
    public bool conserveVelocity = false;
    public bool tilesPushedByPlayer = true;
    public AnimationCurve forceToSqrDistance;
    public float maxSqrPushDistance;
    public float MaxPushForce;
    public GameObject player;


    private void Awake()
	{
        instance = this;

	}
}
