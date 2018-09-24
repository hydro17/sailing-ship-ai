using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosePlayerDetector : MonoBehaviour
{
  EnemyAI enemyAI;

  void Start()
  {
    enemyAI = GetComponentInParent<EnemyAI>();
  }

  void OnTriggerEnter(Collider coll)
  {
    if (coll.gameObject.tag == "PlayerShip")
    {
      enemyAI.DetermineWhetherClosePlayerShipDetected(true);
    }
  }

  void OnTriggerExit(Collider coll)
  {
    if (coll.gameObject.tag == "PlayerShip")
    {
      enemyAI.DetermineWhetherClosePlayerShipDetected(false);
    }
  }
}
