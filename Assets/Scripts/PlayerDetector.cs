using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
  public bool playerDetected;
  EnemyAI enemyAI;

  void Start()
  {
    playerDetected = false;
    enemyAI = GetComponentInParent<EnemyAI>();
  }

  void OnTriggerEnter(Collider coll)
  {
    if (coll.gameObject.tag == "PlayerShip")
    {
      playerDetected = true;
      enemyAI.SetPlayerShipDetected(coll.gameObject);
      enemyAI.RemoveLostPlayerShipLastKnownPosition();
    }
  }

  void OnTriggerExit(Collider coll)
  {
    if (coll.gameObject.tag == "PlayerShip")
    {
      playerDetected = false;
      enemyAI.SetPlayerShipNotDetected();
      enemyAI.SetLostPlayerShipLastKnownPosition(coll.gameObject);
    }
  }
}
