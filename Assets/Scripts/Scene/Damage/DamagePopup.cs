using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;

    public float moveSpeed = 1.5f;
    public float fadeSpeed = 2f;
    public float lifeTime = 1f;

    private Color color;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();

        if (text == null)
        {
            Debug.LogError("DamagePopup thiếu TextMeshPro!");
        }
    }

    void Start()
    {
        color = text.color;


        transform.localScale = Vector3.one * 0.5f;

        transform.position += new Vector3(
            Random.Range(-0.2f, 0.2f),
            0.1f,
            0);
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        color.a -= fadeSpeed * Time.deltaTime;
        text.color = color;

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
            Destroy(gameObject);
    }

    public void SetDamage(int damage, bool critical)
{
    text.text = damage.ToString();

    text.color = critical ? Color.yellow : Color.white;

    color = text.color;
}
}