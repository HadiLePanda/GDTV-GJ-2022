using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameJam
{
    public partial class Utils
    {
        // instantiate/remove enough prefabs to match amount
        public static void BalancePrefabs(GameObject prefab, int amount, Transform parent)
        {
            // instantiate under parent until amount
            for (int i = parent.childCount; i < amount; ++i)
            {
                GameObject.Instantiate(prefab, parent, false);
            }

            // delete everything that's too much
            // (backwards loop because Destroy changes childCount)
            for (int i = parent.childCount - 1; i >= amount; --i)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }
        }

        // find out if any input is currently active by using Selectable.all
        // (FindObjectsOfType<InputField>() is far too slow for huge scenes)
        public static bool AnyUIInputActive()
        {
            foreach (Selectable sel in Selectable.allSelectablesArray)
            {
                if ((sel is InputField inputField && inputField.isFocused) || (sel is TMP_InputField tmpInputField && tmpInputField.isFocused))
                {
                    return true;
                }
            }
            return false;
        }

        // deselect any UI element carefully
        // (it throws an error when doing it while clicking somewhere, so we have to
        //  double check)
        public static void DeselectCarefully()
        {
            if (!Input.GetMouseButton(0) &&
                !Input.GetMouseButton(1) &&
                !Input.GetMouseButton(2))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        // is a 2D point in screen?
        public static bool IsPointInScreen(Vector2 point)
        {
            return 0 <= point.x && point.x <= Screen.width &&
                   0 <= point.y && point.y <= Screen.height;
        }

        // check if the cursor is over a UI or OnGUI element right now
        // note: for UI, this only works if the UI's CanvasGroup blocks Raycasts
        // note: for OnGUI: hotControl is only set while clicking, not while zooming
        public static bool IsCursorOverUserInterface()
        {
            // IsPointerOverGameObject check for left mouse (default)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            // IsPointerOverGameObject check for touches
            for (int i = 0; i < Input.touchCount; ++i)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                {
                    return true;
                }
            }

            // OnGUI check
            return GUIUtility.hotControl != 0;
        }
    }
}