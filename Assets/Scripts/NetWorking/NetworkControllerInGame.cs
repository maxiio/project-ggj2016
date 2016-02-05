﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkControllerInGame : MonoBehaviour
{
    public CompositionRoot compositionroot;
    static public bool LockLocalMovement;

    NetworkView nView;

    public float SyncornizeRate_MainChar = 0.06f;
    public float SyncornizeRate_AI = 1.5f;
    void Awake()
    {
        nView = GetComponent<NetworkView>();
        InvokeRepeating("SyncronizeMainCharData", 1.0f, SyncornizeRate_MainChar);
        InvokeRepeating("SyncronizeAIPos", 0.0f, SyncornizeRate_AI);

    }
    public GameObject Master;

    public GameObject[] AllAI = new GameObject[50];
    public CharacterView[] AllAIChaV = new CharacterView[50];
    public Character[] AllAICha = new Character[50];
    public AIControl[] AllAICon = new AIControl[50];

    public GameObject[] OtherPlayersRemote = new GameObject[5];
    public CharacterView[] OtherPlayersAIChaV = new CharacterView[5];
    public Character[] OtherPlayersCha = new Character[5];

    public GameObject LocalPlayer;
    public CharacterView LocalPlayerChaV;
    public Character LocalPlayerCha;

    int LastAIIndex = 0;
    public Character[] CharactersController = new Character[50];

    public GameManager gameManager;

    // Update is called once per frame



    Vector3 MasterLastPos = Vector3.zero;
    Vector3 MasterVelocity = Vector3.zero;

    Vector3[] AIsLastPos = new Vector3[50];
    Vector3[] AIVelocity = new Vector3[50];

    Vector3[] RPLastPos = new Vector3[5];
    Vector3[] RPVelocity = new Vector3[5];


    public bool[] IsLockPlayers = new bool[5];
    void SyncronizeMainCharData()
    {
        print("Dogg");
        if (Network.isServer)
        {
            nView.RPC("UpdateMasterTrans", RPCMode.Others, Master.transform.position, Master.transform.localScale);
        }
        nView.RPC("UpdateRtPlayerTrans", RPCMode.Others, MpLobby.MyIndex, LocalPlayer.transform.position, LocalPlayer.transform.localScale, LocalPlayerCha.IsDoingAction());
    }

    void SyncronizeAIPos()
    {
        if (Network.isServer)
        {
            for (int i = 0; i < 50; i++)
            {
                if (AllAI[i] != null)
                    nView.RPC("UpdateAITrans", RPCMode.All, i, AllAI[i].transform.position, AllAI[i].transform.localScale);
            }
        }
    }

    void Update()
    {

        for (int i = 0; i <50; i++)
        {
            if (AllAI[i] != null && !MpLobby.IsServer)
                AllAI[i].transform.position = Vector3.SmoothDamp(AllAI[i].transform.position, AIsLastPos[i], ref AIVelocity[i], SyncornizeRate_AI + 0.015f);//Mark, good(seems)

        }
        if (!MpLobby.IsServer)
            Master.transform.position = Vector3.SmoothDamp(Master.transform.position, MasterLastPos, ref MasterVelocity, SyncornizeRate_MainChar + 0.015f);//Mark, good(seems)

        for (int i = 0; i < MpLobby.PlayerCount; i++)
        {
            if (AllAI[i] != null && i != MpLobby.MyIndex)
                OtherPlayersRemote[i].transform.position = Vector3.SmoothDamp(OtherPlayersRemote[i].transform.position, RPLastPos[i], ref RPVelocity[i], SyncornizeRate_MainChar + 0.015f);//Mark, good(seems)
        }

        // if (!LockLocalMovement)
    }


    public void PushAI(int AI_Index, float dirX, float dirY)
    {
        nView.RPC("PushAIRemote", RPCMode.All, AI_Index, dirX, dirY);
    }

    public void PushPlayer(int PlayerIndex, float dirX, float dirY)
    {
        Debug.Log("Player is pushed: " + PlayerIndex);
        nView.RPC("PushPlayerRemote", RPCMode.All, PlayerIndex, dirX, dirY);
    }
    public void SynPushAnim()
    {
        nView.RPC("SynPlayerPush", RPCMode.Others, MpLobby.MyIndex);
    }

    public void KillAnAI(int AIIndex)
    {
        nView.RPC("KillAnAIRemote", RPCMode.Others, AIIndex);

        //Killed an AI;
    }

    public void KillAnPlayer(int RPIndex)
    {
        nView.RPC("KillAnPlayerRemote", RPCMode.Others, RPIndex);
        BroadCastMessage(MpLobby.Names[RPIndex] + " Was Sacrificed");
    }



    public void AllStartTheGame()
    {
        nView.RPC("StartTheRound", RPCMode.All);
    }
    //public void Syncro
    //Create myself remote

    public void Resetted()
    {
        nView.RPC("RmoteResetAllStatus", RPCMode.All);

    }
    public Text Board;
    public void BroadCastMessage(string Message)
    {
        nView.RPC("GotRemoteMEssage", RPCMode.All, Message);
    }

    public void SendMoveDir(float Dirx, float DirY, int NetworkId)
    {
        nView.RPC("GetRemoteDir", RPCMode.All, Dirx, DirY, NetworkId);
    }

    [RPC]
    void GetRemoteDir(float Dirx, float DirY, int NetworkId)
    {
        AllAICon[NetworkId].UpdateMoveDecision_Remote(Dirx, DirY);
    }


    [RPC]
    void GotRemoteMEssage(string Msg)
    {
        Board.text = Msg;

    }
    [RPC]
    void RmoteResetAllStatus()
    {
        gameManager.RemoteRestted();
    }


    [RPC]
    void StartTheRound()
    {
        gameManager.StartTheGame();
    }

    [RPC]
    void KillAnPlayerRemote(int RemotePlayerIndex)
    {
        //AllAICha[RemotePlayerIndex].Kill();
        if (RemotePlayerIndex != MpLobby.MyIndex)
            OtherPlayersCha[RemotePlayerIndex].Kill();
        else
            LocalPlayerCha.Kill();
    }

    [RPC]
    void KillAnAIRemote(int AIIndex)
    {
        AllAICha[AIIndex].Kill();
    }

    [RPC]
    void UpdateAITrans(int ObjIndex, Vector3 NewPos, Vector3 NewScale)
    {

        if (AllAI[ObjIndex] != null)
        {
           // AllAI[ObjIndex].transform.position = NewPos;
             AIsLastPos[ObjIndex] = NewPos;
            AllAI[ObjIndex].transform.localScale = NewScale;
            //  AllAICha[ObjIndex].Move(0.000001f, 0.000001f);//To play Animation
        }
        else
        {
            Debug.LogWarning("An AI isn't be signed ,Index : " + ObjIndex);
        }
    }

    [RPC]
    void UpdateRtPlayerTrans(int ObjIndex, Vector3 NewPos, Vector3 NewScale, bool IsAction)
    {
        if (OtherPlayersRemote[ObjIndex] != null)
        {
            //OtherPlayersRemote[ObjIndex].transform.position = NewPos;
            RPLastPos[ObjIndex] = NewPos;
            OtherPlayersRemote[ObjIndex].transform.localScale = NewScale;
            OtherPlayersCha[ObjIndex].DoAction(IsAction);
            //OtherPlayersCha[ObjIndex].Move(0.000001f, 0.000001f);//To play Animation
        }
    }

    [RPC]
    void SynPlayerPush(int PlayerIndex)
    {
        if (OtherPlayersCha[PlayerIndex] != null)
            OtherPlayersCha[PlayerIndex].Push();
        else
            Debug.Log("Error! Player ID" + PlayerIndex + " not exist");
    }

    [RPC]
    void UpdateMasterTrans(Vector3 LatestPos, Vector3 LocalScale)
    {
        if (Master != null)
        {
            //  Master.transform.position = LatestPos;
            MasterLastPos = LatestPos;
            Master.transform.localScale = LocalScale;
        }
    }

    [RPC]
    void PushAIRemote(int AIIndex, float dirX, float dirY)
    {
        Vector2 NewDir = Vector2.zero;
        NewDir.x = dirX;
        NewDir.y = dirY;
        AllAICha[AIIndex].Fall(NewDir);
    }

    [RPC]
    void PushPlayerRemote(int PlayerIndex, float dirX, float dirY)
    {
        Debug.Log("Got Pushed message");
        Vector2 NewDir = Vector2.zero;
        NewDir.x = dirX;
        NewDir.y = dirY;
        if (PlayerIndex != MpLobby.MyIndex)
        {
            OtherPlayersCha[PlayerIndex].Fall(NewDir);
        }
        else
        {
            //  LockLocalMovement = true;
            LocalPlayerCha.Fall(NewDir);
            //   StartCoroutine(UnlockAfter());
        }
    }

    IEnumerator UnlockAfter()
    {
        yield return new WaitForSeconds(0.5f);
        LockLocalMovement = false;
    }


}