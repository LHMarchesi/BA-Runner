using UnityEngine;
using UnityEngine.EventSystems;

public class PressAnyKeySelectionVisual :
    MonoBehaviour,
    ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler
{
    [Header("Selection Visual")]
    [SerializeField] private GameObject selectedArrows;

    [Header("Pulse")]
    [SerializeField] private RectTransform pulseTarget;
    [SerializeField] private float pulseSpeed = 2.5f;
    [SerializeField] private float pulseAmount = 0.05f;

    private bool isSelected;
    private Vector3 originalScale;

    private void Awake()
    {
        if (pulseTarget == null)
            pulseTarget = transform as RectTransform;

        if (pulseTarget != null)
            originalScale = pulseTarget.localScale;

        UpdateVisual();
    }

    private void Update()
    {
        if (pulseTarget == null)
            return;

        if (!isSelected)
            return;

        float pulse =
            1f +
            Mathf.Sin(Time.unscaledTime * pulseSpeed)
            * pulseAmount;

        pulseTarget.localScale =
            originalScale * pulse;
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateVisual();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;

        if (pulseTarget != null)
        {
            pulseTarget.localScale =
                originalScale;
        }

        UpdateVisual();
    }

    public void OnPointerEnter(
        PointerEventData eventData
    )
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                gameObject
            );
        }
    }

    private void UpdateVisual()
    {
        if (selectedArrows != null)
        {
            selectedArrows.SetActive(
                isSelected
            );
        }
    }

    private void OnDisable()
    {
        isSelected = false;

        if (pulseTarget != null)
        {
            pulseTarget.localScale =
                originalScale;
        }
    }
}