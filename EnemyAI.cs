using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public int health = 100;
    public Image enemyHealth;
    public Image crosshair;
    public float attackDistance = 1f;
    public GameObject player;
    Player hero;

    NavMeshAgent agent;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackDistance;
        enemyHealth.fillAmount = health / 100f;
        hero = player.GetComponent<Player>();
    }

    private void Start()
    {
        StartCoroutine(idleBreak());
        Invoke("CastBeam", 15f);
    }

    public enum enemyStates
    {
        idleBreak,
        walkToPlayer,
        flyDown,
        takeDamage,
        flyToPlayer,
        fireAtPlayer,
        attackPlayer,
        death
    }

    enemyStates currentState;


    void Update()
    {
        switch (currentState)
        {
            case enemyStates.walkToPlayer:
                walkToPlayer();
                break;

            case enemyStates.flyToPlayer:
                flyToPlayer();
                break;

            case enemyStates.fireAtPlayer:
                fireAtPlayer();
                break;
        }

        Ray ray;
        RaycastHit hit;

        //Take Damage....
        if (Input.GetMouseButtonDown(0))
        {
            ray = Camera.main.ScreenPointToRay(crosshair.transform.position);
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.tag == "Enemy")
                {
                    if (health == 0)
                    {
                        if (currentState == enemyStates.flyToPlayer)
                        {
                            animator.SetTrigger("flyDeath");
                            currentState = enemyStates.death;
                            agent.enabled = false;
                            CancelInvoke();
                            StopAllCoroutines();
                            animator.SetTrigger("castEnd");
                            enabled = false;
                            
                        }
                        else if (currentState == enemyStates.walkToPlayer)
                        {
                            animator.SetTrigger("death");
                            currentState = enemyStates.death;
                            agent.enabled = false;
                            CancelInvoke();
                            StopAllCoroutines();
                            animator.SetTrigger("castEnd");
                            enabled = false;
                            
                        }
                    }
                    else
                    {
                        if (currentState == enemyStates.flyToPlayer)
                        {
                            animator.SetTrigger("flyGotHit");
                        }
                        else
                        {
                            animator.SetTrigger("gotHit");
                        }
                        health = health - 10;
                        enemyHealth.fillAmount = health / 100f;
                        StartCoroutine(TakeDamage());
                    }
                }    
            }
        }

        if (hero.health == 0)
        {
            agent.enabled = false;
            CancelInvoke();
            StopAllCoroutines();
            animator.SetTrigger("castEnd");
            animator.SetFloat("locomotion", 0.0f);
            enabled = false;
        }
    }

    IEnumerator TakeDamage()
    {
        if (currentState == enemyStates.flyToPlayer)
        {
            currentState = enemyStates.takeDamage;
            yield return new WaitForSeconds(2f);
            currentState = enemyStates.flyToPlayer;
        }
        else if (currentState == enemyStates.walkToPlayer)
        {
            currentState = enemyStates.takeDamage;
            yield return new WaitForSeconds(2f);
            currentState = enemyStates.walkToPlayer;
            animator.SetFloat("locomotion", 2.0f);
        }
    }

    IEnumerator idleBreak()
    {
        currentState = enemyStates.idleBreak;
        animator.SetTrigger("idleBreak");
        yield return new WaitForSeconds(5f);
        currentState = enemyStates.walkToPlayer;
    }

    void walkToPlayer()
    {
        agent.SetDestination(player.transform.position);
        agent.speed = 2f;
        transform.LookAt(player.transform.position);

        if (agent.remainingDistance - attackDistance < 0.01f)
        {
            animator.SetFloat("locomotion", 0.0f);
            currentState = enemyStates.attackPlayer;
            AttackPlayer();

        } else if (agent.remainingDistance > 0.01f && agent.remainingDistance <= 5f)
        {
            animator.SetFloat("locomotion", 2.0f);
            
        } else
        {
            flyToAir();
            currentState = enemyStates.flyToPlayer;  
        }   
    }


    void flyToPlayer()
    {
        agent.SetDestination(player.transform.position);
        agent.speed = 10f;
        transform.LookAt(player.transform.position);

        if (agent.remainingDistance <= 1f)
        {
            currentState = enemyStates.flyDown;
            StartCoroutine(flyDown());
        }
    }

    void fireAtPlayer()
    {
        transform.LookAt(player.transform.position);
    }

    IEnumerator flyDown()
    {
        animator.SetTrigger("goGround");
        yield return new WaitForSeconds(1f);
        currentState = enemyStates.walkToPlayer;
        animator.SetFloat("locomotion", 2.0f);
    }


    void AttackPlayer()
    {
        currentState = enemyStates.attackPlayer;
        agent.SetDestination(player.transform.position);

        if (agent.remainingDistance - attackDistance < 0.1f)
        {
            int index = Random.Range(1, 3);
            animator.SetTrigger("attack" + index.ToString());
        }
        else if (agent.remainingDistance > 0.01f && agent.remainingDistance <= 5f)
        {
            currentState = enemyStates.walkToPlayer;
            animator.SetFloat("locomotion", 2.0f);
            CancelInvoke("AttackPlayer");
        }

        Invoke("AttackPlayer", 2f);
    }

    void flyToAir()
    {
        animator.SetTrigger("goAir");
    }

    void CastBeam()
    {
        currentState = enemyStates.fireAtPlayer;
        StartCoroutine(fireAtPlayer1());
    }

    IEnumerator fireAtPlayer1()
    {
        animator.SetFloat("locomotion", 2.0f);
        agent.enabled = false;
        animator.SetTrigger("cast");
        yield return new WaitForSeconds(5f);
        animator.SetTrigger("castEnd");
        agent.enabled = true;
        Invoke("CastBeam", 15f);
        yield return new WaitForSeconds(2f);
        currentState = enemyStates.walkToPlayer;
        
    }

}
