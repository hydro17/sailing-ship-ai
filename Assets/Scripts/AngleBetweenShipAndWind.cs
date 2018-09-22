using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleBetweenShipAndWind : MonoBehaviour
{
  WindArea windArea;

  void Awake()
  {
    windArea = GameObject.Find("Wind Area").GetComponent<WindArea>();
  }

  public float GetCurrentShipToWindAngle()
  {
    Vector3 shipDirection = transform.forward;
    shipDirection.y = 0;
    // Vector3 shipDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
    return Vector3.SignedAngle(shipDirection, windArea.windDirection, Vector3.up);
  }
}
