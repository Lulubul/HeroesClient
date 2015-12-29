using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controller;
using UnityEngine;
using NetworkTypes;

namespace Assets.Scripts.Networking
{

    public class UnitsSelected
    {
        public string HeroName;
        public Dictionary<int, string> CreaturesName = new Dictionary<int, string>();
    }

    [Serializable]
    public struct NamedImage
    {
        public string Name;
        public Sprite Image;
    }

    public class Client : MonoBehaviour
    {
        public GamblerHelper Helper;
        private Handler _handler;

        private UnitsSelected _unitsSelected;
        private int lastOpenedCreaturePanelIndex = -1;

        public void Start()
        {
            Helper = GetComponent<GamblerHelper>();
            Helper.action += Join;
            Helper.SelectCreature += SelectCreature;
            Helper.SelectHero += SelectHero;

            _handler = new Handler(Helper);
            Network.Instance.Handler = _handler;
            _unitsSelected = new UnitsSelected();

        }

        #region ButtonActions
        public void Login()
        {
            var parameters = new List<SerializableType>();
            var authetification = new Authentication
            {
                Name = Helper.Name.text,
                Pass = Helper.Pass.text
            };
            parameters.Add(authetification);
            var remoteMethod = new RemoteInvokeMethod(Command.Login, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void Register()
        {
            var parameters = new List<SerializableType>();
            var authetification = new Authentication
            {
                Name = Helper.Name.text,
                Pass = Helper.Pass.text
            };
            parameters.Add(authetification);
            var remoteMethod = new RemoteInvokeMethod(Command.Register, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void Logout()
        {
            var parameters = new List<SerializableType>();
            var authetification = new Gambler()
            {
                Name = Network.Instance.ClientPlayer.Name,
                Id = Network.Instance.ClientPlayer.Id
            };
            parameters.Add(authetification);
            var remoteMethod = new RemoteInvokeMethod(Command.Logout, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void Create()
        {
            var parameters = new List<SerializableType>();
            GameFlow.Instance.RoomName = Helper.RoomName.text;
            var room = new LobbyInfo( Network.Instance.ClientPlayer.Id, Helper.RoomName.text, "Fast", 2, 1);
            parameters.Add(room);
            var remoteMethod = new RemoteInvokeMethod(Command.Create, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void Join(string roomName)
        {
            var parameters = new List<SerializableType>();
            var room = new LobbyInfo(Network.Instance.ClientPlayer.Id, roomName, "aurica", 6, 1);
            GameFlow.Instance.RoomName = roomName;
            parameters.Add(room);
            var remoteMethod = new RemoteInvokeMethod(Command.Join, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void Leave()
        {
            var parameters = new List<SerializableType>();
            var room = new LobbyInfo(Network.Instance.ClientPlayer.Id, Helper.RoomNameDisplay.text, "aurica", 6, 1);
            parameters.Add(room);
            var remoteMethod = new RemoteInvokeMethod(Command.Leave, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void ChangeTeam()
        {
            var parameters = new List<SerializableType>();
            var room = new LobbyInfo(Network.Instance.ClientPlayer.Id, Helper.RoomNameDisplay.text, "aurica", 6, 1);
            parameters.Add(room);
            var remoteMethod = new RemoteInvokeMethod(Command.ChangeTeam, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void StartGame()
        {
            var parameters = new List<SerializableType>();
            GameFlow.Instance.RoomName = Helper.RoomNameDisplay.text;
            var room = new LobbyInfo(Network.Instance.ClientPlayer.Id, Helper.RoomNameDisplay.text, GameType.Fast.ToString(), 2, 2);
            parameters.Add(room);
            var remoteMethod = new RemoteInvokeMethod(Command.Start, parameters);
            BeginSendPackage(remoteMethod);
        }

        public void OpenUnitsChooser()
        {
            Helper.UnitsSelectors.SetActive(true);
        }

        public void OpenCreatureChooser(int index)
        {
            lastOpenedCreaturePanelIndex = index;
            Helper.CreatureSelectors.SetActive(true);
        }

        public void OpenHeroChooser()
        {
            Helper.HeroSelectors.SetActive(true);
        }

        public void SelectHero(string name)
        {
            _unitsSelected.HeroName = name;
            Helper.UnitButtonSelectors[0].sprite = Helper.HeroImages.SingleOrDefault(x=> x.Name == name).Image;
            Helper.HeroSelectors.SetActive(false);
        }

        public void SelectCreature(string name)
        {
            _unitsSelected.CreaturesName[lastOpenedCreaturePanelIndex] = name;
            Helper.UnitButtonSelectors[lastOpenedCreaturePanelIndex].sprite = Helper.CreatureImages.SingleOrDefault(x => x.Name == name).Image;
            Helper.CreatureSelectors.SetActive(false);
        }

        public void SelectUnits()
        {
            var parameters = new List<SerializableType>()
            {
                new Units()
                {
                    Creature1 = _unitsSelected.CreaturesName[1],
                    Creature2 = _unitsSelected.CreaturesName[2],
                    Creature3 = _unitsSelected.CreaturesName[3],
                    Creature4 = _unitsSelected.CreaturesName[4],
                    HeroName = _unitsSelected.HeroName,
                    Team = Network.Instance.ClientPlayer.Team
                }
            };
            var remoteMethod = new RemoteInvokeMethod("BoardBehavior", Command.SelectUnits, parameters);
            BeginSendPackage(remoteMethod);
        }

        #endregion

        public static void BeginSendPackage(RemoteInvokeMethod remoteMethod)
        {
            var bytes = RemoteInvokeMethod.WriteToStream(remoteMethod);
            Network.Instance.Client.Send(bytes, bytes.Length, 0);
        }

        private bool CheckConnection()
        {
            var isConnectedToInternet = Application.internetReachability != NetworkReachability.NotReachable;
            return isConnectedToInternet;
        }
    }
}
