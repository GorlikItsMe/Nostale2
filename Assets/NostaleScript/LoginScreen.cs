using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NosCore.Packets.ServerPackets.CharacterSelectionScreen;
using UnityEngine.SceneManagement;

public class LoginScreen : MonoBehaviour
{
    public Text LoginStatus;
    public GameObject SelectChannelForm;
    public GameObject SelectChannelBtnPrefab;
    public GameObject SelectCharacterForm;
    public GameObject SelectCharacterBtnPrefab;

    NostaleMain nt;
    async void Start()
    {
        nt = GameObject.Find("NostaleMain").GetComponent<NostaleMain>();
        await LoginToServer();
    }
    void Update()
    {
        
    }
    public void UpdateStatus(string text)
    {
        Debug.Log($"[STATUS] {text}");
        LoginStatus.text = text;
    }

    IEnumerator GetRequest(string uri)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }
    }


    async Task<bool> LoginToServer()
    {
        if (nt.GameForgeLogin)
        {
            UpdateStatus("Logowanie do usługi GameForge...");
        }
        else
        {
            UpdateStatus("Pobieranie kluczy deszyfrujących...");
            if (await nt.SetupNostaleVersionAsync() == false) {
                UpdateStatus("BŁĄD. Nie udało się pobrać kluczy deszyfrujących.\nSpróbuj ponownie później.");
                return false;
            }
            UpdateStatus("Łączenie z serwerem Logowania...");
            await Task.Run(() =>
            {
                nt.Connect(nt.loginServerIP, nt.loginServerPort);
                nt.ConnectLogin(); // dodać obslugą błędów
            });
            UpdateStatus("Oczekiwanie na wybór kanału");

            // Rysowanie przycisków wyboru kanału
            int i = 0;
            ClearChannelList();
            TaskCompletionSource<bool> isCharacterSelected = new TaskCompletionSource<bool>();
            List<ClistPacket> CharactersList = null;
            foreach (ChannelInfo c in nt.ChannelList)
            {
                GameObject item = Instantiate(SelectChannelBtnPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                item.transform.SetParent(SelectChannelForm.transform);
                item.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, 100 - 30 * i, 0);
                item.GetComponentInChildren<Text>().text = $"{c.WorldId} {c.Name}";
                item.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                    Debug.Log($"click {c.Host} {c.Port}");
                    UpdateStatus($"Łączenie z {c.WorldCount}.{c.WorldId}.{c.Name}");
                    nt.Connect(c.Host, c.Port);
                    nt.ConnectWorld();
                    CharactersList = nt.GetCharactersList();
                    isCharacterSelected.SetResult(true);
                });
                i++;
            }
            await isCharacterSelected.Task;
            UpdateStatus("Oczekiwanie na wybór Postaci");
            SelectChannelForm.SetActive(false);
            i = 0;
            ClearCharacterlList();
            foreach (ClistPacket c in CharactersList)
            {
                GameObject item = Instantiate(SelectCharacterBtnPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                item.transform.SetParent(SelectCharacterForm.transform);
                item.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, 100 - 30 * i, 0);
                item.GetComponentInChildren<Text>().text = $"{c.Name} lvl: {c.Level}({c.HeroLevel})";
                item.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                    UpdateStatus($"Logowanie jako {c.Name}");
                    nt.SelectCharacter(c.Slot); // obsługa błędów???
                    StartCoroutine(LoadGameScene());
                    nt.StartGamePacketHandlerThread();
                });
                i++;
            }
        }
        return true;
    }
    public void ClearChannelList()
    {
        for (int i = SelectChannelForm.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(SelectChannelForm.transform.GetChild(i).gameObject);
        }
    }
    public void ClearCharacterlList()
    {
        for (int i = SelectCharacterForm.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(SelectCharacterForm.transform.GetChild(i).gameObject);
        }
    }

    IEnumerator LoadGameScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/Game");
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void Test()
    {
        Debug.Log("test");

        GameObject item = Instantiate(SelectChannelBtnPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        item.transform.SetParent(SelectChannelForm.transform);
        item.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, 100, 0);
        item.GetComponentInChildren<Text>().text = "NazwaKanalu";
        item.GetComponentInChildren<Button>().onClick.AddListener(delegate { Debug.Log("click"); });

    }



}
