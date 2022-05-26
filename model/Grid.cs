
using Sfs2X.Entities.Data;

namespace bot
{
    public class Grid
    {
        private List<Gem> gems = new List<Gem>();
        private ISFSArray gemsCode;
        public HashSet<GemType> gemTypes = new HashSet<GemType>();
        private HashSet<GemType> myHeroGemType;

        public Grid(ISFSArray gemsCode, ISFSArray gemModifiers, HashSet<GemType> gemTypes)
        {
            updateGems(gemsCode, gemModifiers);
            this.myHeroGemType = gemTypes;
        }

        public void updateGems(ISFSArray gemsCode, ISFSArray gemModifiers)
        {
            gems.Clear();
            gemTypes.Clear();
            for (int i = 0; i < gemsCode.Size(); i++)
            {
                Gem gem = new Gem(i, (GemType)gemsCode.GetByte(i), gemModifiers != null ? (GemModifier)gemModifiers.GetByte(i) : GemModifier.NONE);
                gems.Add(gem);
                gemTypes.Add(gem.type);
            }
        }

        public Pair<int>? SwapPrioritizeGem(List<GemSwapInfo> listMatchGem, Hero mainAttackHero, Hero subAttackHero, Hero supportHero)
        {
            var gemPrioritizeMainAttack = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
             .Where(gemMatch => gemMatch.type == GemType.BLUE)
             .Where(gemMatch => gemMatch.type == GemType.BROWN)
             .FirstOrDefault();
            if (gemPrioritizeMainAttack != null && mainAttackHero.isAlive())
            {
                return gemPrioritizeMainAttack.getIndexSwapGem();
            }

            var gemPrioritizeSubAttack = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
                .Where(gemMatch => gemMatch.type == GemType.RED)
                .Where(gemMatch => gemMatch.type == GemType.PURPLE)
                .FirstOrDefault();
            if (gemPrioritizeSubAttack != null && subAttackHero.isAlive())
            {
                return gemPrioritizeSubAttack.getIndexSwapGem();
            }

            var gemPrioritizeMSupport = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
               .Where(gemMatch => gemMatch.type == GemType.YELLOW)
               .Where(gemMatch => gemMatch.type == GemType.GREEN)
               .FirstOrDefault();
            if (gemPrioritizeMSupport != null && supportHero.isAlive())
            {
                return gemPrioritizeMSupport.getIndexSwapGem();
            }

            var gemPrioritizeSword = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
               .Where(gemMatch => gemMatch.type == GemType.SWORD)
               .FirstOrDefault();
            if (gemPrioritizeSword != null)
            {
                return gemPrioritizeSword.getIndexSwapGem();
            }

            return null;
        }

        public Pair<int> recommendSwapGemFirstStrategy(Player botPlayer, Player enemyPlayer)
        {

            var supportHero = botPlayer.GetHeroByID(HeroIdEnum.MONK);
            var mainAttackHero = botPlayer.GetHeroByID(HeroIdEnum.CERBERUS);
            var subAttackHero = botPlayer.GetHeroByID(HeroIdEnum.FIRE_SPIRIT);

            List<GemSwapInfo> listMatchGem = suggestMatch();

            if (listMatchGem.Count == 0)
            {
                return new Pair<int>(-1, -1);
            }

            var bonusTurnSwap = SwapPrioritizeGem(listMatchGem, mainAttackHero, subAttackHero, supportHero);
            if (bonusTurnSwap != null)
            {
                return bonusTurnSwap;
            }


            if (botPlayer.firstHeroAlive().getHeroAttack() >= enemyPlayer.firstHeroAlive().getHeroHP())
            {
                List<GemSwapInfo> matchGemSword1 = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).ToList();
                if (matchGemSword1 != null && matchGemSword1?.Count > 0)
                {
                    return EatGemByNumberOfGemMatch(matchGemSword1);
                }
            }

            List<GemSwapInfo> matchSupportGem = listMatchGem
                .Where(gemMatch => gemMatch.type == GemType.YELLOW || gemMatch.type == GemType.GREEN).ToList();
            if (matchSupportGem != null && matchSupportGem?.Count > 0 && supportHero.isAlive() && supportHero.getHeroAttack() <= 7)
            {
                return EatGemByNumberOfGemMatch(matchSupportGem);
            }


            List<GemSwapInfo> matchMainAttackGem = listMatchGem
               .Where(gemMatch => gemMatch.type == GemType.BLUE || gemMatch.type == GemType.BROWN).ToList();
            if (matchMainAttackGem != null && matchMainAttackGem?.Count > 0 && mainAttackHero.isAlive() && mainAttackHero.getHeroAttack() > 7)
            {
                return EatGemByNumberOfGemMatch(matchMainAttackGem);
            }

            List<GemSwapInfo> matchSubAttackGem = listMatchGem
              .Where(gemMatch => gemMatch.type == GemType.RED || gemMatch.type == GemType.PURPLE).ToList();
            if (matchSubAttackGem != null && matchSubAttackGem?.Count > 0 && subAttackHero.isAlive() && subAttackHero.getHeroAttack() > 7)
            {
                return EatGemByNumberOfGemMatch(matchSubAttackGem);
            }

            var enemySupportAlreadyBuff = enemyPlayer.heroes.Any(hero => hero.getHeroAttack() > 8);
            if (matchSubAttackGem != null
                && matchSubAttackGem?.Count > 0
                && !supportHero.isAlive()
                && subAttackHero.isAlive() 
                && subAttackHero.getHeroAttack() <= 7
                && enemySupportAlreadyBuff)
            {
                return EatGemByNumberOfGemMatch(matchSubAttackGem);
            }



            if (matchSupportGem != null && matchSupportGem?.Count > 0 && supportHero.isAlive())
            {
                return matchSupportGem.First().getIndexSwapGem();
            }

            if (matchMainAttackGem != null && matchMainAttackGem?.Count > 0 && mainAttackHero.isAlive())
            {
                return matchMainAttackGem.First().getIndexSwapGem();
            }

            if (matchSubAttackGem != null && matchSubAttackGem?.Count > 0 && subAttackHero.isAlive())
            {
                return matchSubAttackGem.First().getIndexSwapGem();
            }

            GemSwapInfo matchGemSword = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();
            if (matchGemSword != null)
            {
                return matchGemSword.getIndexSwapGem();
            }

            // can update
            //an nhung gem ma hero ben kia dang can

            return EatGemByNumberOfGemMatch(listMatchGem);

            foreach (GemType type in myHeroGemType)
            {
                GemSwapInfo matchGem = listMatchGem.Where(gemMatch => gemMatch.type == type).FirstOrDefault();
                if (matchGem != null)
                {
                    return matchGem.getIndexSwapGem();
                }
            }
            return listMatchGem[0].getIndexSwapGem();
        }


        public Pair<int> EatGemByNumberOfGemMatch(List<GemSwapInfo> listMatchGem)
        {
            GemSwapInfo matchGemSizeThanFour = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4).FirstOrDefault();
            if (matchGemSizeThanFour != null)
            {
                return matchGemSizeThanFour.getIndexSwapGem();
            }
            GemSwapInfo matchGemSizeThanThree = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 3).FirstOrDefault();
            if (matchGemSizeThanThree != null)
            {
                return matchGemSizeThanThree.getIndexSwapGem();
            }
            return listMatchGem[0].getIndexSwapGem();
        }


        public Pair<int> recommendSwapGem()
        {
            List<GemSwapInfo> listMatchGem = suggestMatch();

            Console.WriteLine("recommendSwapGem " + listMatchGem.Count);
            if (listMatchGem.Count == 0)
            {
                return new Pair<int>(-1, -1);
            }

            GemSwapInfo matchGemSizeThanFour = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4).FirstOrDefault();
            if (matchGemSizeThanFour != null)
            {
                return matchGemSizeThanFour.getIndexSwapGem();
            }
            GemSwapInfo matchGemSizeThanThree = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 3).FirstOrDefault();
            if (matchGemSizeThanThree != null)
            {
                return matchGemSizeThanThree.getIndexSwapGem();
            }
            GemSwapInfo matchGemSword = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).FirstOrDefault();
            if (matchGemSword != null)
            {
                return matchGemSword.getIndexSwapGem();
            }

            foreach (GemType type in myHeroGemType)
            {
                GemSwapInfo matchGem = listMatchGem.Where(gemMatch => gemMatch.type == type).FirstOrDefault();
                //listMatchGem.stream().filter(gemMatch -> gemMatch.getType() == type).findFirst();
                if (matchGem != null)
                {
                    return matchGem.getIndexSwapGem();
                }
            }
            return listMatchGem[0].getIndexSwapGem();
        }

        public List<GemSwapInfo> suggestMatch()
        {
            var listMatchGem = new List<GemSwapInfo>();

            var tempGems = new List<Gem>(gems);
            foreach (Gem currentGem in tempGems)
            {
                Gem swapGem = null;
                // If x > 0 => swap left & check
                if (currentGem.x > 0)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x - 1, currentGem.y)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If x < 7 => swap right & check
                if (currentGem.x < 7)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x + 1, currentGem.y)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If y < 7 => swap up & check
                if (currentGem.y < 7)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x, currentGem.y + 1)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
                // If y > 0 => swap down & check
                if (currentGem.y > 0)
                {
                    swapGem = gems[getGemIndexAt(currentGem.x, currentGem.y - 1)];
                    checkMatchSwapGem(listMatchGem, currentGem, swapGem);
                }
            }
            return listMatchGem;
        }

        private void checkMatchSwapGem(List<GemSwapInfo> listMatchGem, Gem currentGem, Gem swapGem)
        {
            swap(currentGem, swapGem);
            HashSet<Gem> matchGems = matchesAt(currentGem.x, currentGem.y);

            swap(currentGem, swapGem);
            if (matchGems.Count > 0)
            {
                listMatchGem.Add(new GemSwapInfo(currentGem.index, swapGem.index, matchGems.Count, currentGem.type));
            }
        }

        private int getGemIndexAt(int x, int y)
        {
            return x + y * 8;
        }

        private void swap(Gem a, Gem b)
        {
            int tempIndex = a.index;
            int tempX = a.x;
            int tempY = a.y;

            // update reference
            gems[a.index] = b;
            gems[b.index] = a;

            // update data of element
            a.index = b.index;
            a.x = b.x;
            a.y = b.y;

            b.index = tempIndex;
            b.x = tempX;
            b.y = tempY;
        }

        private HashSet<Gem> matchesAt(int x, int y)
        {
            HashSet<Gem> res = new HashSet<Gem>();
            Gem center = gemAt(x, y);
            if (center == null)
            {
                return res;
            }

            // check horizontally
            List<Gem> hor = new List<Gem>();
            hor.Add(center);
            int xLeft = x - 1, xRight = x + 1;
            while (xLeft >= 0)
            {
                Gem gemLeft = gemAt(xLeft, y);
                if (gemLeft != null)
                {
                    if (!gemLeft.sameType(center))
                    {
                        break;
                    }
                    hor.Add(gemLeft);
                }
                xLeft--;
            }
            while (xRight < 8)
            {
                Gem gemRight = gemAt(xRight, y);
                if (gemRight != null)
                {
                    if (!gemRight.sameType(center))
                    {
                        break;
                    }
                    hor.Add(gemRight);
                }
                xRight++;
            }
            if (hor.Count >= 3) res.UnionWith(hor);

            // check vertically
            List<Gem> ver = new List<Gem>();
            ver.Add(center);
            int yBelow = y - 1, yAbove = y + 1;
            while (yBelow >= 0)
            {
                Gem gemBelow = gemAt(x, yBelow);
                if (gemBelow != null)
                {
                    if (!gemBelow.sameType(center))
                    {
                        break;
                    }
                    ver.Add(gemBelow);
                }
                yBelow--;
            }
            while (yAbove < 8)
            {
                Gem gemAbove = gemAt(x, yAbove);
                if (gemAbove != null)
                {
                    if (!gemAbove.sameType(center))
                    {
                        break;
                    }
                    ver.Add(gemAbove);
                }
                yAbove++;
            }
            if (ver.Count >= 3) res.UnionWith(ver);

            return res;
        }

        // Find Gem at Position (x, y)
        private Gem gemAt(int x, int y)
        {
            foreach (Gem g in gems)
            {
                if (g != null && g.x == x && g.y == y)
                {
                    return g;
                }
            }
            return null;
        }
    }
}