using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Menu : MonoBehaviour
{
    public GameObject Menu;
    public GameObject Finish;
    [SerializeField] KeyCode Esc;
    bool check = false;
    public static int rest_try;
    // Start is called before the first frame update
    void Start()
    {
        Menu.SetActive(false);
        Time.timeScale = 1f;
       
    }

    // Update is called once per frame
    void Update()
    {
        Pause();
    }
    void Pause()
    {
        if (Finish.active == false) { 
        if (Input.GetKeyDown(Esc))
        {
            check= true;

        }
        if (check == true)
        {
            Menu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
                Cursor.visible= true;
            Time.timeScale = 0f;
        }
        else
        {
            Menu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
        }
        }

    }
    public void Coninue()
    {
        Menu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        check = false;
    }
    public void Restart()
    {
        rest_try++;
        if (Finish.active == true) rest_try = 0;
		SceneManager.LoadScene(3);

	}


	public void Exit()
    {
        SceneManager.LoadScene(0);
    }
}
