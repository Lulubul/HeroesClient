using NetworkTypes;

namespace Assets.Scripts.Model
{
    public class LobbyPlayer
    {
        public int Id {get; set;}
        public string Name { get; set; }
        public string LobbyName { get; set; }
        public Team Team { get; set; }

        public LobbyPlayer() {}

        public LobbyPlayer(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
