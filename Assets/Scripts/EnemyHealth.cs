using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
  public Slider slider;
  public float maxHealth = 100;
  public float currentHealth;

  void Start()
  {
    currentHealth = maxHealth;
    slider.value = maxHealth;
  }

  public void TakeDamage()
  {
    currentHealth -= 10 + Random.Range(0.0f, 5.0f);
    if (currentHealth < 0) currentHealth = 0;
    slider.value = currentHealth;

    if (currentHealth == 0)
    {
      Destroy(GetComponent<FloatObjectScript>());
      Destroy(slider);

      Destroy(this.gameObject, 2);
    }
  }
}
