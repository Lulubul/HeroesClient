using System.Collections.Generic;
using NetworkTypes;
using UnityEngine;
using UnityEngine.UI;


public class CreatureHelper : MonoBehaviour
{
    public GameObject Panel;
    public GameObject AttackMelee;
    public GameObject AttackRange;
    public GameObject Canvas;
    public GameObject DeathIcon;
    public List<AbstractCreature> Creatures;
    public List<GameObject> BlueTeam;
    public List<GameObject> RedTeam;
    public Text DamageText;
    public Text Type;
    public Text Armor;
    public Text Speed;
    public Text Health;
    public Text Count;
}
