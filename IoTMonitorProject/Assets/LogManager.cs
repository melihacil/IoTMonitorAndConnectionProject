using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();
    [SerializeField] TextMeshProUGUI _logArea;
    [SerializeField] Text[] _logText;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
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
            //Debug.Log("Test Logger" +  (myLogQueue.Count - 1));
            yield return waitTime;
        }
    }


    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        //Debug.Log(myLogQueue.Count);
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
        //_logArea.text = myLogQueue.ToString();
        foreach (var textfield in _logText)
            textfield.text = "\n" + string.Join("\n", myLogQueue.ToArray());

        //textfield.textInputUssName = "\n" + string.Join("\n", myLogQueue.ToArray());
    }

    //void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
    //    GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
    //    GUILayout.EndArea();
    //}
}