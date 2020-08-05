using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Text;
using System.IO;
using System.Net;
using System;
using SimpleJSON;

using System.Net.Sockets;

using NosCore.Packets;
using NosCore.Packets.Interfaces;
using NosCore.Packets.ServerPackets.CharacterSelectionScreen;
using NosCore.Packets.ServerPackets.Login;
using NosCore.Packets.ServerPackets.MiniMap;
using System.Threading.Tasks;

public class NostaleMain : MonoBehaviour
{
    public string login = "nabijamrepe9@gmail.com";
    public string password = "nabijamrepe";
    public string loginServerIP = "79.110.84.75";
    public int loginServerPort = 4004;
    public string installation_guid = "05670810-c4b4-4d95-88fc-2043b9c837f0"; // pobierany gdzies z rejestru
    public bool GameForgeLogin = true;

    private string session_token, logingf, dxhash, glhash, version;

    private Thread mainThread;
    private ntAuth ntAuth;
    private List<string> AccountsGameforge;
    public List<ChannelInfo> ChannelList;
    public GameObject PlayerGO;

    NetworkStream serverStream;
    System.Net.Sockets.TcpClient clientSocket;
    int sessionId = 0;
    WorldEncryption WorldEncryption;
    int id = 53535;

    Queue<string> packetsQueue = new Queue<string>();
    IDeserializer Deserializer = new Deserializer(new[] { typeof(AtPacket) });


    public bool AuthGameforge(){
        Debug.Log("Logowanie GameForge... (dodac wsparcie dla kilku jezykow)");
        ntAuth = new ntAuth("pl_PL", "pl", "5ea61643-b22b-4ad6-89dd-175b0be2c9d9");
        return ntAuth.auth(login, password);
    }
    public List<string> GetAccountsGameforge(){
        AccountsGameforge = ntAuth.getAccounts();
        return AccountsGameforge;
    }
    
    public void SelectAccountGameforge(string account_name){
        for (int i = 0; i < AccountsGameforge.Count; i++)
        {
            string account = AccountsGameforge[i];
            if(account.Split(':')[1]==account_name){
                session_token = ntAuth.getToken(account.Split(':')[0]);
                logingf = account.Split(':')[1]+" GF 4"; // PL
                Debug.Log("Logowanie PL");
                Connect(loginServerIP, loginServerPort);
                ConnectLogin();

            }
        }
    }

    public bool SetupNostaleVersion(){
        try
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("https://nostale-version.herokuapp.com");
            if (webRequest != null)
            {
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                System.Net.WebResponse response;
                response = webRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var N = JSON.Parse(responseString);

                glhash = N["hashNostaleClient"].Value;
                dxhash = N["hashNostaleClientX"].Value;
                version = N["version"].Value;
                Debug.Log("[i] Wersja Nostale "+version);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            throw;
        }
        return false;
    }
    public async Task<bool> SetupNostaleVersionAsync()
    {
        bool ret = false;
        await Task.Run(() => {
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create("https://nostale-version.herokuapp.com");
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    System.Net.WebResponse response;
                    response = webRequest.GetResponse();
                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var N = JSON.Parse(responseString);

                    glhash = N["hashNostaleClient"].Value;
                    dxhash = N["hashNostaleClientX"].Value;
                    version = N["version"].Value;
                    Debug.Log("[i] Wersja Nostale " + version);
                    ret = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
                ret = false;
                return;
            }
        });

        return ret;
    }


    public void Connect(string ip = "127.0.0.1", int port = 4004)
    {
        try
        {            
            clientSocket = new System.Net.Sockets.TcpClient();
            clientSocket.Connect(ip, port);
            clientSocket.ReceiveBufferSize = 1024 * 8;
            serverStream = clientSocket.GetStream();
        }
        catch (System.Net.Sockets.SocketException)
        {
            Debug.Log("Cant Connect");
            throw new System.Exception("Cant Connect");
        }
    }

    public bool ConnectLogin()
    {
        Debug.Log("[i] Login...");
        //SetupNostaleVersion();

        var LoginEncryption = new LoginEncryption(dxhash, glhash, version);
        string loginpacketstr;
        if (GameForgeLogin){ loginpacketstr = LoginEncryption.CreateLoginPacket(session_token, installation_guid); }
        else{ loginpacketstr = LoginEncryption.CreateLoginPacketOld(login, password, installation_guid); }

        var loginpacket = LoginEncryption.Encrypt(loginpacketstr);
        Send(loginpacket);

        byte[] loginpacketsuccess_byte = Recv();
        string loginpacketsuccess = LoginEncryption.Decrypt(loginpacketsuccess_byte, loginpacketsuccess_byte.Length);
        
        //"NsTeST 4 nabijamrepe9 2 17238 79.110.84.132:4016:1:1.7.Feniks 79.110.84.132:4014:1:1.5.Feniks 79.110.84.132:4015:0:1.6.Feniks 79.110.84.132:4011:8:1.2.Feniks 79.110.84.132:4012:1:1.3.Feniks 79.110.84.132:4013:0:1.4.Feniks 79.110.84.132:4010:1:1.1.Feniks -1:-1:-1:10000.10000.1");
        // TODO obsługa failc
        if (loginpacketsuccess.Contains("failc")) { Debug.Log("[recv] " + loginpacketsuccess); return false; }

        if (GameForgeLogin)
        {
            IDeserializer Deserializer = new Deserializer(
            new[] {
                    typeof(NsTestPacket),
                    typeof(NsTeStSubPacket),
            });
            var packet = (NsTestPacket)Deserializer.Deserialize(loginpacketsuccess);

            sessionId = packet.SessionId;
            ChannelList = new List<ChannelInfo>();
            foreach (NsTeStSubPacket c in packet.SubPacket)
            {
                if (c.Host == "-1") { continue; }
                ChannelList.Add(
                    new ChannelInfo
                    {
                        Host = c.Host,
                        Port = (int)c.Port,
                        Color = (int)c.Color,
                        WorldCount = c.WorldCount,
                        WorldId = c.WorldId,
                        Name = c.Name
                    }
                );
            }
        }
        else
        {
            //NsTeST Gorlik 50 127.0.0.1:1337:2:1.1.Name 79.110.84.132:4016:1:1.7.Feniks
            string[] p = loginpacketsuccess.Split(' ');

            sessionId = Int32.Parse(p[2]);
            ChannelList = new List<ChannelInfo>();
            for (int i = 3; i < p.Length; i++)
            {
                if(p[i].Contains(":") == false) { continue; /* ostatni pakiet albo zbugowany*/ }
                string[] c = p[i].Split(':');
                if (c[0] == "-1") { continue; }
                ChannelList.Add(
                    new ChannelInfo
                    {
                        Host = c[0],
                        Port = Int32.Parse(c[1]),
                        Color = Int32.Parse(c[2]),
                        WorldCount = Int32.Parse(c[3].Split('.')[0]),
                        WorldId = Int32.Parse(c[3].Split('.')[1]),
                        Name = c[3].Split('.')[2],
                    }
                );
            }
        }

        ChannelList.Sort((x, y) => string.Compare(x.WorldCount.ToString(), y.WorldCount.ToString()));
        ChannelList.Sort((x, y) => string.Compare(x.WorldId.ToString(), y.WorldId.ToString()));
        return true;
    }

    public void ConnectWorld()
    {
        WorldEncryption = new WorldEncryption(sessionId);

        Send(WorldEncryption.Encrypt(id.ToString() + " " + sessionId.ToString(), true));
        Thread.Sleep(1000);
        id++;
        if (GameForgeLogin) { 
            Send(WorldEncryption.Encrypt(id.ToString() + " " + logingf, false));
            id++;
            Send(WorldEncryption.Encrypt(id.ToString() + " thisisgfmode", false));
        }
        else{
            Send(WorldEncryption.Encrypt(id.ToString() + " " + login, false));
            id++;
            Send(WorldEncryption.Encrypt(id.ToString() + " "+password, false));
        }
    }
    public List<ClistPacket> GetCharactersList()
    {
        List<ClistPacket> list = new List<ClistPacket>();
        IDeserializer Deserializer = new Deserializer(
            new[] {
                //typeof(ClistStartPacket),
                typeof(ClistPacket),
                //typeof(ClistEndPacket),
            });
        bool stop = false;
        while (!stop)
        {
            string[] packets = RecvPackets();
            foreach (string packet_raw in packets)
            {
                if (packet_raw.StartsWith("clist_start")) { continue; }
                else if (packet_raw.StartsWith("clist_end")) { stop = true; break; }
                //ClistPacket c = (ClistPacket)Deserializer.Deserialize(packet_raw);
                else if (packet_raw.StartsWith("clist"))
                {
                    ClistPacket c = new ClistPacket
                    {
                        Slot = byte.Parse(packet_raw.Split(' ')[1]),
                        Name = packet_raw.Split(' ')[2],
                        Equipments = new List<short?>() { null, null, null, null, null, null, null, null, null, null },
                        Pets = new List<short?>() { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                        Rename = true
                    };
                    list.Add(c);
                }
                else
                {
                    Debug.LogError("#f8tf3 " + packet_raw);
                }
            }
        }
        return list;
    }
    public bool SelectCharacter(int characterid)
    {
        SendPacket("c_close 0");
        SendPacket("f_stash_end");
        SendPacket("c_close 1");
        SendPacket($"select {characterid}");
        Debug.Log($"[i] Select character {characterid}");
        SendPacket("game_start");
        SendPacket("lbs 0");
        SendPacket("c_close 1");
        SendPacket("npinfo 0");

        byte[] recvPacket = Recv();
        var packets = WorldEncryption.Decrypt(recvPacket, recvPacket.Length).ToArray();
        foreach (var packet_raw in packets)
        {
            if (packet_raw.Contains("OK")) { return true; }
        }
        return false;
    }

    void SelectCharacter_old(int characterid)
        // characterid - int 0-3 (first character is 0)
        {
            byte[] recvPacket;
            while (true)
            {
                bool exit = false;
                recvPacket = Recv();
                var packets = WorldEncryption.Decrypt(recvPacket, recvPacket.Length).ToArray();
                foreach (var packet_raw in packets)
                {
                    if (packet_raw.Contains("fail"))
                    {
                        Debug.Log(packet_raw);
                    }
                    //Console.WriteLine(packet_raw);
                    if(packet_raw == "clist_start 0")
                    {
                        Debug.Log("Postaci konta (characterId):");
                    }
                    if (packet_raw.Split(' ')[0] == "clist")
                    {
                        Debug.Log(packet_raw.Split(' ')[1]+": "+packet_raw.Split(' ')[2]);
                    }    

                    if (packet_raw == "clist_end\n") // \n   <---- jak priv to bez nowej lini, jak official to z naowa linia
                    {
                        SendPacket("c_close 0");
                        SendPacket("f_stash_end");
                        SendPacket("c_close 1");
                        SendPacket($"select {characterid}");           //slot postaci -1 (czyli liczysz od 0)
                        Debug.Log($"[i] Select character {characterid}");
                        SendPacket("game_start");
                        SendPacket("lbs 0");
                        SendPacket("c_close 1");
                        SendPacket("npinfo 0");
                        exit = true;
                        break;
                    }
                }
                if (exit) { break; }
            }
        }
    
    public void SendPacket(string packet)
    {
        id++;
        Send(WorldEncryption.Encrypt(id.ToString() + " " + packet, false));
    }

    public void Send(byte[] outStream)
    {
        serverStream.Write(outStream, 0, outStream.Length);
        serverStream.Flush();   
    }
    public byte[] Recv()
    {
        byte[] inStream = new byte[(int)clientSocket.ReceiveBufferSize]; // 1024
        serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);  //(int)clientSocket.ReceiveBufferSize


        for (int i = 0; i < inStream.Length; i++)
        {
            if (inStream[i] == 0)
            {
                byte[] temp = new byte[i];
                for (int j = 0; j < i; j++)
                {
                    temp[j] = inStream[j];
                }
                return temp;
            }     
        }
        return inStream;
    }

    public string[] RecvPackets()
    {
        byte[] recvPacket = Recv();
        return  WorldEncryption.Decrypt(recvPacket, recvPacket.Length).ToArray();
    }

    public async void FastServerJoin()
    {
        Debug.Log("use FastServerJoin()");
        loginServerIP = "167.86.78.190";
        loginServerPort = 4000;
        GameForgeLogin = false;
        login = "Killrog";
        password = "test";

        NostaleMain nt = GameObject.Find("NostaleMain").GetComponent<NostaleMain>();

        nt.Connect("167.86.78.190", 4000);
        await nt.SetupNostaleVersionAsync();
        nt.ConnectLogin();
        
        nt.Connect("167.86.78.190", 1337);
        nt.ConnectWorld();
        nt.GetCharactersList();
        nt.SelectCharacter(0);

        IEnumerator LoadGameScene_jhasdjas()
        {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Scenes/Game");
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        StartCoroutine(LoadGameScene_jhasdjas());

        nt.StartGamePacketHandlerThread();
    }

    public void Main()
    {
        Debug.LogError("Stara funkcja do usuniecia");
        while (true)
        {
            string[] packets = RecvPackets();
            foreach (string packet_raw in packets)
            {
                packetsQueue.Enqueue(packet_raw);
            }
        }
    }
    public void StartGamePacketHandlerThread()
    {
        mainThread = new Thread(() =>
        {
            while (true)
            {
                string[] packets = RecvPackets();
                foreach (string packet_raw in packets)
                {
                    packetsQueue.Enqueue(packet_raw);
                }
            }
            // try catch?
        });
        mainThread.Start();
    }

    void Update()
    {
        StartCoroutine(PacketsMain());
    }

    IEnumerator PacketsMain()
    {
        if (packetsQueue.Count != 0){
            string packet_raw = packetsQueue.Dequeue();
            string h = packet_raw.Split(' ')[0];
            if (packet_raw.StartsWith("OK"))
            {
                Debug.Log("Pakiet OK tu był");
                /*AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Scenes/Game");
                // Wait until the asynchronous scene fully loads
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
                */
            }
            else if (h == "at")
            {
                var p = (AtPacket)Deserializer.Deserialize(packet_raw);
                Debug.Log("Load map: " + p.MapId);

                GameObject player = Instantiate(PlayerGO, new Vector3(p.PositionX, 0, p.PositionY), Quaternion.identity);
                player.transform.position = new Vector3(p.PositionX, 0, p.PositionY);

                PlayerControler pc = player.GetComponent<PlayerControler>();
                pc.CharacterId = p.CharacterId;
                yield return null;
            }
            else
            {
                if (h != "stat")
                {
                    Debug.LogWarning(packet_raw);
                }
                yield return null;
            }
            yield return null;
        }
        
    
    }

    public void Test()
    {
        Debug.Log("pog?");
        //GameObject player = Instantiate(PlayerGO, new Vector3(0, 0, 0), Quaternion.identity);
        //player.transform.position = new Vector3(100, 0, 50);
    }

}


public class ChannelInfo
{
    public string Host;
    public int Port;
    public int Color;
    public int WorldCount;
    public int WorldId;
    public string Name;
}
