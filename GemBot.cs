using Sfs2X.Entities;
using Sfs2X.Entities.Data;
namespace bot
{
    public class GemBot : BaseBot
    {
        internal void Load()
        {
            Console.WriteLine("Bot.Load()");
        }

        internal void Update(TimeSpan gameTime)
        {
            Console.WriteLine("Bot.Update()");
        }

        protected override void StartGame(ISFSObject gameSession, Room room)
        {
            // Assign Bot player & enemy player
            AssignPlayers(room);

            // Player & Heroes
            ISFSObject objBotPlayer = gameSession.GetSFSObject(botPlayer.displayName);
            ISFSObject objEnemyPlayer = gameSession.GetSFSObject(enemyPlayer.displayName);

            ISFSArray botPlayerHero = objBotPlayer.GetSFSArray("heroes");
            ISFSArray enemyPlayerHero = objEnemyPlayer.GetSFSArray("heroes");

            for (int i = 0; i < botPlayerHero.Size(); i++)
            {
                var hero = new Hero(botPlayerHero.GetSFSObject(i));
                botPlayer.heroes.Add(hero);
            }

            for (int i = 0; i < enemyPlayerHero.Size(); i++)
            {
                enemyPlayer.heroes.Add(new Hero(enemyPlayerHero.GetSFSObject(i)));
            }

            // Gems
            grid = new Grid(gameSession.GetSFSArray("gems"), null, botPlayer.getRecommendGemType());
            currentPlayerId = gameSession.GetInt("currentPlayerId");
            log("StartGame ");

            // SendFinishTurn(true);
            //taskScheduler.schedule(new FinishTurn(true), new Date(System.currentTimeMillis() + delaySwapGem));
            TaskSchedule(delaySwapGem, _ => SendFinishTurn(true));
        }

        protected override void SwapGem(ISFSObject paramz)
        {
            bool isValidSwap = paramz.GetBool("validSwap");
            if (!isValidSwap)
            {
                return;
            }

            HandleGems(paramz);
        }

        protected override void HandleGems(ISFSObject paramz)
        {
            ISFSObject gameSession = paramz.GetSFSObject("gameSession");
            currentPlayerId = gameSession.GetInt("currentPlayerId");
            //get last snapshot
            ISFSArray snapshotSfsArray = paramz.GetSFSArray("snapshots");
            ISFSObject lastSnapshot = snapshotSfsArray.GetSFSObject(snapshotSfsArray.Size() - 1);
            bool needRenewBoard = paramz.ContainsKey("renewBoard");
            // update information of hero
            HandleHeroes(lastSnapshot);
            if (needRenewBoard)
            {
                grid.updateGems(paramz.GetSFSArray("renewBoard"), null);
                TaskSchedule(delaySwapGem, _ => SendFinishTurn(false));
                return;
            }
            // update gem
            grid.gemTypes = botPlayer.getRecommendGemType();

            ISFSArray gemCodes = lastSnapshot.GetSFSArray("gems");
            ISFSArray gemModifiers = lastSnapshot.GetSFSArray("gemModifiers");

            if (gemModifiers != null) log("has gemModifiers");

            grid.updateGems(gemCodes, gemModifiers);

            TaskSchedule(delaySwapGem, _ => SendFinishTurn(false));
        }

        private void HandleHeroes(ISFSObject paramz)
        {
            ISFSArray heroesBotPlayer = paramz.GetSFSArray(botPlayer.displayName);
            for (int i = 0; i < botPlayer.heroes.Count; i++)
            {
                botPlayer.heroes[i].updateHero(heroesBotPlayer.GetSFSObject(i));
            }

            ISFSArray heroesEnemyPlayer = paramz.GetSFSArray(enemyPlayer.displayName);
            for (int i = 0; i < enemyPlayer.heroes.Count; i++)
            {
                enemyPlayer.heroes[i].updateHero(heroesEnemyPlayer.GetSFSObject(i));
            }
        }

        protected override void StartTurn(ISFSObject paramz)
        {
            currentPlayerId = paramz.GetInt("currentPlayerId");
            if (!isBotTurn())
            {
                return;
            }
            FirstHerostrategy();


        }
        public void FirstHerostrategy()
        {
            var supportHero = botPlayer.GetHeroByID(HeroIdEnum.MONK);
            var subDamgerHero = botPlayer.GetHeroByID(HeroIdEnum.FIRE_SPIRIT);
            var mainDamgeHero = botPlayer.GetHeroByID(HeroIdEnum.CERBERUS);
            Hero heroFullMana = botPlayer.anyHeroFullMana();
            if (heroFullMana != null)
            {
                Console.WriteLine(heroFullMana.name+ "hero full Mana");
                var alreadyCastSkill =  DoHeroCastSkillByOrder(mainDamgeHero, subDamgerHero, supportHero);
                if(alreadyCastSkill)
                {
                    return ;
                }
            }
            TaskSchedule(delaySwapGem, _ => SendSwapGem());
        }



          public bool SwapPrioritizeGem(List<GemSwapInfo> listMatchGem, Hero mainAttackHero, Hero subAttackHero, Hero supportHero)
        {
            var gemPrioritizeMainAttack = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
             .Where(gemMatch => gemMatch.type == GemType.BLUE)
             .Where(gemMatch => gemMatch.type == GemType.BROWN)
             .FirstOrDefault();
            if (gemPrioritizeMainAttack != null && mainAttackHero.isAlive())
            {
                return true;
            }

            var gemPrioritizeSubAttack = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
                .Where(gemMatch => gemMatch.type == GemType.RED)
                .Where(gemMatch => gemMatch.type == GemType.PURPLE)
                .FirstOrDefault();
            if (gemPrioritizeSubAttack != null && subAttackHero.isAlive())
            {
                return true;
            }

            var gemPrioritizeMSupport = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
               .Where(gemMatch => gemMatch.type == GemType.YELLOW)
               .Where(gemMatch => gemMatch.type == GemType.GREEN)
               .FirstOrDefault();
            if (gemPrioritizeMSupport != null && supportHero.isAlive())
            {
                return true;
            }

            var gemPrioritizeSword = listMatchGem.Where(gemMatch => gemMatch.sizeMatch > 4)
               .Where(gemMatch => gemMatch.type == GemType.SWORD)
               .FirstOrDefault();
            if (gemPrioritizeSword != null)
            {
                return true;
            }

            return false;
        }

        public bool DoHeroCastSkillByOrder(Hero mainAttackHero, Hero subAttackHero, Hero supportHero)
        {
            List<GemSwapInfo> listMatchGem = grid.suggestMatch();

            GemSwapInfo matchSupportGem = listMatchGem
               .Where(gemMatch => gemMatch.type == GemType.YELLOW || gemMatch.type == GemType.GREEN)
               .FirstOrDefault();

            if (botPlayer.firstHeroAlive().getHeroAttack() >= enemyPlayer.firstHeroAlive().getHeroHP())
            {
                List<GemSwapInfo> matchGemSword1 = listMatchGem.Where(gemMatch => gemMatch.type == GemType.SWORD).ToList();
                if (matchGemSword1 != null && matchGemSword1?.Count > 0)
                {
                    return false;
                }
            }

            var tempGems = new List<Gem>(grid.gems);
            var currentRedGem = tempGems.Where(x => x.type == GemType.RED).Count();
            var enemyHeroByAttack = enemyPlayer.heroes.OrderByDescending(x => x.getHeroAttack()).ToList();

            bool canGetExtraTurn = SwapPrioritizeGem(listMatchGem,mainAttackHero,subAttackHero,supportHero);
            if (canGetExtraTurn)
            {
                return false;
            }

            if (botPlayer.anyHeroFullMana() == null)
            {
                return false;
            }

            var supportWait = false;
            var enemyFireSpirit = enemyPlayer.GetHeroByID(HeroIdEnum.FIRE_SPIRIT);
            if(enemyFireSpirit != null && enemyFireSpirit.isAlive() 
                && enemyFireSpirit.isFullMana()
                && supportHero.isAlive() 
                && supportHero.isFullMana() 
                && supportHero.getHeroAttack() < 9)
            {
                supportWait = true;
            }
            if (supportHero.isAlive() && supportHero.isFullMana() && supportHero.getHeroAttack() < 9 && supportWait == false)
            {
                TaskSchedule(delaySwapGem, _ => SendCastSkill(supportHero));
                return true;
            }
            else if (supportHero.isAlive() == false)
            {
                if(mainAttackHero.isAlive() && mainAttackHero.isFullMana() && mainAttackHero.getHeroAttack() > 8)
                {
                      TaskSchedule(delaySwapGem, _ => SendCastSkill(mainAttackHero));
                    return true;
                }
                if (subAttackHero.isAlive() && subAttackHero.isFullMana())
                {
                    foreach (var hero in enemyHeroByAttack)
                    {
                        if((hero.getHeroAttack() + currentRedGem) >= hero.getHeroHP() && hero.isAlive())
                        {
                           TaskSchedule(delaySwapGem, _ => SendCastSkill(subAttackHero));
                           return true;
                        }
                    }
                 }
                if (mainAttackHero.isAlive() && mainAttackHero.isFullMana())
                {
                    TaskSchedule(delaySwapGem, _ => SendCastSkill(mainAttackHero));
                    return true;
                }
                if(subAttackHero.isAlive() && subAttackHero.isFullMana()){
                      TaskSchedule(delaySwapGem, _ => SendCastSkill(subAttackHero));
                      return true;
                }
            }
            else
            {
                if (mainAttackHero.isAlive() 
                    && mainAttackHero.isFullMana() 
                    && mainAttackHero.getHeroAttack() > 8)
                {
                    TaskSchedule(delaySwapGem, _ => SendCastSkill(mainAttackHero));
                    return true;
                } 
                else if (subAttackHero.isAlive() 
                    && subAttackHero.isFullMana() 
                    && subAttackHero.getHeroAttack() > 8)
                {
                    TaskSchedule(delaySwapGem, _ => SendCastSkill(subAttackHero));
                    return true;
                }
                else if (mainAttackHero.isAlive()
                    && mainAttackHero.isFullMana()
                    && supportHero.getHeroMana(supportHero) <= 3
                    && supportHero.getHeroMana(supportHero) > 0
                    && mainAttackHero.getHeroAttack() <7
                    && matchSupportGem != null)
                {
                    return false;
                }
                else if (subAttackHero.isAlive() 
                    && subAttackHero.isFullMana()
                    && subAttackHero.getHeroAttack() <7
                    && supportHero.getHeroMana(supportHero) <= 3
                    && supportHero.getHeroMana(supportHero) > 0
                    && matchSupportGem != null)
                {
                    return false;
                }
                   else if (subAttackHero.isAlive() && subAttackHero.isFullMana())
                {
                    TaskSchedule(delaySwapGem, _ => SendCastSkill(subAttackHero));
                    return true;
                }
                else if(mainAttackHero.isAlive() && mainAttackHero.isFullMana())
                {
                    TaskSchedule(delaySwapGem, _ => SendCastSkill(mainAttackHero));
                    return true;
                }
                else if(supportWait == true)
                {
                    return false;
                }
                else if(supportHero.isAlive() && supportHero.isFullMana())
                {
                     TaskSchedule(delaySwapGem, _ => SendCastSkill(supportHero));
                    return true;
                }
            }

            

            return false;
        }
    
        protected bool isBotTurn()
        {
            return botPlayer.playerId == currentPlayerId;
        }
    }
}