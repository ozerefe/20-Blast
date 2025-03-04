using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    public GameObject infoPanel; // Bilgi paneli

    public void OpenInfoPanel()
    {
        infoPanel.SetActive(true); // Paneli aç
    }

    public void CloseInfoPanel()
    {
        infoPanel.SetActive(false); // Paneli kapat
    }
    public void ExitGame()
    {
        Debug.Log("Game is exiting..."); // Unity Editör'de test için log ekleyelim
        Application.Quit(); // Oyunu kapat
    }
}
