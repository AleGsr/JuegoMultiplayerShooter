using Unity.Netcode;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public RectTransform mainMenu;
    public RectTransform menu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
}
