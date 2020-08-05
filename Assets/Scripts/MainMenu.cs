using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using System.Threading;
using NosCore.Packets.ServerPackets.CharacterSelectionScreen;


public class MainMenu : MonoBehaviour
{
    GameObject MainMenuObj;
    GameObject SelectAccountGameObj;

    public GameObject characterSelectBtn;

    public GameObject NostaleMainObj;

    private List<string> GFaccounts = null;

    List<ChannelInfo> ChannelList = null;
    List<ClistPacket> CharactersList = null;
    NostaleMain nt;
    static Thread mainThread = null;
    public void PlayGame(){
        Thread thisThread = new Thread(() =>
        {
            Debug.Log("Wybór Konta GF");
            
            if(nt.AuthGameforge() == false){
                Debug.Log("Gameforge Login error");
                return;
            }
            GFaccounts = nt.GetAccountsGameforge();
            Debug.Log("[T] PlayGame close");
        });
        thisThread.Start();
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Scenes/Game");
    }

    public void SelectAccountGameforge(string acc_name)
    {
        Thread thisThread = new Thread(() =>
        {
            Debug.Log("Wybór Konta");
            nt.SelectAccountGameforge(acc_name);

            if(nt.ChannelList.Count == 0) { Debug.Log("blad failc"); }

            ChannelList = nt.ChannelList;
            Debug.Log("[T] SelectAccountGameforge close");
        });
        thisThread.Start(); 
    }

    public void SelectChannel(string Host, int Port)
    {
        Thread thisThread = new Thread(() =>
        {
            Debug.Log("Łaczenie z "+Host+":"+Port.ToString());
            nt.Connect(Host, Port);
            nt.ConnectWorld();

            CharactersList = nt.GetCharactersList();
            Debug.Log("[T] SelectChannel close");
        });
        thisThread.Start();
    }

    public void SelectCharacterAndStartMain(int Slot)
    {
        mainThread = new Thread(() =>
        {
            nt.SelectCharacter(Slot);
            nt.Main(); // try catch?
        });
        mainThread.Start();
    }

    void ClearTempChildObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void Start()
    {
        ClearTempChildObjects();
        nt = NostaleMainObj.GetComponentInChildren<NostaleMain>();
    }

    void OnApplicationQuit()
    {
        if (mainThread != null)
        {
            Debug.Log("zamykam mainThread");
            mainThread.Abort();
        }
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

    public void Update()
    {
        if(GFaccounts != null)
        {
            //NostaleMain nt = NostaleMainObj.GetComponentInChildren<NostaleMain>();
            ClearTempChildObjects();
            for (int i = 0; i < GFaccounts.Count; i++)
            {
                string acc_name = GFaccounts[i].Split(':')[1];
                Debug.Log(acc_name);

                GameObject item = Instantiate(characterSelectBtn, new Vector3(0, 0, 0), Quaternion.identity);
                item.transform.parent = transform;
                item.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, 30 * i + 50, 0);
                item.GetComponentInChildren<Text>().text = acc_name;
                item.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectAccountGameforge(acc_name); });
            }
            GFaccounts = null;
        }

        if(ChannelList != null)
        {
            ClearTempChildObjects();
            for (int i = 0; i < ChannelList.Count; i++)
            {
                ChannelInfo chan = ChannelList[i];
               
                GameObject item = Instantiate(characterSelectBtn, new Vector3(0, 100, 0), Quaternion.identity);
                item.transform.parent = transform;
                item.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, 100 - 30 * i, 0);
                item.GetComponentInChildren<Text>().text = chan.WorldCount + "."+chan.WorldId+" "+chan.Name;
                item.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectChannel(chan.Host,chan.Port); });
            }
            ChannelList = null;
        }

        if (CharactersList != null)
        {
            ClearTempChildObjects();
            for (int i = 0; i < CharactersList.Count; i++)
            {
                ClistPacket c = CharactersList[i];

                GameObject item = Instantiate(characterSelectBtn, new Vector3(0, 100, 0), Quaternion.identity);
                item.transform.parent = transform;
                item.GetComponentInChildren<RectTransform>().localPosition = new Vector3(0, 100 - 30 * i, 0);
                item.GetComponentInChildren<Text>().text = c.Name + " lvl:" + c.Level + " (" + c.HeroLevel+")";
                item.GetComponentInChildren<Button>().onClick.AddListener(delegate { SelectCharacterAndStartMain(c.Slot); });
            }
            CharactersList = null;
        }
        
    }
}
