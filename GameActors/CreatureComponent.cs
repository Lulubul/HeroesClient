using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Controller;

using Assets.Scripts.Model;
using AttackType;
using UnityEngine.UI;
using NetworkTypes;

namespace GameActors
{
    public enum CreatureType { Melee, Range }
    public enum CreatureStatus { Alive, Death }

    public class CreatureComponent : MonoBehaviour
    {
        public CreatureHelper creatureHelper;
        public RectTransform Text;

        public Text _textComponent;
        private int _current;
        private const float MoveSpeed = 12.0f;
        private bool _move;
        private bool _throwProjectile;
        private float _startTime;
        private float _journeyLength;
        private Vector3 _startMarker;
        private Vector3 _endMarker;
        public Animator _anim;
        public AttackBehavior behavior;
        public CreatureType Type;
        public Team Team;
        public CreatureStatus Status;
        public List<Vector3> Points;
        public int Index;

        public static bool DisplayPanel;
        public GamePiece Piece;
        public AbstractCreature Attributes;
        public Queue<Action> ExecuteOnMainThread;
        private float _damageTimeLeft = -2f;
        private float _receiveDamageTime = -2f;
        public GameObject ReceiveDamageSprite;
        private bool _mustDie;

        public void Awake()
        {
            if (transform.GetChild(0).GetComponent<Animator>() != null)
            {
                _anim = transform.GetChild(0).GetComponent<Animator>();
            }
            Status = CreatureStatus.Alive;
            var manager = GameObject.Find("GameManager");
            if (manager != null)
            {
                creatureHelper = manager.GetComponent<CreatureHelper>();
            }
            _current = 0;
            _move = false;

        }

        public void InitializeBehavior()
        {
            var arrow = transform.GetChild(1).gameObject;
            if (Type == CreatureType.Melee)
            {
                behavior = new AttackMelee(Attributes.Damage, creatureHelper.AttackMelee, arrow);
                return;
            }
            behavior = new AttackRange(Attributes.Damage, Attributes.Range, creatureHelper.AttackRange, arrow);
        }

        public void Start()
        {
            ExecuteOnMainThread = new Queue<Action>();
            ArrangeUi();
            _textComponent = Text.GetComponent<Text>();
            _textComponent.text = Attributes.Count.ToString();

        }

        private void ArrangeUi()
        {
            Vector2 pos = transform.position;
            Vector2 viewportPoint = Camera.main.WorldToViewportPoint(pos);

            Text.anchorMin = viewportPoint;
            Text.anchorMax = viewportPoint; 
        }

        public void Push(int index)
        {
            if (index == Points.Count) return;
            _current = index;
            _startTime = Time.time - 0.01f;
            _endMarker =  Points[_current];
            _startMarker = transform.position;
            _journeyLength = Vector3.Distance(transform.position, _endMarker);
            
            _anim.SetFloat("speed", 1);
            _anim.CrossFade("walk", 0.1f);
            _move = true;
        }

        public void Update()
        {

            if (_move)
            {
                Move();
                ArrangeUi();
            }

            if (_throwProjectile)
            {
                Throw();
            }

            if (behavior != null && behavior.Hit)
            {
                _throwProjectile = false;
                behavior.Hit = false;
                Messenger<CreatureComponent>.Broadcast("Action finish", this);
            }

            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("attack"))
            {
                _anim.SetFloat("attack", 0);
            }

            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue().Invoke();
            }

            if (_damageTimeLeft > 0)
            {
                _damageTimeLeft -= 0.01f;
            }

            if (_damageTimeLeft < 0 && _damageTimeLeft > -2)
            {
                _damageTimeLeft = -2;
                ExecuteOnMainThread.Enqueue(HideDamage);
            }

            if (_receiveDamageTime > 0f)
            {
                _receiveDamageTime -= 0.01f;
            }

            if (_receiveDamageTime < 0f && _receiveDamageTime > -2)
            {
                _receiveDamageTime = -2;
                ExecuteOnMainThread.Enqueue(() =>
                {
                    _textComponent.text = Attributes.Count.ToString();
                    ReceiveDamageSprite.SetActive(false);
                });
            }
        }

        private void HideDamage()
        {
            creatureHelper.DamageText.gameObject.SetActive(false);
            ReceiveDamageSprite.SetActive(false);
        }

        private void Throw()
        {
            behavior.MoveProjectile();
        }

        public void Display(Vector3 pos)
        {
            if (Status == CreatureStatus.Death) return;
            creatureHelper.Panel.SetActive(true);
            var myCanvas = creatureHelper.Canvas.GetComponent<Canvas>();
            var rect = myCanvas.transform as RectTransform;
            if (rect != null)
            {
                Vector2 position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos, myCanvas.worldCamera, out position);
                creatureHelper.Panel.transform.position = myCanvas.transform.TransformPoint(position);
            }

            creatureHelper.Type.text = Type.ToString();
            creatureHelper.Armor.text = Attributes.Armor.ToString();
            creatureHelper.Speed.text = Attributes.Speed.ToString();
            creatureHelper.Health.text = Attributes.Health.ToString();
            creatureHelper.Count.text = Attributes.Count.ToString();
            DisplayPanel = true;
        }

        public void Move() 
        {
            var distCovered = (Time.time - _startTime) * MoveSpeed;
            var fracJourney = distCovered / _journeyLength;
            transform.position = Vector3.Lerp(_startMarker, _endMarker, fracJourney);
            if (_current == Points.Count)
            {
                _move = false;
                transform.position = _endMarker;
                _current = -1;
                Points.Clear();
                Points.TrimExcess();
                _anim.SetFloat("speed", 0);
                if (!behavior.Hit)
                {
                    Messenger<CreatureComponent>.Broadcast("Action finish", this);
                }
                return;
            }
            if (!(Vector3.Distance(transform.position, _endMarker) < 0.1f)) return;
            _current++;
            Push(_current);
        }

        public void Attack() 
        {
            _throwProjectile = true;
        }

        public void Defense() 
        {
            Messenger<CreatureComponent>.Broadcast("Action finish", this);
        }

        public void ReceiveDamage(double damage)
        {
            if (damage < 0)
            {
                damage *= -1;
            }
            var totalHealth = (Attributes.Count - 1) * Attributes.MaxHealth + Attributes.Health - damage;
            _damageTimeLeft = 2f;

            Attributes.Health = totalHealth <= 0 ? 0 : totalHealth % (Attributes.MaxHealth + 1);
            Attributes.Count = totalHealth <= 0 ? 0 : (int)totalHealth / Attributes.MaxHealth + 1;

            ExecuteOnMainThread.Enqueue(() =>
            {
                creatureHelper.DamageText.gameObject.SetActive(true);
                creatureHelper.DamageText.text = "Damage: " + damage;
            });

            if (totalHealth <= 0)
            {
                _mustDie = true;
            }
        }

        public void Die()
        {
            GameFlow.Instance.Channel.DieCreature(this);
        }

        public void OnMouseOver()
        {
            if (DisplayPanel == false)
            {
                Messenger<CreatureComponent>.Broadcast("CreatureComponent Selected", this);
            }
        }

        public void OnMouseExit()
        {
            Messenger<CreatureComponent>.Broadcast("CreatureComponent Exit", this);
        }

        public void OnMouseDown()
        {
            if (DisplayPanel == false && TileBehaviour.OnMove == false)
            {
                Messenger<CreatureComponent>.Broadcast("CreatureComponent Attack", this);
            }
        }

        void OnTriggerEnter(Component other)
        {
            if (other.gameObject.name.Contains("goblin_arrow"))
            {
                var target = other.gameObject.transform.parent.GetComponent<CreatureComponent>().behavior.Target;
                if (target.transform.position == transform.position)
                {
                    if (_mustDie)
                    {
                        Die();
                    }
                    ExecuteOnMainThread.Enqueue(() =>
                    {
                        ReceiveDamageSprite.SetActive(true);
                    });
                    _receiveDamageTime = 1f;
                }
                
            }
        }
    }
}
