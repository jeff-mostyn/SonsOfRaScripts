[System.Serializable]
public class PO_Immunity : PO_BlessingCast
{
    public PO_Immunity(Blessing.blessingID _blesingId, string _rewiredPlayerKey) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
    }
}
