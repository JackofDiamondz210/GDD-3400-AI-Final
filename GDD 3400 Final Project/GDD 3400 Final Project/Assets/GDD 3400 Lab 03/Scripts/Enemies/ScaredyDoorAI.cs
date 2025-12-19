using UnityEngine;
using System.Collections;

public class ScaredyDoorAI : MonoBehaviour
{
    [Header("Sound Reaction")]
    [SerializeField] float scareThreshold = 8f;
    [SerializeField] float detectionRadius = 10f;

    [Header("Door Movement")]
    [SerializeField] float openHeight = 4f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float openTime = 10f;


    Vector3 closedPosition;
    Vector3 openPosition;

    bool isOpen = false;
    Coroutine doorRoutine;

    private void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    //making sure SoundManager still works even after resetting scene
    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    IEnumerator SubscribeWhenReady()
    {
        while (SoundManager.Instance == null)
            yield return null;

        SoundManager.Instance.OnSoundEmitted += OnSoundHeard;
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.OnSoundEmitted -= OnSoundHeard;
    }

    void OnSoundHeard(SoundEvent sound)
    {
        if (isOpen) return;

        float distance = Vector3.Distance(transform.position, sound.position);

        if (distance <= detectionRadius && sound.loudness >= scareThreshold)
        {
            StartDoorSequence();
        }
    }

    void StartDoorSequence()
    {
        if (doorRoutine != null)
        {
            StopCoroutine(doorRoutine);
        }

        doorRoutine = StartCoroutine(DoorRoutine());
    }

    IEnumerator DoorRoutine()
    {
        isOpen = true;

        
        while (Vector3.Distance(transform.position, openPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, openPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        //Stay open for certain duration set
        yield return new WaitForSeconds(openTime);

        while (Vector3.Distance(transform.position, closedPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, closedPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isOpen = false;
        doorRoutine = null;
    }

    //too see the detection radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
