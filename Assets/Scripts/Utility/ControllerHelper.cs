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
                bumperText.text = "Rotate";
                triggerText.text = "Grab";
                menuText.text = "Confirm";
                pad.SetActive(false);
                break;

            case ControllerMode.Labeling:
                bumper.SetActive(false);
                triggerText.text = "Add Label";
                menuText.text = "Settings";
                pad.SetActive(false);
                break;

            case ControllerMode.Menu:
                bumper.SetActive(false);
                triggerText.text = "Select";
                menuText.text = "Close Menu";
                pad.SetActive(false);
                break;

            case ControllerMode.MenuScrolling:
                bumper.SetActive(false);
                triggerText.text = "Scroll";
                menuText.text = "Close Menu";
                padText.text = "Scroll";
                break;
    
            case ControllerMode.Selecting:
                bumper.SetActive(false);
                triggerText.text = "Select";
                menu.SetActive(false);
                pad.SetActive(false);
                break;
    
            case ControllerMode.Scrolling:
                bumper.SetActive(false);
                triggerText.text = "Scroll";
                menu.SetActive(false);
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