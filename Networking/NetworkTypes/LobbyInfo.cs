namespace NetworkTypes
{
    public class LobbyInfo : SerializableType
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string GameType { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        
        public LobbyInfo()
        {
            PlayerId = 0;
            Name = "";
            GameType = "aurica";
            MaxPlayers = 6;
            CurrentPlayers = 1;
        }

        public LobbyInfo( int playerId, string name, string gameType, int maxPlayers, int currentPlayers )
        {
            PlayerId = playerId;
            Name = name;
            GameType = gameType;
            MaxPlayers = maxPlayers;
            CurrentPlayers = currentPlayers;
        }
    }
}
