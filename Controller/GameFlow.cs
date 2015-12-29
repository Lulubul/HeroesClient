using Events;

namespace Assets.Scripts.Controller
{

    public enum GameType {Fast, Classic}
    public enum MessageType {Network, Local}
    public sealed class GameFlow
    {
        private static readonly GameFlow instance = new GameFlow();
        public static GameFlow Instance { get { return instance; } }
        public GameType GameType;
        public MessageType MessageType;
        public MessageChannel Channel;
        public string RoomName;
        public bool IsGameCreator;

        static GameFlow() {}
        private GameFlow() {}
    }
}
