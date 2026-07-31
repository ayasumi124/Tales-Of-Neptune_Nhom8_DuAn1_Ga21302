using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image elementIcon;
    [SerializeField] private Image keyIcon;
    [SerializeField] private TextMeshProUGUI keyText;

    private ElementData elementData;
    private KeyCode slotKey;

    public ElementData ElementData =>
        elementData;

    public KeyCode SlotKey =>
        slotKey;

    public bool IsEmpty =>
        elementData == null;

    public void Setup(
        ElementData data,
        KeyCode key)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        elementData = data;
        slotKey = key;

        gameObject.SetActive(true);

        if (elementIcon != null)
        {
            elementIcon.gameObject.SetActive(true);
            elementIcon.enabled = true;

            // Tự lấy icon từ ElementData.
            elementIcon.sprite =
                data.elementIcon;

            Color color =
                elementIcon.color;

            color.a = 1f;
            elementIcon.color = color;
        }

        if (keyIcon != null)
        {
            keyIcon.gameObject.SetActive(true);
            keyIcon.enabled = true;
        }

        if (keyText != null)
        {
            keyText.gameObject.SetActive(true);
            keyText.text = GetKeyName(key);
        }

        Debug.Log(
            $"ElementSlotUI hiển thị: " +
            $"{data.elementName} - phím {key}"
        );
    }

    public void Clear()
    {
        elementData = null;
        slotKey = KeyCode.None;

        if (elementIcon != null)
        {
            elementIcon.sprite = null;
            elementIcon.enabled = false;
            elementIcon.gameObject.SetActive(false);
        }

        if (keyIcon != null)
        {
            keyIcon.enabled = false;
            keyIcon.gameObject.SetActive(false);
        }

        if (keyText != null)
        {
            keyText.text = "";
            keyText.gameObject.SetActive(false);
        }
    }

    private string GetKeyName(
        KeyCode key)
    {
        switch (key)
        {
            case KeyCode.F:
                return "F";

            case KeyCode.R:
                return "R";

            default:
                return key.ToString();
        }
    }
}