using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{

    [SerializeField] private List<GameObject> _gamePanels;

    int activePanel = 0;

    private void Update()
    {
        if(Application.platform != RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainPanel();
            if (WifiServerController.instance != null)
            {
                WifiServerController.instance.CloseServer();
            }
        }
    }
    public void OpenMainPanel()
    {
        foreach (GameObject panel in _gamePanels)
        {
            panel.SetActive(false);
        }
        _gamePanels[0].SetActive(true);
    }



    public void ReturnToMainPanel()
    {
        _gamePanels[activePanel].SetActive(false);
        _gamePanels[0].SetActive(true);

    }


    public void OpenIndexedPanel(int index)
    {
        activePanel = index;
        _gamePanels[activePanel].SetActive(true);
        _gamePanels[0].SetActive(false);
    }

    public void CloseApplication()
    {
        Application.Quit();
    }

}
