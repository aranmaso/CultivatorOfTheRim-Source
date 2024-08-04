using RimWorld;
using System.Text;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Verse.Sound;
using UnityEngine.Assertions.Must;

namespace CultivatorOfTheRim
{
    public class CompAuctionHouse : ThingComp
    {
        public int coolDown = 900000;

        public int tickSinceAuction = 0;

        public int nextAuctionDay;

        public int auctionDuration = 5000;

        public int itemAuctionTimer = 0;

        public bool auctionInProgress = false;

        public bool isSellingPawn = false;

        public List<ThingCategoryDef> thingCategoryDefChoice => Props.categoryChoice;
        /*{
            ThingCategoryDefOf.Apparel,
            ThingCategoryDefOf.Weapons,
            ThingCategoryDefOf.Manufactured,
            ThingCategoryDefOf.Medicine,
            ThingCategoryDefOf.Neurotrainers,
            ThingCategoryDefOf.Drugs,
            RimAuction_DefOf.Artifacts,
            ThingCategoryDefOf.Buildings,
            ThingCategoryDefOf.BuildingsArt,
            ThingCategoryDefOf.BuildingsSpecial
        };*/

        public ThingCategoryDef choosenCategoryDef = ThingCategoryDefOf.Weapons;

        public string choosenCategoryDefString
        {
            get
            {
                if (choosenCategoryDef != null)
                {
                    return choosenCategoryDef.LabelCap.ToString();
                }
                return "none";
            }
        }

        public IEnumerable<int> raiseOption = new List<int>() { 10, 20, 50, 100, 200, 1000 };

        public IEnumerable<ThingDef> paymentOption => Props.paymentOption;

        public ThingDef choosenPaymentOption = ThingDefOf.Silver;

        public IDictionary<int, float> raiseChanceForRando = new Dictionary<int, float>()
        {
            {100, 0.50f },
            {200, 0.40f },
            {400, 0.09f },
            {800, 0.006f },
            {1000, 0.004f }
        };

        public int choosenRaiseOption = 0;

        public List<ThingDef> currentAuctionItemList = new List<ThingDef>();

        public Thing currentItemOnAuction;

        public int currentBid;

        public Pawn bidder;

        public Pawn choosenBidder;

        public bool isLastItem = false;

        public bool isBonusItem = false;

        public bool isYourItem = false;

        public int tickUntilNextBidder = 250;

        public int tickSinceLastRandoBid = 0;
        public Thing sellingItem;

        public CompProperties_AuctionHouse Props => (CompProperties_AuctionHouse)props;
        public override void PostPostMake()
        {
            base.PostPostMake();
            if (tickSinceAuction <= 0)
            {
                //tickSinceAuction = Rand.Range(coolDown / 2, coolDown);
                //nextAuctionDay = Find.TickManager.TicksGame + Rand.Range(coolDown / 2, coolDown);
                nextAuctionDay = Find.TickManager.TicksGame + Props.tickTillNextAuction.RandomInRange;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref nextAuctionDay, "nextAuctionDay", 0);
            Scribe_Values.Look(ref itemAuctionTimer, "itemAuctionTimer", 0);
            Scribe_Values.Look(ref tickUntilNextBidder, "tickUntilNextBidder", 0);
            Scribe_Values.Look(ref tickSinceLastRandoBid, "tickSinceLastRandoBid", 0);
            Scribe_Values.Look(ref auctionInProgress, "auctionInProgress", false);
            Scribe_Values.Look(ref isLastItem, "isLastItem", false);
            Scribe_Values.Look(ref isBonusItem, "isBonusItem", false);
            Scribe_Values.Look(ref isYourItem, "isYourItem", false);
            Scribe_Values.Look(ref isSellingPawn, "isSellingPawn", false);
            if (currentItemOnAuction is Pawn)
            {
                Scribe_References.Look(ref currentItemOnAuction, "currentItemOnAuction", true);
            }
            else
            {
                Scribe_Deep.Look(ref currentItemOnAuction, "currentItemOnAuction");
            }

            Scribe_Collections.Look(ref currentAuctionItemList, "currentAuctionItemList", LookMode.Def);
            Scribe_Values.Look(ref currentBid, "currentBid", 0);
            Scribe_Values.Look(ref choosenRaiseOption, "currenchoosenRaiseOptiontBid", 100);
            Scribe_Defs.Look(ref choosenPaymentOption, "choosenPaymentOption");
            Scribe_Defs.Look(ref choosenCategoryDef, "choosenCategoryDef");
            Scribe_References.Look(ref bidder, "bidder", saveDestroyedThings: true);
            if (currentItemOnAuction is Pawn)
            {
                Scribe_References.Look(ref sellingItem, "sellingItem", true);
            }
            else
            {
                Scribe_Deep.Look(ref sellingItem, "sellingItem");
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            /*if (tickSinceAuction > 0)
            {
                tickSinceAuction--;
            }*/
            if (Find.TickManager.TicksGame >= nextAuctionDay && !auctionInProgress)
            {
                StartAuctionEvent();
            }

            if (itemAuctionTimer > 0)
            {
                tickSinceLastRandoBid++;
                if (tickUntilNextBidder > 0)
                {
                    tickUntilNextBidder--;
                }
                int num = 0;
                if (currentItemOnAuction is not Pawn)
                {
                    if (currentItemOnAuction.stackCount > 1)
                    {
                        num = Mathf.RoundToInt(currentItemOnAuction.MarketValue * currentItemOnAuction.stackCount);
                    }
                    else
                    {
                        num = Mathf.RoundToInt(currentItemOnAuction.MarketValue);
                    }
                }
                if (tickUntilNextBidder <= 0 && itemAuctionTimer > 1)
                {
                    tickUntilNextBidder = Props.tickDelayForNPCBidder.RandomInRange;
                    if (currentBid <= 0)
                    {
                        if (currentItemOnAuction.stackCount > 1)
                        {
                            currentBid += Mathf.RoundToInt((currentItemOnAuction.MarketValue * Props.startingPriceMultiplier) * currentItemOnAuction.stackCount);
                        }
                        else
                        {
                            currentBid += Mathf.FloorToInt(currentItemOnAuction.MarketValue * Props.startingPriceMultiplier);
                        }

                    }
                    if (currentBid < num * Props.firstThreshold)
                    {
                        if (Rand.Value < Props.chanceOne)
                        {
                            RandomPawnBid();
                            tickSinceLastRandoBid = 0;
                        }
                    }
                    else if (currentBid >= num * Props.firstThreshold && currentBid < num * Props.secondThreshold)
                    {
                        if (Rand.Value < Props.chanceTwo)
                        {
                            RandomPawnBid();
                            tickSinceLastRandoBid = 0;
                        }
                    }
                    else if (currentBid >= num * Props.secondThreshold && currentBid < num * Props.thirdThreshold)
                    {
                        if (Rand.Value < Props.chanceThree)
                        {
                            RandomPawnBid();
                            tickSinceLastRandoBid = 0;
                        }
                    }
                    else if (currentBid >= num * Props.thirdThreshold && currentBid < num * Props.fourthThreshold)
                    {
                        if (Rand.Value < Props.chanceFour)
                        {
                            RandomPawnBid();
                            tickSinceLastRandoBid = 0;
                        }
                    }
                    else if (currentBid >= num * Props.fourthThreshold)
                    {
                        if (Rand.Value < Props.chanceFive)
                        {
                            RandomPawnBid();
                            tickSinceLastRandoBid = 0;
                        }
                    }

                }
                itemAuctionTimer--;
            }
            if (itemAuctionTimer <= 0 && auctionInProgress)
            {
                if (bidder != null && bidder.Faction.IsPlayer)
                {
                    CalculatePayment(choosenPaymentOption, bidder, currentBid);
                }
                else if (!currentAuctionItemList.NullOrEmpty())
                {
                    currentItemOnAuction.Destroy();
                    GetNewAuctionItem();
                }
                else if (currentAuctionItemList.NullOrEmpty() && isLastItem)
                {
                    if (sellingItem != null)
                    {
                        if (!isYourItem)
                        {
                            isYourItem = true;
                            GetNewAuctionItem();
                        }
                        else
                        {
                            CalculateReward(choosenPaymentOption, currentBid);
                        }
                    }
                    else// if (currentAuctionItemList.NullOrEmpty() && isLastItem && sellingItem == null)
                    {
                        currentItemOnAuction = null;
                        isYourItem = false;
                        auctionInProgress = false;
                        currentAuctionItemList.Clear();
                        //tickSinceAuction = Rand.Range(coolDown / 2, coolDown);
                        nextAuctionDay = Find.TickManager.TicksGame + Rand.Range(coolDown / 2, coolDown);
                    }

                }

            }
        }
        public void StartAuctionEvent()
        {
            GetAuctionItemList();
            if (currentItemOnAuction == null)
            {
                CreateAuctionItem();
            }
            currentBid = Mathf.FloorToInt((currentItemOnAuction.MarketValue * currentItemOnAuction.stackCount) * 0.75f);
            itemAuctionTimer = auctionDuration;
            auctionInProgress = true;
        }
        public void RandomPawnBid()
        {
            IEnumerable<Faction> allFactions = Find.World.factionManager.AllFactions;
            Pawn leader = allFactions.Where(x => !x.IsPlayer && x.leader != bidder && !x.Hidden && !x.temporary).RandomElement().leader;
            int num = raiseChanceForRando.RandomElementByWeight(x => x.Value).Key;
            currentBid += num;
            bidder = leader;
            /*Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction faction, false);
            Pawn leader = faction.leader;
            if(bidder != leader)
            {
                int num = raiseChanceForRando.RandomElementByWeight(x => x.Value).Key;
                currentBid += num;
                bidder = leader;
            }*/
        }
        public void CreateAuctionItem()
        {
            ThingDef tempThingDef = currentAuctionItemList.RandomElement();
            Thing thing = null;
            bool isBuilding = false;
            if (tempThingDef.thingCategories.Contains(ThingCategoryDefOf.Buildings) ||
                tempThingDef.thingCategories.Contains(ThingCategoryDefOf.BuildingsArt) ||
                tempThingDef.thingCategories.Contains(ThingCategoryDefOf.BuildingsSpecial))
            {
                isBuilding = true;
            }
            if ((tempThingDef.IsWithinCategory(ThingCategoryDefOf.Weapons) || tempThingDef.IsWithinCategory(ThingCategoryDefOf.Apparel) || isBuilding) && tempThingDef.MadeFromStuff)
            {
                thing = ThingMaker.MakeThing(tempThingDef, DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsStuff && x.IsMetal).RandomElement());
            }
            else
            {
                thing = ThingMaker.MakeThing(tempThingDef);
            }
            thing.stackCount = Mathf.Clamp(thing.def.stackLimit, 1, 250);
            if (thing.def.IsWithinCategory(CTR_DefOf.Artifacts) || thing.def.IsWithinCategory(ThingCategoryDefOf.BodyParts) || thing.def.IsWithinCategory(ThingCategoryDefOf.Items))
            {
                thing.stackCount = Rand.Range(1, 10);
            }
            if (thing.TryGetComp<CompQuality>() != null)
            {
                thing.TryGetComp<CompQuality>().SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
            }
            currentItemOnAuction = thing;
            currentAuctionItemList.Remove(thing.def);
            if (currentAuctionItemList.Count == 0)
            {
                isLastItem = true;
            }
        }
        public void GetNewAuctionItem()
        {
            if (!isYourItem)
            {
                //currentItemOnAuction.Destroy();
                CreateAuctionItem();
                currentBid = Mathf.FloorToInt(currentItemOnAuction.MarketValue * currentItemOnAuction.stackCount);

            }
            else
            {
                //currentItemOnAuction.Destroy();
                currentItemOnAuction = sellingItem;
                int num = 0;
                do
                {
                    try
                    {
                        if (!isSellingPawn)
                        {
                            currentBid = Mathf.FloorToInt(currentItemOnAuction.MarketValue * currentItemOnAuction.stackCount);
                        }
                        else
                        {
                            currentBid = Mathf.FloorToInt(currentItemOnAuction.MarketValue);
                        }

                    }
                    catch (Exception)
                    {
                    }
                    num++;
                }
                while (num <= 100 && currentBid <= 0);
                //int num = Mathf.FloorToInt(currentItemOnAuction.MarketValue * currentItemOnAuction.stackCount);
                //currentBid = 0;
            }
            bidder = null;
            currentBid = Mathf.FloorToInt(currentItemOnAuction.MarketValue * currentItemOnAuction.stackCount);
            itemAuctionTimer = auctionDuration;

        }
        public void GetAuctionItemList()
        {
            if (!currentAuctionItemList.NullOrEmpty())
            {
                currentAuctionItemList.Clear();
            }
            IEnumerable<ThingDef> source = (from thingDef in DefDatabase<ThingDef>.AllDefs
                                            where IsValid(thingDef)
                                            select thingDef).ToList();
            int num = 5;
            num = Rand.Range(5, 8);
            for (int i = 0; i < num; i++)
            {
                currentAuctionItemList.Add(source.RandomElement());
            }
            if (Rand.Value < 0.25f)
            {
                isBonusItem = true;
                currentAuctionItemList.Add(source.RandomElement());
            }
            else
            {
                isBonusItem = false;
            }
            string text = "Auction Day";
            string text2 = "an announcement from global Auction House. an auction session has begin!";
            if (isBonusItem)
            {
                text2 += "\n" + "today auction got a extra mystery item";
            }
            Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent,parent);
            /*foreach(var item in source.Where(x => !currentAuctionItemList.Contains(x)).InRandomOrder())
            {
                currentAuctionItemList.Add(item);
                num++;
                if(num >= 5)
                {
                    break;
                }
            }*/
            isLastItem = false;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (auctionInProgress)
            {
                Command_Action raiseBid = new Command_Action();
                raiseBid.defaultLabel = "raise bid by: " + choosenRaiseOption;
                raiseBid.defaultDesc = "raise bid by: " + choosenRaiseOption;
                if (isBonusItem && isLastItem)
                {
                    raiseBid.icon = ContentFinder<Texture2D>.Get(Props.uiIcon);
                }
                else
                {
                    if (currentItemOnAuction is Pawn)
                    {
                        raiseBid.icon = Widgets.GetIconFor((Pawn)currentItemOnAuction, currentItemOnAuction.DrawSize, Rot4.South, true, out var scale, out var angle, out var IconProp, out var color);
                    }
                    else
                    {
                        raiseBid.icon = currentItemOnAuction != null ? Widgets.GetIconFor(currentItemOnAuction.def) : ContentFinder<Texture2D>.Get(Props.uiIcon);
                    }

                }
                raiseBid.action = delegate
                {
                    if (choosenBidder != null && choosenRaiseOption > 0 && tickSinceLastRandoBid >= 120 && !isYourItem)
                    {
                        bidder = choosenBidder;
                        currentBid += choosenRaiseOption;
                    }
                    else
                    {
                        if (isYourItem)
                        {
                            Messages.Message("can't bid on your own item", MessageTypeDefOf.NeutralEvent);
                        }
                        if (choosenBidder == null)
                        {
                            Messages.Message("please choose bidder", MessageTypeDefOf.NeutralEvent);
                        }
                        if (choosenRaiseOption <= 0)
                        {
                            Messages.Message("please choose bid amount", MessageTypeDefOf.NeutralEvent);
                        }
                        if (tickSinceLastRandoBid < 120)
                        {
                            Messages.Message("can't bid within 2 second of last bid", MessageTypeDefOf.NeutralEvent);
                        }
                    }
                };
                yield return raiseBid;

                Command_Action chooseBidder = new Command_Action();
                chooseBidder.defaultLabel = "Choose Bidder";
                chooseBidder.icon = ContentFinder<Texture2D>.Get(Props.uiIcon);
                chooseBidder.action = delegate
                {
                    List<FloatMenuOption> choice = new List<FloatMenuOption>();
                    IEnumerable<Pawn> colonist = parent.Map.mapPawns.FreeColonists.Where(x => !x.Dead && !x.Downed && !x.IsSlave && !x.IsPrisoner); ;
                    foreach (var option in colonist)
                    {
                        choice.Add(new FloatMenuOption(option.Name.ToStringShort, delegate
                        {
                            chooseBidder.defaultLabel = option.Name.ToStringFull.ToString();
                            choosenBidder = option;

                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(choice));
                };
                yield return chooseBidder;

                Command_Action chooseBidOption = new Command_Action();
                chooseBidOption.defaultLabel = "bid: " + choosenRaiseOption;
                chooseBidOption.defaultDesc = "bid: " + choosenRaiseOption;
                chooseBidOption.icon = Widgets.GetIconFor(ThingDefOf.Silver);
                chooseBidOption.action = delegate
                {
                    List<FloatMenuOption> choice = new List<FloatMenuOption>();
                    foreach (int option in raiseOption)
                    {
                        choice.Add(new FloatMenuOption(option.ToString() + " Silver", delegate
                        {
                            chooseBidOption.defaultLabel = "Bid: " + option;
                            choosenRaiseOption = option;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(choice));
                };
                yield return chooseBidOption;

                Command_Action choosePaymentOption = new Command_Action();
                choosePaymentOption.defaultLabel = "payment: " + choosenPaymentOption.LabelCap;
                choosePaymentOption.defaultDesc = "payment: " + choosenPaymentOption.LabelCap;
                choosePaymentOption.icon = choosenPaymentOption != null ? Widgets.GetIconFor(choosenPaymentOption) : Widgets.GetIconFor(ThingDefOf.Silver);
                choosePaymentOption.action = delegate
                {
                    List<FloatMenuOption> choice = new List<FloatMenuOption>();
                    foreach (var option in paymentOption)
                    {
                        choice.Add(new FloatMenuOption(option.LabelCap, delegate
                        {
                            chooseBidOption.defaultLabel = "payment: " + option.LabelCap;
                            choosenPaymentOption = option;
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(choice));
                };
                yield return choosePaymentOption;

                Command_Action giveup = new Command_Action();
                giveup.defaultLabel = "give up current Item";
                giveup.defaultDesc = "give up current Item";
                giveup.icon = ContentFinder<Texture2D>.Get(Props.uiIconGiveup);
                giveup.action = delegate
                {
                    if (isYourItem)
                    {
                        Messages.Message("can't give uo your own item", MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        if (bidder != null && bidder.Faction.IsPlayer)
                        {
                            bidder = null;
                            itemAuctionTimer = 1;
                        }
                        else
                        {
                            itemAuctionTimer = 1;
                        }
                    }

                };
                yield return giveup;
            }
            else
            {

                Command_Action startNowPayCost = new Command_Action();
                startNowPayCost.defaultLabel = "pay 2500 spirit stone to start Auction";
                startNowPayCost.defaultDesc = "by paying 2500 spirit stone to start auction now";
                startNowPayCost.icon = ContentFinder<Texture2D>.Get(Props.uiIcon);
                startNowPayCost.action = delegate
                {
                    if (ColonyHasEnoughMoney(CTR_DefOf.CTR_SpiritStone, parent.Map, 2500))
                    {
                        nextAuctionDay = Find.TickManager.TicksGame + 60;
                        TradeUtility.LaunchThingsOfType(CTR_DefOf.CTR_SpiritStone, 2500, parent.Map, null);
                    }
                    else
                    {
                        Messages.Message("not enough silver", MessageTypeDefOf.NeutralEvent);
                    }
                };
                yield return startNowPayCost;

                Command_Action chooseCategory = new Command_Action();
                chooseCategory.defaultLabel = "Category: " + choosenCategoryDefString;
                //chooseCategory.defaultDesc = "Category: " + "none";
                chooseCategory.icon = ContentFinder<Texture2D>.Get(Props.uiIcon); ;
                chooseCategory.action = delegate
                {
                    List<FloatMenuOption> choice = new List<FloatMenuOption>();
                    foreach (var option in thingCategoryDefChoice)
                    {
                        choice.Add(new FloatMenuOption(option.LabelCap, delegate
                        {
                            choosenCategoryDef = option;
                        }));
                    }
                    choice.Add(new FloatMenuOption("none", delegate
                    {
                        choosenCategoryDef = null;
                    }));
                    Find.WindowStack.Add(new FloatMenu(choice));
                };
                yield return chooseCategory;

                yield return new Command_Action
                {
                    defaultLabel = "Sell Item",
                    defaultDesc = "Put your item on auction",
                    icon = ContentFinder<Texture2D>.Get(Props.uiIcon),
                    action = delegate
                    {
                        Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo t)
                        {

                            if (!isSellingPawn)
                            {
                                ThingDef tempThingDef = t.Thing.def;
                                if (IsSellItemValid(tempThingDef))
                                {
                                    if (sellingItem == null)
                                    {
                                        Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(t.Cell, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(t.Cell, parent.Map);
                                        effecter.Cleanup();
                                        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.Map));
                                        if (t.Thing is MinifiedThing minifiedThing)
                                        {
                                            sellingItem = minifiedThing.InnerThing;
                                            minifiedThing.Destroy();
                                        }
                                        else
                                        {
                                            sellingItem = t.Thing;
                                        }
                                        if (sellingItem.Spawned)
                                        {
                                            sellingItem.DeSpawn();
                                        }
                                    }
                                    else
                                    {
                                        t.Thing.DeSpawn();
                                        GenPlace.TryPlaceThing(sellingItem, t.Cell, parent.Map, ThingPlaceMode.Near);
                                        Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(t.Cell, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(t.Cell, parent.Map);
                                        effecter.Cleanup();
                                        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.Map));
                                        if (t.Thing is MinifiedThing minifiedThing)
                                        {
                                            sellingItem = minifiedThing.InnerThing;
                                            minifiedThing.Destroy();
                                        }
                                        else
                                        {
                                            sellingItem = t.Thing;
                                        }
                                        if (sellingItem.Spawned)
                                        {
                                            sellingItem.DeSpawn();
                                        }
                                    }

                                }
                                else
                                {
                                    sellingItem = null;
                                    Messages.Message("not a valid item", MessageTypeDefOf.NeutralEvent);
                                }
                            }
                            else
                            {
                                Pawn p = t.Pawn;
                                if (p.IsSlave || p.IsPrisoner || (p.AnimalOrWildMan() && p.Faction == Faction.OfPlayer))
                                {
                                    if (sellingItem == null)
                                    {
                                        Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(t.Cell, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(t.Cell, parent.Map);
                                        effecter.Cleanup();
                                        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.Map));
                                        sellingItem = t.Thing;
                                        sellingItem.DeSpawn();
                                    }
                                    else
                                    {
                                        GenPlace.TryPlaceThing(sellingItem, parent.Position.RandomAdjacentCell8Way(), parent.Map, ThingPlaceMode.Near);
                                        Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(t.Cell, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(t.Cell, parent.Map);
                                        effecter.Cleanup();
                                        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.Map));
                                        sellingItem = t.Thing;
                                        sellingItem.DeSpawn();
                                    }
                                }
                                else
                                {
                                    sellingItem = null;
                                    Messages.Message("not a valid target", MessageTypeDefOf.NeutralEvent);
                                }
                            }


                        });
                    }
                };

                if (sellingItem != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Eject Content",
                        defaultDesc = "eject current colony item that was put on auction",
                        icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
                        action = delegate
                        {
                            GenPlace.TryPlaceThing(sellingItem, parent.Position.RandomAdjacentCell8Way(), parent.Map, ThingPlaceMode.Near);
                            sellingItem = null;
                        }
                    };
                }
                Command_Toggle sellingPawnToggle = new Command_Toggle();
                if (isSellingPawn)
                {
                    sellingPawnToggle.defaultLabel = "Pawn Selling Mode";
                    sellingPawnToggle.defaultDesc = "colony is auctioning away prisoner or slave";
                }
                else
                {
                    sellingPawnToggle.defaultLabel = "Non-Pawn Selling Mode";
                    sellingPawnToggle.defaultDesc = "colony is no longer auctioning away prisoner or slave";
                }
                sellingPawnToggle.icon = ContentFinder<Texture2D>.Get(Props.uiIcon);
                sellingPawnToggle.isActive = () => isSellingPawn;
                sellingPawnToggle.toggleAction = delegate
                {
                    isSellingPawn = !isSellingPawn;
                };
                yield return sellingPawnToggle;
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "start auction now",
                    action = delegate
                    {
                        tickSinceAuction = 60;
                        nextAuctionDay = Find.TickManager.TicksGame + 60;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "skip current item",
                    action = delegate
                    {
                        itemAuctionTimer = 60;
                    }
                };
            }
        }
        public TargetingParameters GetTargetingParameters()
        {
            if (!isSellingPawn)
            {
                return new TargetingParameters
                {
                    canTargetPawns = false,
                    canTargetAnimals = false,
                    canTargetBuildings = false,
                    canTargetItems = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    validator = (TargetInfo x) => x.Thing is not Pawn
                };
            }
            else
            {
                return new TargetingParameters
                {
                    canTargetPawns = true,
                    canTargetAnimals = true,
                    canTargetBuildings = false,
                    canTargetItems = false,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    validator = (TargetInfo x) => x.Thing is Pawn
                };
            }

        }
        private bool IsSellItemValid(ThingDef thingDef)
        {
            if (thingDef.category == ThingCategory.Item)
            {
                if (paymentOption.Contains(thingDef))
                {
                    return false;
                }
                if (thingDef.IsCorpse)
                {
                    return false;
                }
                /*if (thingDef.MadeFromStuff)
                {
                    return false;
                }*/
                if (thingDef.IsEgg)
                {
                    return false;
                }
                /*if (thingDef.IsRawFood() || thingDef.IsProcessedFood)
                {
                    return false;
                }*/
                if (thingDef.destroyOnDrop)
                {
                    return false;
                }
                if (thingDef.isUnfinishedThing)
                {
                    return false;
                }
                if (thingDef.tradeTags != null && thingDef.tradeTags.Any((string tag) => tag.Contains("CE") && tag.Contains("Ammo")))
                {
                    return false;
                }
                if (choosenCategoryDef != null && !thingDef.IsWithinCategory(choosenCategoryDef))
                {
                    return false;
                }
                if (choosenCategoryDef == ThingCategoryDefOf.Buildings || choosenCategoryDef == ThingCategoryDefOf.BuildingsArt || choosenCategoryDef == ThingCategoryDefOf.BuildingsSpecial || choosenCategoryDef == null)
                {
                    if (thingDef == ThingDefOf.MinifiedThing)
                    {
                        return true;
                    }
                }
                if (thingDef.BaseMarketValue <= 0)
                {
                    return false;
                }
                return true;
            }
            else if (thingDef.category == ThingCategory.Building)
            {
                if (thingDef.minifiedDef == null)
                {
                    return false;
                }
                if (choosenCategoryDef != null && !thingDef.IsWithinCategory(choosenCategoryDef))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        private bool IsValid(ThingDef thingDef)
        {
            if (thingDef.category == ThingCategory.Item)
            {
                if (paymentOption.Contains(thingDef))
                {
                    return false;
                }
                if (thingDef.IsCorpse)
                {
                    return false;
                }
                /*if (thingDef.MadeFromStuff)
                {
                    return false;
                }*/
                if (thingDef.IsEgg)
                {
                    return false;
                }
                /*if (thingDef.IsRawFood() || thingDef.IsProcessedFood)
                {
                    return false;
                }*/
                if (thingDef.destroyOnDrop)
                {
                    return false;
                }
                if (thingDef.isUnfinishedThing)
                {
                    return false;
                }
                if (thingDef.tradeTags != null && thingDef.tradeTags.Any((string tag) => tag.Contains("CE") && tag.Contains("Ammo")))
                {
                    return false;
                }
                if (choosenCategoryDef != null && !thingDef.IsWithinCategory(choosenCategoryDef))
                {
                    return false;
                }
                if (thingDef.BaseMarketValue <= 0)
                {
                    return false;
                }
                return true;
            }
            else if (thingDef.category == ThingCategory.Building)
            {
                if (thingDef.minifiedDef == null)
                {
                    return false;
                }
                if (choosenCategoryDef != null && !thingDef.IsWithinCategory(choosenCategoryDef))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (Find.TickManager.TicksGame < nextAuctionDay)
            {
                stringBuilder.AppendLine("time until next auction: " + (nextAuctionDay - Find.TickManager.TicksGame).ToStringTicksToPeriod(true, true, true, true));
                if (choosenCategoryDef != null)
                {
                    stringBuilder.AppendLine("prefer item category: " + choosenCategoryDef.LabelCap);
                }
                else
                {
                    stringBuilder.AppendLine("prefer item category: " + "none");
                }
                if (sellingItem != null)
                {
                    stringBuilder.AppendLine("selling item: " + sellingItem.LabelCap);
                }
            }
            if (auctionInProgress)
            {
                stringBuilder.AppendLine("Item left: " + currentAuctionItemList.Count);
                if (currentItemOnAuction != null)
                {
                    if (!isBonusItem)
                    {
                        stringBuilder.AppendLine("current Item: " + currentItemOnAuction.LabelCap);
                    }
                    else if (isBonusItem)
                    {
                        if (isLastItem && !isYourItem)
                        {
                            stringBuilder.AppendLine("current Item: " + "???");
                        }
                        else
                        {
                            stringBuilder.AppendLine("current Item: " + currentItemOnAuction.LabelCap);
                        }
                    }
                }
                if (isYourItem)
                {
                    stringBuilder.AppendLine("your item is on auction");
                }
                if (bidder != null)
                {
                    stringBuilder.AppendLine("Bidder: " + bidder.Name.ToStringFull);
                }
                else
                {
                    stringBuilder.AppendLine("Bidder: no bidder");
                }
                if (tickSinceLastRandoBid >= 120)
                {
                    stringBuilder.AppendLine("you can now bid");
                }
                else
                {
                    stringBuilder.AppendLine("someone else bidded: " + tickSinceLastRandoBid.ToStringTicksToPeriod(true));
                }
                stringBuilder.AppendLine("current Bid: " + currentBid + " Silver" + "(" + Mathf.FloorToInt(currentBid / choosenPaymentOption.BaseMarketValue) + " " + choosenPaymentOption.LabelCap + ")");
                stringBuilder.AppendLine("time until next item: " + itemAuctionTimer.ToStringTicksToPeriod(true, true, true, true));
                if (Prefs.DevMode)
                {
                    stringBuilder.AppendLine("tickUntilNextBidder: " + tickUntilNextBidder);
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public void CalculatePayment(ThingDef payment, Pawn pawn, int finalBid)
        {
            int finalPrice = Mathf.FloorToInt(finalBid / payment.BaseMarketValue);
            bool enoughMoney = ColonyHasEnoughMoney(payment, pawn.Map, finalPrice);
            if (!enoughMoney)
            {
                Messages.Message("Not Enough " + payment.LabelCap + "!", MessageTypeDefOf.NegativeEvent);
                if (!currentAuctionItemList.NullOrEmpty())
                {
                    GetNewAuctionItem();
                }
                else
                {
                    currentItemOnAuction = null;
                    auctionInProgress = false;
                    currentAuctionItemList.Clear();
                    nextAuctionDay = Find.TickManager.TicksGame + Rand.Range(coolDown / 2, coolDown);
                }
                return;
            }
            else
            {
                TradeUtility.LaunchThingsOfType(payment, finalPrice, pawn.Map, null);
                SpawnDropPod(pawn.Map, currentItemOnAuction);
                if (!currentAuctionItemList.NullOrEmpty())
                {
                    GetNewAuctionItem();
                }
                else
                {
                    currentItemOnAuction = null;
                    auctionInProgress = false;
                    currentAuctionItemList.Clear();
                    nextAuctionDay = Find.TickManager.TicksGame + Rand.Range(coolDown / 2, coolDown);
                }
                return;
            }

            //GenSpawn.Spawn(currentItemOnAuction, parent.Position, parent.Map);            
        }

        public void CalculateReward(ThingDef payment, int finalBid)
        {
            if (!currentItemOnAuction.DestroyedOrNull())
            {
                currentItemOnAuction.Destroy();
                sellingItem = null;
            }

            int finalPrice = Mathf.FloorToInt(finalBid / payment.BaseMarketValue);
            List<Thing> moneys = new List<Thing>();
            int num = finalPrice;
            while (num > 0)
            {
                Thing rew = ThingMaker.MakeThing(payment);
                rew.stackCount = Mathf.Min(payment.stackLimit, num);
                num -= rew.stackCount;
                moneys.Add(rew);
            }
            foreach (Thing rew in moneys)
            {
                SpawnDropPod(parent.Map, rew);
            }
            currentItemOnAuction = null;
            auctionInProgress = false;
            isYourItem = false;
            currentAuctionItemList.Clear();
            nextAuctionDay = Find.TickManager.TicksGame + Rand.Range(coolDown / 2, coolDown);
            return;
        }
        public static bool ColonyHasEnoughMoney(ThingDef thing, Map map, int fee)
        {
            return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
                    where t.def == thing
                    select t).Sum((Thing t) => t.stackCount) >= fee;
        }
        public void SpawnDropPod(Map map, Thing t)
        {
            Thing newThing = t;
            if (newThing.def.CanHaveFaction)
            {
                newThing.SetFaction(Faction.OfPlayer);
            }
            newThing = newThing.TryMakeMinified();
            IntVec3 dropSpot = parent.Position;
            IEnumerable<Building_OrbitalTradeBeacon> beaconList = Building_OrbitalTradeBeacon.AllPowered(map).Where(x => !x.Position.Roofed(map));
            if (!beaconList.EnumerableNullOrEmpty())
            {
                foreach (var item in Building_OrbitalTradeBeacon.AllPowered(map).InRandomOrder())
                {
                    dropSpot = item.TradeableCells.RandomElement();
                    break;
                }
            }
            else
            {
                IEnumerable<IntVec3> droplist = GenRadial.RadialCellsAround(parent.Position, 10f, true).Where(x => !x.Roofed(map) && map.reachability.CanReachColony(x));
                if (!droplist.EnumerableNullOrEmpty())
                {
                    dropSpot = droplist.RandomElement();
                }
                if (droplist.EnumerableNullOrEmpty())
                {
                    foreach (var item in Building_OrbitalTradeBeacon.AllPowered(map).InRandomOrder())
                    {
                        if (item.Position.GetRoof(map) == RoofDefOf.RoofRockThick)
                        {
                            continue;
                        }
                        dropSpot = item.TradeableCells.RandomElement();
                        break;
                    }
                }
            }

            /*if (dropSpot == null)
            {
                dropSpot = map.AllCells.Where(x => !x.Roofed(map) && x.InBounds(map) && !x.Impassable(map) && !x.Fogged(map) && map.reachability.CanReachColony(x)).RandomElement();
            }*/
            if (dropSpot != parent.Position)
            {
                ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                activeDropPodInfo.SingleContainedThing = newThing;
                activeDropPodInfo.leaveSlag = false;
                DropPodUtility.MakeDropPodAt(dropSpot, map, activeDropPodInfo);
            }
            else
            {
                foreach (var item in Building_OrbitalTradeBeacon.AllPowered(map).InRandomOrder())
                {
                    dropSpot = item.TradeableCells.RandomElement();
                    break;
                }
                if (dropSpot == null)
                {
                    dropSpot = parent.Position.RandomAdjacentCell8Way();
                }
                GenPlace.TryPlaceThing(newThing, dropSpot, parent.Map, ThingPlaceMode.Near);
                Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(dropSpot, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(dropSpot, parent.Map);
                effecter.Cleanup();
                if (ModsConfig.RoyaltyActive)
                {
                    SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(dropSpot, parent.Map));
                }
            }
        }
    }
}
