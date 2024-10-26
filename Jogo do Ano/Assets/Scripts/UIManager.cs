using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void iniciarJogo(){
        SceneManager.LoadScene("fase_inicial");
    }

    public void voltarMenuPrincipal(){
        SceneManager.LoadScene("Menu");
    }

    public void loadGameOver(){
        SceneManager.LoadScene("TelaGameOver", LoadSceneMode.Additive);
    }

    public void sairAplicacao(){
        Application.Quit();
    }
}
