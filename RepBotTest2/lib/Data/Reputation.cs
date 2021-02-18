namespace RepBot.lib
{
    public class Reputation
    {
        public ulong RepId;
        public ulong UserId;
        public string Reason;
        public int RepAmount;
        public bool GoodRep => RepAmount >= 0;
        public string GetRepAmount() => RepAmount.ToString("+0;-#");
        public Reputation(ulong repId, ulong userId, bool goodRep, string reason, int repWeight = 1)
        {
            RepId = repId;
            UserId = userId;
            Reason = reason;
            RepAmount = goodRep ? repWeight : repWeight * -1;
        }
    

    }
}