using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Networking;
using UnityEngine;
using UnityEngine.Networking;

public struct NamesData
{
    public List<string> names;
}


public class MenuManager : MonoBehaviour
{
    public RectTransform mainMenu;
    public RectTransform menu;
    public TextMeshProUGUI labelHealth;

    public TMP_Dropdown dropdownNames;
    List<string> allowedNames = new List<string>();

    public string API_URL = "http://monsterballgo.com/api/";
    public string ENDPOINT_NAMES = "names";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetNames();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnButtonCreate()
    {
        //ocultar el menu
        mainMenu.gameObject.SetActive(false);
        NetworkManager.Singleton.StartHost();
    }


    public void OnButtonJoin()
    {
        mainMenu.gameObject.SetActive(false);
        NetworkManager.Singleton.StartClient();
    }

    void GetNames()
    {
        //allowedNames.Add("Fer");
        //allowedNames.Add("Dany");
        //allowedNames.Add("Ale");

        dropdownNames.ClearOptions();
        dropdownNames.AddOptions(allowedNames);

        StartCoroutine(GetNamesFromServer());

    }

    IEnumerator GetNamesFromServer()
    {
        //hacer una petición de tipo GET
        UnityWebRequest request = UnityWebRequest.Get(API_URL + ENDPOINT_NAMES);

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log(request.downloadHandler.text);
            string json = request.downloadHandler.text;
            NamesData namesData = JsonUtility.FromJson<NamesData>(json);
            allowedNames.AddRange(namesData.names);
            dropdownNames.ClearOptions();
            dropdownNames.AddOptions(allowedNames);
        }
        else
        {
            Debug.Log("error al hacer la peticion web");
        }
    }

}
