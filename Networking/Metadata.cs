namespace Assets.Scripts.Networking
{
    public enum Command
    {
        Connect,
        Login,
        Logout,
        Register,
        Create,
        Leave,
        Join,
        ChangeTeam,
        Lobby,
        Search,
        Start,
        Disconnect,
        Shoot,
        Die,
        Move,
        SyncRooms,
        SyncLobby,
        UpdateLobby,
        FinishAction,
        Turn,
        SyncHero,
        InitializeBoard,
        Attack,
        Defend,
        GameIsReady,
        SelectUnits,
        SendUnits,
        EndGame,
        GetLobbies
    };

    public enum Response
    {
        Succed,
        Fail
    }

    public enum GameType
    {
        Classic,
        Fast
    }

    public interface NetworkActions
    {
    }
}
