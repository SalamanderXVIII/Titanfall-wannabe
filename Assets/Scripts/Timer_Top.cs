using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer_Top : MonoBehaviour
{
    public TMP_Text retryResult;

    private int sec = 0;
    private int min = 0;
    public TMP_Text timerResult;
    [SerializeField] private int delta = 0;
    // Start is called before the first frame update
    void Start()
    {
        retryResult = GameObject.Find("Retry_checker").GetComponent<TMP_Text>();
        timerResult = GameObject.Find("Timer").GetComponent<TMP_Text>();
        StartCoroutine(ITimer());
    }
    IEnumerator ITimer()
    {
        while (true)
        {
            if (sec == 59)
            {
                min++;
                sec = -1;
            }
            sec += delta;
            timerResult.text = "TIME: " + min.ToString("D2") + " : " + sec.ToString("D2");
            yield return new WaitForSeconds(1);
        }
    }
    // Update is called once per frame
    void Update()
    {
        string a = Pause_Menu.rest_try.ToString();
        retryResult.text = "RESTART: " + a.ToString();
    }
}
