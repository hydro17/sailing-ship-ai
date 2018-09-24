using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController1 : MonoBehaviour
{
  public GameObject[] sails;
  public float shipVelocity;
  public float maxSpeed;

  Rigidbody shipRb;

  void Start()
  {
    shipRb = GetComponent<Rigidbody>();
    maxSpeed = 10.0f;
    shipVelocity = 0;
  }

  void Update()
  {
    Movement();
  }

  void FixedUpdate()
  {
    shipVelocity = shipRb.velocity.magnitude;

    if (shipVelocity > maxSpeed)
    {
      shipVelocity = maxSpeed;
    }

    float turn = Input.GetAxis("Horizontal");
    shipRb.AddTorque(Vector3.up * shipVelocity * turn * 50 * Time.deltaTime);

    shipRb.velocity = transform.forward * shipVelocity;
  }

  void Movement()
  {
    if (Input.GetKey(KeyCode.W))
    {
      foreach (GameObject sail in sails)
      {
        sail.GetComponent<WindOnSailEffect>().SailFactor = 1.0f;
      }
    }

    if (Input.GetKey(KeyCode.S))
    {
      foreach (GameObject sail in sails)
      {
        sail.GetComponent<WindOnSailEffect>().SailFactor = 0.0f;
      }
    }
  }
}
