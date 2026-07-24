using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour
{
    [Header("Scatter")]
    public float scatterDistance = 0.9f;

    [Header("Jump")]
    public float jumpHeight = 1.0f;
    public float jumpDuration = 0.55f;

    [Header("Bounce")]
    public int bounceCount = 4;
    public float bounceHeight = 0.35f;
    public float bounceDuration = 0.22f;

    [Header("Spin")]
    public float rotateSpeed = 540f;

    void Start()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;

        float distance =
            Random.Range(0.3f, scatterDistance);

        Vector3 target =
            transform.position +
            (Vector3)(dir * distance);

        StartCoroutine(DropRoutine(target));
    }

    IEnumerator DropRoutine(Vector3 target)
    {
        // Cú văng đầu
        yield return Jump(transform.position,
                          target,
                          jumpHeight,
                          jumpDuration,
                          1f);

        PlayBounce(1f);

        float h = bounceHeight;

        for (int i = 0; i < bounceCount; i++)
        {
            float volume =
                Mathf.Lerp(0.8f, 0.25f,
                (float)i / (bounceCount - 1));

            yield return Jump(
                target,
                target,
                h,
                bounceDuration,
                volume);

            PlayBounce(volume);

            h *= 0.55f;
        }

        transform.rotation = Quaternion.identity;
    }

    IEnumerator Jump(
        Vector3 start,
        Vector3 end,
        float height,
        float duration,
        float spinScale)
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            float ease =
                Mathf.SmoothStep(0, 1, t);

            Vector3 pos =
                Vector3.Lerp(start, end, ease);

            pos.y +=
                Mathf.Sin(ease * Mathf.PI) * height;

            transform.position = pos;

            transform.Rotate(
                0,
                0,
                rotateSpeed * spinScale * Time.deltaTime);

            yield return null;
        }

        transform.position = end;
    }

    void PlayBounce(float volume)
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySFX(
    AudioManager.Instance.coinBounceSound,
    volume);
    }
}