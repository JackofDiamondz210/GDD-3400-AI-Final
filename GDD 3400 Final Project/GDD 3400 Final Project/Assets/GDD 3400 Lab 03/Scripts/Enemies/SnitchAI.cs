using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SnitchAI : MonoBehaviour
{
    //movcement soeed and patrol settings
    [Header("Patrol Settings")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float waypointTolerance = 0.2f;
    [SerializeField] float patrolSpeed = 3.5f;

    //sighting distance and angle
    [Header("Sight Settings")]
    [SerializeField] float viewDistance = 10f;
    [SerializeField] float viewAngle = 60f;

    //setting loudness threshold of the snotch alert
    [Header("Scream Settings")]
    [SerializeField] float screamLoudness = 10f; //threshhold
    [SerializeField] float alertRadius = 10f; //how far the sound travels

    //the sound then the snitch is alerted
    [Header("Audio")]
    [SerializeField] AudioSource alarmSource;

    //patrol info
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isAlerted = false;
    private Transform player;

    //getting the navmesh agent, setting the speed and checking for waypoints
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (isAlerted) return; //stop patrolling forever

        Patrol();
        CheckSight();
    }

    //patrol along made waypoints
    private void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void CheckSight()
    {

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        float angle = Vector3.Angle(transform.forward, direction);

        if (Physics.Raycast(transform.position + Vector3.up * 1f, direction.normalized, out RaycastHit hit, viewDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                TriggerAlert();
            }
        }
    }

    //once triggered change bools and sound alarm
    private void TriggerAlert()
    {
        isAlerted = true;
        agent.isStopped = true;
        agent.speed = 0f;

        // Play alarm sound once
        if (alarmSource != null && !alarmSource.isPlaying)
        {
            alarmSource.Play();
        }

        // Emit SoundEvent for other AI
        SoundManager.Instance.EmitSound(transform.position, alertRadius);

        Debug.Log($"{name} has spotted the player and is now alerted!");
    }

    //too see the detection radius, and sight
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.forward * viewDistance;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);

        Gizmos.DrawWireSphere(transform.position, alertRadius);
    }
}
