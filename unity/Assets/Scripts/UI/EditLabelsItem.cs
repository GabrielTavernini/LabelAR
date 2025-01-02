using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditLabelsItem : MonoBehaviour
{
    [SerializeField] private TMP_Text textField;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    // Start is called before the first frame update
    void Start()
    {
        editButton.onClick.AddListener(edit);
        deleteButton.onClick.AddListener(delete);
    }

    public void setText(string text) {
        textField.text = text;
    }

    private void edit() {
        gameObject.GetComponentInParent<EditLabels>().EditButtonClicked(textField.text);
    }

    private void delete() {
        gameObject.GetComponentInParent<EditLabels>().InitiateDelete(textField.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
