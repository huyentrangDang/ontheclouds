namespace bot
{
    public class GemSwapInfo 
    {
        private int index1;
        private int index2;
        public int sizeMatch;
        public GemType type;

        public GemSwapInfo(int index1, int index2, int sizeMatch, GemType type)
        {
            this.index1 = index1;
            this.index2 = index2;
            this.sizeMatch = sizeMatch;
            this.type = type;
        }

        public Pair<int> getIndexSwapGem() {
            return new Pair<int>(index1, index2);
        }
    }
}