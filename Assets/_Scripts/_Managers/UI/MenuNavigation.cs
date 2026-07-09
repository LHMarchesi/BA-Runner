using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private RectTransform indicator;

    private GameObject lastSelected;
    private MenuButton lastButton;
    private Coroutine restoreSelectionRoutine;

    private void Update()
    {
        if (indicator == null || EventSystem.current == null)
            return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            if (lastSelected != null && !lastSelected.activeInHierarchy)
            {
                lastSelected = null;
                lastButton = null;
                indicator.gameObject.SetActive(false);
                return;
            }

            if (restoreSelectionRoutine == null && lastSelected != null)
            {
                restoreSelectionRoutine = StartCoroutine(RestoreSelection());
            }

            return;
        }

        if (!selected.activeInHierarchy)
            return;

        lastSelected = selected;

        MenuButton currentButton = selected.GetComponentInParent<MenuButton>();

        if (currentButton == null || currentButton.IndicatorPoint == null)
        {
            indicator.gameObject.SetActive(false);
            lastButton = null;
            return;
        }

        indicator.gameObject.SetActive(true);

        if (currentButton == lastButton)
        {
            indicator.position = currentButton.IndicatorPoint.position;
            return;
        }

        lastButton = currentButton;
        indicator.position = currentButton.IndicatorPoint.position;
    }

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
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }

        restoreSelectionRoutine = null;
    }
}