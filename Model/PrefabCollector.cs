using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Networking;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class PrefabCollector : MonoBehaviour
    {
        public List<GameObject> Prefabs;
        public List<NamedImage> CreatureImages;

        public GameObject GetPrefab(string prefabName)
        {
            var prefab = Prefabs.SingleOrDefault(x => x.name == prefabName);
            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            return instance;
        }
    }
}
