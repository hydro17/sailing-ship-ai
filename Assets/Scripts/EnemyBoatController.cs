using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoatController : MonoBehaviour
{
  public GameObject mast;
  public GameObject[] sails;
  public float shipVelocity;
  // public float angularVelocityValue;
  public float maxSpeed = 10;

  // float horizontalSailsInput;
  // private WindEffect windEffect;
  private Rigidbody shipRb;
  Vector3 currentShipDirection;
  public Vector3 designatedShipDirection;
  public float angleBetweenCurrentAndDesignatedShipDirection;

  //  float velocityToTorqueFactor = 1.0f; //something like maneuverability
  // Vector3 lastShipDirection;
  // public float centripetalForceFactor = 1.0f;

  //--change to local--start
  public float currentSailsToShipAngle;
  public float diffBetweenNewAndCurrentSailsToShipAngle;
  //--change to local--end

  // float duration = 1;
  // float startTime;
  // float previousAngleToRotate = 0;
  // public float currentAngleToRotate = 0;

  void Start()
  {
    shipRb = GetComponent<Rigidbody>();
    currentShipDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
    designatedShipDirection = currentShipDirection;

    // TEST
    // startTime = Time.time;
    // TEST
  }

  void FixedUpdate()
  {
    Turn();
  }

  public void SailsDown()
  {
    foreach (GameObject sail in sails)
    {
      sail.GetComponent<WindOnSailEffect>().sailFactor = 1.0f;
      Debug.Log("SSSSSSSSSSAAAAAAAAAAAAAILLLLLLLLLSSSSS DOWN");
    }
  }

  public void SailsUp()
  {
    foreach (GameObject sail in sails)
    {
      sail.GetComponent<WindOnSailEffect>().sailFactor = 0;
    }
  }

  public void Turn()
  {
    // If the direction of the ship's velocity vector is compatible with the ship's direction
    // shipVelocityDirectionFactor = 1
    // otherwise shipVelocityDirectionFactor = -1
    // We ignore the ship's drift
    int directionFactor;
    float velocityToAngularVelocityFactor = 4;
    int shipVelocityDirectionFactor = (Vector3.Dot(transform.forward, shipRb.velocity) > 0) ? 1 : -1;

    shipVelocity = new Vector2(shipRb.velocity.x, shipRb.velocity.z).magnitude;

    if (this.gameObject.tag == "EnemyShip")
    {
      if (shipVelocity > maxSpeed)
        shipVelocity = maxSpeed;
    }

    if (this.gameObject.tag == "PlayerShip")
    {
      if (shipVelocity > maxSpeed / 2f)
        shipVelocity = maxSpeed / 2f;
    }

    // TEST
    currentShipDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
    // TEST

    angleBetweenCurrentAndDesignatedShipDirection = Vector3.SignedAngle(currentShipDirection, designatedShipDirection, Vector3.up);
    if (Mathf.Abs(angleBetweenCurrentAndDesignatedShipDirection) > 0.5)
    {
      // TEST
      if (angleBetweenCurrentAndDesignatedShipDirection > 0) directionFactor = 1;
      else directionFactor = -1;

      float changeDirection = shipVelocity * velocityToAngularVelocityFactor * directionFactor * Time.deltaTime;

      if (Mathf.Abs(angleBetweenCurrentAndDesignatedShipDirection) < 10)
        changeDirection = changeDirection * Mathf.Abs(angleBetweenCurrentAndDesignatedShipDirection) / 10;
      if (Mathf.Abs(changeDirection) > Mathf.Abs(angleBetweenCurrentAndDesignatedShipDirection))
        changeDirection = angleBetweenCurrentAndDesignatedShipDirection;

      transform.Rotate(0.0f, changeDirection, 0.0f);
      // TEST
      // currentAngleToRotate = Mathf.SmoothStep(0, angleBetweenCurrentAndDesignatedShipDirection, (Time.time - startTime) / duration) - previousAngleToRotate;
      // previousAngleToRotate = Mathf.SmoothStep(0, angleBetweenCurrentAndDesignatedShipDirection, (Time.time - startTime) / duration);
      // transform.Rotate(0.0f, currentAngleToRotate, 0.0f);
    }

    float yVelocity = shipRb.velocity.y;
    Vector3 localShipVelocity = transform.InverseTransformDirection(shipRb.velocity);
    localShipVelocity.x = 0.0f;
    localShipVelocity.y = 0.0f;
    // localShipVelocity.z = shipVelocity * shipVelocityDirectionFactor * ((shipVelocityDirectionFactor > 0) ? 1.0f : 0.3f);
    localShipVelocity.z = shipVelocity * shipVelocityDirectionFactor;
    Vector3 globalShipVelocity = transform.TransformDirection(localShipVelocity);
    globalShipVelocity.y = yVelocity;
    shipRb.velocity = globalShipVelocity;
  }

  public void SetNewShipDirection(Vector3 newShipDirection)
  {
    designatedShipDirection = newShipDirection;
    // currentShipDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
    // Debug.Log("newShipDirection: " + Vector3.SignedAngle(currentShipDirection, designatedShipDirection, Vector3.up));
    // duration = 2.0f;
    // startTime = Time.time;
    // previousAngleToRotate = 0;
  }

  public void SetSailsAtGivenAngle(float newSailsToShipAngle)
  {
    currentSailsToShipAngle = (mast.transform.localEulerAngles.y < 180) ?
          mast.transform.localEulerAngles.y : mast.transform.localEulerAngles.y - 360;
    diffBetweenNewAndCurrentSailsToShipAngle = newSailsToShipAngle - currentSailsToShipAngle;
    if (Mathf.Abs(diffBetweenNewAndCurrentSailsToShipAngle) > 0.5)
    {
      if (diffBetweenNewAndCurrentSailsToShipAngle > 0) currentSailsToShipAngle += 30 * Time.deltaTime;
      else currentSailsToShipAngle -= 30 * Time.deltaTime;
    }

    // horizontalSailsInput = Mathf.Clamp(horizontalSailsInput, -60, 60);
    mast.transform.localEulerAngles = new Vector3(mast.transform.localEulerAngles.x, currentSailsToShipAngle,
          mast.transform.localEulerAngles.z);
  }
}
