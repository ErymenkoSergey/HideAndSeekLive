using UnityEngine;
using Photon.Pun;
using System;

[DefaultExecutionOrder(-800)]
public class bl_RoomMenu : bl_MonoBehaviour
{
    [Header("LeftRoom")]
    [Range(0.0f, 5)]
    public float DelayLeave = 1.5f;

    public Action<Team> onWaitUntilRoundFinish; //event called when a player enter but have to wait until current round finish.
    public bool isCursorLocked { get; private set; }
    public bool isPlaying { get; set; }
    public bool isFinish { get; set; } = false;

    private bool _mobileInput = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!isConnected)
            return;

        base.Awake();
        _mobileInput = bl_GameData.Instance.MobileInput;
#if ULSP
        if (bl_DataBase.IsUserLogged) { bl_DataBase.Instance.RecordTime(); }
#endif
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;

#if INPUT_MANAGER
        bl_Input.CheckGamePadRequired();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath += OnPlayerLocalDeath;
#if MFPSM
        bl_TouchHelper.OnPause += TogglePause;
#endif
        bl_PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath -= OnPlayerLocalDeath;
#if MFPSM
        bl_TouchHelper.OnPause -= TogglePause;
#endif
        bl_PhotonCallbacks.LeftRoom -= OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnPlayerSpawn()
    {
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = true;
        isPlaying = true;
    }

    private void OnPlayerLocalDeath()
    {
        bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.enabled = false;
        isPlaying = false;
    }

    public override void OnUpdate()
    {
        isCursorLocked = bl_UtilityHelper.GetCursorState;
        PauseControll();
        ScoreboardInput();
    }

    private void PauseControll()
    {
        if (!_mobileInput)
        {
            if (bl_MobileInput.Pause())
                TogglePause();
        }
        else
        {
            if (bl_MobileInput.GetButtonDown("Pause"))
                TogglePause();
        }
    }

    /// <summary>
    /// Toggle the current paused state
    /// </summary>
    public void TogglePause()
    {
        if (!bl_GameManager.Instance.FirstSpawnDone || isFinish)
            return;

        bool paused = isPaused;
        paused = !paused;
        bl_UIReferences.Instance.ShowMenu(paused);
        bl_UtilityHelper.LockCursor(!paused);
        bl_UCrosshair.Instance.Show(!paused);
        bl_EventHandler.DispatchGamePauseEvent(paused);
    }

    private void ScoreboardInput()
    {
        if (bl_UIReferences.Instance.isOnlyMenuActive || isFinish)
            return;

        if (!_mobileInput)
        {
            if (bl_MobileInput.Scoreboard())
            {
                bool asb = bl_UIReferences.Instance.isScoreboardActive;
                asb = !asb;
                bl_UIReferences.Instance.ShowScoreboard(asb);
            }
            else if (bl_MobileInput.Scoreboard(GameInputType.Up))
            {
                bool asb = bl_UIReferences.Instance.isScoreboardActive;
                asb = !asb;
                bl_UIReferences.Instance.ShowScoreboard(asb);
            }
        }
        else
        {
            if (bl_MobileInput.GetButtonDown("Scoreboard"))
            {
                bool asb = bl_UIReferences.Instance.isScoreboardActive;
                asb = !asb;
                bl_UIReferences.Instance.ShowScoreboard(asb);
            }
            else if (bl_MobileInput.GetButtonDown("Scoreboard", GameInputType.Up))
            {
                bool asb = bl_UIReferences.Instance.isScoreboardActive;
                asb = !asb;
                bl_UIReferences.Instance.ShowScoreboard(asb);
            }
        }
    }

    public void OnAutoTeam()
    {
        bl_UtilityHelper.LockCursor(true);
        isPlaying = true;
    }

    public void JoinTeam(int id)
    {
        Team team = (Team)id;
        string tn = team.GetTeamName();
        string joinText = isOneTeamMode ? bl_GameTexts.JoinedInMatch : bl_GameTexts.JoinIn;

#if LOCALIZATION
        joinText = isOneTeamMode ? bl_Localization.Instance.GetText(17) : bl_Localization.Instance.GetText(23);
#endif
        if (isOneTeamMode)
        {
            bl_KillFeed.Instance.SendMessageEvent(string.Format("{0} {1}", PhotonNetwork.NickName, joinText));
        }
        else
        {
            string jt = string.Format("{0} {1}", joinText, tn);
            bl_KillFeed.Instance.SendTeamHighlightMessage(PhotonNetwork.NickName, jt, team);
        }
#if !PSELECTOR
        bl_UtilityHelper.LockCursor(true);
#else
        if (!bl_PlayerSelector.InMatch)
        {
            bl_UtilityHelper.LockCursor(true);
        }
#endif
        //if player only spawn when a new round start
        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && bl_GameManager.Instance.GameMatchState == MatchState.Playing)
        {
            //subscribe to the start round event
            if (onWaitUntilRoundFinish != null) { onWaitUntilRoundFinish.Invoke(team); }
            bl_GameManager.Instance.SetLocalPlayerToTeam(team);//set the player to the selected team but not spawn yet.
            return;
        }
        //set the player to the selected team and spawn the player
        bl_GameManager.Instance.SpawnPlayer(team);
    }

    /// <summary>
    /// Leave the current room (if exist) and return to the lobby
    /// </summary>
    public void LeftOfRoom()
    {
#if ULSP
        if (bl_DataBase.IsUserLogged)
        {
            Player p = PhotonNetwork.LocalPlayer;
            bl_ULoginMFPS.SaveLocalPlayerKDS();
            bl_DataBase.Instance.StopAndSaveTime();
        }
#endif
        //Good place to save info before reset statistics
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
#if UNITY_EDITOR
            if (isApplicationQuitting) { return; }
#endif
            bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
        }
    }

    public bool isMenuOpen => bl_UIReferences.Instance.State != bl_UIReferences.RoomMenuState.Hidde;
    public bool isPaused { get { return bl_UIReferences.Instance.isMenuActive; } }

    /// <summary>
    /// Called from the server when the left room request was retrieved.
    /// </summary>
    public void OnLeftRoom()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        bl_MatchTimeManager.Instance.enabled = false;
        if (bl_UIReferences.Instance != null)
            StartCoroutine(bl_UIReferences.Instance.FinalFade(true));
    }

    public bool isApplicationQuitting { get; set; } = false;
    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private static bl_RoomMenu _instance;
    public static bl_RoomMenu Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomMenu>(); }
            return _instance;
        }
    }
}