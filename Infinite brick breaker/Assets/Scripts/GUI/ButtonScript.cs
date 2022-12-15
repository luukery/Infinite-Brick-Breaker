using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the script that has all button options. It is attached to the Managers so all the buttons are always avalible and easy to call upon.
/// </summary>
public class ButtonScript : MonoBehaviour
{
    [HideInInspector] public GameObject startMenu;
    private void Start()
    {
        startMenu = GameObject.Find("Main menu");   //a quick way to find the main menu canvas. It is not neccesary to make it public so for one frame il do this to get it.
    }
    public void StartGame()
    {
        GameObject hh = GetComponent<GameManager>().healthHolder;
        GameObject paddle = GameObject.Find("Paddle");      //not the cleanest way, but saving it is a waste, because this is the only time the refference is needed.

        GameManager.instance.NextLevel(true);
        GameManager.instance.launchSpeed = GameManager.instance.launchSpeedSave;
        LevelGeneratorManager.instance.levelsGenerated = 0;

       GameObject g = Instantiate(hh, Vector2.zero, Quaternion.identity);

        g.transform.rotation = paddle.transform.rotation;
        g.transform.parent = paddle.transform;
        g.transform.position = paddle.transform.position;
       
        GetComponent<ButtonScript>().enabled = false;
        startMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();        
    }
}
