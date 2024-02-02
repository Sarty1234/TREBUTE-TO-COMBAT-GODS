using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListContantFiller : MonoBehaviour
{
    
    public TMP_InputField LobbyNameInputField;
    public TMP_Dropdown GameModeDropDown;
    public GameObject LobbyListElement;
    public GameObject LobbyListContainer;
    public int MaxElementCount;
    public float singleElementLength = 50;
    public float distanceBetweenElements;
    public float startPosition;


    private float curentPosition;


    private void OnEnable()
    {
        StopCoroutine(refreshLobbyListCoroutine());
        StartCoroutine(refreshLobbyListCoroutine());
    }


    private void OnDisable()
    {
        StopCoroutine(refreshLobbyListCoroutine());
    }


    public async void refreshContent()
    {
        if (LobbyListContainer.GetComponentsInChildren<Transform>().Length != 0)
        {
            Transform[] listOfItems = LobbyListContainer.GetComponentsInChildren<Transform>();
            foreach (var item in listOfItems)
            {
                if (item.transform != transform) Destroy(item.gameObject);
            }
        }


        
        LobbyManager.LobbyToShowElement[] elements = await LobbyManager.Instance.getLobbies(MaxElementCount, GameModeDropDown.options[GameModeDropDown.value].text, LobbyNameInputField.text);
        curentPosition = startPosition;
        if (elements == null) return;

        foreach (var item in elements)
        {
            string lobbyId = item.id;
            GameObject element = Instantiate(LobbyListElement, LobbyListContainer.transform);


            element.GetComponentInChildren<TMP_Text>().text = item.name;

            element.GetComponentInChildren<Button>().onClick.AddListener(delegate { JoinLobbyButtonClicked(lobbyId); });
            element.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChangeCanvasScreen.Instance.LoadObject("InLobby"); });

            element.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            element.transform.localPosition = new Vector3(element.transform.localPosition.x, curentPosition, element.transform.localPosition.z);
            
            
            
            curentPosition -= singleElementLength;
            curentPosition -= distanceBetweenElements;

        }
    }


    public void JoinLobbyButtonClicked(string id)
    {
        LobbyManager.Instance.JoinLobby(id);
    }


    IEnumerator<WaitForSecondsRealtime> refreshLobbyListCoroutine()
    {
        var delay = new WaitForSecondsRealtime(10f);


        while (true)
        {
            refreshContent();
            Debug.Log($"Lobbies were refreshed");
            yield return delay;
        }
    }
}
