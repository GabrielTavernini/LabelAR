using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditLabels : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RectTransform scrollViewContent;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject helpViewSettings;
    [SerializeField] private GameObject editPopup;
    [SerializeField] private GameObject scrollView;
    [SerializeField] private Orchestrator orchestrator;

    private EditLabelPayload currentEdit;
    private GameObject currentEditButton;
    private GameObject currentBuilding;
    private readonly string buttonPrefix = "Button";
    
    // Start is called before the first frame update
    void Start()
    {
        SpawnButtons();
        
        cancelButton.onClick.AddListener(Reset);
        inputField.onSubmit.AddListener(CommitEdit);
        helpButton.onClick.AddListener(OpenHelp);
        closeButton.onClick.AddListener(CloseHelp);
        deleteButton.onClick.AddListener(DeleteLabel);
        exitButton.onClick.AddListener(Exit);
    }

    void OnEnable() {
        SpawnButtons();
    }

    void SpawnButtons() {
        foreach(Label l in Request.response.labels)
            if(GameObject.Find(buttonPrefix + l.name) == null) 
                CreateItem(l.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateItem(string labelName) {
        GameObject item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
        item.transform.SetParent(scrollViewContent.transform);
        item.transform.localScale = new Vector3(1f, 1f, 1f);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.name = buttonPrefix + labelName;
        
        item.GetComponent<EditLabelsItem>().setText(labelName);
    }

    public void EditButtonClicked(string name) {
        orchestrator.EditLabel(name);
    }

    public void InitiateEdit(string name, GameObject building) {        
        currentEditButton = GameObject.Find(buttonPrefix + name);
        currentEdit = new EditLabelPayload();
        currentEdit.oldName = name;
        currentBuilding = building;

        scrollView.SetActive(false);
        editPopup.SetActive(true);
        orchestrator.keyboard.SetActive(true);
        inputField.ActivateInputField();
        inputField.Select();
        inputField.text = name;
        inputField.caretPosition = inputField.text.Length;
    }
    private void Reset() {
        if(currentBuilding != null)
            currentBuilding.GetComponent<MeshRenderer>().material = orchestrator.highlightMaterial;

        currentBuilding = null;
        currentEditButton = null;
        currentEdit = null;
        inputField.text = "";
        orchestrator.keyboard.SetActive(false);
        editPopup.SetActive(false);
        scrollView.SetActive(true);
        orchestrator.CancelLabelEdit();
    }

    private void CommitEdit(string newName) {
        if(currentEdit == null 
            || newName.Trim().Length == 0 
            || newName == currentEdit.oldName
            || Request.response.labels.Any(l => l.name == newName)) 
        { 
            Reset();
            return;
        }
        
        currentEdit.newName = newName;
        GameObject.Find(currentEdit.oldName).GetComponent<TMP_Text>().text = newName;
        GameObject.Find(currentEdit.oldName).name = newName;
        currentEditButton.GetComponent<EditLabelsItem>().setText(newName);
        currentEditButton.name = buttonPrefix + newName;
        Request.response.labels.Find(l => l.name == currentEdit.oldName).name = newName;

        StartCoroutine(Request.EditLabel(currentEdit));
        Reset();
    }

    public void InitiateDelete(string name) {
        DeleteLabelPayload payload = new DeleteLabelPayload();
        payload.name = name;

        Destroy(GameObject.Find(name));
        Destroy(GameObject.Find(buttonPrefix + name));
        Label label = Request.response.labels.Find(l => l.name == name);
        label.buildings.ForEach(b => orchestrator.SetHighlight(GameObject.Find(b), false));
        Request.response.labels.Remove(label);

        StartCoroutine(Request.DeleteLabel(payload));
    } 

    void DeleteLabel() {
        InitiateDelete(currentEdit.oldName);
        Reset();
    }


    public void OpenHelp()
    {
        helpViewSettings.SetActive(!helpViewSettings.activeSelf);
    }

    public void CloseHelp()
    {
        helpViewSettings.SetActive(false);
    }

    void Exit() {
        Reset();
        orchestrator.BackToViewSettings();
    }

}
