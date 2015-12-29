using Assets.Scripts.Model;
using GameActors;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public Tile TileHex;
    public Color Color;
    public SpriteRenderer Sprite;
    public static bool OnMove = false;

    public void Start() 
    {
        Sprite = GetComponent<SpriteRenderer>();
        Color = new Color32(255, 255, 255, 255);
    }

    public void ChangeColor(Color color)
    {
        Sprite.color = color;
    }

    public void ResetColor()
    {
        Sprite.color = TileHex.CanPass ? Color : Color.gray;
        TileHex.CanSelect = false;
    }

    public void SelectMaterial()
    {
        if (Sprite == null)
        {
            Sprite = GetComponent<SpriteRenderer>();
        }
        Sprite.color = TileHex.CanPass ? Sprite.color : Sprite.color = Color.gray;
    }

    public void Select()
    {
        if (OnMove == false && TileHex.CanPass && TileHex.CanSelect && !CreatureComponent.DisplayPanel)
        {
            Messenger<TileBehaviour>.Broadcast("Tile selected", this);
        }
    }    
}
