using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
  public float speed =5f;
    
 public void Update()
 {
     //transform.Translate(userDirection * movespeed * Time.deltaTime); 
     
     
        
            // Swap the position of the cylinder.
         //transform.position = Vector3.MoveTowards(transform.position, target.position, movespeed * Time.deltaTime);
        transform.Translate(0, 0, Time.deltaTime*speed);
 }
}
