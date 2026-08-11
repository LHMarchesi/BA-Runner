using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private RectTransform indicator;
    [SerializeField] private GameObject initialSelected;
    

    private GameObject lastSelected;
    private MenuButton lastButton;
    [Header("Initial Selection")]
    
    private Coroutine restoreSelectionRoutine;
    private Coroutine assignSelectionRoutine;
    private void Start()
    {
        if (initialSelected != null)
        {
            AssignButton(
                initialSelected
            );
        }
    }
    private void Update()
    {
        if (indicator == null || EventSystem.current == null)
            return;

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        // =====================================================
        // NO HAY NADA SELECCIONADO
        // =====================================================
        if (selected == null)
        {
            /*
             * Si estamos esperando para asignar un botón,
             * no dejamos que RestoreSelection interfiera.
             */
            if (assignSelectionRoutine != null)
                return;

            /*
             * Si el último seleccionado fue destruido
             * o pertenece a un panel que acaba de cerrarse.
             */
            if (
                lastSelected != null &&
                !lastSelected.activeInHierarchy
            )
            {
                lastSelected = null;
                lastButton = null;

                indicator.gameObject.SetActive(false);

                return;
            }

            /*
             * Unity a veces pierde temporalmente la selección.
             * Intentamos restaurarla.
             */
            if (
                restoreSelectionRoutine == null &&
                lastSelected != null
            )
            {
                restoreSelectionRoutine =
                    StartCoroutine(
                        RestoreSelection()
                    );
            }

            return;
        }

        // =====================================================
        // OBJETO SELECCIONADO
        // =====================================================
        if (!selected.activeInHierarchy)
            return;

        lastSelected = selected;

        MenuButton currentButton =
            selected.GetComponentInParent<MenuButton>();
        
        if (
            currentButton == null ||
            !currentButton.UseGlobalIndicator ||
            currentButton.IndicatorPoint == null
        )
        {
            indicator.gameObject.SetActive(false);
            lastButton = null;
            return;
        }

        indicator.gameObject.SetActive(true);

        if (currentButton == lastButton)
        {
            indicator.position =
                currentButton.IndicatorPoint.position;

            return;
        }

        lastButton = currentButton;

        indicator.position =
            currentButton.IndicatorPoint.position;
    }

    // =========================================================
    // ASSIGN BUTTON
    // =========================================================

    public void AssignButton(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning(
                "[MenuNavigation] AssignButton recibió un target null."
            );

            return;
        }

        /*
         * Cancelamos cualquier intento anterior de restaurar
         * o asignar selección.
         */
        if (restoreSelectionRoutine != null)
        {
            StopCoroutine(restoreSelectionRoutine);
            restoreSelectionRoutine = null;
        }

        if (assignSelectionRoutine != null)
        {
            StopCoroutine(assignSelectionRoutine);
            assignSelectionRoutine = null;
        }

        assignSelectionRoutine =
            StartCoroutine(
                AssignButtonRoutine(target)
            );
    }

    private IEnumerator AssignButtonRoutine(
        GameObject target
    )
    {
        /*
         * Esperamos un frame para que Unity termine de
         * abrir/cerrar paneles y actualizar el Canvas.
         */
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (
            target == null ||
            !target.activeInHierarchy
        )
        {
            Debug.LogWarning(
                "[MenuNavigation] No se pudo seleccionar el botón " +
                "porque no está activo."
            );

            assignSelectionRoutine = null;
            yield break;
        }

        Selectable selectable =
            target.GetComponent<Selectable>();

        if (selectable == null)
        {
            Debug.LogWarning(
                $"[MenuNavigation] '{target.name}' no tiene " +
                "un componente Selectable."
            );

            assignSelectionRoutine = null;
            yield break;
        }

        if (
            !selectable.IsActive() ||
            !selectable.IsInteractable()
        )
        {
            Debug.LogWarning(
                $"[MenuNavigation] '{target.name}' no está disponible."
            );

            assignSelectionRoutine = null;
            yield break;
        }

        if (EventSystem.current == null)
        {
            assignSelectionRoutine = null;
            yield break;
        }

        /*
         * Limpiamos primero la selección anterior.
         * Esto hace que Unity reconozca correctamente
         * incluso si seleccionamos el mismo botón.
         */
        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(
            target
        );

        selectable.Select();

        lastSelected = target;

        assignSelectionRoutine = null;
    }

    // =========================================================
    // RESTORE SELECTION
    // =========================================================

    private IEnumerator RestoreSelection()
    {
        yield return null;

        if (
            EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null &&
            lastSelected != null &&
            lastSelected.activeInHierarchy
        )
        {
            Selectable selectable =
                lastSelected.GetComponent<Selectable>();

            if (
                selectable != null &&
                selectable.IsActive() &&
                selectable.IsInteractable()
            )
            {
                EventSystem.current.SetSelectedGameObject(
                    lastSelected
                );

                selectable.Select();
            }
        }

        restoreSelectionRoutine = null;
    }

    // =========================================================
    // OPTIONAL
    // =========================================================

    public void ClearSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                null
            );
        }

        lastSelected = null;
        lastButton = null;

        if (indicator != null)
        {
            indicator.gameObject.SetActive(false);
        }
    }
}
