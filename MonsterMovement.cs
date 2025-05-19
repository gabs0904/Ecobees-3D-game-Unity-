using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
  public float speed =5f;
    
 public void Update()
 {

        transform.Translate(0, 0, Time.deltaTime*speed);
 }
}
