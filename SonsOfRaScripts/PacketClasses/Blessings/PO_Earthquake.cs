[System.Serializable]
public class PO_Earthquake : PO_BlessingCast
{
    public float[] location;

    public PO_Earthquake(Blessing.blessingID _blesingId, string _rewiredPlayerKey) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
    }
}
