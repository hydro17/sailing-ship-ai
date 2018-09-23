using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
  EnemyAI enemyShipAI;
  PlayerAI playerShipAI;
  public bool isObstacle;

  void Start()
  {
    if (transform.parent.gameObject.tag == "EnemyShip") enemyShipAI = GetComponentInParent<EnemyAI>();
    if (transform.parent.gameObject.tag == "PlayerShip") playerShipAI = GetComponentInParent<PlayerAI>();
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
}
