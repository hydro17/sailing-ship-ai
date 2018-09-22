using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ RequireComponent (typeof(Rigidbody))]

public class FloatObjectScript : MonoBehaviour {
    public int WaterLevel = 0;
    public float FloatTreshold = 2.0f;
    public float WaterDensity = 0.125f;
    public float DownForce = 4.0f;

    private float _forceFactory;
    private Vector3 _floatForce;
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        _forceFactory = 1.0f - ((transform.position.y - WaterLevel) / FloatTreshold);

        if (_forceFactory > 0.0f)
        {
            _floatForce = -Physics.gravity * GetComponent<Rigidbody>().mass * (_forceFactory - GetComponent<Rigidbody>().velocity.y * WaterDensity);
            _floatForce += new Vector3(0.0f, -DownForce * GetComponent<Rigidbody>().mass, 0.0f);
            GetComponent<Rigidbody>().AddForceAtPosition(_floatForce, transform.position);
        }
	}
}
