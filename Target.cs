using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
        public float health;

    /// 'Hits' the target for a certain amount of damage
    public void Hit(float damage) {
        health -= damage;
         if(health <= 0) {
            Destroy(gameObject);
        }
    } 
    void Update() {
        if(health <= 0) {
            Destroy(gameObject);
        }
    }

     
}
