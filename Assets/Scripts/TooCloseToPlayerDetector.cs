using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooCloseToPlayerDetector : MonoBehaviour
{
  public bool tooClose;
  EnemyAI enemyAI;

  void Start()
  {
    enemyAI = GetComponentInParent<EnemyAI>();
    tooClose = false;
  }

  void OnTriggerEnter(Collider otherColl)
  {
    if (otherColl.gameObject.tag == "PlayerShip")
    {
      tooClose = true;
      enemyAI.SetObstacleDetected(otherColl.gameObject);
    }
  }

  void OnTriggerExit(Collider otherColl)
  {
    if (otherColl.gameObject.tag == "PlayerShip")
    {
      tooClose = false;
      enemyAI.SetObstacleNotDetected(otherColl.gameObject);
    }
  }
}
