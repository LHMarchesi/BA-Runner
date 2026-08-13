using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    [Header("Navigation Scope")]
    [SerializeField] private RectTransform navigationRoot;

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

       
        if (
            navigationRoot != null &&
            !navigationRoot.gameObject.activeInHierarchy
        )
        {
            indicator.gameObject.SetActive(false);

            lastButton = null;

            return;
        }

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        // =====================================================
        // NO HAY SELECCIÓN
        // =====================================================

        if (selected == null)
        {
            if (assignSelectionRoutine != null)
                return;

            if (
                lastSelected != null &&
                (
                    !lastSelected.activeInHierarchy ||
                    !BelongsToThisNavigation(lastSelected)
                )
            )
            {
                lastSelected = null;
                lastButton = null;

                indicator.gameObject.SetActive(false);

                return;
            }

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

      
        if (!BelongsToThisNavigation(selected))
        {
            indicator.gameObject.SetActive(false);

            lastButton = null;

            return;
        }

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

        lastButton = currentButton;

        indicator.position =
            currentButton.IndicatorPoint.position;
    }


    private IEnumerator AssignButtonAfterUIEvent(
    GameObject target
)
    {
       
        yield return new WaitForEndOfFrame();

        yield return null;

        Canvas.ForceUpdateCanvases();

        if (
            target == null ||
            !target.activeInHierarchy
        )
        {

            assignSelectionRoutine = null;
            yield break;
        }

        ForceSelection(target);

        /*
         * Verificación.
         */
        yield return null;

        if (
            EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject
                != target
        )
        {
            ForceSelection(target);
        }

        assignSelectionRoutine = null;
    }
    private bool BelongsToThisNavigation(GameObject target)
    {
        if (target == null)
            return false;

        /*
         * Si no asignamos root, conserva el comportamiento anterior.
         */
        if (navigationRoot == null)
            return true;

        Transform targetTransform = target.transform;

        return
            targetTransform == navigationRoot ||
            targetTransform.IsChildOf(navigationRoot);
    }
    // =========================================================
    // ASSIGN BUTTON
    // =========================================================

    public void AssignButton(GameObject target)
    {
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

        ForceSelection(target);

        assignSelectionRoutine =
            StartCoroutine(
                AssignButtonAfterUIEvent(target)
            );
    }

    public void AssignButton()
    {
        if (initialSelected == null)
        {
            Debug.LogWarning(
                $"[MenuNavigation] '{gameObject.name}' " +
                $"no tiene Default Button asignado."
            );

            return;
        }

        AssignButton(initialSelected);
    }


    private void ForceSelection(GameObject target)
    {
        if (
            target == null ||
            !target.activeInHierarchy ||
            EventSystem.current == null
        )
        {
            return;
        }

        Selectable selectable =
            target.GetComponent<Selectable>();

        if (selectable == null)
        {
            selectable =
                target.GetComponentInParent<Selectable>();
        }

        if (
            selectable == null ||
            !selectable.IsActive() ||
            !selectable.IsInteractable()
        )
        {
            return;
        }

        GameObject selectableObject =
            selectable.gameObject;

        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(
            selectableObject
        );

        selectable.Select();

        lastSelected = selectableObject;
        lastButton =
            selectable.GetComponentInParent<MenuButton>();
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
