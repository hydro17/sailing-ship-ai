using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AngleBetweenShipAndWind))]
[RequireComponent(typeof(EnemyBoatController))]
public class OptimalSailToShipAngle : MonoBehaviour
{
  public float currentShipToWindAngle;
  float lastShipToWindAngle;
  float fastestSailToShipAngle;

  AngleBetweenShipAndWind angleBetweenShipAndWind;
  EnemyBoatController enemyBoatController;

  delegate void ActiveState();
  ActiveState activeState;

  const float degToRad = 0.0174532f;

  void Start()
  {
    enemyBoatController = GetComponent<EnemyBoatController>();
    angleBetweenShipAndWind = GetComponent<AngleBetweenShipAndWind>();

    currentShipToWindAngle = angleBetweenShipAndWind.GetCurrentShipToWindAngle();
    fastestSailToShipAngle = GetFastestSailsToShipAngle(currentShipToWindAngle);
    lastShipToWindAngle = currentShipToWindAngle;

    activeState = state_SetOptimalSailToShipAngle;
  }

  void Update()
  {
    activeState();
  }

  // STATE start ---------------------------------------------
  void state_SetOptimalSailToShipAngle()
  {
    currentShipToWindAngle = angleBetweenShipAndWind.GetCurrentShipToWindAngle();
    if (currentShipToWindAngle != lastShipToWindAngle)
    {
      fastestSailToShipAngle = GetFastestSailsToShipAngle(currentShipToWindAngle);
      lastShipToWindAngle = currentShipToWindAngle;
    }
    enemyBoatController.SetSailsAtGivenAngle(fastestSailToShipAngle);
  }
  // STATE end -----------------------------------------------

  float GetFastestSailsToShipAngle(float currentShipToWindAngle)
  {
    float max = -1;
    float fastestSailsToShipAngle = 0;

    for (int sailsToShipAngle = -70; sailsToShipAngle <= 70; sailsToShipAngle++)
    {
      float sailsToShipAngleInRad = sailsToShipAngle * degToRad;
      float sailsToWindAngleInRad = (currentShipToWindAngle - sailsToShipAngle) * degToRad;
      float a = Mathf.Cos(sailsToShipAngleInRad) * Mathf.Cos(sailsToWindAngleInRad);
      if (a > max)
      {
        max = a;
        fastestSailsToShipAngle = sailsToShipAngle;
      }
    }

    return fastestSailsToShipAngle;
  }
}
