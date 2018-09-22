using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
  //  public Vector3 collisionPoint;
  EnemyAI enemyShipAI;
  PlayerAI playerShipAI;
  //  EnemyBoatController enemyBoatController;
  //  Transform enemyShipTransform;
  public bool isObstacle;
  //  public float rayMaxDistance = 30 + 5; //equal to sphere colider radius + 5
  //  public float angleDegNeededToAvoidObstacle;
  //  public Vector3 shipDirectionToAvoidObstacle;
  //  public float minDistance;
  //  public float minDistanceShipToRayAngleDeg;

  //  private struct RaycastInfo
  //  {
  //     public float shipToRayAngleDeg;
  //     public Vector3 rayDirection;
  //     public float distance;
  //  }

  //  // private struct TurningData
  //  // {
  //  //    public string direction;
  //  //    public float angleDeg;
  //  // }

  void Start()
  {
    if (transform.parent.gameObject.tag == "EnemyShip") enemyShipAI = GetComponentInParent<EnemyAI>();
    if (transform.parent.gameObject.tag == "PlayerShip") playerShipAI = GetComponentInParent<PlayerAI>();
    // enemyBoatController = GetComponentInParent<EnemyBoatController>();
    // enemyShipTransform = GetComponentInParent<Transform>();
    isObstacle = false;

  }

  void OnTriggerEnter(Collider otherColl)
  {
    if (otherColl.gameObject.tag == "obstacle")
    {
      isObstacle = true;
      if (transform.parent.gameObject.tag == "EnemyShip") enemyShipAI.SetObstacleDetected(otherColl.gameObject);
      if (transform.parent.gameObject.tag == "PlayerShip") playerShipAI.SetObstacleDetected(otherColl.gameObject);
    }
  }

  void OnTriggerExit(Collider otherColl)
  {
    if (otherColl.gameObject.tag == "obstacle")
    {
      isObstacle = false;
      if (transform.parent.gameObject.tag == "EnemyShip") enemyShipAI.SetObstacleNotDetected(otherColl.gameObject);
      if (transform.parent.gameObject.tag == "PlayerShip") playerShipAI.SetObstacleNotDetected(otherColl.gameObject);
    }
  }

  //    void OnTriggerStay(Collider otherColl) {
  //    Vector3 closestPointOfObstacle = otherColl.gameObject.GetComponent<Collider>().ClosestPointOnBounds(enemyShipTransform.position);
  //     Vector3 directionAndLength = closestPointOfObstacle - enemyShipTransform.position;
  //     Debug.DrawRay(enemyShipTransform.position, directionAndLength, Color.white, 0.0f);
  // }


  //  void AvoidObstacle(Collider collider)
  //  {
  //     angleDegNeededToAvoidObstacle = GetAngleDegNeededToAvoidObstacle(collider);

  //     //angle correction considering wind direction
  //     //-------------------------------------------------------------------------------------------------------
  //     float currentWindToShipAngle = enemyShipAI.GetCurrentWindToShipAngle();
  //     // if (currentWindToShipAngle - angleDegNeededToAvoidObstacle > 130) angleDegNeededToAvoidObstacle -= 120;
  //     // if (currentWindToShipAngle - angleDegNeededToAvoidObstacle < -130) angleDegNeededToAvoidObstacle += 120;
  //     //--------------------------------------------------------------------------------------------------------
  //     if (currentWindToShipAngle - angleDegNeededToAvoidObstacle > 130) angleDegNeededToAvoidObstacle = -130 + currentWindToShipAngle - 120;
  //     if (currentWindToShipAngle - angleDegNeededToAvoidObstacle < -130) angleDegNeededToAvoidObstacle = 130 + currentWindToShipAngle + 120;
  //     //--------------------------------------------------------------------------------------------------------

  //     shipDirectionToAvoidObstacle = enemyShipAI.GetRotatedVector(enemyShipTransform.forward, angleDegNeededToAvoidObstacle * enemyShipAI.DegToRad);

  //     RaycastHit hitInfo;
  //     while (Physics.Raycast(enemyShipTransform.position, shipDirectionToAvoidObstacle, out hitInfo, rayMaxDistance + 10))
  //     {
  //        if (angleDegNeededToAvoidObstacle >= 0) angleDegNeededToAvoidObstacle += 5;
  //        else angleDegNeededToAvoidObstacle -= 5;
  //        shipDirectionToAvoidObstacle = enemyShipAI.GetRotatedVector(enemyShipTransform.forward, angleDegNeededToAvoidObstacle * enemyShipAI.DegToRad);
  //     }

  //     // Debug.DrawRay(enemyShipTransform.position, shipDirectionToAvoidObstacle);
  //     enemyBoatController.SetNewShipDirection(shipDirectionToAvoidObstacle);
  //  }




  //  float GetAngleDegNeededToAvoidObstacle(Collider collider)
  //  {
  //     RaycastInfo[] raycastInfo = new RaycastInfo[72];

  //     float shipToRayAngleDeg = 0; //od 0 do 355 stopni co 5 stopni
  //     for (int i = 0; i < 72; i++)
  //     {
  //        Vector3 rayDirection = enemyShipAI.GetRotatedVector(enemyShipTransform.forward, shipToRayAngleDeg * enemyShipAI.DegToRad);

  //        raycastInfo[i].shipToRayAngleDeg = shipToRayAngleDeg;
  //        raycastInfo[i].rayDirection = rayDirection;

  //        RaycastHit hitInfo;
  //        Ray ray = new Ray() { origin = enemyShipTransform.position, direction = rayDirection };
  //        // if (Physics.Raycast(enemyShipTransform.position, rayDirection, out hitInfo, rayMaxDistance))
  //        if (collider.Raycast(ray, out hitInfo, rayMaxDistance))
  //           raycastInfo[i].distance = hitInfo.distance;
  //        else
  //           raycastInfo[i].distance = 100000f;

  //        shipToRayAngleDeg += 5f;
  //     }

  //     // for (int i = 0; i < 72; i++)
  //     // {
  //     //    Debug.Log(i + " " + raycastInfo[i].shipToRayAngleDeg + " " + raycastInfo[i].distance);
  //     // }

  //     //looking for a least element
  //     minDistance = 100000f;
  //     minDistanceShipToRayAngleDeg = 370f;
  //     for (int i = 0; i < 72; i++)
  //     {
  //        if (raycastInfo[i].distance < minDistance)
  //        {
  //           minDistance = raycastInfo[i].distance;
  //           minDistanceShipToRayAngleDeg = raycastInfo[i].shipToRayAngleDeg;
  //        }
  //     }

  //     // Debug.Log(minDistance + " " + minDistanceShipToRayAngleDeg);

  //     // TurningData turningData;
  //     float avoidObstacleAngleDeg;
  //     if (minDistanceShipToRayAngleDeg >= 270 && minDistanceShipToRayAngleDeg < 360)
  //     {
  //        // turningData.direction = "right";
  //        // turningData.angleDeg = 100 - (360 - minDistanceShipToRayAngleDeg);
  //        avoidObstacleAngleDeg = 100 - (360 - minDistanceShipToRayAngleDeg);
  //     }
  //     else
  //     {
  //        if (minDistanceShipToRayAngleDeg > 0 && minDistanceShipToRayAngleDeg <= 90)
  //        {
  //           // turningData.direction = "left";
  //           // turningData.angleDeg = -100 + minDistanceShipToRayAngleDeg;
  //           avoidObstacleAngleDeg = -100 + minDistanceShipToRayAngleDeg;
  //        }
  //        else
  //        {
  //           if (minDistanceShipToRayAngleDeg == 0)
  //           {
  //              if (Random.value > 0.5)
  //              {
  //                 // turningData.direction = "right";
  //                 // turningData.angleDeg = 100;
  //                 avoidObstacleAngleDeg = 100;
  //              }
  //              else
  //              {
  //                 // turningData.direction = "left";
  //                 // turningData.angleDeg = -100;
  //                 avoidObstacleAngleDeg = -100;
  //              }
  //           }
  //           else //minDistanceShipToRayAngleDeg < 270 && minDistanceShipToRayAngleDeg > 90
  //           {
  //              avoidObstacleAngleDeg = 0; //Don't change the ship direction
  //           }
  //        }
  //     }
  //     return avoidObstacleAngleDeg;
  //     // return turningData;
  //  }

}

// void OnCollisionEnter(Collision collision)
// {
//   isObstacle = true;
//   Vector3 collisionPoint = collision.contacts[0].point;
//   Vector3 vectorToCollisionPoint = collisionPoint - enemyShipTransform.position;
//   Debug.DrawRay(enemyShipTransform.position, vectorToCollisionPoint, Color.white);
// }
