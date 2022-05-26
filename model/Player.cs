
namespace bot {
    public class Player
    {
        public int playerId;
        public string displayName;
        public List<Hero> heroes;
        public HashSet<GemType> heroGemType;

        public Player(int playerId, string name)
        {
            this.playerId = playerId;
            this.displayName = name;

            heroes = new List<Hero>();
            heroGemType = new HashSet<GemType>();
        }

        public Hero anyHeroFullMana() {
            foreach(var hero in heroes){
                if (hero.isAlive() && hero.isFullMana()) return hero;
            }

            return null;
        }

        public Hero firstHeroAlive() {
            foreach(var hero in heroes){
                if (hero.isAlive()) return hero;
            }

            return null;
        }
        public Hero highgestAttackEnemyHero()
        {
            return heroes.OrderByDescending(x => x.getHeroAttack()).First();
        }


        public HashSet<GemType> getRecommendGemType() {
            heroGemType.Clear();
            foreach(var hero in heroes){
                if (!hero.isAlive()) continue;
                
                foreach(var gt in hero.gemTypes){
                    heroGemType.Add((GemType)gt);
                }
            }

            return heroGemType;
        }

        public Hero GetHeroByID(HeroIdEnum id)
        {
            return heroes.Where(hero => hero.id == id).FirstOrDefault();
        }
    }
}