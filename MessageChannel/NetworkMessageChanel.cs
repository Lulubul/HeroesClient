using System.Collections.Generic;
using GameActors;
using Assets.Scripts.Networking;
using NetworkTypes;
using Assets.Scripts.Controller;
using Assets.Scripts.Model;

namespace Events
{
    public class NetworkMessageChannel : MessageChannel
    {
        public LobbyPlayer Player;

        public NetworkMessageChannel(LobbyPlayer player)
        {
            Player = player;
        }

        public override void MovePiece(Point start, Point destination)
        {
            TileBehaviour.OnMove = true;
            var parameters = new List<SerializableType> {start, destination};
            var remoteMethod = new RemoteInvokeMethod("BoardBehavior", Command.Move, parameters);
            BeginSendPackage(remoteMethod);
        }

        public override void FinishAction()
        {
            TileBehaviour.OnMove = false;
            var remoteMethod = new RemoteInvokeMethod("BoardBehavior", Command.FinishAction);
            BeginSendPackage(remoteMethod);
        }

        public override void DieCreature(CreatureComponent creature)
        {
            var parameters = new List<SerializableType> { creature.Piece.Location };
            var remoteMethod = new RemoteInvokeMethod("BoardBehavior", Command.Die, parameters);
            BeginSendPackage(remoteMethod);
        }

        public override void DefenseCreature(CreatureComponent creature)
        {
            var parameters = new List<SerializableType> { creature.Piece.Location };
            var remoteMethod = new RemoteInvokeMethod("BoardBehavior", Command.Defend, parameters);
            BeginSendPackage(remoteMethod);
        }
        
        public override void AttackCreature(int targetIndex, int creatureIndex)
        {
            var args = new List<SerializableType>
            {
                new AttackModel()
                {
                    TargetCreatureIndex = targetIndex,
                    SenderCreatureIndex = creatureIndex,
                    Damage = 0
                }
            };
            var remoteMethod = new RemoteInvokeMethod("BoardBehavior", Command.Attack, args);
            BeginSendPackage(remoteMethod);
        }

        private static void BeginSendPackage(RemoteInvokeMethod remoteMethod)
        {
            var bytes = RemoteInvokeMethod.WriteToStream(remoteMethod);
            Network.Instance.Client.Send(bytes, bytes.Length, 0);
        }

    }
}
