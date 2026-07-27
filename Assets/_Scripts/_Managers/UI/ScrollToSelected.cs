using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float scrollDuration = 0.18f;

    [Header("Focus Position")]
    [Tooltip(
        "Posición del nivel dentro del Viewport al avanzar hacia la derecha. " +
        "Un valor menor deja más espacio visible por delante."
    )]
    [SerializeField, Range(0.1f, 0.5f)]
    private float focusWhenMovingRight = 0.30f;

    [Tooltip(
        "Posición del nivel dentro del Viewport al avanzar hacia la izquierda. " +
        "Un valor mayor deja más espacio visible por delante."
    )]
    [SerializeField]
    private float focusWhenMovingLeft;

    [Tooltip(
        "Diferencia horizontal mínima para considerar que cambió de columna."
    )]
    [SerializeField, Min(0f)]
    private float horizontalDirectionThreshold = 5f;

    private GameObject lastSelected;
    private Coroutine scrollRoutine;

    private bool hasPreviousTargetPosition;
    private float previousTargetContentX;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void Awake()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    private void Update()
    {
        if (
            EventSystem.current == null ||
            scrollRect == null ||
            scrollRect.content == null
        )
        {
            return;
        }

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            lastSelected = null;
            return;
        }

        if (selected == lastSelected)
            return;

        lastSelected = selected;

        RectTransform selectedRect =
            selected.transform as RectTransform;

        if (selectedRect == null)
            return;

        // Evita reaccionar a botones externos al Scroll View,
        // como el botón de volver.
        if (!selectedRect.IsChildOf(scrollRect.content))
            return;

        int horizontalDirection =
            GetHorizontalDirection(selectedRect);

        /*
         * Si cambiamos de rama pero el botón está en la misma
         * posición horizontal, no movemos el scroll.
         */
        if (
            hasPreviousTargetPosition &&
            horizontalDirection == 0
        )
        {
            return;
        }

        ScrollTo(selectedRect, horizontalDirection);
    }

    private int GetHorizontalDirection(
        RectTransform target
    )
    {
        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                scrollRect.content,
                target
            );

        float currentTargetX =
            targetBounds.center.x;

        if (!hasPreviousTargetPosition)
        {
            hasPreviousTargetPosition = true;
            previousTargetContentX = currentTargetX;

            return 0;
        }

        float difference =
            currentTargetX - previousTargetContentX;

        previousTargetContentX =
            currentTargetX;

        if (difference > horizontalDirectionThreshold)
            return 1;

        if (difference < -horizontalDirectionThreshold)
            return -1;

        return 0;
    }

    private void ScrollTo(
        RectTransform target,
        int horizontalDirection
    )
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
        }

        scrollRoutine = StartCoroutine(
            ScrollToRoutine(
                target,
                horizontalDirection
            )
        );
    }

    private IEnumerator ScrollToRoutine(
        RectTransform target,
        int horizontalDirection
    )
    {
        // Esperamos a que Unity actualice el layout.
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform content =
            scrollRect.content;

        RectTransform viewport =
            scrollRect.viewport;

        if (viewport == null)
        {
            viewport =
                scrollRect.GetComponent<RectTransform>();
        }

        if (
            target == null ||
            content == null ||
            viewport == null
        )
        {
            scrollRoutine = null;
            yield break;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float contentWidth =
            content.rect.width;

        float viewportWidth =
            viewport.rect.width;

        float hiddenWidth =
            contentWidth - viewportWidth;

        if (hiddenWidth <= 0f)
        {
            scrollRect.horizontalNormalizedPosition = 0f;

            scrollRoutine = null;
            yield break;
        }

        /*
         * Obtenemos la posición actual del botón dentro
         * del espacio local del Viewport.
         */
        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport,
                target
            );

        float focusPosition;

        if (horizontalDirection > 0)
        {
            // Avanzamos hacia la derecha.
            // El botón queda hacia la izquierda del Viewport.
            focusPosition = focusWhenMovingRight;
        }
        else if (horizontalDirection < 0)
        {
            // Avanzamos hacia la izquierda.
            // El botón queda hacia la derecha del Viewport.
            focusPosition = focusWhenMovingLeft;
        }
        else
        {
            // Primera selección.
            focusPosition = 0.5f;
        }

        float desiredTargetX =
            Mathf.Lerp(
                viewport.rect.xMin,
                viewport.rect.xMax,
                focusPosition
            );

        float currentTargetX =
            targetBounds.center.x;

        /*
         * Distancia necesaria para llevar el nivel hasta
         * la posición de enfoque.
         */
        float viewportDifference =
            desiredTargetX - currentTargetX;

        /*
         * Cuando el contenido se desplaza hacia la izquierda,
         * horizontalNormalizedPosition aumenta.
         */
        float normalizedDifference =
            viewportDifference / hiddenWidth;

        float initialPosition =
            scrollRect.horizontalNormalizedPosition;

        float targetPosition =
            Mathf.Clamp01(
                initialPosition - normalizedDifference
            );

        scrollRect.StopMovement();

        if (scrollDuration <= 0f)
        {
            scrollRect.horizontalNormalizedPosition =
                targetPosition;

            scrollRoutine = null;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime / scrollDuration
                );

            float smoothTime =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    normalizedTime
                );

            scrollRect.horizontalNormalizedPosition =
                Mathf.Lerp(
                    initialPosition,
                    targetPosition,
                    smoothTime
                );

            yield return null;
        }

        scrollRect.horizontalNormalizedPosition =
            targetPosition;

        scrollRoutine = null;
    }

    private void OnDisable()
    {
        if (scrollRoutine != null)
        {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }

        lastSelected = null;

        hasPreviousTargetPosition = false;
        previousTargetContentX = 0f;
    }
}
