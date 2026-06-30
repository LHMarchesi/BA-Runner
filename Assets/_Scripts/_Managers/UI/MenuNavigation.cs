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
        if (indicator == null)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (restoreSelectionRoutine == null && lastSelected != null)
            {
                restoreSelectionRoutine = StartCoroutine(RestoreSelection());
            }

            return;
        }

        lastSelected = EventSystem.current.currentSelectedGameObject;


        MenuButton current =
            lastSelected.GetComponent<MenuButton>();

        if (current == null)
            return;

        if (current == lastButton)
            return;

        lastButton = current;

        indicator.position =
            current.IndicatorPoint.position;
    }

    private IEnumerator RestoreSelection()
    {
        yield return null;

        EventSystem.current.SetSelectedGameObject(lastSelected);

        restoreSelectionRoutine = null;
    }
}

