using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum ControllerMode {
    Adjustment,
    Labeling,
    Menu,
    MenuScrolling,
    Selecting,
    Scrolling,
    Hidden,
}

public class ControllerHelper: MonoBehaviour
{
    [SerializeField] private GameObject bumper;
    [SerializeField] private GameObject trigger;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject pad;


    void Start()
    {
        SetMode(ControllerMode.Scrolling);
    }

    public void SetMode(ControllerMode mode)
    {
        SetAllActive(true);
        TMP_Text bumperText = bumper.GetComponent<TMP_Text>();
        TMP_Text triggerText = trigger.GetComponent<TMP_Text>();
        TMP_Text menuText = menu.GetComponent<TMP_Text>();
        TMP_Text padText = pad.GetComponent<TMP_Text>();

        Debug.Log("Setting controller mode to " + mode);

        switch (mode)
        {
            case ControllerMode.Adjustment:
                SetAllActive(false);
                menu.SetActive(true);
                bumper.SetActive(true);
                trigger.SetActive(true);
                bumperText.text = "Rotate";
                triggerText.text = "Grab";
                menuText.text = "Confirm";
                break;

            case ControllerMode.Labeling:
                SetAllActive(false);
                menu.SetActive(true);
                trigger.SetActive(true);
                triggerText.text = "Add Label";
                menuText.text = "Settings";
                break;

            case ControllerMode.Menu:
                SetAllActive(false);
                menu.SetActive(true);
                trigger.SetActive(true);
                triggerText.text = "Select";
                menuText.text = "Close Menu";
                break;

            case ControllerMode.MenuScrolling:
                SetAllActive(false);
                pad.SetActive(true);
                menu.SetActive(true);
                trigger.SetActive(true);
                triggerText.text = "Select";
                menuText.text = "Close Menu";
                padText.text = "Scroll";
                break;
    
            case ControllerMode.Selecting:
                SetAllActive(false);
                trigger.SetActive(true);
                triggerText.text = "Select";
                break;
    
            case ControllerMode.Scrolling:
                SetAllActive(false);
                pad.SetActive(true);
                trigger.SetActive(true);
                triggerText.text = "Select";
                padText.text = "Scroll";
                break;

            case ControllerMode.Hidden:
                SetAllActive(false);
                break;
        }
    }

    private void SetAllActive(bool value)
    {
        bumper.SetActive(value);
        trigger.SetActive(value);
        menu.SetActive(value);
        pad.SetActive(value);
    }
}