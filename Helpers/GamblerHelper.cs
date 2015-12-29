using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Networking;
using UnityEngine;
using UnityEngine.UI;
using NetworkTypes;

public delegate void DelegateAction(string message);

public class GamblerHelper : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject LobbyPanel;
    public GameObject RoomPanel;
    public GameObject CreatePanel;
    public InputField Name;
    public InputField Pass;
    public Text RoomName;
    public Text PlayerName;
    public Text RoomNameDisplay;
    public List<Text> Players;
    public GameObject WarningMessage;
    public GameObject Canvas;
    public GameObject LobbyRooms;
    public GameObject LobbyRoomPrefab;
    public GameObject UnitsSelectors;
    public GameObject CreatureSelectors;
    public GameObject HeroSelectors;

    public GameObject HeroPanel;
    public GameObject UnitPrefab;
    public GameObject CreaturePanel;

    public List<Image> UnitButtonSelectors = new List<Image>(); 

    public Queue<Action> ExecuteOnMainThread;
    public DelegateAction action;
    public DelegateAction SelectCreature;
    public DelegateAction SelectHero;

    public List<SerializableType> Creatures;
    public List<SerializableType> Heroes;

    public NamedImage[] HeroImages;
    public NamedImage[] CreatureImages; 

    private readonly Dictionary<string, Transform> _lobbies = new Dictionary<string, Transform>();

    public void Start()
    {
        ExecuteOnMainThread = new Queue<Action>();
    }

    public void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    public void HideLogin(List<SerializableType> lobbies, string playerName)
    {
        SetLabelsInLobbyPanel(playerName);
        FillLobby(lobbies);
    }

    public void HideLogin(string playerName)
    {
        SetLabelsInLobbyPanel(playerName);
    }

    private void SetLabelsInLobbyPanel(string playerName)
    {
        LoginPanel.SetActive(false);
        RoomPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        PlayerName.text = playerName;
    }

    public void FillLobby(List<SerializableType> lobbies)
    {
        foreach (var lobby in lobbies)
        {
            AddLobby(lobby);
        }
    }

    public void FillCreatures(List<SerializableType> creatures)
    {
        ExecuteOnMainThread.Enqueue(() =>
        {
            Creatures = creatures;
            foreach (var serializableType in Creatures)
            {
                var creature = serializableType as CreatureInfo;
                var unitUi = Instantiate(UnitPrefab);
                var button = unitUi.transform.GetChild(0);
                button.GetComponent<Image>().sprite =
                    CreatureImages.SingleOrDefault(x => x.Name == creature.Name).Image;
                button.GetComponent<Button>().onClick.AddListener(delegate { SelectCreature(creature.Name); });
                unitUi.transform.GetChild(1).GetComponent<Text>().text = GetCreatureInfo(creature);
                unitUi.transform.SetParent(CreaturePanel.transform);
            }
        });
    }

    public void FillHeroes(List<SerializableType> heroes)
    {
        ExecuteOnMainThread.Enqueue(() =>
        {
            Heroes = heroes;
            foreach (var serializableType in Heroes)
            {
                var hero = serializableType as HeroInfo;
                var unitUi = Instantiate(UnitPrefab);
                var button = unitUi.transform.GetChild(0);
                button.GetComponent<Image>().sprite =
                    HeroImages.SingleOrDefault(x => x.Name == hero.Name).Image;
                button.GetComponent<Button>().onClick.AddListener(delegate { SelectHero(hero.Name); });
                unitUi.transform.GetChild(1).GetComponent<Text>().text = GetHeroInfo(hero);
                unitUi.transform.SetParent(HeroPanel.transform);
            }
        });
    }

    private string GetHeroInfo(HeroInfo hero)
    {
        var info =
                "Name: " + hero.Name + Environment.NewLine
                + "Race: " + hero.RaceEnum + Environment.NewLine
                + "Type: " + hero.Type;
        return info;
    }

    private string GetCreatureInfo(CreatureInfo creature)
    {
        return "Name: "+ creature.Name + Environment.NewLine
               + "Damage: " + creature.Damage + Environment.NewLine
               + "Health: "  + creature.MaxHealth + Environment.NewLine
               + "Speed: " + creature.Speed + Environment.NewLine
               + "Type: " + creature.Type;
    }

    public void AddLobby(SerializableType lobby)
    {
        var lobbyInfo = lobby as LobbyInfo;
        var lobbyPanel = Instantiate(LobbyRoomPrefab);
        lobbyPanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(delegate { action(lobbyInfo.Name); });
        lobbyPanel.transform.GetChild(0).GetComponent<Text>().text = lobbyInfo.Name;
        lobbyPanel.transform.GetChild(1).GetComponent<Text>().text = lobbyInfo.CurrentPlayers + " / " + lobbyInfo.MaxPlayers;
        lobbyPanel.transform.SetParent(LobbyRooms.transform);
        _lobbies.Add(lobbyInfo.Name, lobbyPanel.transform);
    }

    public void UpdateLobby(LobbyInfo lobbyInfo)
    {
        _lobbies[lobbyInfo.Name].GetChild(1).GetComponent<Text>().text = lobbyInfo.CurrentPlayers + " / " + lobbyInfo.MaxPlayers;
    }

    public void ShowError()
    {
        WarningMessage.SetActive(true);
    }

    public void ShowLogin()
    {
        LoginPanel.SetActive(true);
        LobbyPanel.SetActive(false);
    }

    public void ShowRoom(string roomName, List<SerializableType> players)
    {
        RoomPanel.SetActive(true);
        LobbyPanel.SetActive(false);
        CreatePanel.SetActive(false);
        RoomNameDisplay.text = roomName;
        foreach (Gambler player in players)
        {
            AddPlayer(player);
        }
    }

    public void RefillRoom(List<SerializableType> players)
    {
        foreach (var slot in Players)
        {
            slot.text = "Open";
        }
        foreach (Gambler player in players)
        {
            AddPlayer(player);
        }
    }

    public void AddPlayer(Gambler player)
    {
        Players[player.Slot].text = player.Name;
    }

    public void ShowCreateRoom()
    {
        CreatePanel.SetActive(true);
    }

    public void ChangeTeam(int slot)
    {
        Players[slot].text = "Open";
    }

    public void StartGame()
    {
        Application.LoadLevel("First");
    }
}

