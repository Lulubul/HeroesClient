using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AttackType
{
    public abstract class AttackBehavior
    {
        protected double Damage;
        protected double Range;
        protected GameObject AttackIcon;
        public GameObject Target;
        public bool Hit;

        public virtual void Attack(int count, Vector3 position) {}
        public virtual void MoveProjectile() {}

        public void DisplayAttackIcon(GameObject canvas)
        {
            var myCanvas = canvas.GetComponent<Canvas>();
            AttackIcon.SetActive(true);
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
            AttackIcon.transform.position = myCanvas.transform.TransformPoint(pos);
        }

        public void HideAttackIcon()
        {
            AttackIcon.SetActive(false);
        }
    }
}
