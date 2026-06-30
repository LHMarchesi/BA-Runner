using UnityEngine;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField] private RectTransform indicator;

    private GameObject lastSelected;
    private MenuButton lastButton;

    private void Update()
    {
        if (indicator == null)
            return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (lastSelected != null)
                EventSystem.current.SetSelectedGameObject(lastSelected);

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
}
