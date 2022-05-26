namespace bot
{
    public class Gem
    {
        private const int HEIGHT = 8;
        private const int WIDTH = 8;
        public int index;
        public int x;
        public int y;
        public GemType type;

        public GemModifier modifier;

        public Gem(int index, GemType type, GemModifier gemModifier)
        {
            this.index = index;
            this.type = type;
            this.modifier = gemModifier;
            updatePosition();
        }

        private void updatePosition()
        {
            y = index / HEIGHT;
            x = index - y * WIDTH;
        }

        public bool sameType(Gem other)
        {
            return this.type == other.type;
        }

        public bool sameType(GemType type)
        {
            return this.type == type;
        }

        public bool equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Gem gem = (Gem)o;

            if (index != gem.index) return false;
            return type == gem.type;
        }

        public int hashCode()
        {
            int result = index;
            result = 31 * result + (type != null ? type.GetHashCode() : 0);
            return result;
        }
    }
}