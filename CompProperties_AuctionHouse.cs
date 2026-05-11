using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class CompProperties_AuctionHouse : CompProperties
    {
        public List<ThingDef> paymentOption;
        public string uiIcon;
        public string uiIconGiveup;

        public float startingPriceMultiplier;

        public IntRange tickDelayForNPCBidder;

        public IntRange tickTillNextAuction = new IntRange(450000, 900000);

        public List<ThingCategoryDef> categoryChoice;

        public float firstThreshold = 1.5f;
        public float chanceOne = 1.0f;

        public float secondThreshold = 1.75f;
        public float chanceTwo = 0.3f;

        public float thirdThreshold = 2.0f;
        public float chanceThree = 0.15f;

        public float fourthThreshold = 3.0f;
        public float chanceFour = 0.1f;

        public float chanceFive = 0.05f;
        
        public Texture2D uiIconTexture;
        public Texture2D uiIconGiveupTexture;
        public Texture2D uiIconCancel;
        public CompProperties_AuctionHouse()
        {
            compClass = typeof(CompAuctionHouse);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                uiIconTexture = ContentFinder<Texture2D>.Get(uiIcon);
                uiIconGiveupTexture = ContentFinder<Texture2D>.Get(uiIconGiveup);
                uiIconCancel = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
            });
        }
    }
}
