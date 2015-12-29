using UnityEngine;
using System.Globalization;
using System.Linq;
using NetworkTypes;

public class ShowCreatureInfo : MonoBehaviour
{
    public GameObject Panel;
    public CreatureHelper CreatureHelper;
    public int Index;

    public void ShowInfo()
    {
        var creature = CreatureHelper.Creatures.SingleOrDefault(x => x.Index == Index);
        if (creature == null || creature.Status == CreatureStatus.Death)
        {
            return;
        }

        Panel.SetActive(true);
        CreatureHelper.Count.text = creature.Count.ToString();
        CreatureHelper.Armor.text = creature.Armor.ToString();
        CreatureHelper.Health.text = creature.Health.ToString(CultureInfo.InvariantCulture);
        CreatureHelper.Speed.text = creature.Speed.ToString();
        CreatureHelper.Type.text = creature.Type.ToString();
    }

}
