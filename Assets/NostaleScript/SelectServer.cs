using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectServer : MonoBehaviour
{
    public string Name = "Server Name";
    public string Host = "";
    public int Port = 4000;
    public bool GameForgeLogin = false;

    public GameObject nostaleMain;
    public GameObject serversList;
    public GameObject loginForm;

    public void Start()
    {
        serversList.SetActive(true);
        loginForm.SetActive(false);
    }
    public void Select()
    {
        serversList.SetActive(false);
        loginForm.SetActive(true);

        Button loginbtn = loginForm.GetComponentInChildren<Button>();
        InputField loginInput = loginForm.transform.Find("Login").GetComponent<InputField>();
        InputField passwdInput = loginForm.transform.Find("Password").GetComponent<InputField>();

        loginbtn.onClick.AddListener(delegate { Login(loginInput.text, passwdInput.text); });
    }

    public void Login(string login, string password)
    {
        Debug.Log("Logowanie... ("+login+" "+password+")");
        NostaleMain nt = nostaleMain.GetComponent<NostaleMain>();
        nt.loginServerIP = Host;
        nt.loginServerPort = Port;
        nt.GameForgeLogin = GameForgeLogin;
        nt.login = login;
        nt.password = password;
        StartCoroutine(LoadLoginScreenScene());
    }

    IEnumerator LoadLoginScreenScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/LoginScreen");
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void Skip()
    {
        Debug.Log("skip...");
        NostaleMain nt = nostaleMain.GetComponent<NostaleMain>();
        nt.loginServerIP = "167.86.78.190";
        nt.loginServerPort = 4000;
        nt.GameForgeLogin = false;
        nt.login = "Killrog";
        nt.password = "test";
        StartCoroutine(LoadLoginScreenScene());
    }
}
