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

  bool ourShipIsInZoneBetweenParallelZones = false;
  bool ourShipIsInParallelZone = false;
  bool ourShipDirectionIsConsistentWithTowardsPlayerShipDirection = false;

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
  GameObject ourShip; 

  delegate void ActiveState();
  ActiveState activeState;

  void Start()
  {
    ourShip = this.gameObject;

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

  // =========================================================
  // FSM STATES
  // =========================================================

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
      angleInDegToAvoidObstacle = AIHelperFunctions.GetAngleInDegToAvoidObstacle(ourShip, obstacle1, windArea, additionalRandomAngle);
    }
    else // if there are two obstacles
    {

      float angleBetweenShortestShiptToObstacle1VectorAndShipDirection =
        AIHelperFunctions.GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle1, transform);
      float angleBetweenShortestShiptToObstacle2VectorAndShipDirection =
        AIHelperFunctions.GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle2, transform);

      if ((angleBetweenShortestShiptToObstacle1VectorAndShipDirection > 0
        && angleBetweenShortestShiptToObstacle2VectorAndShipDirection < 0)
        ||
        (angleBetweenShortestShiptToObstacle1VectorAndShipDirection < 0
        && angleBetweenShortestShiptToObstacle2VectorAndShipDirection > 0))
      {

        if (Mathf.Abs(angleBetweenShortestShiptToObstacle1VectorAndShipDirection) <
          Mathf.Abs(angleBetweenShortestShiptToObstacle2VectorAndShipDirection))
        {
          angleInDegToAvoidObstacle = AIHelperFunctions.GetGreaterAngleInDegToAvoidObstacle(ourShip, obstacle1, additionalRandomAngle);
        }
        else
        {
          angleInDegToAvoidObstacle = AIHelperFunctions.GetGreaterAngleInDegToAvoidObstacle(ourShip, obstacle2, additionalRandomAngle);
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
          angleInDegToAvoidObstacle = AIHelperFunctions.GetAngleInDegToAvoidObstacle(ourShip, obstacle1, windArea, additionalRandomAngle);
        }
        else
        {
          angleInDegToAvoidObstacle = AIHelperFunctions.GetAngleInDegToAvoidObstacle(ourShip, obstacle2, windArea, additionalRandomAngle);
        }

      }
      else //such situation should not exist!!!
      {
        angleInDegToAvoidObstacle = 0;
        Debug.Log("Such situation should not exist!!!");
      }
    }

    Vector3 rotatedVector = AIHelperFunctions.GetRotatedVector(ourShipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
    enemyBoatController.SetNewShipDirection(rotatedVector);
  }
  // STATE end -----------------------------------------------

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
      towardsLostPlayerLastKnownPositionDirection = AIHelperFunctions.GetRotatedVector(windArea.windDirection, -150 * Mathf.Deg2Rad);
    }
    else if (angleBetweenTowardsLostPlayerLastKnownPositionDirectionAndWindDirection < -150)
    {
      towardsLostPlayerLastKnownPositionDirection = AIHelperFunctions.GetRotatedVector(windArea.windDirection, 150 * Mathf.Deg2Rad);
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
        rotatedVector = AIHelperFunctions.GetRotatedVector(windArea.windDirection, -150 * Mathf.Deg2Rad);
      }
      else if (currentShipToWindAngle - angle < -150)
      {
        rotatedVector = AIHelperFunctions.GetRotatedVector(windArea.windDirection, 150 * Mathf.Deg2Rad);
      }
      else
      {
        rotatedVector = AIHelperFunctions.GetRotatedVector(transform.forward, angle * Mathf.Deg2Rad);
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
      AIHelperFunctions.IsOurShipDirectionConsistentWithTowardsOtherShipDirection(shipDirection, towardsPlayerShipDirection);

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
      towardsPlayerShipDirection = AIHelperFunctions.GetRotatedVector(windArea.windDirection, -150 * Mathf.Deg2Rad);
    }
    else if (angleBetweenTowardsPlayerShipDirectionAndWindDirection < -150)
    {
      towardsPlayerShipDirection = AIHelperFunctions.GetRotatedVector(windArea.windDirection, 150 * Mathf.Deg2Rad);
    }

    enemyBoatController.SetNewShipDirection(towardsPlayerShipDirection);
  }
  // STATE end -----------------------------------------------

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

    if ((angleBetweenPlayerShipDirectionAndOurShipDirection <= 10)
      && (AIHelperFunctions.GetDistanceBetweenPerpendicular(this.gameObject, playerShip) <= 3))
    {
      activeState = state_Battle;
      return;
    }

    if (closeToPlayerShip)
    {
      if (angleBetweenPlayerShipDirectionAndOurShipDirection > 0.5) AIHelperFunctions.SetShipsParallel(ourShip, playerShip, enemyBoatController);
    }
    else //not closeToPlayerShip
    {
      if (AIHelperFunctions.IsOurShipDirectionConsistentWithTowardsOtherShipDirection(ourShipDirection, towardsPlayerShipDirection))
      {
        if (angleBetweenPlayerShipDirectionAndOurShipDirection > 0.5) AIHelperFunctions.SetShipsParallel(ourShip, playerShip, enemyBoatController);
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

    if ((angleBetweenPlayerShipDirectionAndOurShipDirection > 10)
      || (AIHelperFunctions.GetDistanceBetweenPerpendicular(this.gameObject, playerShip) > 3))
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

  // =========================================================
  // FUNCTIONS
  // =========================================================

  private void ChangeStateTo_PlayerSearch_Or_GoToLastKnownPlayerShipPosition()
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

  private void DetermineInWhichZoneIsOurShip()
  {
    distanceBetweenParallel = AIHelperFunctions.GetDistanceBetweenParallel(ourShip, playerShip);

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
