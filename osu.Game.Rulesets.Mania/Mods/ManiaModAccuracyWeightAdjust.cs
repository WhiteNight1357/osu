// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;


namespace osu.Game.Rulesets.Mania.Mods
{
    public partial class ManiaModAccuracyWeightAdjust : ModAccuracyWeightAdjust, IApplicableToScoreProcessor
    {
        public override Type[] IncompatibleMods => [typeof(ModAccuracyChallenge)];

        public override string SettingDescription => $"Perfect:{PerfectWeight.Value * 100}%, Great:{GreatWeight.Value * 100}%, Good:{GoodWeight.Value * 100}%, Ok:{OkWeight.Value * 100}%, Meh:{MehWeight.Value * 100}%, Miss:{MissWeight.Value * 100}%";

        [SettingSource("Perfect", "Adjust accuracy weight of judgement PERFECT.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> PerfectWeight { get; } = new BindableDouble
        {
            MinValue = 0.00,
            MaxValue = 1.01,
            Precision = 0.01,
            Default = 1.0,
            Value = 1.0,
        };

        [SettingSource("Great", "Adjust accuracy weight of judgement GREAT.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> GreatWeight { get; } = new BindableDouble
        {
            MinValue = 0.00,
            MaxValue = 1.01,
            Precision = 0.01,
            Default = 0.98,
            Value = 0.98,
        };

        [SettingSource("Good", "Adjust accuracy weight of judgement GOOD.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> GoodWeight { get; } = new BindableDouble
        {
            MinValue = 0.00,
            MaxValue = 1.01,
            Precision = 0.01,
            Default = 0.66,
            Value = 0.66,
        };

        [SettingSource("Ok", "Adjust accuracy weight of judgement OK.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> OkWeight { get; } = new BindableDouble
        {
            MinValue = 0.00,
            MaxValue = 1.01,
            Precision = 0.01,
            Default = 0.33,
            Value = 0.33,
        };

        [SettingSource("Meh", "Adjust accuracy weight of judgement MEH.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> MehWeight { get; } = new BindableDouble
        {
            MinValue = 0.00,
            MaxValue = 1.01,
            Precision = 0.01,
            Default = 0.16,
            Value = 0.16,
        };

        [SettingSource("Miss", "Adjust accuracy weight of judgement MISS.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> MissWeight { get; } = new BindableDouble
        {
            MinValue = 0.00,
            MaxValue = 1.01,
            Precision = 0.01,
            Default = 0.0,
            Value = 0.0,
        };

        private Bindable<int> countPerfect = new Bindable<int>();
        private Bindable<int> countGreat = new Bindable<int>();
        private Bindable<int> countGood = new Bindable<int>();
        private Bindable<int> countOk = new Bindable<int>();
        private Bindable<int> countMeh = new Bindable<int>();
        private Bindable<int> countMiss = new Bindable<int>();
        private Bindable<int> countIgnoreHit = new Bindable<int>();
        private int maxhits;

        private HitResult[] availableHitResult = [HitResult.Perfect, HitResult.Great, HitResult.Good, HitResult.Ok, HitResult.Meh, HitResult.Miss];

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {

            foreach (var hitResult in availableHitResult)
            {
                var judgementCount = new JudgementCount
                {
                    Types = [hitResult],
                    ResultCount = new BindableInt()
                };

                Counters.Add(judgementCount);

                Results.Add(hitResult, judgementCount);
            }

            /// This is for HitResult.IgnoreHit generated from hold note body
            var judgementCount4ih = new JudgementCount
            {
                Types = [HitResult.IgnoreHit],
                ResultCount = new BindableInt()
            };
            Counters.Add(judgementCount4ih);
            Results.Add(HitResult.IgnoreHit, judgementCount4ih);

            maxhits = scoreProcessor.MaxHits;

            scoreProcessor.NewJudgement += judgement => updateCount(judgement, false);
            scoreProcessor.JudgementReverted += judgement => updateCount(judgement, true);
            countPerfect.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.Perfect)).ResultCount);
            countGreat.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.Great)).ResultCount);
            countGood.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.Good)).ResultCount);
            countOk.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.Ok)).ResultCount);
            countMeh.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.Meh)).ResultCount);
            countMiss.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.Miss)).ResultCount);
            countIgnoreHit.BindTo(Counters.FirstOrDefault(judgementcount => judgementcount.Types.Contains(HitResult.IgnoreHit)).ResultCount);
        }

        private void updateCount(JudgementResult judgement, bool revert)
        {
            if (!Results.TryGetValue(judgement.Type, out var count))
                return;

            if (revert)
                count.ResultCount.Value--;
            else
                count.ResultCount.Value++;

            updateWeightAdjustedAccuracy();
        }


        private void updateWeightAdjustedAccuracy()
        {
            int totalhits = countPerfect.Value + countGreat.Value + countGood.Value + countOk.Value + countMeh.Value + countMiss.Value + countIgnoreHit.Value;
            WeightAdjustedAccuracy.Value = totalhits > 0 ? (countPerfect.Value * PerfectWeight.Value + countGreat.Value * GreatWeight.Value + countGood.Value * GoodWeight.Value + countOk.Value * OkWeight.Value + countMeh.Value * MehWeight.Value + countMiss.Value * MissWeight.Value) / (totalhits - countIgnoreHit.Value) : PerfectWeight.Value;
            MinimumWeightAdjustedAccuracy.Value = totalhits > 0 ? (countPerfect.Value * PerfectWeight.Value + countGreat.Value * GreatWeight.Value + countGood.Value * GoodWeight.Value + countOk.Value * OkWeight.Value + countMeh.Value * MehWeight.Value + (countMiss.Value + maxhits - totalhits) * MissWeight.Value) / (maxhits - countIgnoreHit.Value) : 0;
            MaximumWeightAdjustedAccuracy.Value = totalhits > 0 ? ((countPerfect.Value + maxhits - totalhits) * PerfectWeight.Value + countGreat.Value * GreatWeight.Value + countGood.Value * GoodWeight.Value + countOk.Value * OkWeight.Value + countMeh.Value * MehWeight.Value + countMiss.Value * MissWeight.Value) / (maxhits - countIgnoreHit.Value) : PerfectWeight.Value;
        }
    }
}
