using System;
using NetworkTypes;

namespace Assets.Scripts.Model
{

    [Serializable]
    public class GamePiece : SpacialObject
    {
        public GamePiece(Point location) : base(location)
        {

        }
    }
}
