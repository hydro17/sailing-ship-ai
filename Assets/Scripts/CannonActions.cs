using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonActions : MonoBehaviour
{

  void Start()
  {
    // StartCoroutine(Example());
  }

  // IEnumerator Example()
  // {
  //     print(Time.time);
  //     yield return new WaitForSecondsRealtime(3);
  //     print(Time.time);
  //   StartCoroutine(BackAndForth());
  // }

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
    // Vector3 initialPosition = transform.position;
    // Vector3 intermediatePosition = Vector3.zero;

    float timeOfTheEndOfTheMoveBack = Time.time + 0.1f;

    while (Time.time < timeOfTheEndOfTheMoveBack)
    {
      transform.Translate(-transform.forward * 5.0f * Time.deltaTime, Space.World);
      yield return null;
      // intermediatePosition = transform.position;
    }

    float timeOfTheEndOfTheMoveForward = Time.time + 2.0f;

    while (Time.time < timeOfTheEndOfTheMoveForward)
    {
      // transform.Translate((initialPosition - intermediatePosition) / 2 * Time.deltaTime, Space.World);
      transform.Translate(transform.forward / 3.65f * Time.deltaTime, Space.World);
      yield return null;
    }

    // transform.position = initialPosition; // just in case
  }

}
