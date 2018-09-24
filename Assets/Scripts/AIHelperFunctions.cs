using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHelperFunctions
{
  public static Vector3 GetRotatedVector(Vector3 initialVector, float angleRad)
  {
    //multiplying by -1 because we want angle > 0 to mean turn righ, not left
    angleRad = -angleRad;
    float x1 = initialVector.x;
    float z1 = initialVector.z;

    float x2 = x1 * Mathf.Cos(angleRad) - z1 * Mathf.Sin(angleRad);
    float z2 = x1 * Mathf.Sin(angleRad) + z1 * Mathf.Cos(angleRad);

    return new Vector3(x2, 0, z2);
  }

  // -----------------------------------------------------------------------
  public static float GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(GameObject obstacle, Transform shipTransform)
  {
    // shortestShiptToObstacleVector means perpendicular vector
    Vector3 shipPosition = shipTransform.position;
    Vector3 shipDirection = shipTransform.forward;

    Vector3 pointOfObstacleClosestToShip = obstacle.GetComponent<Collider>().ClosestPointOnBounds(shipPosition);
    Vector3 shortestShiptToObstacleVector = pointOfObstacleClosestToShip - shipPosition;

    Debug.DrawRay(shipPosition, shortestShiptToObstacleVector, Color.white, 0.0f);

    float angleBetweenShortestShiptToObstacleVectorAndShipDirection =
      Vector3.SignedAngle(shortestShiptToObstacleVector, shipDirection, Vector3.up);

    return angleBetweenShortestShiptToObstacleVectorAndShipDirection;
  }

  // -----------------------------------------------------------------------
  public static float GetVectorToWindAngle(Vector3 vector, Vector3 windDirection)
  {
    return Vector3.SignedAngle(vector, windDirection, Vector3.up);
  }

  // -----------------------------------------------------------------------
  public static bool IsOurShipDirectionConsistentWithTowardsOtherShipDirection(Vector3 ourShipDirection, Vector3 towardsOtherShipDirection)
  {
    if (Vector3.Dot(ourShipDirection, towardsOtherShipDirection) > 0) return true;
    else return false;
  }

  // -----------------------------------------------------------------------
  public static float GetDistanceBetweenParallel(GameObject ourShip, GameObject otherShip)
  {
    // Protection against calling this method when playerShip == null
    if (!otherShip)
    {
      return 1000000;
    }

    float distanceBetweenParallel;

    Vector3 towardsPlayerShipDirection = otherShip.transform.position - ourShip.transform.position;
    towardsPlayerShipDirection.y = 0;

    Vector3 playerShipDirection = otherShip.transform.forward;
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

  // -----------------------------------------------------------------------
  public static float GetDistanceBetweenPerpendicular(GameObject ourShip, GameObject otherShip)
  {
    // Protection against calling this method when playerShip == null
    if (!otherShip)
    {
      return 1000000;
    }

    float distanceBetweenPerpendicular;

    Vector3 towardsPlayerShipDirection = otherShip.transform.position - ourShip.transform.position;
    towardsPlayerShipDirection.y = 0;

    Vector3 playerShipDirection = otherShip.transform.forward;
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

  // -----------------------------------------------------------------------
  public static float GetOurShipToOtherShipAngle(GameObject ourShip, GameObject otherShip)
  {
    Vector3 ourShipDirection = new Vector3(ourShip.transform.forward.x, 0, ourShip.transform.forward.z);
    Vector3 otherShipDirection = new Vector3(otherShip.transform.forward.x, 0, otherShip.transform.forward.z);
    return Vector3.SignedAngle(ourShipDirection, otherShipDirection, Vector3.up);
  }

  // -----------------------------------------------------------------------
  public static float GetAngleInDegToAvoidObstacle(GameObject ourShip, GameObject obstacle, WindArea windArea, float additionalRandomAngle)
  {
    Vector3 shipDirection = ourShip.transform.forward;

    float angleBetweenShortestShiptToObstacleVectorAndShipDirection =
      GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle, ourShip.transform); //this is a signed angle - range: 0-180 or 0-(-180)

    float angleInDegToAvoidObstacle;

    if (angleBetweenShortestShiptToObstacleVectorAndShipDirection > 0)
    {
      angleInDegToAvoidObstacle = (90 - angleBetweenShortestShiptToObstacleVectorAndShipDirection) + additionalRandomAngle;

      // due to wind correction of angleInDegToAvoidObstacle
      Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
      if (GetVectorToWindAngle(rotatedVector, windArea.windDirection) > 150)
      {
        angleInDegToAvoidObstacle += GetVectorToWindAngle(rotatedVector, windArea.windDirection) - 150;
      }

    }
    else if (angleBetweenShortestShiptToObstacleVectorAndShipDirection < 0)
    {
      angleInDegToAvoidObstacle = (-90 - angleBetweenShortestShiptToObstacleVectorAndShipDirection) - additionalRandomAngle;

      // due to wind correction of angleInDegToAvoidObstacle
      Vector3 rotatedVector = GetRotatedVector(shipDirection, angleInDegToAvoidObstacle * Mathf.Deg2Rad);
      if (GetVectorToWindAngle(rotatedVector, windArea.windDirection) < -150)
      {
        angleInDegToAvoidObstacle += GetVectorToWindAngle(rotatedVector, windArea.windDirection) + 150;
      }

    }
    else
    { //perpendicular
      angleInDegToAvoidObstacle = 90 + 20;
    }

    return angleInDegToAvoidObstacle;
  }

  // -----------------------------------------------------------------------
  public static float GetGreaterAngleInDegToAvoidObstacle(GameObject ourShip, GameObject obstacle, float additionalRandomAngle)
  {
    float angleBetweenShortestShiptToObstacleVectorAndShipDirection =
      GetAngleBetweenShortestShiptToObstacleVectorAndShipDirection(obstacle, ourShip.transform); //this is a signed angle - range: 0-180 or 0-(-180)

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

  public static void SetShipsParallel(GameObject ourShip, GameObject otherShip, EnemyBoatController enemyBoatController)
  {
    float ourShipToOtherShipAngle = GetOurShipToOtherShipAngle(ourShip, otherShip);
    Vector3 otherShipDirection = new Vector3(otherShip.transform.forward.x, 0, otherShip.transform.forward.z);

    if ((ourShipToOtherShipAngle >= -90) && (ourShipToOtherShipAngle <= 90))
    {
      //set our ship parallel to player ship
      enemyBoatController.SetNewShipDirection(otherShipDirection);
    }
    else
    {
      //set our ship parallel but opposite to player ship
      Vector3 rotatedVector = GetRotatedVector(otherShipDirection, 180 * Mathf.Deg2Rad);
      enemyBoatController.SetNewShipDirection(rotatedVector);
    }
  }
}
