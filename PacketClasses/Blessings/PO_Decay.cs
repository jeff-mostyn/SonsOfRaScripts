[System.Serializable]
public class PO_Decay : PO_BlessingCast
{
    public float[] location;

    public PO_Decay(Blessing.blessingID _blesingId, string _rewiredPlayerKey, float[] _location) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
        location = _location;
    }
}
