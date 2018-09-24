using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindArea : MonoBehaviour
{
  public float windStrength = 5;
  public Vector3 windDirection = Vector3.right;
  public GameObject arrow;

  float changeDirectiontime;

  const float degToRad = 0.0174532f;

  private void Start()
  {
    changeDirectiontime = Time.time + Random.Range(10.0f, 20.0f);
    arrow.transform.eulerAngles = new Vector3(0, 90, 0);
  }

  private void Update()
  {
    if (Time.time > changeDirectiontime)
    {
      changeDirectiontime = Time.time + Random.Range(10.0f, 20.0f);

      float rotationAngleInDeg = Random.Range(-90.0f, 90.0f);
      windDirection = AIHelperFunctions.GetRotatedVector(windDirection, rotationAngleInDeg * degToRad);
      arrow.transform.eulerAngles = new Vector3(0, Vector3.SignedAngle(Vector3.forward, windDirection, Vector3.up), 0); ;
    }
  }
}
