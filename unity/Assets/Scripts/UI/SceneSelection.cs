using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagicLeap.OpenXR.Features.LocalizationMaps;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using System.Text.RegularExpressions;
using Unity.XR.CoreUtils;

public class SceneSelection : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RectTransform scrollViewContent;
    [SerializeField] private Orchestrator orchestrator;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject helpSceneSelection;

    private LocalizationMap[] maps;
    private MagicLeapLocalizationMapFeature localizationMapFeature;
    private Dictionary<string, LocalizationMap> mapsDictionary = new();

    void Start()
    {
        #if UNITY_EDITOR
        orchestrator.gameObject.SetActive(true);
        #endif
        helpButton.onClick.AddListener(OpenHelp);
        closeButton.onClick.AddListener(CloseHelp);

        localizationMapFeature = OpenXRSettings.Instance.GetFeature<MagicLeapLocalizationMapFeature>();
        if (localizationMapFeature == null || !localizationMapFeature.enabled)
            return;

        XrResult result = localizationMapFeature.GetLocalizationMapsList(out maps);
        if (result == XrResult.Success) {
            foreach(LocalizationMap map in maps) {
                string name = map.Name.Split('\0')[0];
                mapsDictionary.Add(name, map);
                Debug.Log("Added map: " + name + " UUID: " + map.MapUUID);

                if (!Orchestrator.DEMO)
                    createItem(name);
            }
            
            // Hardcoded spaces for the demo
            if(Orchestrator.DEMO) {
                foreach(string space in Request.spaces.Keys)
                    createItem(space);
            }
        }
        
    }

    private void createItem(string mapName) {
        Debug.Log("Creating item " + mapName);

        GameObject item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
        item.transform.SetParent(scrollViewContent.transform);
        item.transform.localScale = new Vector3(1f, 1f, 1f);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.name = mapName;
        
        item.GetComponentInChildren<TMP_Text>().text = mapName;
        Sprite image = Resources.Load<Sprite>("Images/Spaces/" + mapName);
        image = image == null ? Resources.Load<Sprite>("Images/Spaces/Default") : image;
        item.GetNamedChild("Image").GetComponent<Image>().sprite = image;
        item.GetComponent<Button>().onClick.AddListener(() => selectSpace(mapName));
    }

    private void selectSpace(string name) {
        Debug.Log("Selected space:" + name);
        if(!mapsDictionary.ContainsKey(name)) return;

        mapsDictionary.TryGetValue(name, out LocalizationMap map);
        Debug.Log($"Request localization in: {map.MapUUID}");
        XrResult result = localizationMapFeature.RequestMapLocalization(map.MapUUID);
        Debug.Log($"Localize request result: {result}");

        orchestrator.StartCoroutine(orchestrator.Open(name));
    }
    public void OpenHelp()
    {
        helpSceneSelection.SetActive(!helpSceneSelection.activeSelf);
    }

    public void CloseHelp()
    {
        helpSceneSelection.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
