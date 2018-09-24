using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonActions : MonoBehaviour
{

  public IEnumerator FireToOpponentShip(GameObject opponent)
  {
    float fireDelay = Random.Range(0.0f, 0.4f);
    yield return new WaitForSecondsRealtime(fireDelay);

    if (opponent.tag == "PlayerShip") opponent.GetComponent<PlayerHealth>().TakeDamage();
    if (opponent.tag == "EnemyShip") opponent.GetComponent<EnemyHealth>().TakeDamage();
    // possibly decreasing number of cannonBalls eg. ourShip passed as argument and ourShip.DecreaseNumberOfCannonBalls()

    StartCoroutine(BackAndForth());
  }

  IEnumerator BackAndForth()
  {
    float timeOfTheEndOfTheMoveBack = Time.time + 0.1f;

    while (Time.time < timeOfTheEndOfTheMoveBack)
    {
      transform.Translate(-transform.forward * 5.0f * Time.deltaTime, Space.World);
      yield return null;
    }

    float timeOfTheEndOfTheMoveForward = Time.time + 2.0f;

    while (Time.time < timeOfTheEndOfTheMoveForward)
    {
      transform.Translate(transform.forward / 3.65f * Time.deltaTime, Space.World);
      yield return null;
    }
  }

}
