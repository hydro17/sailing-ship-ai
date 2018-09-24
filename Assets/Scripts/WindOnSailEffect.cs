using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindOnSailEffect : MonoBehaviour
{
  public bool isInWindArea;
  WindArea windArea;
  Vector3 sailDirection;
  public float sailToWindAngle;
  public Vector3 forceActingOnSail;

  float sailFactor = 0;
  float windStrength;

  public float SailFactor
  {
    get
    {
      return sailFactor;
    }

    set
    {
      sailFactor = value;
    }
  }

  void Start()
  {
    isInWindArea = false;
  }

  void Update()
  {
    if (isInWindArea)
    {
      windStrength = windArea.windStrength;
      sailDirection = transform.forward;
      sailToWindAngle = Vector3.Angle(sailDirection, windArea.windDirection) * Mathf.Deg2Rad;

      if (Mathf.Cos(sailToWindAngle) < 0) windStrength = windStrength * 0.3f;

      forceActingOnSail = sailDirection * windStrength * Mathf.Cos(sailToWindAngle) * sailFactor;
    }
  }

  void OnTriggerEnter(Collider coll)
  {
    if (coll.gameObject.tag == "windArea")
    {
      isInWindArea = true;
      windArea = coll.gameObject.GetComponent<WindArea>();
    }
  }

  void OnTriggerExit(Collider coll)
  {
    if (coll.gameObject.tag == "windArea")
    {
      isInWindArea = false;
      windArea = null;
      forceActingOnSail = Vector3.zero;
    }
  }

}
