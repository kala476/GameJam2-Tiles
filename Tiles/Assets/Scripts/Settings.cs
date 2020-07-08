using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings instance;

    public Color activeTileColor;
    public Color inactiveTileColor;

    public bool tilesPushedByPlayer = true;
    public AnimationCurve forceToSqrDistance;
    public float maxSqrPushDistance;
    public float MaxPushForce;
    public GameObject player;

    private void Awake()
	{
        instance = this;

	}
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
