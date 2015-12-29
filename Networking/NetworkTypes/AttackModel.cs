namespace NetworkTypes
{
    public class AttackModel : SerializableType
    {
        public int TargetCreatureIndex { get; set; }
        public int SenderCreatureIndex { get; set; }
        public double Damage { get; set; }
    }
}
