using UnityEngine;
using System.Collections;

public class SleepingThornBushAI : MonoBehaviour
{
    //getting sound loudness threshold
    [Header("Sound Detection")]
    [SerializeField] float detectionRadius = 5f;
    [SerializeField] float scareThreshold = 5f;

    //getting ahold of the transform
    [Header("Visual")]
    [SerializeField] Transform visual;

    //getting our transofrm to chnage to new size and be able to change over time
    [Header("Scale")]
    [SerializeField] Vector3 asleepScale = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] Vector3 awakeScale = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] float growSpeed = 2f;
    [SerializeField] float awakeTime = 10f;

    Coroutine routine;

    private void Awake()
    {
        if (visual == null)
        {
            visual = transform.GetChild(0);
        }

        visual.localScale = asleepScale;
    }

    //making sure SoundManager still works even after resetting scene
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

    void OnSoundHeard(SoundEvent sound)
    {
        float distance = Vector3.Distance(transform.position, sound.position);

        if (distance <= detectionRadius && sound.loudness >= scareThreshold)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(GrowThenSleep());
        }
    }

    IEnumerator GrowThenSleep()
    {
        while (Vector3.Distance(visual.localScale, awakeScale) > 0.01f)
        {
            visual.localScale = Vector3.MoveTowards(visual.localScale, awakeScale, growSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(awakeTime);

        while (Vector3.Distance(visual.localScale, asleepScale) > 0.01f)
        {
            visual.localScale = Vector3.MoveTowards(visual.localScale, asleepScale, growSpeed * Time.deltaTime);
            yield return null;
        }

        routine = null;
    }

    //too see the detection radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
