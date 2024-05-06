using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();
    [SerializeField] TextMeshProUGUI _logArea;
    [SerializeField] Text _logText;

    void Start()
    {
        Debug.Log("Started up logging.");
        StartCoroutine(nameof(TestLogger));
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }



    IEnumerator TestLogger()
    {
        WaitForSeconds waitTime = new WaitForSeconds(1);
        while (true)
        {
            Debug.Log("Test Logger" + myLogQueue.Count);
            Debug.LogWarning("Warning Logger");
            Debug.LogError("Error logger");
            yield return waitTime;
        }
    }


    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        Debug.Log(myLogQueue.Count);
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();

        //_logArea.text = myLogQueue.ToString();

        _logText.text = "\n" + string.Join("\n", myLogQueue.ToArray());
    }

    //void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
    //    GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
    //    GUILayout.EndArea();
    //}
}