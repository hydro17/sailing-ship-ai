using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AngleBetweenShipAndWind))]
[RequireComponent(typeof(EnemyBoatController))]
public class PlayerAI : MonoBehaviour
{
  public GameObject[] cannons;
  float[] cannonsTimeOfNextFire;

  public bool closeToPlayerShip;

  public float minDistBetweenParallel;
  public float maxDistBetweenParallel;
  public float distanceBetweenParallel;

  public bool ourShipIsInZoneBetweenParallelZones = false;
  public bool ourShipIsInParallelZone = false;
  public bool ourShipDirectionIsConsistentWithTowardsPlayerShipDirection = false;
  public bool ourShipAndPlayerShipDirectionsWereInconsistentOnExit = false;

  float changeDirectionTime;
  AngleBetweenShipAndWind angleBetweenShipAndWind;

  WindArea windArea;
  EnemyBoatController enemyBoatController;
  GameObject playerShip = null;
  GameObject obstacle1 = null, obstacle2 = null;

  delegate void ActiveState();
  ActiveState activeState;

  const float degToRad = 0.0174532f;

  void Start()
  {
    enemyBoatController = GetComponent<EnemyBoatController>();
    angleBetweenShipAndWind = GetComponent<AngleBetweenShipAndWind>();
    windArea = GameObject.Find("Wind Area").GetComponent<WindArea>();

    minDistBetweenParallel = 10;
    maxDistBetweenParallel = 30;

    cannonsTimeOfNextFire = new float[6];
    for (int i = 0; i < cannonsTimeOfNextFire.Length; i++)
    {
      cannonsTimeOfNextFire[i] = Time.time;
    }

    enemyBoatController.SailsDown();

    changeDirectionTime = Time.time + RandomPeriodOfTime();

    activeState = state_PlayerSearching;
  }

  void Update()
  {
    activeState();
  }

  // STATE start ---------------------------------------------
  void state_AvoidingObstacle()
  {

    if (!obstacle1)
    {
      activeState = state_PlayerSearching;
      return;
    }

    // if (!obstacle1) {
    //   if (!playerShip) {
    //     activeState = state_PlayerSearching;
    //     return;
    //   } else {
    //     activeState = state_GoToPlayerShip;
    //     return;
    //   }
    // }

    // Vector3 shipPosition = transform.position;
    Vector3 shipDirection = transform.forward;
    float angleInDegToAvoidObstacle;

    if (obstacle1 && !obstacle2)
    {
      angleInDegToAvoidObstacle = GetAngleInDegToAvoidObstacle(obstacle1);

    }
    else
    {

      float angleBetweenShortestShiptToObstacle1VectorAndShipDirection =
        GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle1);
      float angleBetweenShortestShiptToObstacle2VectorAndShipDirection =
        GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle2);

      if ((angleBetweenShortestShiptToObstacle1VectorAndShipDirection > 0
        && angleBetweenShortestShiptToObstacle2VectorAndShipDirection < 0)
        ||
        (angleBetweenShortestShiptToObstacle1VectorAndShipDirection < 0
        && angleBetweenShortestShiptToObstacle2VectorAndShipDirection > 0))
      {

        if (Mathf.Abs(angleBetweenShortestShiptToObstacle1VectorAndShipDirection) <
          Mathf.Abs(angleBetweenShortestShiptToObstacle2VectorAndShipDirection))
        {
          angleInDegToAvoidObstacle = GetGreaterAngleInDegToAvoidObstacle(obstacle1);
        }
        else
        {
          angleInDegToAvoidObstacle = GetGreaterAngleInDegToAvoidObstacle(obstacle2);
        }

      }
      else if ((angleBetweenShortestShiptToObstacle1VectorAndShipDirection > 0
      && angleBetweenShortestShiptToObstacle2VectorAndShipDirection == 0)
      ||
      (angleBetweenShortestShiptToObstacle2VectorAndShipDirection > 0
      && angleBetweenShortestShiptToObstacle1VectorAndShipDirection == 0))
      {
        angleInDegToAvoidObstacle = 90 + 20;

      }
      else if ((angleBetweenShortestShiptToObstacle1VectorAndShipDirection < 0
      && angleBetweenShortestShiptToObstacle2VectorAndShipDirection == 0)
      ||
      (angleBetweenShortestShiptToObstacle2VectorAndShipDirection < 0
      && angleBetweenShortestShiptToObstacle1VectorAndShipDirection == 0))
      {
        angleInDegToAvoidObstacle = -90 - 20;

      }
      else if ((angleBetweenShortestShiptToObstacle1VectorAndShipDirection > 0
      && angleBetweenShortestShiptToObstacle2VectorAndShipDirection > 0)
      ||
      (angleBetweenShortestShiptToObstacle1VectorAndShipDirection < 0
      && angleBetweenShortestShiptToObstacle2VectorAndShipDirection < 0))
      {

        if (Mathf.Abs(angleBetweenShortestShiptToObstacle1VectorAndShipDirection) <
          Mathf.Abs(angleBetweenShortestShiptToObstacle2VectorAndShipDirection))
        {
          angleInDegToAvoidObstacle = GetAngleInDegToAvoidObstacle(obstacle1);
        }
        else
        {
          angleInDegToAvoidObstacle = GetAngleInDegToAvoidObstacle(obstacle2);
        }

      }
      else
      {
        angleInDegToAvoidObstacle = 0;
        Debug.Log("Such situation should not exist!!!");
      }
    }

    Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * degToRad);
    enemyBoatController.SetNewShipDirection(rotatedVector);
  }
  // STATE end -----------------------------------------------

  private float GetAngleInDegToAvoidObstacle(GameObject obstacle)
  {
    Vector3 shipDirection = transform.forward;

    float angleBetweenShortestShiptToObstacleVectorAndShipDirection =
      GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle); //this is a signed angle - range: 0-180 or 0-(-180)

    float angleInDegToAvoidObstacle;

    if (angleBetweenShortestShiptToObstacleVectorAndShipDirection > 0)
    {
      angleInDegToAvoidObstacle = (90 - angleBetweenShortestShiptToObstacleVectorAndShipDirection) + 20;

      // due to wind correction of angleInDegToAvoidObstacle
      Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * degToRad);
      if (GetVectorToWindAngle(rotatedVector) > 150)
      {
        angleInDegToAvoidObstacle += GetVectorToWindAngle(rotatedVector) - 150;
      }

    }
    else if (angleBetweenShortestShiptToObstacleVectorAndShipDirection < 0)
    {
      angleInDegToAvoidObstacle = (-90 - angleBetweenShortestShiptToObstacleVectorAndShipDirection) - 20;

      // due to wind correction of angleInDegToAvoidObstacle
      Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * degToRad);
      if (GetVectorToWindAngle(rotatedVector) < -150)
      {
        angleInDegToAvoidObstacle += GetVectorToWindAngle(rotatedVector) + 150;
      }

    }
    else
    { //perpendicular
      angleInDegToAvoidObstacle = 90 + 20;
    }

    return angleInDegToAvoidObstacle;
  }

  private float GetGreaterAngleInDegToAvoidObstacle(GameObject obstacle)
  {
    float angleBetweenShortestShiptToObstacleVectorAndShipDirection =
      GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle); //this is a signed angle - range: 0-180 or 0-(-180)

    float angleInDegToAvoidObstacle;
    if (angleBetweenShortestShiptToObstacleVectorAndShipDirection > 0)
    {
      angleInDegToAvoidObstacle = -angleBetweenShortestShiptToObstacleVectorAndShipDirection - 90 - 20;
    }
    else if (angleBetweenShortestShiptToObstacleVectorAndShipDirection < 0)
    {
      angleInDegToAvoidObstacle = -angleBetweenShortestShiptToObstacleVectorAndShipDirection + 90 + 20;
    }
    else
    { //perpendicular
      angleInDegToAvoidObstacle = 90 + 20;
    }

    return angleInDegToAvoidObstacle;
  }

  private float GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(GameObject obstacle)
  {
    // ShortestShiptToObstacleVector == perpendicular vector
    Vector3 shipPosition = transform.position;
    Vector3 shipDirection = transform.forward;

    Vector3 pointOfObstacleClosestToShip = obstacle.GetComponent<Collider>().ClosestPointOnBounds(shipPosition);
    Vector3 shortestShiptToObstacleVector = pointOfObstacleClosestToShip - shipPosition;

    Debug.DrawRay(shipPosition, shortestShiptToObstacleVector, Color.white, 0.0f);

    float angleBetweenShortestShiptToObstacleVectorAndShipDirection =
      Vector3.SignedAngle(shortestShiptToObstacleVector, shipDirection, Vector3.up);

    return angleBetweenShortestShiptToObstacleVectorAndShipDirection;
  }

  // STATE start ---------------------------------------------
  void state_PlayerSearching()
  {
    if (obstacle1)
    {
      activeState = state_AvoidingObstacle;
      return;
    }

    if (playerShip)
    {
      Vector3 playerShipDirection = playerShip.transform.forward;
      playerShipDirection.y = 0;

      Vector3 ourShipDirection = transform.forward;
      ourShipDirection.y = 0;

      float angleBetweenPlayerShipDirectionAndOurShipDirection = Vector3.Angle(playerShipDirection, ourShipDirection);
      if (angleBetweenPlayerShipDirectionAndOurShipDirection > 90)
      {
        angleBetweenPlayerShipDirectionAndOurShipDirection = 180 - angleBetweenPlayerShipDirectionAndOurShipDirection;
      }

      if ((angleBetweenPlayerShipDirectionAndOurShipDirection <= 5) && (GetDistanceBetweenPerpendicular() <= 3))
      {
        activeState = state_Battle;
        return;
      }
    }

    //change ship direction periodically
    if (Time.time > changeDirectionTime)
    {
      changeDirectionTime = Time.time + RandomPeriodOfTime();
      float currentShipToWindAngle = angleBetweenShipAndWind.GetCurrentShipToWindAngle();
      float angle = Random.Range(-45.0f, 45.0f);

      Vector3 rotatedVector;
      if (currentShipToWindAngle - angle > 150)
      {
        rotatedVector = GetRotatedVector(windArea.windDirection, -150 * degToRad);
      }
      else if (currentShipToWindAngle - angle < -150)
      {
        rotatedVector = GetRotatedVector(windArea.windDirection, 150 * degToRad);
      }
      else
      {
        rotatedVector = GetRotatedVector(transform.forward, angle * degToRad);
      }

      enemyBoatController.SetNewShipDirection(rotatedVector);
    }
  }
  // STATE end -----------------------------------------------

  private bool IsOurShipDirectionConsistentWithTowardsPlayerShipDirection(Vector3 shipDirection, Vector3 towardsPlayerShipDirection)
  {
    if (Vector3.Dot(shipDirection, towardsPlayerShipDirection) > 0) return true;
    else return false;
  }

  private void DetermineInWhichZoneIsOurShip()
  {
    distanceBetweenParallel = GetDistanceBetweenParallel();

    if (distanceBetweenParallel > maxDistBetweenParallel)
    {
      ourShipIsInParallelZone = false;
      ourShipIsInZoneBetweenParallelZones = false;
    }
    else if (distanceBetweenParallel >= minDistBetweenParallel && distanceBetweenParallel <= maxDistBetweenParallel)
    {
      ourShipIsInParallelZone = true;
      ourShipIsInZoneBetweenParallelZones = false;
    }
    else
    {  //distance < minDistBetweenParallel
      ourShipIsInParallelZone = false;
      ourShipIsInZoneBetweenParallelZones = true;
    }
  }

  float GetDistanceBetweenParallel()
  {
    // Protection against calling this method when playerShip == null
    if (!playerShip)
    {
      return 1000000;
    }

    float distanceBetweenParallel;

    Vector3 towardsPlayerShipDirection = playerShip.transform.position - transform.position;
    towardsPlayerShipDirection.y = 0;

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    float towardsPlayerShipDirectionToPlayerShipAngle = Vector3.SignedAngle(towardsPlayerShipDirection, playerShipDirection, Vector3.up);

    float towardsPlayerShipDirectionToPlayerShipAngleRad = towardsPlayerShipDirectionToPlayerShipAngle * degToRad;
    if (Mathf.Abs(towardsPlayerShipDirectionToPlayerShipAngle) <= 90)
    {
      distanceBetweenParallel = towardsPlayerShipDirection.magnitude * Mathf.Sin(Mathf.Abs(towardsPlayerShipDirectionToPlayerShipAngleRad));
    }
    else
    {
      distanceBetweenParallel = towardsPlayerShipDirection.magnitude * Mathf.Sin(Mathf.PI - Mathf.Abs(towardsPlayerShipDirectionToPlayerShipAngleRad));
    }

    return distanceBetweenParallel;
  }

  float GetDistanceBetweenPerpendicular()
  {
    // Protection against calling this method when playerShip == null
    if (!playerShip)
    {
      return 1000000;
    }

    float distanceBetweenPerpendicular;

    Vector3 towardsPlayerShipDirection = playerShip.transform.position - transform.position;
    towardsPlayerShipDirection.y = 0;

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    float towardsPlayerShipDirectionToPlayerShipAngle = Vector3.SignedAngle(towardsPlayerShipDirection, playerShipDirection, Vector3.up);

    float towardsPlayerShipDirectionToPlayerShipAngleRad = towardsPlayerShipDirectionToPlayerShipAngle * degToRad;
    if (Mathf.Abs(towardsPlayerShipDirectionToPlayerShipAngle) <= 90)
    {
      distanceBetweenPerpendicular = towardsPlayerShipDirection.magnitude * Mathf.Cos(Mathf.Abs(towardsPlayerShipDirectionToPlayerShipAngleRad));
    }
    else
    {
      distanceBetweenPerpendicular = towardsPlayerShipDirection.magnitude * Mathf.Cos(Mathf.PI - Mathf.Abs(towardsPlayerShipDirectionToPlayerShipAngleRad));
    }

    return distanceBetweenPerpendicular;
  }

  // STATE start ---------------------------------------------
  void state_Battle()
  {
    // START of checking the state change conditions ---------------------

    if (obstacle1)
    {
      activeState = state_AvoidingObstacle;
      return;
    }

    if (!playerShip)
    {
      activeState = state_PlayerSearching;
      return;
    }

    // This check can be performed only if playerShip != null
    DetermineInWhichZoneIsOurShip();

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    Vector3 ourShipDirection = transform.forward;
    ourShipDirection.y = 0;

    float angleBetweenPlayerShipDirectionAndOurShipDirection = Vector3.Angle(playerShipDirection, ourShipDirection);
    if (angleBetweenPlayerShipDirectionAndOurShipDirection > 90)
    {
      angleBetweenPlayerShipDirectionAndOurShipDirection = 180 - angleBetweenPlayerShipDirectionAndOurShipDirection;
    }

    if ((angleBetweenPlayerShipDirectionAndOurShipDirection > 5) || (GetDistanceBetweenPerpendicular() > 3))
    {
      activeState = state_PlayerSearching;
      return;
    }
    // END of checking the state change conditions---------------

    Debug.DrawLine(transform.position, playerShip.transform.position, Color.blue, 0.0f, true);

    for (int i = 0; i < cannonsTimeOfNextFire.Length; i++)
    {
      if (Time.time < cannonsTimeOfNextFire[i]) continue;

      cannonsTimeOfNextFire[i] = Time.time + 4 + Random.Range(0.0f, 1.0f);

      Transform cannonTransform = cannons[i].GetComponent<Transform>();
      Ray cannonRay = new Ray(cannonTransform.position, cannonTransform.forward);

      float fireDistance = maxDistBetweenParallel;
      RaycastHit hit;
      if (Physics.Raycast(cannonRay, out hit, fireDistance))
      {
        if (hit.collider.tag == "PlayerShip" || hit.collider.tag == "EnemyShip")
        {
          StartCoroutine(cannons[i].GetComponent<CannonActions>().FireToOpponentShip(hit.collider.gameObject));
        }
      }
    }
  }
  // STATE end -----------------------------------------------

  public float GetVectorToWindAngle(Vector3 vector)
  {
    return Vector3.SignedAngle(vector, windArea.windDirection, Vector3.up);
  }

  public Vector3 GetRotatedVector(Vector3 initialVector, float angleRad)
  {
    //multiplying by -1 because we want angle > 0 to mean turn righ, not left
    angleRad = -angleRad;
    float x1 = initialVector.x;
    float z1 = initialVector.z;

    float x2 = x1 * Mathf.Cos(angleRad) - z1 * Mathf.Sin(angleRad);
    float z2 = x1 * Mathf.Sin(angleRad) + z1 * Mathf.Cos(angleRad);

    return new Vector3(x2, 0, z2);
  }

  float GetOurShipToPlayerShipAngle()
  {
    Vector3 ourShipDirection = new Vector3(transform.forward.x, 0, transform.forward.z);
    Vector3 playerShipDirection = new Vector3(playerShip.transform.forward.x, 0, playerShip.transform.forward.z);
    return Vector3.SignedAngle(ourShipDirection, playerShipDirection, Vector3.up);
  }

  float RandomPeriodOfTime()
  {
    return Random.Range(2, 4);
  }

  public void SetPlayerShipDetected(GameObject playerShip)
  {
    this.playerShip = playerShip;
    // if (this.playerShip) opponentDetected = true;
  }

  public void SetPlayerShipNotDetected()
  {
    this.playerShip = null;
  }

  public void DetermineWhetherClosePlayerShipDetected(bool isPlayerShipDetected)
  {
    Debug.Log("Player ship - close player detected");
    closeToPlayerShip = isPlayerShipDetected;
  }

  public void SetObstacleDetected(GameObject obstacle)
  {
    if (!obstacle1)
    {
      obstacle1 = obstacle;
    }
    else if (!obstacle2)
    {
      obstacle2 = obstacle;
    }
    else
    {
      Debug.Log("More than two obstacles at a time!!!");
    }
  }

  public void SetObstacleNotDetected(GameObject obstacle)
  {
    if (obstacle == obstacle1)
    {
      if (!obstacle2)
      {
        obstacle1 = null;
      }
      else
      {
        obstacle1 = obstacle2;
        obstacle2 = null;
      }
    }
    else if (obstacle == obstacle2)
    {
      obstacle2 = null;
    }
    else
    {
      Debug.Log("Unknown obstacle to remove!!!");
    }
  }
}
