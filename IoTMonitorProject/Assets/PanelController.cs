using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{

    [SerializeField] private List<GameObject> _gamePanels;


    public void OpenMainPanel()
    {
        foreach (GameObject panel in _gamePanels)
        {
            panel.SetActive(false);
        }
        _gamePanels[0].SetActive(true);
    }



    public void ReturnToMainPanel(int index)
    {
        _gamePanels[index].SetActive(false);
        _gamePanels[0].SetActive(true);

    }


    public void OpenIndexedPanel(int index)
    {
        _gamePanels[index].SetActive(true);
        _gamePanels[0].SetActive(false);

    }

    public void CloseApplication()
    {
        Application.Quit();
    }

}
