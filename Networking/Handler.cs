using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Controller;
using Assets.Scripts.Model;
using UnityEngine;
using NetworkTypes;

namespace Assets.Scripts.Networking
{
    public class Handler : NetworkActions
    {
        private readonly Type _type;
        private readonly GamblerHelper _helper;
        private Gambler _gambler;

        public Handler(GamblerHelper helper)
        {
            _type = GetType();
            _helper = helper;
        }
        public void Execute(string command, string serviceClassName, List<SerializableType> parameters)
        {
            object[] fields = { parameters };
            var theMethod = _type.GetMethod(command);
            theMethod.Invoke(this, fields);
        }

        #region Pregame 
        public void Connect(List<SerializableType> returnList)
        {
            var message = returnList[0] as SimpleMessage;
            Debug.Log(message.Message);
        }

        public void Login(List<SerializableType> returnList)
        {
            var user = returnList[0] as Gambler;
            var response = (Response)Enum.Parse(typeof(Response), user.Response);
            if (response == Response.Succed)
            {
                Network.Instance.ClientPlayer = new LobbyPlayer(user.Id, user.Name);
                returnList.RemoveAt(0);
                if (returnList.Count > 0)
                {
                    _helper.ExecuteOnMainThread.Enqueue(() =>
                    {
                        _helper.HideLogin(returnList, user.Name);
                    });
                    return;
                }
                _helper.ExecuteOnMainThread.Enqueue(() =>
                {
                    _helper.HideLogin(user.Name);
                });
                return;
            }
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.ShowError();
            });
        }

        public void Register(List<SerializableType> returnList)
        {
            var user = returnList[0] as Gambler;
            var response = (Response)Enum.Parse(typeof(Response), user.Response);
            if (response == Response.Succed)
            {
                var id = user.Id;
                var name = user.Name;
                Network.Instance.ClientPlayer = new LobbyPlayer(id, name);
                returnList.RemoveAt(0);
                if (returnList.Count > 0)
                {
                    _helper.ExecuteOnMainThread.Enqueue(() =>
                    {
                        _helper.HideLogin(returnList, name);
                    });
                    return;
                }
                _helper.ExecuteOnMainThread.Enqueue(() =>
                {
                    _helper.HideLogin(name);
                });
            }
        }

        public void Logout(List<SerializableType> returnList)
        {
            var message = returnList[0] as ResponseMessage;
            var response = (Response)Enum.Parse(typeof(Response), message.Response);
            if (response == Response.Succed)
            {
                _helper.ExecuteOnMainThread.Enqueue(() =>
                {
                    _helper.ShowLogin();
                });
            }
        }

        public void SyncRooms(List<SerializableType> returnList)
        {
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.FillLobby(returnList);
            });
        }

        public void SyncLobby(List<SerializableType> returnList)
        {
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                if (returnList.Count == 1)
                {
                    _helper.AddPlayer(returnList[0] as Gambler);
                    return;
                }
                _helper.RefillRoom(returnList);
            });
        }

        public void Create(List<SerializableType> returnList)
        {
            var message = returnList[0] as ResponseMessage;
            var response = (Response)Enum.Parse(typeof(Response), message.Response);

            if (response != Response.Succed) return;
            var roomName = message.Message;
            var playerName = Network.Instance.ClientPlayer.Name;
            Network.Instance.ClientPlayer.LobbyName = roomName;
            Network.Instance.ClientPlayer.Team = Team.Red;
            GameFlow.Instance.IsGameCreator = true;
            var users = new List<SerializableType>();
            var user = new Gambler()
            {
                Name = playerName,
                Slot = 0,
                Id = Network.Instance.ClientPlayer.Id
            };
            users.Add(user);

            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.ShowRoom(roomName, users);
            });
        }

        public void Join(List<SerializableType> returnList)
        {
            GameFlow.Instance.IsGameCreator = false;
            var lobby = returnList[0] as LobbyInfo;
            Network.Instance.ClientPlayer.LobbyName = lobby.Name;
            returnList.Remove(lobby);
            Network.Instance.ClientPlayer.Team = (returnList[returnList.Count - 1] as Gambler).Slot % 2 == 0 ? Team.Red : Team.Blue;
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.ShowRoom(lobby.Name, returnList);
            });
        }

        public void ChangeTeam(List<SerializableType> playerList)
        {
            Network.Instance.ClientPlayer.Team = Network.Instance.ClientPlayer.Team == Team.Blue ? Team.Red : Team.Blue;
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.RefillRoom(playerList);
            });
        }

        public void Start(List<SerializableType> playerList)
        {
            GameFlow.Instance.MessageType = MessageType.Network;
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.StartGame();
            });
        }

        public void Leave(List<SerializableType> playerList)
        {
            GameFlow.Instance.IsGameCreator = false;
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.HideLogin("Aurel");
            });
        }

        public void UpdateLobby(List<SerializableType> playerList)
        {
            _helper.ExecuteOnMainThread.Enqueue(() =>
            {
                _helper.UpdateLobby(playerList[0] as LobbyInfo);
            });
        }

        public void SendUnits(List<SerializableType> unitsInfo)
        {
            var heroesInfo = unitsInfo.Where(x => x.GetType() == typeof(HeroInfo)).ToList();
            var creaturesInfo = unitsInfo.Where(x => x.GetType() == typeof(CreatureInfo)).ToList();
            _helper.FillHeroes(heroesInfo);
            _helper.FillCreatures(creaturesInfo);
        }
        

        #endregion

        #region FastBattle
        public void SyncHero(List<SerializableType> heroes)
        {
            var board = heroes[0] as BoardInfo;
            var hero1 = heroes[1] as AbstractHero;
            var hero2 = heroes[2] as AbstractHero;
            if (GameFlow.Instance.Channel.SyncHeroesCallback != null)
            {
                GameFlow.Instance.Channel.SyncHeroesCallback(board, hero1, hero2);
            }
        }
        
        public void Move(List<SerializableType> pointList)
        {
            if (GameFlow.Instance.Channel.MoveCallback == null) return;
            var points = pointList.Cast<Point>().Reverse().ToList();
            GameFlow.Instance.Channel.MoveCallback(points);
        }

        public void GameIsReady(List<SerializableType> useless)
        {
            GameFlow.Instance.Channel.GameIsReadyCallback();
        }

        public void FinishAction(List<SerializableType> turns)
        {
            GameFlow.Instance.Channel.TurnCallback(turns[0] as NextTurn);
        }

        public void Attack(List<SerializableType> attackModel)
        {
            GameFlow.Instance.Channel.Attack(attackModel[0] as AttackModel);
        }

        public void Die(List<SerializableType> point)
        {
            GameFlow.Instance.Channel.DieCallback(point[0] as Point);
        }

        public void EndGame(List<SerializableType> turn)
        {
            GameFlow.Instance.Channel.FinishGame(turn[0] as NextTurn);
        }
        #endregion
    }
}
