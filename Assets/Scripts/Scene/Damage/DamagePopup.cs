using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshPro text;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float lifeTime = 1f;

    [Header("Spawn")]
    [SerializeField] private float randomOffsetX = 0.2f;
    [SerializeField] private float offsetY = 0.1f;
    [SerializeField] private float popupScale = 0.5f;

    private Color color;
    private float remainingLifeTime;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshPro>();

        if (text == null)
        {
            Debug.LogError(
                "DamagePopup thiếu component TextMeshPro."
            );

            enabled = false;
            return;
        }

        remainingLifeTime = lifeTime;
    }

    private void Start()
    {
        transform.localScale =
            Vector3.one * popupScale;

        transform.position += new Vector3(
            Random.Range(-randomOffsetX, randomOffsetX),
            offsetY,
            0f
        );

        color = text.color;
    }

    private void Update()
    {
        transform.position +=
            Vector3.up * moveSpeed * Time.deltaTime;

        remainingLifeTime -= Time.deltaTime;

        color.a -= fadeSpeed * Time.deltaTime;
        color.a = Mathf.Clamp01(color.a);

        text.color = color;

        if (remainingLifeTime <= 0f)
            Destroy(gameObject);
    }

    public void SetDamage(int damage, bool critical)
    {
        if (text == null)
            return;

        text.text = damage.ToString();

        text.color = critical
            ? Color.yellow
            : Color.white;

        color = text.color;
    }
}