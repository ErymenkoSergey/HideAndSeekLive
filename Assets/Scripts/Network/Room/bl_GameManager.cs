using System;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_GameManager : bl_PhotonHelper, IInRoomCallbacks, IConnectionCallbacks {

    public static int LocalPlayerViewID = -1;
    public static int SuicideCount = 0;
    public static bool Joined = false;

    public MatchState GameMatchState;
    public Action onAllPlayersRequiredIn;

    public List<Player> connectedPlayerList = new List<Player>();
    [HideInInspector] public List<MFPSPlayer> OthersActorsInScene = new List<MFPSPlayer>();

    #region Public properties
    public MFPSPlayer LocalActor { get; set; } = new MFPSPlayer();
    public Team LocalPlayerTeam { get; set; }
    public bool spawnInQueque { get; set; }
    public int Headshots { get; set; }
    public IGameMode GameModeLogic { get; private set; }
    public bool GameFinish { get; set; }
    public new GameObject LocalPlayer { get; set; }
    public bl_PlayerReferences LocalPlayerReferences { get; set; }
    public bl_OverridePlayerPrefab OverridePlayerPrefab { get; set; }
    public string LastInstancedPlayerPrefabName { get; private set; }
    #endregion

    #region Private members
    private int WaitingPlayersAmount = 1;
    private float StartPlayTime;
    private bool registered = false;
#if UMM
    private Canvas MiniMapCanvas = null;
#endif
    #endregion

    #region Unity Method
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (!registered) { PhotonNetwork.AddCallbackTarget(this); registered = true; }

        PhotonNetwork.IsMessageQueueRunning = true;
        bl_UtilityHelper.BlockCursorForUser = false;
        Joined = false;
        SuicideCount = 0;
        StartPlayTime = Time.time;

        LocalActor.isRealPlayer = true;
        LocalActor.Name = PhotonNetwork.NickName;

        bl_UCrosshair.Instance.Show(false);
#if UMM
        bl_MiniMap mm = FindObjectOfType<bl_MiniMap>();
        if (mm != null)
        {
            MiniMapCanvas = mm.m_Canvas;
            MiniMapCanvas.enabled = false;
        }
        else
        {
            Debug.Log("Minimap is enabled but not integrated in this map");
        }
#endif
        if (bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.WaitingRoom && PhotonNetwork.LocalPlayer.GetPlayerTeam() != Team.None)
        {
            Invoke(nameof(SpawnPlayerWithCurrentTeam), 2);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if(GameModeLogic == null)//check on start cuz game mode should be assigned on awake
        {
            Debug.LogWarning("No Game Mode has been assigned yet!");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onRemoteActorChange += OnRemoteActorChange;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onRemoteActorChange -= OnRemoteActorChange;
        if(registered)
        PhotonNetwork.RemoveCallbackTarget(this);
        if(GameModeLogic != null)
        {
            bl_PhotonCallbacks.PlayerEnteredRoom -= GameModeLogic.OnOtherPlayerEnter;
            bl_PhotonCallbacks.RoomPropertiesUpdate -= GameModeLogic.OnRoomPropertiesUpdate;
            bl_PhotonCallbacks.PlayerLeftRoom -= GameModeLogic.OnOtherPlayerLeave;
            bl_EventHandler.onLocalPlayerDeath -= GameModeLogic.OnLocalPlayerDeath;
        }
    }
    #endregion

    /// <summary>
    /// Spawn the local player in the give team
    /// </summary>
    /// <param name="playerTeam"></param>
    /// <returns></returns>
    public bool SpawnPlayer(Team playerTeam)
    {
        if (spawnInQueque) return false;//there's a reserved spawn incoming, don't spawn before that

        //if there is a local player already instance
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);//destroy it
        }

        //if the game finish
        if (GameFinish)
        {
            bl_RoomCamera.Instance?.SetActive(false);
            return false;
        }

        //set the player team to the player properties
        Hashtable PlayerTeam = new Hashtable();
        PlayerTeam.Add(PropertiesKeys.TeamKey, playerTeam.ToString());
        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerTeam);
        LocalPlayerTeam = playerTeam;

        //spawn the player model
#if !PSELECTOR
        SpawnPlayerModel(playerTeam);
#else
        if (bl_PlayerSelector.InMatch)
        {
            if (!bl_PlayerSelector.Instance.TrySpawnSelectedPlayer(playerTeam)) return false;
        }
        else
        {
            bl_PlayerSelector.SpawnPreSelectedPlayer(playerTeam);
        }
#endif
        return true;
    }

    /// <summary>
    /// Spawn player based in the team
    /// </summary>
    /// <returns></returns>
    public void SpawnPlayerModel(Team playerTeam)
    {
        Vector3 pos;
        Quaternion rot;
        bl_SpawnPointManager.Instance.GetPlayerSpawnPosition(playerTeam, out pos, out rot);
        
        GameObject playerPrefab = bl_GameData.Instance.Player1.gameObject;
        if (OverridePlayerPrefab == null)
        {
            if (playerTeam == Team.Maniac) playerPrefab = bl_GameData.Instance.Player2.gameObject;
        }
        else
        {
            playerPrefab = OverridePlayerPrefab.GetPlayerForTeam(playerTeam);
        }

        InstancePlayer(playerPrefab, pos, rot, playerTeam);
        AfterSpawnSetup();

        if (!FirstSpawnDone && bl_MatchInformationDisplay.Instance != null) { bl_MatchInformationDisplay.Instance.DisplayInfo(); }
        FirstSpawnDone = true;
        bl_UCrosshair.Instance.Show(true);
    }

    /// <summary>
    /// Instanced the given player prefab
    /// </summary>
    public void InstancePlayer(GameObject prefab, Vector3 position, Quaternion rotation, Team team)
    {
        //set the some common data that will be sync right after the player is instanced in the other clients
        var commonData = new object[1];
        commonData[0] = team;

        LastInstancedPlayerPrefabName = prefab.name;

        //instantiate the player prefab
        LocalPlayer = PhotonNetwork.Instantiate(prefab.name, position, rotation, 0, commonData);

        LocalPlayerReferences = LocalPlayer.GetComponent<bl_PlayerReferences>();
        LocalActor.Actor = LocalPlayer.transform;
        LocalActor.ActorView = LocalPlayer.GetComponent<PhotonView>();
        LocalActor.Team = team;
        LocalActor.AimPosition = LocalPlayer.GetComponent<bl_PlayerSettings>().AimPositionReference.transform;

        bl_EventHandler.DispatchPlayerLocalSpawnEvent();
    }

    /// <summary>
    /// Assign a player to a team but not instance it
    /// </summary>
    public void SetLocalPlayerToTeam(Team team)
    {
        Hashtable PlayerTeam = new Hashtable();
        PlayerTeam.Add(PropertiesKeys.TeamKey, team.ToString());
        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerTeam, null);
        LocalPlayerTeam = team;
        Joined = true;
    }

    /// <summary>
    /// Instance a Player only if already has been instanced and is alive
    /// </summary>
    public void SpawnPlayerIfAlreadyInstanced()
    {
        if (LocalPlayer == null)
            return;

        Team t = PhotonNetwork.LocalPlayer.GetPlayerTeam();
        SpawnPlayer(t);
    }

    /// <summary>
    /// If Player exist, them destroy
    /// </summary>
    public void DestroyPlayer(bool ActiveCamera)
    {
        if (LocalPlayer != null)
        {
            PhotonNetwork.Destroy(LocalPlayer);
        }
        bl_RoomCamera.Instance?.SetActive(ActiveCamera);
    }

    /// <summary>
    /// Called when the room round time finish
    /// </summary>
    public void OnGameTimeFinish(bool gameOver)
    {
        GameFinish = true;
        if(GameModeLogic != null) { GameModeLogic.OnFinishTime(gameOver); }
    }

    /// <summary>
    /// Make the local player spawn in a random spawn point of the current team
    /// </summary>
    public void SpawnPlayerWithCurrentTeam()
    {
        if(SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam()))
        bl_RoomMenu.Instance.OnAutoTeam();
    }

    /// <summary>
    /// Called after the local player spawn
    /// </summary>
    public void AfterSpawnSetup()
    {
        bl_RoomCamera.Instance?.SetActive(false);
        StartCoroutine(bl_UIReferences.Instance.FinalFade(false, false, 0));
        bl_UtilityHelper.LockCursor(true);
        if (!Joined) { StartPlayTime = Time.time; }
        Joined = true;

#if UMM
        if (MiniMapCanvas != null)
        {
            MiniMapCanvas.enabled = true;
        }
        else
        {
            Debug.LogWarning("MiniMap addon is enabled but not integrated in this scene");
        }
#endif
    }

    #region GameModes
    public bool IsGameMode(GameMode mode, IGameMode logic)
    {
        bool isIt = GetGameMode == mode;
        if (isIt)
        {
            if (GameModeLogic != null)
            {
                Debug.LogError("A GameMode has been assigned before, only 1 game mode can be assigned per match.");
                return false;
            }
            GameModeLogic = logic;
            bl_PhotonCallbacks.PlayerEnteredRoom += logic.OnOtherPlayerEnter;
            bl_PhotonCallbacks.RoomPropertiesUpdate += logic.OnRoomPropertiesUpdate;
            bl_PhotonCallbacks.PlayerLeftRoom += logic.OnOtherPlayerLeave;
            bl_EventHandler.onLocalPlayerDeath += GameModeLogic.OnLocalPlayerDeath;
            Debug.Log("Game Mode: " + mode.GetName());
        }
        return isIt;
    }

    /// <summary>
    /// Should called when the local player score in game
    /// </summary>
    public void SetPointFromLocalPlayer(int points, GameMode mode)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    /// <summary>
    /// Should called when a non player object score in game, ex: AI
    /// This should be called by master client only
    /// </summary>
    public void SetPoint(int points, GameMode mode, Team team)
    {
        if (GameModeLogic == null) return;
        if (mode != GetGameMode) return;

        GameModeLogic.OnLocalPoint(points, team);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLocalPlayerKill()
    {
        if (GameModeLogic == null) return;

        GameModeLogic.OnLocalPlayerKill();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isLocalPlayerWinner()
    {
        if (GameModeLogic == null) return false;
        return GameModeLogic.isLocalPlayerWinner;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public bool WaitForPlayers(int MinPlayers)
    {
        if (MinPlayers > 1)
        {
            if (isOneTeamMode)
            {
                if (PhotonNetwork.PlayerList.Length >= MinPlayers) return false;
            }
            else
            {
                if (PhotonNetwork.PlayerList.Length >= MinPlayers)
                {
                    if (PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Hiding).Length > 0 && PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Maniac).Length > 0)
                    {
                        if(onAllPlayersRequiredIn != null) { onAllPlayersRequiredIn.Invoke(); }
                        return false;
                    }
                }
            }
        }
        WaitingPlayersAmount = MinPlayers;
        SetGameState(MatchState.Waiting);
        return true;
    }

    /// <summary>
    /// This is a event callback
    /// here is caching all 'actors' in the scene (players and bots)
    /// </summary>
    public void OnRemoteActorChange(string actorName, MFPSPlayer playerData, bool spawning)
    {
        if (OthersActorsInScene.Exists(x => x.Name == actorName))
        {
            int id = OthersActorsInScene.FindIndex(x => x.Name == actorName);
            if (spawning)
            {
                if (playerData != null)
                {
                    OthersActorsInScene[id] = playerData;
                }
                else
                {
                    OthersActorsInScene[id].isAlive = spawning;
                }
            }
            else
            {
                if(OthersActorsInScene[id].Actor == null)
                {
                    OthersActorsInScene[id].isAlive = false;
                }
            }
        }
        else
        {
            if (spawning)
            {
               if(playerData == null) { Debug.LogWarning($"Actor data for {actorName} has not been build yet."); return; }
               if(playerData.ActorView == null) { playerData.ActorView = playerData.Actor?.GetComponent<PhotonView>(); }
                OthersActorsInScene.Add(playerData);
            }
        }
    }

    /// <summary>
    /// Find a player or bot by their PhotonView ID
    /// </summary>
    /// <returns></returns>
    public Transform FindActor(int ViewID)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if(OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.ViewID == ViewID) 
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if(LocalPlayer != null && LocalPlayer.GetPhotonView().ViewID == ViewID) { return LocalPlayer.transform; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their PhotonPlayer
    /// </summary>
    /// <returns></returns>
    public Transform FindActor(Player player)
    {
        if (player == null) return null;
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.Owner != null && OthersActorsInScene[i].ActorView.Owner.ActorNumber == player.ActorNumber)
            {
                return OthersActorsInScene[i].Actor;
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().Owner.ActorNumber == player.ActorNumber) { return LocalPlayer.transform; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their PhotonPlayer
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer FindActor(string actorName)
    {
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].Actor.name == actorName)
            {
                return OthersActorsInScene[i];
            }
        }
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().Owner.NickName == actorName) { return LocalActor; }
        return null;
    }

    /// <summary>
    /// Find a player or bot by their ViewID
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer FindMFPSActor(int viewID)
    {
        if (LocalPlayer != null && LocalPlayer.GetPhotonView().ViewID == viewID) { return LocalActor; }
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].ActorView != null && OthersActorsInScene[i].ActorView.ViewID == viewID)
            {
                return OthersActorsInScene[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    public MFPSPlayer GetMFPSPlayer(string nickName)
    {
        MFPSPlayer player = OthersActorsInScene.Find(x => x.Name == nickName);
        if(player == null && nickName == LocalName)
        {
            player = LocalActor;
        }
        return player;
    }

    /// <summary>
    /// 
    /// </summary>
    public MFPSPlayer[] GetMFPSPlayerInTeam(Team team)
    {
        List<MFPSPlayer> list = new List<MFPSPlayer>();
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].Team == team)
            {
                list.Add(OthersActorsInScene[i]);
            }
        }
        if (LocalActor.Team == team) list.Add(LocalActor);
        return list.ToArray();
    }

    public List<MFPSPlayer> GetNonTeamMatePlayers(bool includeBots = true)
    {
        Team playerTeam = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
        List<MFPSPlayer> list = new List<MFPSPlayer>();
        for (int i = 0; i < OthersActorsInScene.Count; i++)
        {
            if (OthersActorsInScene[i].Team != playerTeam)
            {
                if (OthersActorsInScene[i].isRealPlayer) { list.Add(OthersActorsInScene[i]); }
                else if (includeBots) { list.Add(OthersActorsInScene[i]); }
            }
        }
        return list;
    }

    #region PUN
    [PunRPC]
    void RPCSyncGame(MatchState state)
    {
        Debug.Log("Game sync by master, match state: " + state.ToString());
        GameMatchState = state;
        if (!PhotonNetwork.IsMasterClient)
        {
            bl_MatchTimeManager.Instance.Init();
        }
    }

    public void SetGameState(MatchState state)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        PhotonNetwork.RegisterPhotonView(photonView);
       // Debug.Log("Game State Update: " + state.ToString());
        photonView.RPC(nameof(RPCMatchState), RpcTarget.All, state);
    }

    [PunRPC]
    void RPCMatchState(MatchState state)
    {
        GameMatchState = state;
        bl_EventHandler.DispatchMatchStateChange(state);
        CheckPlayersInMatch();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPlayersInMatch()
    {
        //if still waiting
        if (!bl_MatchTimeManager.Instance.Initialized && GameMatchState == MatchState.Waiting)
        {
            bool ready = false;
            if (isOneTeamMode)
            {
                ready = PhotonNetwork.PlayerList.Length >= WaitingPlayersAmount;
            }
            else
            {
                int team1Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Hiding).Length;
                int team2Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Maniac).Length;
                int totalPlayers = PhotonNetwork.PlayerList.Length;

                if (bl_AIMananger.Instance.BotsActive)
                {
                    totalPlayers += bl_AIMananger.Instance.BotsStatistics.Count;
                    team1Count += bl_AIMananger.Instance.GetAllBotsInTeam(Team.Hiding).Count;
                    team2Count += bl_AIMananger.Instance.GetAllBotsInTeam(Team.Maniac).Count;
                }

                //if the minimum amount of players are in the game
                if (totalPlayers >= WaitingPlayersAmount)
                {
                    //and they are split in both teams
                    if ((team1Count > 0 && team2Count > 0) || WaitingPlayersAmount <= 1)
                    {
                        //we are ready to start
                        ready = true;
                    }
                    else
                    {
                        //otherwise wait until player split in both teams
#if LOCALIZATION
                        bl_UIReferences.Instance.SetWaitingPlayersText(bl_Localization.Instance.GetText(128), true);
#else
                        bl_UIReferences.Instance.SetWaitingPlayersText(bl_GameTexts.WaitingTeamBalance, true);
#endif
                        return;
                    }
                }
            }
            if (ready)//all needed players in game
            {
                //master set the call to start the match
                if (PhotonNetwork.IsMasterClient)
                {
                    bl_MatchTimeManager.Instance.InitAfterWait();
                }
                SetGameState(MatchState.Starting);
                bl_MatchTimeManager.Instance.SetTimeState(RoomTimeState.Started, true);
                onAllPlayersRequiredIn?.Invoke();
                bl_UIReferences.Instance.SetWaitingPlayersText("", false);
            }
            else
            {
                bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, 2), true);
            }
        }
        else
        {
            bl_UIReferences.Instance.SetWaitingPlayersText("", false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    //PLAYER EVENTS
    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player connected: " + newPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            //master sync the require match info to be sure all players have the same info at the start
            photonView.RPC("RPCSyncGame", newPlayer, GameMatchState);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player disconnected: " + otherPlayer.NickName);
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
      
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //when a player has join to a team
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            //make sure has join to a team
            if ((string)changedProps[PropertiesKeys.TeamKey] != Team.None.ToString())
            {
                CheckPlayersInMatch();
            }
            else
            {
                if (GameMatchState == MatchState.Waiting)
                {
                    bl_UIReferences.Instance.SetWaitingPlayersText(string.Format(bl_GameTexts.WaitingPlayers, PhotonNetwork.PlayerList.Length, WaitingPlayersAmount), true);
                }
            }
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("The old masterclient left, we have a new masterclient: " + newMasterClient.NickName);
        bl_ChatRoom.Instance?.AddChatLocally($"We have a new MasterClient: {newMasterClient.NickName}");
    }

    public void OnConnected()
    {     
    }

    public void OnConnectedToMaster()
    {    
    }

    public void OnDisconnected(DisconnectCause cause)
    {
#if UNITY_EDITOR
        if (bl_RoomMenu.Instance.isApplicationQuitting) { return; }
#endif
        Debug.Log("Clean up a bit after server quit, cause: " + cause.ToString());
        PhotonNetwork.IsMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel(bl_GameData.Instance.OnDisconnectScene);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {    
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {       
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {     
    }
    #endregion

    #region Getters
    private Camera cameraRender = null;
    public Camera CameraRendered
    {
        get
        {
            if (cameraRender == null)
            {
                // Debug.Log("Not Camera has been setup.");
                return Camera.current;
            }
            return cameraRender;
        }
        set
        {
            if (cameraRender != null && cameraRender.isActiveAndEnabled)
            {
                //if the current render over the set camera, keep it as renderer camera
                if (cameraRender.depth >= value.depth) return;
            }
            cameraRender = value;
        }
    }

    private bool m_enterInGame = false;
    public bool FirstSpawnDone
    {
        get
        {
            return m_enterInGame;
        }
        set
        {
            m_enterInGame = value;
        }
    }

    public float PlayedTime => (Time.time - StartPlayTime);

    public static bool isLocalAlive
    {
        get
        {
            return bl_GameManager.Instance.LocalActor.isAlive;
        }
        set
        {
            bl_GameManager.Instance.LocalActor.isAlive = value;
        }
    }

    public GameObject m_RoomCamera { get { return bl_RoomCamera.Instance.gameObject; } }

    private static bl_GameManager _instance;
    public static bl_GameManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_GameManager>(); }
            return _instance;
        }
    }
    #endregion
}