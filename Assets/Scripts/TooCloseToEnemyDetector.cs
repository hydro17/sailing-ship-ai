using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooCloseToEnemyDetector : MonoBehaviour
{
  public bool tooClose;
  PlayerAI playerAI;

  void Start()
  {
    playerAI = GetComponentInParent<PlayerAI>();
    tooClose = false;
  }

  void OnTriggerEnter(Collider otherColl)
  {
    if (otherColl.gameObject.tag == "EnemyShip")
    {
      tooClose = true;
      // Debug.Log("Too close to enemy");
      playerAI.SetObstacleDetected(otherColl.gameObject);
    }
  }

  void OnTriggerExit(Collider otherColl)
  {
    if (otherColl.gameObject.tag == "EnemyShip")
    {
      tooClose = false;
      // Debug.Log("Already not too close to enemy");
      playerAI.SetObstacleNotDetected(otherColl.gameObject);
    }
  }
}
