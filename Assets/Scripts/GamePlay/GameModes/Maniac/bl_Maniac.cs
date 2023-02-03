using Photon.Pun;
using Photon.Realtime;
using MFPS.GameModes.TeamDeathMatch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class bl_Maniac : bl_PhotonHelper, IGameMode
{
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        Initialize();
    }

    public bool isLocalPlayerWinner
    {
        get
        {
            return GetWinnerTeam() == PhotonNetwork.LocalPlayer.GetPlayerTeam();
        }
    }

    public Team GetWinnerTeam() //?
    {
        //var countTeam1 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1);
        //var countTeam2 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1);
        //Debug.Log($"GetWinnerTeam {countTeam1.Length } / {countTeam2.Length}");
        //Team winner = Team.None;

        //if (countTeam1.Length == 0)
        //    winner = Team.Team2;
        //else if (countTeam2.Length == 0)
        //    winner = Team.Team1;
        //else
        //    winner = Team.None;
        //return winner;

        int team1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        Team winner = Team.None;
        if (team1 > team2)
        { winner = Team.Team1; }
        else if (team1 < team2)
        { winner = Team.Team2; }
        else
        { winner = Team.None; }
        return winner;
    }

    private List<MFPSPlayer> _teamOne = new List<MFPSPlayer>();
    private List<MFPSPlayer> _teamTwo = new List<MFPSPlayer>();
    private bool _isLoopCheckWinner = true;
    private float _loopTime = 1.0f;

    private async void DeleyCheckWinner()
    {
        await Task.Delay(10000);
        StartCoroutine(CheckPlayerCounts());
    }

    private IEnumerator CheckPlayerCounts() // чекать нужно кол-во живых игроков...
    {
        while (_isLoopCheckWinner)
        {
            Debug.Log($"CheckPlayerCounts {_isLoopCheckWinner}");
            ClearTeams();
            var countTeam = bl_GameManager.Instance.OthersActorsInScene;
            Debug.Log($"CheckPlayerCounts All countTeam {countTeam.Count}");
            Team winner = Team.None;

            for (int i = 0; i < countTeam.Count; i++)
            {
                if (countTeam[i].Team == Team.Team1 && countTeam[i].isAlive == true)
                {
                    _teamOne.Add(countTeam[i]);
                }
                if (countTeam[i].Team == Team.Team2 && countTeam[i].isAlive == true)
                {
                    _teamTwo.Add(countTeam[i]);
                }
            }

            Debug.Log($"CheckPlayerCounts All _teamOne.Count {_teamOne.Count }/ _teamTwo.Count {_teamTwo.Count} ");

            if (_teamOne.Count == 0)
            {
                winner = Team.Team2;
                SetUIWinner(winner);
            }
            else if (_teamTwo.Count == 0)
            {
                winner = Team.Team1;
                SetUIWinner(winner);
            }

            yield return new WaitForSeconds(_loopTime);

            Debug.Log($"CheckPlayerCounts Stop Corut !!! ");
        }
        

        //var countTeam2 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2);
        //Debug.Log($"GetWinnerTeam {countTeam1.Length } / {countTeam2.Length}");


        //Team winner = Team.None;

        //if (countTeam1.Length == 0)
        //{
        //    winner = Team.Team2;
        //    SetUIWinner(winner);
        //}
        //else if (countTeam2.Length == 0)
        //{
        //    winner = Team.Team1;
        //    SetUIWinner(winner);
        //}
        //else
        //{
        //    winner = Team.None;
        //}

        //return winner;
    }

    private void ClearTeams()
    {
        _teamOne.Clear(); 
        _teamTwo.Clear();
    }

    //private void CheckPlayerCounts() // чекать нужно кол-во живых игроков...
    //{
    //    var countTeam1 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1);
    //    var countTeam2 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2);
    //    Debug.Log($"GetWinnerTeam {countTeam1.Length } / {countTeam2.Length}");


    //    Team winner = Team.None;

    //    if (countTeam1.Length == 0)
    //    {
    //        winner = Team.Team2;
    //        SetUIWinner(winner);
    //    }
    //    else if (countTeam2.Length == 0)
    //    {
    //        winner = Team.Team1;
    //        SetUIWinner(winner);
    //    }
    //    else
    //    {
    //        winner = Team.None;
    //    }

    //    //return winner;
    //}

    private void SetUIWinner(Team winner)
    {
        _isLoopCheckWinner = false;
        Debug.LogError($"GetWinnerTeam  WWWinnner dsasaddsadas  : {winner}");
    }


    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.TDM, this))
        {
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
            bl_TeamDeathMatchUI.Instance.ShowUp();
        }
        else
        {
            bl_TeamDeathMatchUI.Instance.Hide();
        }

        DeleyCheckWinner();
    }

    public void OnFinishTime(bool gameOver)
    {
        //determine the winner
        string finalText = "";
        if (!PhotonNetwork.OfflineMode && GetWinnerTeam() != Team.None)
        {
            finalText = GetWinnerTeam().GetTeamName();
        }
        else
        {
            finalText = bl_GameTexts.NoOneWonName;
        }
        bl_UIReferences.Instance.SetFinalText(finalText);
    }

    public void OnLocalPlayerDeath()
    {
        //? check count Players two Teams..
    }

    public void OnLocalPlayerKill()
    {
        PhotonNetwork.CurrentRoom.SetTeamScore(PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
        PhotonNetwork.CurrentRoom.SetTeamScore(teamToAddPoint);
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score) || propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            int team1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
            bl_TeamDeathMatchUI.Instance.SetScores(team1, team2);
            CheckScores(team1, team2);
        }


    }

    private void CheckScores(int team1, int team2)
    {
        if (PhotonNetwork.OfflineMode || !bl_RoomSettings.Instance.RoomInfoFetched)
            return;

        //check if any of the team reach the max kills
        if (team1 >= bl_RoomSettings.Instance.GameGoal)
        {
            bl_MatchTimeManager.Instance.FinishRound();
            return;
        }
        if (team2 >= bl_RoomSettings.Instance.GameGoal)
        {
            bl_MatchTimeManager.Instance.FinishRound();
        }
    }
}
