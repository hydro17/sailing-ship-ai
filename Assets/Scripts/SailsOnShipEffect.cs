using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailsOnShipEffect : MonoBehaviour
{
  public GameObject[] sails;
  Rigidbody shipRb;
  Vector3 forcePushigShipForward;

  // DO NOT REMOVE - can be used in the future ---- start
  // private float shipVelocity;
  // Vector3 forcePushingShipAside; //tilting the ship to the sides
  // DO NOT REMOVE - can be used in the future ---- end

  void Start()
  {
    shipRb = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    Vector3 forceActingOnShip = Vector3.zero;
    foreach (GameObject sail in sails)
    {
      forceActingOnShip += sail.GetComponent<WindOnSailEffect>().forceActingOnSail;
    }

    Vector3 shipAxis = transform.forward;
    float shipAxisToForceActingOnShipAngle = Vector3.SignedAngle(shipAxis, forceActingOnShip, transform.up) * Mathf.Deg2Rad;
    forcePushigShipForward = shipAxis * forceActingOnShip.magnitude * Mathf.Cos(shipAxisToForceActingOnShipAngle);

    // DO NOT REMOVE - can be used in the future ---- start
    // shipVelocity = new Vector2(shipRb.velocity.x, shipRb.velocity.z).magnitude;
    // forcePushingShipAside = new Vector3(0, 0, shipVelocity * 10) * forceActingOnShip.magnitude * Mathf.Sin(shipAxisToForceActingOnShipAngle) * (Time.deltaTime / 2);
    // DO NOT REMOVE - can be used in the future ---- end

    shipRb.AddForce(forcePushigShipForward);

    // DO NOT REMOVE - can be used in the future ---- start
    // shipRb.AddRelativeTorque(-1 * forcePushingShipAside); //tilting the ship to the sides
    // DO NOT REMOVE - can be used in the future ---- end
  }
}
