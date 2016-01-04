using UnityEngine;
using System.Globalization;
using System.Linq;
using NetworkTypes;

public class ShowCreatureInfo : MonoBehaviour
{
    public GameObject Panel;
    public CreatureHelper CreatureHelper;
    public AbstractCreature Creature;

    public void ShowInfo()
    {
        if (Creature == null || Creature.Status == CreatureStatus.Death)
        {
            Panel.SetActive(false);
            return;
        }
        Panel.SetActive(true);
        CreatureHelper.Count.text = Creature.Count.ToString();
        CreatureHelper.Armor.text = Creature.Armor.ToString();
        CreatureHelper.Health.text = Creature.Health.ToString(CultureInfo.InvariantCulture);
        CreatureHelper.Speed.text = Creature.Speed.ToString();
        CreatureHelper.Type.text = Creature.Type.ToString();
    }

}
