using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Networking;
using Events;
using GameActors;
using NetworkTypes;
using PathFinding;
using UnityEngine;
using UnityEngine.UI;
using CreatureType = GameActors.CreatureType;
using Team = NetworkTypes.Team;

namespace Assets.Scripts.Controller
{
    public class BoardBehaviorMultiplayer : MonoBehaviour
    {
        public GameObject HexPrefab;
        public GameObject Hero1Prefab;
        public GameObject Hero2Prefab;
        public GameObject Canvas;
        public GameObject TextPrefab;
        public GameObject PrefabDatabase;

        private int _width;
        private int _height;
        private List<GameObject> _path = new List<GameObject>();
        private List<TileBehaviour> _tiles;
        private Game _game;
        private float _spacing;
        private float _hexWidth;
        private Hero _hero1;
        private Hero _hero2;

        public List<GameObject> Creatures;
        public GameObject Console;
        public GameObject Defend;

        private List<List<Tile>> _movementRange = new List<List<Tile>>();
        private bool _canMove;
        private CreatureComponent _currentCreature;
        private Tile _selectedTile;
        public bool MustAttack;
        private CreatureComponent _attackCreature;
        private PrefabCollector _prefabCollector;
        private LobbyPlayer _player;
        private Vector3 _heroPosition;
        private Vector3 _enemyHeroPosition;
        private readonly List<CreatureComponent> _creaturesComponent = new List<CreatureComponent>();
        private readonly List<GamePiece> _gamePieces = new List<GamePiece>(); 
        public Queue<Action> ExecuteOnMainThread;

        private GameObject _hero1Obj;
        private GameObject _hero2Obj;
        private bool _isAllPlayersReady;
        private bool _isFirstRound;
        private CreatureHelper _creatureHelper;
        private readonly Dictionary<string, string> _creaturePrefabDictionary = new Dictionary<string, string>();
        private readonly Dictionary<int, Image> _creatureImageDictionary = new Dictionary<int, Image>();
        private Dictionary<NetworkTypes.CreatureType, Sprite> _imageDictionary; 

        void Awake()
        {
            ExecuteOnMainThread = new Queue<Action>();
            _creatureHelper = GameObject.Find("GameManager").GetComponent<CreatureHelper>();
            _player = Networking.Network.Instance.ClientPlayer;
            _creatureHelper.Creatures = new List<AbstractCreature>();

            MustAttack = false;
            _canMove = false;
            _heroPosition = GetWorldCoordinates(-1, -1, 0);
            _enemyHeroPosition = GetWorldCoordinates(_width, 0, 0);

            GameFlow.Instance.Channel = new NetworkMessageChannel(_player);
            GameFlow.Instance.Channel.MoveCallback += DrawPath;
            GameFlow.Instance.Channel.TurnCallback += ChangeTurn;
            GameFlow.Instance.Channel.SyncHeroesCallback += SyncHero;
            GameFlow.Instance.Channel.Attack += Attack;
            GameFlow.Instance.Channel.GameIsReadyCallback += GameIsReady;
            GameFlow.Instance.Channel.DieCallback += Die;


            Messenger<TileBehaviour>.AddListener("Tile selected", MoveCreatureToSelectedTile);
            Messenger<CreatureComponent>.AddListener("Action finish", FinishAction);
            Messenger<CreatureComponent>.AddListener("CreatureComponent Selected", MouseOverCreature);
            Messenger<CreatureComponent>.AddListener("CreatureComponent Exit", MouseExitCreature);
            Messenger<CreatureComponent>.AddListener("CreatureComponent Attack", MouseClickCreature);


            var roomName = new SimpleMessage { Message = GameFlow.Instance.RoomName };
            var parameters = new List<SerializableType> { roomName };
            var remoteInvokeMethod = new RemoteInvokeMethod("", Command.InitializeBoard, parameters);
            Client.BeginSendPackage(remoteInvokeMethod);

            _hero1Obj = Instantiate(Hero1Prefab) as GameObject;
            _hero2Obj = Instantiate(Hero2Prefab) as GameObject;

            _prefabCollector = PrefabDatabase.GetComponent<PrefabCollector>();

            _creaturePrefabDictionary.Add("Marksman", "GoblinHex");
            _creaturePrefabDictionary.Add("Troglodyte", "OrcHex");

            _imageDictionary = new Dictionary<NetworkTypes.CreatureType, Sprite>()
            {
                { NetworkTypes.CreatureType.Range, _prefabCollector.CreatureImages.SingleOrDefault(x => x.Name == "Range").Image},
                { NetworkTypes.CreatureType.Melee, _prefabCollector.CreatureImages.SingleOrDefault(x => x.Name == "Melee").Image}
            };

        }

        public void Update()
        {
            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue().Invoke();
            }

            if (_isAllPlayersReady && _isFirstRound && _creaturesComponent.Count > 0)
            {
                _isFirstRound = false;
                GameFlow.Instance.Channel.FinishAction();
            }
        }

        #region State of Game
        public void GameIsReady()
        {
            _isAllPlayersReady = true;
        }

        public void FinishAction(CreatureComponent creature)
        {
            GameFlow.Instance.Channel.FinishAction();
            TileBehaviour.OnMove = false;
            _canMove = false;
            ExecuteOnMainThread.Enqueue(ResetState);
        }

        public void ResetState()
        {
            _tiles.ForEach(x => x.ResetColor());
            _creatureHelper.AttackMelee.SetActive(false);
            _creatureHelper.AttackRange.SetActive(false);
            _movementRange.Clear();
            _movementRange.TrimExcess();
            _path.ForEach(Destroy);
        }

        public void ChangeTurn(NextTurn turn)
        {
            TileBehaviour.OnMove = false;
            ExecuteOnMainThread.Enqueue(() => HighlightCurrentCreature(false, _currentCreature));
            _currentCreature = _creaturesComponent.SingleOrDefault(x => x.Index == turn.CreatureIndex);
            ExecuteOnMainThread.Enqueue(() => HighlightCurrentCreature(true, _currentCreature));

            if (_currentCreature == null)
            {
                return;
            }

            if (!IsYourTurn(_currentCreature))
            {
                ExecuteOnMainThread.Enqueue(() =>
                {
                    _creatureHelper.AttackMelee.SetActive(false);
                    _creatureHelper.AttackRange.SetActive(false);
                    Console.GetComponent<Text>().text = "Enemy Turn";
                });
                return;
            }
            ExecuteOnMainThread.Enqueue(() =>
            {
                Console.GetComponent<Text>().text = "Your Turn";
            });
            ExecuteOnMainThread.Enqueue(CheckPath);
            _canMove = true;
        }
        #endregion

        #region Sync Creatures

        public void SyncHero(BoardInfo board, AbstractHero h1, AbstractHero h2)
        {
            InitializeBoard(board);
            InstantiateHeroes(h1, h2);
            InstantiateCreature();
        }

        private void InitializeBoard(BoardInfo board)
        {
            _width = board.Width;
            _height = board.Height;
            _spacing = (float)board.Spacing;
            _hexWidth = (float)board.HexWidth;
            _game = new Game(_width, _height);
            Creatures = new List<GameObject>();
            _tiles = new List<TileBehaviour>();
        }

        private void InstantiateHeroes(AbstractHero h1, AbstractHero h2)
        {
            SetTransform(_hero1Obj, 1, _heroPosition);
            SetTransform(_hero2Obj, -1, _enemyHeroPosition);
            ExecuteOnMainThread.Enqueue(() =>
            {
                _hero1 = _hero1Obj.GetComponent<Hero>();
                _hero1.Attributes = h1;
                _hero2 = _hero2Obj.GetComponent<Hero>();
                _hero2.Attributes = h2;
            });
        }

        private void InstantiateCreature()
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                GenerateCreatures(_hero1);
                GenerateCreatures(_hero2);
                CreateBoard();
            });
        }

        private void GenerateCreatures(Hero hero)
        {
            foreach (var abstractCreature in hero.Attributes.Creatures)
            {
                var path = _creaturePrefabDictionary[abstractCreature.Name];
                if (abstractCreature.Team == Team.Blue) path += "Left";
                var instanceCreature = _prefabCollector.GetPrefab(path);
                var creatureComponent = instanceCreature.GetComponent<CreatureComponent>();
                instanceCreature.transform.position = GetWorldCoordinates(abstractCreature.Piece.X, abstractCreature.Piece.Y, -1);
                creatureComponent.Piece = GeneratePiece(abstractCreature.Piece.X, abstractCreature.Piece.Y);
                _gamePieces.Add(creatureComponent.Piece);
                creatureComponent.Index = abstractCreature.Index;
                creatureComponent.Team = hero.Attributes.HeroTeam;
                abstractCreature.Health = abstractCreature.MaxHealth;
                creatureComponent.Attributes = abstractCreature;
                creatureComponent.InitializeBehavior();
                hero.InstanceCreatures.Add(instanceCreature);
                var index = (creatureComponent.Index - 1) % 4;
                if (abstractCreature.Team == Team.Red)
                {
                    var imageComponent = _creatureHelper.RedTeam[index].GetComponent<Image>();
                    _creatureHelper.RedTeam[index].GetComponent<ShowCreatureInfo>().Index = creatureComponent.Index;
                    imageComponent.sprite = _imageDictionary[abstractCreature.Type];
                    _creatureImageDictionary.Add(abstractCreature.Index, imageComponent);
                    _creatureHelper.Creatures.Add(creatureComponent.Attributes);
                }
                else
                {
                    var imageComponent = _creatureHelper.BlueTeam[index].GetComponent<Image>();
                    _creatureHelper.BlueTeam[index].GetComponent<ShowCreatureInfo>().Index = creatureComponent.Index;
                    imageComponent.sprite = _imageDictionary[abstractCreature.Type];
                    _creatureImageDictionary.Add(abstractCreature.Index, imageComponent);
                    _creatureHelper.Creatures.Add(creatureComponent.Attributes);
                }
                AttachInfoText(creatureComponent);
            }
        }

        private GamePiece GeneratePiece(int x, int y)
        {
            _game.BlockOutTiles(x, y);
            return new GamePiece(new Point(x, y));
        }

        private void AttachInfoText(CreatureComponent creatureComponent)
        {
            var text = Instantiate(TextPrefab);
            text.transform.SetParent(Canvas.transform);
            creatureComponent.Text = text.GetComponent<RectTransform>();
            creatureComponent.Text.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            creatureComponent.Text.localPosition = new Vector3(0, 0, -2);
            _creaturesComponent.Add(creatureComponent);
        }

        private void CreateBoard()
        {
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var tile = Instantiate(HexPrefab);
                    var tileTransform = tile.transform;
                    tileTransform.position = GetWorldCoordinates(x, y, 0);
                    tile.name = "tile" + x + "_" + y;
                    tileTransform.parent = transform;

                    var tb = (TileBehaviour)tileTransform.GetComponent("TileBehaviour");
                    tb.TileHex = _game.GameBoard[x, y];
                    tb.SelectMaterial();
                    _tiles.Add(tb);
                }
            }
            _game.InitialiseGamePieces(_gamePieces);
            _isFirstRound = true;
        }

        private void SetTransform(GameObject element, float xScaleFactor, Vector3 position)
        {
            if (element == null)
            {
                return;
            }
            ExecuteOnMainThread.Enqueue(() =>
            {
                element.transform.position = position;
                element.transform.localScale = new Vector3(element.transform.localScale.x * xScaleFactor,
                    element.transform.localScale.y, element.transform.localScale.z);
            });
        }

        #endregion

        #region Grid

        public void DrawPath(List<Point> path)
        {
            if (_currentCreature == null) return;
            ExecuteOnMainThread.Enqueue(() =>
            {
                _path = new List<GameObject>();
                foreach (var point in path)
                {
                    var hex = Instantiate(HexPrefab) as GameObject;
                    var tileTransform = hex.transform;
                    tileTransform.position = GetWorldCoordinates(point.X, point.Y, 0);
                    tileTransform.parent = transform;
                    hex.GetComponent<SpriteRenderer>().color = Color.blue;
                    _path.Add(hex);
                }
                _currentCreature.Points = _path.Select(x => x.transform.position).ToList();
                var startPosition = path.First();
                var destinationPosition = path.Last();
                _currentCreature.Piece.Location = destinationPosition;
                SetTileHex(startPosition, true);
                SetTileHex(destinationPosition, false);
                _currentCreature.Push(0);
            });
        }

        public void SetTileHex(Point tilePoint, bool value)
        {
            if (_currentCreature == null) return;
            var tileBehavior = _tiles.SingleOrDefault(x => x.TileHex == _game.GameBoard[tilePoint.X, tilePoint.Y]);
            if (tileBehavior == null) return;
            tileBehavior.TileHex.CanPass = value;
            tileBehavior.TileHex.CanSelect = value;

            if (value)
            {
                tileBehavior.ChangeColor(Color.gray);
                return;
            }
            tileBehavior.ResetColor();
        }

        public Vector3 GetWorldCoordinates(int x, int y, float z)
        {
            var xOffset = y % 2 == 0 ? 0 : -_hexWidth / 2;
            return new Vector3(x * _hexWidth + xOffset, y * _spacing, z);
        }

        private void CheckPath()
        {
            TileBehaviour.OnMove = false;
            var start = _game.AllTiles.SingleOrDefault(o => o.X == _currentCreature.Piece.X && o.Y == _currentCreature.Piece.Y);
            _movementRange = PathFind.MovementRange(start, _currentCreature.Attributes.Speed);
            var movementTiles = _movementRange.SelectMany(t => t);
            var avaibleTiles = _tiles.Where(t => t.TileHex.CanPass && movementTiles.Any(tile => tile == t.TileHex && tile.CanPass));
            foreach (var tile in avaibleTiles)
            {
                tile.TileHex.CanSelect = true;
                tile.ChangeColor(Color.red);
            }
            _tiles.Find(x => x.TileHex == start).ChangeColor(Color.cyan);
        }

        #endregion

        #region Check Validity

        private bool IsYourTurn(CreatureComponent creature)
        {
            return GameFlow.Instance.IsGameCreator && creature.Team == Team.Red ||
                   !GameFlow.Instance.IsGameCreator && creature.Team == Team.Blue;
        }
    
        private bool IsTilesNeighbours(Tile selectedCreature, Tile currentCreature)
        {
            return currentCreature.AllNeighbours.Any(tile => tile.X == selectedCreature.X && tile.Y == selectedCreature.Y);
        }

        private bool IsTargetValid(CreatureComponent creature)
        {
            if (_currentCreature == null)
            {
                return false;
            }
            var isSameTeam = _canMove && !TileBehaviour.OnMove && _currentCreature.Team == creature.Team;
            return IsYourTurn(_currentCreature) && !isSameTeam;
        }

        #endregion

        #region Mouse Events

        public void MouseOverCreature(CreatureComponent creature)
        {
            if (!IsTargetValid(creature))
            {
                return;
            }

            if (_currentCreature.Type == CreatureType.Melee)
            {
                var xCoor = _currentCreature.Piece.Location.X;
                var yCoor = _currentCreature.Piece.Location.Y;
                var currentCreatureTile = _game.GameBoard[xCoor, yCoor];
                var selectedCreatureTile = _game.GameBoard[creature.Piece.Location.X, creature.Piece.Location.Y];
                if (IsTilesNeighbours(currentCreatureTile, selectedCreatureTile))
                {
                    _selectedTile = null;
                    _attackCreature = creature;
                    _currentCreature.behavior.DisplayAttackIcon(_currentCreature.creatureHelper.Canvas);
                    return;
                }
                var closestTile = _movementRange
                    .SelectMany(tile => tile)
                    .FirstOrDefault(tile => IsTilesNeighbours(selectedCreatureTile, tile));
                if (closestTile == null) return;
                _currentCreature.behavior.DisplayAttackIcon(_currentCreature.creatureHelper.Canvas);
                _selectedTile = closestTile;
            }
            _attackCreature = creature;
            _currentCreature.behavior.DisplayAttackIcon(_currentCreature.creatureHelper.Canvas);
        }

        public void MouseExitCreature(CreatureComponent creature)
        {
            _attackCreature = null;
            _selectedTile = null;
            if (_currentCreature == null) return;
            _currentCreature.behavior.HideAttackIcon();
        }

        public void MouseClickCreature(CreatureComponent creature)
        {
            if (!IsTargetValid(creature) || _attackCreature == null)
            {
                return;
            }
            _currentCreature.behavior.Target = creature.transform.gameObject;
            TileBehaviour.OnMove = true;
            ExecuteOnMainThread.Enqueue(() =>
            {
                _creatureHelper.AttackMelee.SetActive(false);
                _creatureHelper.AttackRange.SetActive(false);
            });

            if (_selectedTile != null && _currentCreature.Type == CreatureType.Melee)
            {
                var tileBehavior = _tiles.SingleOrDefault(x => x.TileHex == _selectedTile);
                MoveCreatureToSelectedTile(tileBehavior);
                MustAttack = true;
                _selectedTile = null;
                _attackCreature = null;
                return;
            }

            var targetIndex = _currentCreature.behavior.Target.GetComponent<CreatureComponent>().Index;
            GameFlow.Instance.Channel.AttackCreature(targetIndex, _currentCreature.Index);
            _attackCreature = null;

        }

        #endregion

        #region Creature Events

        public void MoveCreatureToSelectedTile(TileBehaviour tileBehaviour)
        {
            var location = _currentCreature.Piece.Location;
            var start = location;
            var destination = new Point(tileBehaviour.TileHex.X, tileBehaviour.TileHex.Y);
            _currentCreature.Piece = new GamePiece(destination);
            GameFlow.Instance.Channel.MovePiece(location, start, destination);
        }

        public void Attack(AttackModel attackInfo)
        {
            _currentCreature = _creaturesComponent.SingleOrDefault(x => x.Index == attackInfo.SenderCreatureIndex);
            var target = _creaturesComponent.SingleOrDefault(x => x.Index == attackInfo.TargetCreatureIndex);
            ExecuteOnMainThread.Enqueue(() =>
            {
                _currentCreature.Attack();
                _currentCreature.behavior.Target = target.gameObject;
                _currentCreature.behavior.Attack(5, _currentCreature.transform.position);
                target.ReceiveDamage(attackInfo.Damage);
                _currentCreature._anim.SetFloat("attack", 1);
            });
        }

        public void DefenseCreature()
        {
            GameFlow.Instance.Channel.DefenseCreature(_currentCreature);
        }

        public void Die(Point location)
        {
            var tile = _game.GameBoard[location.X, location.Y];
            var tileBehavior = _tiles.SingleOrDefault(x => x.TileHex == tile);
            if (tileBehavior == null) return;
            tileBehavior.TileHex.CanPass = true;
            tileBehavior.TileHex.CanSelect = true;
            var creatureComponent = _creaturesComponent.SingleOrDefault(x => x.Piece.X == location.X && x.Piece.Y == location.Y);
            ExecuteOnMainThread.Enqueue(() => DisableCreatureComponent(creatureComponent, tileBehavior));
        }

        private void DisableCreatureComponent(CreatureComponent creatureComponent, TileBehaviour tileBehavior)
        {
            _creatureImageDictionary[creatureComponent.Attributes.Index].sprite = new Sprite();
            creatureComponent.gameObject.SetActive(false);
            creatureComponent.GetComponent<SphereCollider>().enabled = false;
            creatureComponent._textComponent.gameObject.SetActive(false);
            creatureComponent.Status = GameActors.CreatureStatus.Death;
            tileBehavior.ResetColor();
        }

        private void HighlightCurrentCreature(bool highlight, CreatureComponent creature)
        {
            if (creature == null)
            {
                return;
            }

            if (highlight)
            {
                _creatureImageDictionary[creature.Index].color = Color.magenta;
                return;
            }

            _creatureImageDictionary[creature.Index].color = Color.white;
        }

        #endregion

    }
}
