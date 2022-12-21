using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
     public float chaseSpeed = 6f;
    public float idleSpeed = 3f;
    public float turnSpeed = 6f;
    public int damageDone = 1;

    /// <summary>
    /// Seconds waited after looking for player before looking again.
    /// </summary>
    public float linecastInterval = 0.5f;
    public float linecastIntervalWhenChasing = 0.2f;

    /// <summary>
    /// Seconds waited after attacking before can attack again.
    /// </summary>
    public float attackCooldown = 1.0f;

    /// <summary>
    /// How much enemy is knocked back when hit by a projectile.
    /// </summary>
    public float shotKnockback = 10;

    public AudioClip hitPlayerSound;
    public AudioClip deathSound;
    public AudioClip startChasingSound;

    private MoveScript moveScript;
    private HealthScript healthScript;
    private CameraScript cameraScript;
    private Transform player;
    private Animator animator;
    private Vector3 playerLastSeenLocation;

    private bool canSeePlayer = false;
    private bool canAttack = true;
    private bool isIdle = true;
    private bool hitWall = false;
    private int wallLayerMask;

    void Awake()
    {
        moveScript = GetComponent<MoveScript>();
        healthScript = GetComponent<HealthScript>();
        var camera = Camera.main.gameObject;
       // cameraScript = camera.GetComponentInChildren<CameraScript>();
        player = GameObject.Find("player").transform;
        animator = GetComponent<Animator>();
        wallLayerMask = 1 << LayerMask.NameToLayer("Wall");
    }

    void Start()
    {
        animator.SetBool("isIdle", true);
        playerLastSeenLocation = transform.position;
        StartCoroutine(SeekPlayer());
    }

    /*void OnDestroy()
    {
        if (cameraScript != null)
        {
            cameraScript.Shake(new Vector3(0.8f, 0.8f, 0.8f));
        }
    }
*/
    void Update()
    {
        if (isIdle)
        {
            // TODO
        }
        else
        {
            if (canSeePlayer) // Chasing player.
            {
                moveScript.MoveTowards(player.position, chaseSpeed);
            }
            else // Moving towards playerLastSeenLocation.
            {
                if ((playerLastSeenLocation - transform.position).magnitude < 0.1f) // Player got away.
                {
                    startIdle();
                }
                else
                {
                    moveScript.MoveTowards(playerLastSeenLocation, chaseSpeed);
                }
            }

            // Rotate towards the movement direction.
            var moveDirection = moveScript.direction;
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.Euler(0, 0, targetAngle), turnSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Reduce health when collided with a shot.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        ShotScript shot = other.gameObject.GetComponent<ShotScript>();
        if (shot != null && !shot.isEnemyShot)
        {
            AudioSource.PlayClipAtPoint(shot.shotHitSound, transform.position);
            rigidbody2D.AddForce(shotKnockback * shot.rigidbody2D.velocity);
            healthScript.Damage(shot.damage);
            Destroy(shot.gameObject);
        }
    }

    /// <summary>
    /// Handle collision with player.
    /// </summary>
    void OnCollisionStay2D(Collision2D coll)
    {
        HealthScript health = coll.transform.GetComponentInChildren<HealthScript>();
        if (health != null && health.isEnemy == false && canAttack)
        {
            StartCoroutine(AttackPlayer(health));
        }
    }

    /// <summary>
    /// Handle collision with walls.
    /// </summary>
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!hitWall && coll.gameObject.tag.ToLower() == "wall")
        {
            hitWall = true;
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if (hitWall && coll.gameObject.tag.ToLower() == "wall")
        {
            hitWall = false;
        }
    }

    /// <summary>
    /// Attack player, disable attacking for the duration of cooldown.
    /// </summary>
    IEnumerator AttackPlayer(HealthScript health)
    {
        AudioSource.PlayClipAtPoint(hitPlayerSound, transform.position);
        health.Damage(damageDone);
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    /// <summary>
    /// Look for the player, go after him if found.
    /// </summary
    IEnumerator SeekPlayer()
    {
        canSeePlayer = !Physics2D.Linecast(transform.position, player.position, wallLayerMask);
        if (canSeePlayer)
        {
            playerLastSeenLocation = player.position;
            canSeePlayer = true;
            if (isIdle) stopIdle();
        }
        var waitFor = canSeePlayer ? linecastIntervalWhenChasing : linecastInterval;
        yield return new WaitForSeconds(waitFor);
        yield return StartCoroutine(SeekPlayer());
    }
    #region IdleBehaviour
    void startIdle()
    {
        animator.SetBool("isIdle", true);
        isIdle = true;
        StartCoroutine("IdleBehaviour");
    }
    void stopIdle()
    {
        animator.SetBool("isIdle", false);
        AudioSource.PlayClipAtPoint(startChasingSound, transform.position);
        isIdle = false;
        StopCoroutine("IdleBehaviour");
    }
    /// <summary>
    /// Move in short random bursts.
    /// </summary>
    /// <returns></returns>
    IEnumerator IdleBehaviour()
    {
        var direction = new Vector3(moveScript.direction.x, moveScript.direction.y, 0);
        var target = transform.position + direction * 2;
        moveScript.MoveTowards(target, idleSpeed);

        while (true)
        {
            if (hitWall || (target - transform.position).magnitude < 0.2f)
            {
                float currAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float newAngle = Random.value * 180 + 90;
                var rotation = Quaternion.FromToRotation(transform.position, target);

                target = transform.position - direction * 2;
                moveScript.MoveTowards(target, idleSpeed);
            }
            yield return null;
        }
    }

    #endregion
}
