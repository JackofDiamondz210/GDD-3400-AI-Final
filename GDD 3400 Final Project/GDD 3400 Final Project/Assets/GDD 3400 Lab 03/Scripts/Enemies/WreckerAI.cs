using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class WreckerAI : MonoBehaviour
{
    //wandering settings
    [Header("Wandering Settings")]
    [SerializeField] float wanderRadius = 10f;
    [SerializeField] float wanderSpeed = 3f;
    [SerializeField] float stoppingDistance = 0.5f;

    //speed of chasing Wrecker
    [Header("Chase Settings")]
    [SerializeField] float chaseSpeed = 6f;

    //assigning snitches to specific wreckers
    [Header("Assigned Snitches")]
    [SerializeField] List<GameObject> assignedSnitches;

    //growing wrecker settings when snitch is alerted
    [Header("Growth Settings")]
    [SerializeField] Vector3 normalScale = Vector3.one;
    [SerializeField] Vector3 grownScale = new Vector3(100f, 100f, 100f);
    [SerializeField] float growSpeed = 10f;
    [SerializeField] float growTime = 20f; //how long it stays grown

    private Coroutine growRoutine;

    //wandering settings
    private NavMeshAgent agent;
    private Vector3 wanderTarget;
    private bool isChasing = false;



    //getting navmesh, wander speed, and wander target
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = wanderSpeed;
        PickNewWanderTarget();
    }

    //making sure SoundManager works because for some reason my enemies wouldnt react to sound
    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    IEnumerator SubscribeWhenReady()
    {
        while (SoundManager.Instance == null)
        {
            yield return null;
        }

        SoundManager.Instance.OnSoundEmitted += OnSoundHeard;
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnSoundEmitted -= OnSoundHeard;
        }
    }

    private void Update()
    {
        //continue to wander when chasing is over
        if (!isChasing)
        {
            Wander();
        }
        else
        {
            //continuously update destination toward assigned snitch
            foreach (var snitch in assignedSnitches)
            {
                if (snitch != null)
                {
                    agent.SetDestination(snitch.transform.position);
                }
            }
        }
    }

    //getting the wrecker to wander randomly
    void Wander()
    {
        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            PickNewWanderTarget();
        }
    }

    //wander with new target picked out within radius
    void PickNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            agent.SetDestination(wanderTarget);
        }
    }

    //when the wrecker hears the alert from its specific snitch it will go to that snitch and wreck everything
    private void OnSoundHeard(SoundEvent sound)
    {
        //check if the sound came from any of the assigned snitches
        foreach (var snitch in assignedSnitches)
        {
            if (snitch == null) continue;

            if (sound.position == snitch.transform.position) // exact Snitch trigger
            {
                isChasing = true;
                agent.speed = chaseSpeed;
                Debug.Log($"{name} is chasing assigned snitch {snitch.name}");
                break; //only chase one snitch at a time
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //collision with assigned Snitch
        if (assignedSnitches.Contains(other.gameObject))
        {
            isChasing = false;
            agent.isStopped = true;

            Destroy(other.gameObject);
            Debug.Log($"{name} collided with and destroyed {other.name}");

            //when the snith is destroyed grow
            Grow();
        }
    }

    //making sure grow is called appropriatly
    public void Grow()
    {
        if (growRoutine != null)
        {
            StopCoroutine(growRoutine);
        }
        growRoutine = StartCoroutine(GrowAndShrink());
    }

    private IEnumerator GrowAndShrink()
    {
        //stopping movement
        agent.isStopped = true;

        //grow to attack player
        while (Vector3.Distance(transform.localScale, grownScale) > 0.01f)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, grownScale, growSpeed * Time.deltaTime);
            yield return null;
        }

        //stay grown for growTime
        yield return new WaitForSeconds(growTime);

        //shrink back
        while (Vector3.Distance(transform.localScale, normalScale) > 0.01f)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, normalScale, growSpeed * Time.deltaTime);
            yield return null;
        }

        //resume movement
        agent.isStopped = false;

        growRoutine = null;
    }

    //too see the wandering radius and path
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(agent.destination, 0.2f);
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}
