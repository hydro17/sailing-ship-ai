using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AngleBetweenShipAndWind))]
[RequireComponent(typeof(EnemyBoatController))]
public class EnemyAI : MonoBehaviour
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

  float changeDirectionTime;
  AngleBetweenShipAndWind angleBetweenShipAndWind;

  static Vector3 doesNotExist = Vector3.zero; //any value is good, we will check objects references and not values
  Vector3 lostPlayerLastKnownPosition = doesNotExist;

  WindArea windArea;
  EnemyBoatController enemyBoatController;
  public GameObject playerShip = null;
  GameObject corner0, corner1, corner2, corner3;
  GameObject obstacle1 = null, obstacle2 = null;
  float additionalRandomAngle;

  delegate void ActiveState();
  ActiveState activeState;

  void Start()
  {
    enemyBoatController = GetComponent<EnemyBoatController>();
    angleBetweenShipAndWind = GetComponent<AngleBetweenShipAndWind>();
    windArea = GameObject.Find("Wind Area").GetComponent<WindArea>();

    corner0 = GameObject.Find("Corner 0");
    corner1 = GameObject.Find("Corner 1");
    corner2 = GameObject.Find("Corner 2");
    corner3 = GameObject.Find("Corner 3");

    minDistBetweenParallel = 10;
    maxDistBetweenParallel = 30;

    cannonsTimeOfNextFire = new float[6];
    for (int i = 0; i < cannonsTimeOfNextFire.Length; i++)
    {
      cannonsTimeOfNextFire[i] = Time.time;
    }

    enemyBoatController.SailsDown();

    changeDirectionTime = Time.time + RandomPeriodOfTime();

    activeState = state_PlayerSearch;
  }

  void Update()
  {
    activeState();
  }

  // STATE start ---------------------------------------------
  void state_AvoidObstacles()
  {
    // START of checking the state change conditions ---------------------
    if (!obstacle1)
    {
      if (!playerShip)
      {
        ChangeStateTo_PlayerSearch_Or_GoToLastKnownPlayerShipPosition();
        return;
      }

      if (playerShip)
      {
        activeState = state_GoToPlayerShip;
        return;
      }
    }
    // END of checking the state change conditions---------------

    Vector3 ourShipDirection = transform.forward;
    ourShipDirection.y = 0;

    float angleInDegToAvoidObstacle;

    // if there is only one obstacle
    if (obstacle1 && !obstacle2)
    {
      angleInDegToAvoidObstacle = GetAngleInDegToAvoidObstacle(obstacle1);
    }
    else // if there are two obstacles
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
        angleInDegToAvoidObstacle = 90 + additionalRandomAngle;

      }
      else if ((angleBetweenShortestShiptToObstacle1VectorAndShipDirection < 0
      && angleBetweenShortestShiptToObstacle2VectorAndShipDirection == 0)
      ||
      (angleBetweenShortestShiptToObstacle2VectorAndShipDirection < 0
      && angleBetweenShortestShiptToObstacle1VectorAndShipDirection == 0))
      {
        angleInDegToAvoidObstacle = -90 - additionalRandomAngle;

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
      else //such situation should not exist!!!
      {
        angleInDegToAvoidObstacle = 0;
        Debug.Log("Such situation should not exist!!!");
      }
    }

    Vector3 rotatedVector = GetRotatedVector(ourShipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
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
      angleInDegToAvoidObstacle = (90 - angleBetweenShortestShiptToObstacleVectorAndShipDirection) + additionalRandomAngle;

      // due to wind correction of angleInDegToAvoidObstacle
      Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
      if (GetVectorToWindAngle(rotatedVector) > 150)
      {
        angleInDegToAvoidObstacle += GetVectorToWindAngle(rotatedVector) - 150;
      }

    }
    else if (angleBetweenShortestShiptToObstacleVectorAndShipDirection < 0)
    {
      angleInDegToAvoidObstacle = (-90 - angleBetweenShortestShiptToObstacleVectorAndShipDirection) - additionalRandomAngle;

      // due to wind correction of angleInDegToAvoidObstacle
      Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
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
      angleInDegToAvoidObstacle = -angleBetweenShortestShiptToObstacleVectorAndShipDirection - 90 - additionalRandomAngle;
    }
    else if (angleBetweenShortestShiptToObstacleVectorAndShipDirection < 0)
    {
      angleInDegToAvoidObstacle = -angleBetweenShortestShiptToObstacleVectorAndShipDirection + 90 + additionalRandomAngle;
    }
    else
    { //perpendicular
      angleInDegToAvoidObstacle = 90 + additionalRandomAngle;
    }

    return angleInDegToAvoidObstacle;
  }

  private float GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(GameObject obstacle)
  {
    // shortestShiptToObstacleVector means perpendicular vector
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
  void state_GoToLastKnownPlayerShipPosition()
  {
    // START of checking the state change conditions ---------------------
    if (obstacle1)
    {
      activeState = state_AvoidObstacles;
      return;
    }

    if (playerShip)
    {
      lostPlayerLastKnownPosition = doesNotExist;
      activeState = state_GoToPlayerShip;
      return;
    }

    Vector3 towardsLostPlayerLastKnownPositionDirection = lostPlayerLastKnownPosition - transform.position;
    towardsLostPlayerLastKnownPositionDirection.y = 0;

    // if player not found near lostPlayerLastKnownPosition
    if (towardsLostPlayerLastKnownPositionDirection.magnitude < 20)
    {
      lostPlayerLastKnownPosition = doesNotExist;
      activeState = state_PlayerSearch;
      return;
    }
    // END of checking the state change conditions---------------

    Debug.DrawLine(transform.position, lostPlayerLastKnownPosition, Color.magenta, 0.0f, true);

    // float angleBetweenOurShipDirectionAndTowardsLostPlayerLastKnownPositionDirection = 
    //   Vector3.SignedAngle(ourShipDirection, towardsLostPlayerLastKnownPositionDirection, Vector3.up);

    // Taking wind into account
    float angleBetweenTowardsLostPlayerLastKnownPositionDirectionAndWindDirection =
      Vector3.SignedAngle(towardsLostPlayerLastKnownPositionDirection, windArea.windDirection, Vector3.up);

    if (angleBetweenTowardsLostPlayerLastKnownPositionDirectionAndWindDirection > 150)
    {
      towardsLostPlayerLastKnownPositionDirection = GetRotatedVector(windArea.windDirection, -150 * Mathf.Deg2Rad);
    }
    else if (angleBetweenTowardsLostPlayerLastKnownPositionDirectionAndWindDirection < -150)
    {
      towardsLostPlayerLastKnownPositionDirection = GetRotatedVector(windArea.windDirection, 150 * Mathf.Deg2Rad);
    }

    enemyBoatController.SetNewShipDirection(towardsLostPlayerLastKnownPositionDirection);
  }
  // STATE end -----------------------------------------------

  // STATE start ---------------------------------------------
  void state_PlayerSearch()
  {
    if (obstacle1)
    {
      activeState = state_AvoidObstacles;
      return;
    }

    if (playerShip)
    {
      activeState = state_GoToPlayerShip;
      return;
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
        rotatedVector = GetRotatedVector(windArea.windDirection, -150 * Mathf.Deg2Rad);
      }
      else if (currentShipToWindAngle - angle < -150)
      {
        rotatedVector = GetRotatedVector(windArea.windDirection, 150 * Mathf.Deg2Rad);
      }
      else
      {
        rotatedVector = GetRotatedVector(transform.forward, angle * Mathf.Deg2Rad);
      }

      enemyBoatController.SetNewShipDirection(rotatedVector);
    }
  }
  // STATE end -----------------------------------------------

  // STATE start ---------------------------------------------
  void state_GoToPlayerShip()
  {
    // START of checking the state change conditions ---------------------
    if (obstacle1)
    {
      activeState = state_AvoidObstacles;
      return;
    }

    if (!playerShip)
    {
      Debug.Log("GoToPlayerShip ------> ChangeStateTo_PlayerSearch_Or_GoToLastKnownPosition");
      ChangeStateTo_PlayerSearch_Or_GoToLastKnownPlayerShipPosition();
      return;
    }

    Vector3 towardsPlayerShipDirection = playerShip.transform.position - transform.position;
    towardsPlayerShipDirection.y = 0;

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    Vector3 shipDirection = transform.forward;
    shipDirection.y = 0;

    // This check can be performed only if playerShip != null
    DetermineInWhichZoneIsOurShip();
    ourShipDirectionIsConsistentWithTowardsPlayerShipDirection =
      IsOurShipDirectionConsistentWithTowardsPlayerShipDirection(shipDirection, towardsPlayerShipDirection);

    if (ourShipIsInParallelZone)
    {
      if (closeToPlayerShip)
      {
        activeState = state_SailParallelToPlayerShip;
        return;
      }

      if (!closeToPlayerShip)
      {
        if (ourShipDirectionIsConsistentWithTowardsPlayerShipDirection)
        {
          activeState = state_SailParallelToPlayerShip;
          return;
        }
      }
    }

    if (ourShipIsInZoneBetweenParallelZones)
    {
      activeState = state_GoToParallelZone;
      return;
    }
    // END of checking the state change conditions---------------

    Debug.DrawLine(transform.position, playerShip.transform.position, Color.white, 0.0f, true);

    // Taking wind into account
    float angleBetweenTowardsPlayerShipDirectionAndWindDirection = Vector3.SignedAngle(towardsPlayerShipDirection, windArea.windDirection, Vector3.up);

    if (angleBetweenTowardsPlayerShipDirectionAndWindDirection > 150)
    {
      towardsPlayerShipDirection = GetRotatedVector(windArea.windDirection, -150 * Mathf.Deg2Rad);
    }
    else if (angleBetweenTowardsPlayerShipDirectionAndWindDirection < -150)
    {
      towardsPlayerShipDirection = GetRotatedVector(windArea.windDirection, 150 * Mathf.Deg2Rad);
    }

    enemyBoatController.SetNewShipDirection(towardsPlayerShipDirection);
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
    else //distance < minDistBetweenParallel
    {
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

    float towardsPlayerShipDirectionToPlayerShipAngleRad = towardsPlayerShipDirectionToPlayerShipAngle * Mathf.Deg2Rad;
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

    float towardsPlayerShipDirectionToPlayerShipAngleRad = towardsPlayerShipDirectionToPlayerShipAngle * Mathf.Deg2Rad;
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

  void ChangeStateTo_PlayerSearch_Or_GoToLastKnownPlayerShipPosition()
  {
    if (lostPlayerLastKnownPosition == doesNotExist)
    {
      activeState = state_PlayerSearch;
      return;
    }

    if (lostPlayerLastKnownPosition != doesNotExist)
    {
      activeState = state_GoToLastKnownPlayerShipPosition;
      return;
    }
  }

  // STATE start ---------------------------------------------
  void state_SailParallelToPlayerShip()
  {
    // START of checking the state change conditions ---------------------
    if (obstacle1)
    {
      activeState = state_AvoidObstacles;
      return;
    }

    if (!playerShip)
    {
      Debug.Log("SailParallelToPlayerShip ------> ChangeStateTo_PlayerSearch_Or_GoToLastKnownPosition");
      ChangeStateTo_PlayerSearch_Or_GoToLastKnownPlayerShipPosition();
      return;
    }

    // This check can be performed only if playerShip != null
    DetermineInWhichZoneIsOurShip();

    if (!ourShipIsInParallelZone && !ourShipIsInZoneBetweenParallelZones)
    {
      activeState = state_GoToPlayerShip;
      return;
    }

    if (!ourShipIsInParallelZone && ourShipIsInZoneBetweenParallelZones)
    {
      activeState = state_GoToParallelZone;
      return;
    }
    // END of checking the state change conditions---------------

    Vector3 towardsPlayerShipDirection = playerShip.transform.position - transform.position;

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    Vector3 ourShipDirection = transform.forward;
    ourShipDirection.y = 0;

    Debug.DrawLine(transform.position, playerShip.transform.position, Color.red, 0.0f, true);

    float angleBetweenPlayerShipDirectionAndOurShipDirection = Vector3.Angle(playerShipDirection, ourShipDirection);
    if (angleBetweenPlayerShipDirectionAndOurShipDirection > 90)
    {
      angleBetweenPlayerShipDirectionAndOurShipDirection = 180 - angleBetweenPlayerShipDirectionAndOurShipDirection;
    }

    if ((angleBetweenPlayerShipDirectionAndOurShipDirection <= 10) && (GetDistanceBetweenPerpendicular() <= 3))
    {
      activeState = state_Battle;
      return;
    }

    if (closeToPlayerShip)
    {
      if (angleBetweenPlayerShipDirectionAndOurShipDirection > 0.5) SetShipsParallel();
    }
    else //not closeToPlayerShip
    {
      if (IsOurShipDirectionConsistentWithTowardsPlayerShipDirection(ourShipDirection, towardsPlayerShipDirection))
      {
        if (angleBetweenPlayerShipDirectionAndOurShipDirection > 0.5) SetShipsParallel();
      }
      else
      {
        activeState = state_GoToPlayerShip;
      }
    }
  }
  // STATE end -----------------------------------------------

  // STATE start ---------------------------------------------
  void state_GoToParallelZone()
  {
    // START of checking the state change conditions ---------------------
    if (obstacle1)
    {
      activeState = state_AvoidObstacles;
      return;
    }

    if (!playerShip)
    {
      ChangeStateTo_PlayerSearch_Or_GoToLastKnownPlayerShipPosition();
      return;
    }

    // This check can be performed only if playerShip != null
    DetermineInWhichZoneIsOurShip();

    if (ourShipIsInParallelZone && !ourShipIsInZoneBetweenParallelZones)
    {
      activeState = state_SailParallelToPlayerShip;
      return;
    }
    // END of checking the state change conditions---------------

    Vector3 towardsPlayerShipDirection = playerShip.transform.position - transform.position;

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    Vector3 ourShipDirection = transform.forward;
    ourShipDirection.y = 0;

    Vector3 towardsCorner0Direction = corner0.transform.position - transform.position;
    Vector3 towardsCorner1Direction = corner1.transform.position - transform.position;
    Vector3 towardsCorner2Direction = corner2.transform.position - transform.position;
    Vector3 towardsCorner3Direction = corner3.transform.position - transform.position;

    Debug.DrawLine(transform.position, playerShip.transform.position, Color.green, 0.0f, true);

    float angleBetweenPlayerShipDirectionAndTowardsPlayerShipDirection = Vector3.SignedAngle(playerShipDirection, towardsPlayerShipDirection, Vector3.up);


    if (angleBetweenPlayerShipDirectionAndTowardsPlayerShipDirection >= -90 && angleBetweenPlayerShipDirectionAndTowardsPlayerShipDirection <= 90)
    {
      float angleBetweenOurShipDirectionAndTowardsCorner1Direction = Vector3.SignedAngle(ourShipDirection, towardsCorner1Direction, Vector3.up);
      float angleBetweenOurShipDirectionAndTowardsCorner2Direction = Vector3.SignedAngle(ourShipDirection, towardsCorner2Direction, Vector3.up);

      if (Mathf.Abs(angleBetweenOurShipDirectionAndTowardsCorner1Direction) < Mathf.Abs(angleBetweenOurShipDirectionAndTowardsCorner2Direction))
      {
        enemyBoatController.SetNewShipDirection(towardsCorner1Direction);
      }
      else
      {
        enemyBoatController.SetNewShipDirection(towardsCorner2Direction);
      }
    }
    else if (angleBetweenPlayerShipDirectionAndTowardsPlayerShipDirection < -90 || angleBetweenPlayerShipDirectionAndTowardsPlayerShipDirection > 90)
    {
      float angleBetweenOurShipDirectionAndTowardsCorner0Direction = Vector3.SignedAngle(ourShipDirection, towardsCorner0Direction, Vector3.up);
      float angleBetweenOurShipDirectionAndTowardsCorner3Direction = Vector3.SignedAngle(ourShipDirection, towardsCorner3Direction, Vector3.up);

      if (Mathf.Abs(angleBetweenOurShipDirectionAndTowardsCorner0Direction) < Mathf.Abs(angleBetweenOurShipDirectionAndTowardsCorner3Direction))
      {
        enemyBoatController.SetNewShipDirection(towardsCorner0Direction);
      }
      else
      {
        enemyBoatController.SetNewShipDirection(towardsCorner3Direction);
      }
    }
  }
  // STATE end -----------------------------------------------

  // STATE start ---------------------------------------------
  void state_Battle()
  {
    // START of checking the state change conditions ---------------------
    if (obstacle1)
    {
      activeState = state_AvoidObstacles;
      return;
    }

    if (!playerShip)
    {
      activeState = state_PlayerSearch;
      return;
    }

    Vector3 playerShipDirection = playerShip.transform.forward;
    playerShipDirection.y = 0;

    Vector3 ourShipDirection = transform.forward;
    ourShipDirection.y = 0;

    float angleBetweenPlayerShipDirectionAndOurShipDirection = Vector3.Angle(playerShipDirection, ourShipDirection);
    if (angleBetweenPlayerShipDirectionAndOurShipDirection > 90)
    {
      angleBetweenPlayerShipDirectionAndOurShipDirection = 180 - angleBetweenPlayerShipDirectionAndOurShipDirection;
    }

    if ((angleBetweenPlayerShipDirectionAndOurShipDirection > 10) || (GetDistanceBetweenPerpendicular() > 3))
    {
      activeState = state_SailParallelToPlayerShip;
      return;
    }
    // END of checking the state change conditions---------------

    Debug.DrawLine(transform.position, playerShip.transform.position, Color.yellow, 0.0f, true);

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
        if (hit.collider.tag == "PlayerShip")
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

  void SetShipsParallel()
  {
    float ourShipToPlayerShipAngle = GetOurShipToPlayerShipAngle();
    Vector3 playerShipDirection = new Vector3(playerShip.transform.forward.x, 0, playerShip.transform.forward.z);

    if ((ourShipToPlayerShipAngle >= -90) && (ourShipToPlayerShipAngle <= 90))
    {
      //set ship (enemys ship) parallel to player ship
      enemyBoatController.SetNewShipDirection(playerShipDirection);
    }
    else
    {
      //set ship (enemys ship) parallel but opposite to player ship
      Vector3 rotatedVector = GetRotatedVector(playerShipDirection, 180 * Mathf.Deg2Rad);
      enemyBoatController.SetNewShipDirection(rotatedVector);
    }
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

  float RandomAngleWhileAvoidingObstacle()
  {
    return Random.Range(15, 30);
  }

  public void SetPlayerShipDetected(GameObject playerShip)
  {
    this.playerShip = playerShip;
  }

  public void SetPlayerShipNotDetected()
  {
    this.playerShip = null;
  }

  public void SetLostPlayerShipLastKnownPosition(GameObject playerShip)
  {
    lostPlayerLastKnownPosition = playerShip.transform.position;
  }

  public void RemoveLostPlayerShipLastKnownPosition()
  {
    lostPlayerLastKnownPosition = doesNotExist;
  }

  public void DetermineWhetherClosePlayerShipDetected(bool isPlayerShipDetected)
  {
    closeToPlayerShip = isPlayerShipDetected;
  }

  public void SetObstacleDetected(GameObject obstacle)
  {
    if (!obstacle1)
    {
      obstacle1 = obstacle;
      additionalRandomAngle = RandomAngleWhileAvoidingObstacle();
      Debug.Log("obstacle1: " + obstacle1.tag);
    }
    else if (!obstacle2)
    {
      obstacle2 = obstacle;
      Debug.Log("obstacle2: " + obstacle2.tag);
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
