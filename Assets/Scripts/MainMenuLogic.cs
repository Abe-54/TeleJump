using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("NarritiveTest");
    }
}
