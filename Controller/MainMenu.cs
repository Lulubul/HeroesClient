﻿using UnityEngine;

namespace Assets.Scripts.Controller
{
    public class MainMenu : MonoBehaviour {

        private GameFlow _gameFlow;

        public void Start()
        {
            _gameFlow = GameFlow.Instance;
        }

        public void Multiplayer()
        {
            _gameFlow.MessageType = MessageType.Network;
            Application.LoadLevel("Client");
        }

        public void Exit()
        {
            Application.Quit();
        }


    }
}
