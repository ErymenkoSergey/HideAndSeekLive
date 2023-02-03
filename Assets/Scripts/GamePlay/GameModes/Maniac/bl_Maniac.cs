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

    public Team GetWinnerTeam()
    {
        Debug.Log($"BL_Maniac: GetWinnerTeam ");
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

    [SerializeField] private List<Player> _teamOne = new List<Player>(); //Hidings
    [SerializeField] private List<Player> _teamTwo = new List<Player>(); // Maniac

    private int SetDeathsTeamOne;
    private int SetDeathsTeamTwo;
    private int countDeathsTeam1 = 0;
    private int countDeathsTeam2 = 0;

    private bool _isLoopCheckWinner = true;
    private float _loopTime = 1.0f;

    private async void DeleyCheckWinner()
    {
        await Task.Delay(12000);
        StartCoroutine(CheckPlayerCounts());
    }

    private IEnumerator CheckPlayerCounts() // чекать нужно кол-во живых игроков...
    {
        var teeamms = PhotonNetwork.PlayerList; // общее кол-во игроков

        for (int i = 0; i < teeamms.Length; i++) // разделили на комманды 
        {
            if (teeamms[i].GetPlayerTeam() == Team.Team1)
            {
                _teamOne.Add(teeamms[i]);
            }

            if (teeamms[i].GetPlayerTeam() == Team.Team2)
            {
                _teamTwo.Add(teeamms[i]);
            }
        }

        SetDeathsTeamOne = _teamOne.Count; // записали кол-во игроков в каждой команде
        SetDeathsTeamTwo = _teamTwo.Count;

        while (_isLoopCheckWinner) // теперь проверяем 
        {
            foreach (var player in _teamOne)
            {
                countDeathsTeam1 += player.GetDeaths();
                Debug.Log($"player.NickName {player.NickName} / {player.GetDeaths()} / count1: {countDeathsTeam1}");
            }

            foreach (var player in _teamTwo)
            {
                countDeathsTeam2 += player.GetDeaths();
                Debug.Log($"player.NickName {player.NickName} / {player.GetDeaths()} / count2: {countDeathsTeam2}");
            }


            ////Debug.Log($"CheckPlayerCounts {_isLoopCheckWinner}");
            //ClearTeams();
            //var countTeam = bl_GameManager.Instance.OthersActorsInScene;




            ////Debug.Log($"CheckPlayerCounts All countTeam {countTeam.Count}");


            //for (int i = 0; i < countTeam.Count; i++)
            //{
            //    if (countTeam[i].Team == Team.Team1 && countTeam[i].isAlive == true)
            //    {
            //        _teamOne.Add(countTeam[i]);
            //    }
            //    if (countTeam[i].Team == Team.Team2 && countTeam[i].isAlive == true)
            //    {
            //        _teamTwo.Add(countTeam[i]);
            //    }
            //}

            //Debug.Log($"CheckPlayerCounts All _teamOne.Count {_teamOne.Count }/ _teamTwo.Count {_teamTwo.Count} ");
            Team winner = Team.None;
            Debug.Log($"CheckPlayerCounts First {SetDeathsTeamOne} == {countDeathsTeam1} ");
            Debug.Log($"CheckPlayerCounts Seconds {SetDeathsTeamTwo} == {countDeathsTeam2} ");
            if (SetDeathsTeamOne == countDeathsTeam1)
            {
                winner = Team.Team2;
                SetUIWinner(winner);
            }
            if (SetDeathsTeamTwo == countDeathsTeam2)
            {
                winner = Team.Team1;
                SetUIWinner(winner);
            }

            yield return new WaitForSeconds(_loopTime);
           // ClearTeams();
            Debug.Log($"CheckPlayerCounts Stop Corut !!! ");
        }
        
    }

    private void ClearTeams()
    {
        _teamOne.Clear(); 
        _teamTwo.Clear();
    }


    private void SetUIWinner(Team winner)
    {
        _isLoopCheckWinner = false;
        Debug.LogError($"GetWinnerTeam  WWWinnner dsasaddsadas  : {winner}");
        GameOver(winner);
    }


    public void Initialize()
    {
        Debug.Log($"BL_Maniac: Initialize ");
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
        Debug.Log($"BL_Maniac: OnFinishTime {gameOver}");
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
        Debug.Log($"BL_Maniac: OnLocalPlayerDeath ");
        //? check count Players two Teams..
    }

    public void OnLocalPlayerKill()
    {
        Debug.Log($"BL_Maniac: OnLocalPlayerKill ");
        PhotonNetwork.CurrentRoom.SetTeamScore(PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
        Debug.Log($"BL_Maniac: OnLocalPoint {points} / {teamToAddPoint} ");
        PhotonNetwork.CurrentRoom.SetTeamScore(teamToAddPoint);
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
        Debug.Log($"BL_Maniac: OnOtherPlayerEnter {newPlayer}");
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {
        Debug.Log($"BL_Maniac: OnOtherPlayerLeave {otherPlayer}");
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score) || propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            Debug.Log($"BL_Maniac: OnRoomPropertiesUpdate ");
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
        Debug.Log($"BL_Maniac: CheckScores {team1} / {team2} ");

        //check if any of the team reach the max kills
        if (team1 >= bl_RoomSettings.Instance.GameGoal)
        {
            //GameOver();
            return;
        }
        if (team2 >= bl_RoomSettings.Instance.GameGoal)
        {
            //GameOver();
        }
    }


    private void GameOver(Team winner)
    {
        string finalText = $"{winner}";
        //finalText = GetWinnerTeam().GetTeamName();
        bl_UIReferences.Instance.SetFinalText(finalText);

        bl_MatchTimeManager.Instance.FinishRound();
    }
}
