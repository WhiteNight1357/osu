// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAccuracyWeightAdjust : Mod
    {
        public override string Name => @"Accuracy Weight Adjust";

        public override LocalisableString Description => @"Change accuracy weight of judgements.";

        public override string Acronym => "AW";

        public override ModType Type => ModType.Conversion;

        public override IconUsage? Icon => null;

        public override double ScoreMultiplier => 1;

        public override bool RequiresConfiguration => true;

        public BindableDouble WeightAdjustedAccuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1.01 };
        public BindableDouble MinimumWeightAdjustedAccuracy = new BindableDouble(0) { MinValue = 0, MaxValue = 1.01 };
        public BindableDouble MaximumWeightAdjustedAccuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1.01 };

        protected Dictionary<HitResult, JudgementCount> Results = new Dictionary<HitResult, JudgementCount>();

        protected List<JudgementCount> Counters = new List<JudgementCount>();

        protected struct JudgementCount
        {
            public HitResult[] Types { get; set; }

            public BindableInt ResultCount { get; set; }
        }
    }
}
