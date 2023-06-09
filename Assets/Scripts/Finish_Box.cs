using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Finish_Box : MonoBehaviour
{
    private TMP_Text retryResult;
    private TMP_Text retryChecker;
    public GameObject Menu;
    public GameObject Timer_Top;
    private TMP_Text ResultT;
    private TMP_Text Timer;
    private bool checker = false;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        checker = true;
        Menu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        //Debug.Log(a);
        Update();
        ResultT.text = Timer.text;
        retryResult.text = retryChecker.text;
        Timer_Top.SetActive(false);
        Time.timeScale = 0f;
    }
    void Start()
    {
        Menu.SetActive(false);

    }
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
    public void Exit()
    {
        SceneManager.LoadScene(0);
    }
    // Update is called once per frame
    void Update()
    {
        if (checker == true)
        {
            retryResult = GameObject.Find("Result_Rest").GetComponent<TMP_Text>();
            retryChecker = GameObject.Find("Retry_checker").GetComponent<TMP_Text>();
            ResultT = GameObject.Find("Result_Time").GetComponent<TMP_Text>();
            Timer = GameObject.Find("Timer").GetComponent<TMP_Text>();
            checker = false;
        }
    }
    
    
}
