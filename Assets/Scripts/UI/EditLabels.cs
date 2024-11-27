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
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject keyboard;
    [SerializeField] private GameObject editPopup;
    [SerializeField] private GameObject scrollView;
    [SerializeField] private Orchestrator orchestrator;

    private EditLabelPayload currentEdit;
    private GameObject currentEditButton;
    private readonly string buttonPrefix = "Button";
    
    // Start is called before the first frame update
    void Start()
    {
        foreach(Label l in Request.response.labels)
            CreateItem(l.name);
        
        cancelButton.onClick.AddListener(Cancel);
        inputField.onSubmit.AddListener(CommitEdit);
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

    public void InitiateEdit(string name) {
        currentEditButton = GameObject.Find(buttonPrefix + name);
        currentEdit = new EditLabelPayload();
        currentEdit.oldName = name;

        scrollView.SetActive(false);
        editPopup.SetActive(true);
        inputField.Select();
        inputField.ActivateInputField();
        inputField.text = name;
        inputField.caretPosition = inputField.text.Length;
        keyboard.SetActive(true);
    }
    private void Cancel() {
        currentEditButton = null;
        currentEdit = null;
        inputField.text = "";
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
            Cancel();
            return;
        }
        
        currentEdit.newName = newName;
        GameObject.Find(currentEdit.oldName).GetComponent<TMP_Text>().text = newName;
        GameObject.Find(currentEdit.oldName).name = newName;
        currentEditButton.GetComponent<EditLabelsItem>().setText(newName);
        currentEditButton.name = buttonPrefix + newName;
        Request.response.labels.Find(l => l.name == name).name = newName;

        StartCoroutine(Request.EditLabel(currentEdit));
        Cancel();
    }

    public void InitiateDelete(string name) {
        DeleteLabelPayload payload = new DeleteLabelPayload();
        payload.name = name;

        Destroy(GameObject.Find(name));
        Destroy(GameObject.Find(buttonPrefix + name));
        Label label = Request.response.labels.Find(l => l.name == name);
        label.buildings.ForEach(b => GameObject.Find(b).GetComponent<MeshRenderer>().material = orchestrator.material);
        Request.response.labels.Remove(label);

        StartCoroutine(Request.DeleteLabel(payload));
    } 
}
