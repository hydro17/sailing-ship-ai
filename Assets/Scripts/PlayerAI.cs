using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AngleBetweenShipAndWind))]
[RequireComponent(typeof(EnemyBoatController))]
public class PlayerAI : MonoBehaviour
{
  public GameObject[] cannons;
  float[] cannonsTimeOfNextFire;

  public float minDistBetweenParallel;
  public float maxDistBetweenParallel;
  public float distanceBetweenParallel;

  float changeDirectionTime;
  AngleBetweenShipAndWind angleBetweenShipAndWind;

  WindArea windArea;
  EnemyBoatController enemyBoatController;
  GameObject enemyShip = null;
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

    minDistBetweenParallel = 10;
    maxDistBetweenParallel = 30;

    cannonsTimeOfNextFire = new float[6];
    for (int i = 0; i < cannonsTimeOfNextFire.Length; i++)
    {
      cannonsTimeOfNextFire[i] = Time.time;
    }

    enemyBoatController.SailsDown();
    changeDirectionTime = Time.time + RandomPeriodOfTime();
    activeState = state_MoveRandomly;
  }

  void Update()
  {
    activeState();
  }

  // =========================================================
  // FSM STATES
  // =========================================================

  // STATE start ---------------------------------------------
  void state_AvoidingObstacle()
  {

    if (!obstacle1)
    {
      activeState = state_MoveRandomly;
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
      angleInDegToAvoidObstacle = AIHelperFunctions.GetAngleInDegToAvoidObstacle(ourShip, obstacle1, windArea, additionalRandomAngle);

    }
    else
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
          angleInDegToAvoidObstacle = AIHelperFunctions.GetAngleInDegToAvoidObstacle(ourShip, obstacle1, windArea, additionalRandomAngle);
        }
        else
        {
          angleInDegToAvoidObstacle = AIHelperFunctions.GetAngleInDegToAvoidObstacle(ourShip, obstacle2, windArea, additionalRandomAngle);
        }

      }
      else
      {
        angleInDegToAvoidObstacle = 0;
        Debug.Log("Such situation should not exist!!!");
      }
    }

    Vector3 rotatedVector = AIHelperFunctions.GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
    enemyBoatController.SetNewShipDirection(rotatedVector);
  }
  // STATE end -----------------------------------------------

  // STATE start ---------------------------------------------
  void state_MoveRandomly()
  {
    if (obstacle1)
    {
      activeState = state_AvoidingObstacle;
      return;
    }

    if (enemyShip)
    {
      Vector3 playerShipDirection = enemyShip.transform.forward;
      playerShipDirection.y = 0;

      Vector3 ourShipDirection = transform.forward;
      ourShipDirection.y = 0;

      float angleBetweenPlayerShipDirectionAndOurShipDirection = Vector3.Angle(playerShipDirection, ourShipDirection);
      if (angleBetweenPlayerShipDirectionAndOurShipDirection > 90)
      {
        angleBetweenPlayerShipDirectionAndOurShipDirection = 180 - angleBetweenPlayerShipDirectionAndOurShipDirection;
      }

      if ((angleBetweenPlayerShipDirectionAndOurShipDirection <= 5)
        && (AIHelperFunctions.GetDistanceBetweenPerpendicular(this.gameObject, enemyShip) <= 3))
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
  void state_Battle()
  {
    // START of checking the state change conditions ---------------------

    if (obstacle1)
    {
      activeState = state_AvoidingObstacle;
      return;
    }

    if (!enemyShip)
    {
      activeState = state_MoveRandomly;
      return;
    }

    Vector3 enemyShipDirection = enemyShip.transform.forward;
    enemyShipDirection.y = 0;

    Vector3 ourShipDirection = transform.forward;
    ourShipDirection.y = 0;

    float angleBetweenEnemyShipDirectionAndOurShipDirection = Vector3.Angle(enemyShipDirection, ourShipDirection);
    if (angleBetweenEnemyShipDirectionAndOurShipDirection > 90)
    {
      angleBetweenEnemyShipDirectionAndOurShipDirection = 180 - angleBetweenEnemyShipDirectionAndOurShipDirection;
    }

    if ((angleBetweenEnemyShipDirectionAndOurShipDirection > 5)
      || (AIHelperFunctions.GetDistanceBetweenPerpendicular(this.gameObject, enemyShip) > 3))
    {
      activeState = state_MoveRandomly;
      return;
    }
    // END of checking the state change conditions---------------

    Debug.DrawLine(transform.position, enemyShip.transform.position, Color.blue, 0.0f, true);

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

  // =========================================================
  // FUNCTIONS
  // =========================================================

  float RandomPeriodOfTime()
  {
    return Random.Range(2, 4);
  }

  float RandomAngleWhileAvoidingObstacle()
  {
    return Random.Range(15, 30);
  }

  public void SetEnemyShipDetected(GameObject enemyShip)
  {
    this.enemyShip = enemyShip;
  }

  public void SetEnemyShipNotDetected()
  {
    this.enemyShip = null;
  }

  public void SetObstacleDetected(GameObject obstacle)
  {
    if (!obstacle1)
    {
      obstacle1 = obstacle;
      additionalRandomAngle = RandomAngleWhileAvoidingObstacle();
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
