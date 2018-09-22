using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
  public bool enemyDetected;
  PlayerAI playerAI;

  void Start()
  {
    enemyDetected = false;
    playerAI = GetComponentInParent<PlayerAI>();
  }

  void OnTriggerEnter(Collider coll)
  {
    if (coll.gameObject.tag == "EnemyShip")
    {
      enemyDetected = true;
      playerAI.SetPlayerShipDetected(coll.gameObject); //shoud be SetEnemyShipDetected
    }
  }

  void OnTriggerExit(Collider coll)
  {
    if (coll.gameObject.tag == "EnemyShip")
    {
      enemyDetected = false;
      playerAI.SetPlayerShipNotDetected(); //shoud be SetEnemyShipNotDetected
    }
  }
}
