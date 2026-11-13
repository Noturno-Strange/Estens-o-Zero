using System.Collections.Generic;
using ExtensaoZero.Campaign;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class CampaignRulesTests
    {
        [Test]
        public void Prologue_IsAlwaysUnlocked_AndChaptersUnlockInOrder()
        {
            var completed = new List<int>();
            Assert.IsTrue(CampaignCatalog.IsUnlocked(0, completed));
            Assert.IsFalse(CampaignCatalog.IsUnlocked(1, completed));
            completed.Add(0);
            Assert.IsTrue(CampaignCatalog.IsUnlocked(1, completed));
            Assert.IsFalse(CampaignCatalog.IsUnlocked(2, completed));
            Assert.IsFalse(CampaignCatalog.IsUnlocked(CampaignCatalog.ChapterCount, completed));
        }

        [Test]
        public void NextChapter_IsFirstNotCompleted_AndClampsAtEnd()
        {
            Assert.AreEqual(0, CampaignCatalog.NextChapter(new List<int>()));
            Assert.AreEqual(2, CampaignCatalog.NextChapter(new List<int> { 0, 1 }));
            Assert.AreEqual(CampaignCatalog.ChapterCount - 1, CampaignCatalog.NextChapter(new List<int> { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void Catalog_HasSixChapters_WithBriefingAndRecommendation()
        {
            for (var i = 0; i < CampaignCatalog.ChapterCount; i++)
            {
                var info = CampaignCatalog.Get(i);
                Assert.AreEqual(i, info.Index);
                Assert.IsFalse(string.IsNullOrEmpty(info.Title));
                Assert.IsFalse(string.IsNullOrEmpty(info.Briefing));
                Assert.IsNotEmpty(info.Recommended);
            }
        }

        [Test]
        public void Endings_RequireTheirFlags()
        {
            var flags = new List<string>();
            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.Isolate, flags, 5));
            flags.Add(EndingRules.FlagFireRecords);
            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.Isolate, flags, 2), "Isolar exige três circuitos isolados.");
            Assert.IsTrue(EndingRules.IsAvailable(EndingKind.Isolate, flags, 3));

            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.Restore, flags, 0));
            flags.Add(EndingRules.FlagOriginalAllLayers);
            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.Restore, flags, 0));
            flags.Add(EndingRules.FlagRestoredClean);
            Assert.IsTrue(EndingRules.IsAvailable(EndingKind.Restore, flags, 0));

            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.Answer, flags, 0));
            flags.Add(EndingRules.FlagAnaName);
            Assert.IsTrue(EndingRules.IsAvailable(EndingKind.Answer, flags, 0));
            flags.Add(EndingRules.FlagAbandoned);
            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.Answer, flags, 0), "Abandono bloqueia o final Atender.");
            Assert.IsFalse(EndingRules.IsAvailable(EndingKind.None, flags, 9));
        }

        [Test]
        public void Endings_HaveTitleRequirementAndEpilogue()
        {
            foreach (var ending in new[] { EndingKind.Isolate, EndingKind.Restore, EndingKind.Answer })
            {
                Assert.IsFalse(string.IsNullOrEmpty(EndingRules.Title(ending)));
                Assert.IsFalse(string.IsNullOrEmpty(EndingRules.Requirement(ending)));
                Assert.IsFalse(string.IsNullOrEmpty(EndingRules.Epilogue(ending)));
            }
        }

        [Test]
        public void Loadout_AllowsAtMostTwo_AndRoundTripsMask()
        {
            var loadout = new Loadout();
            Assert.IsTrue(loadout.Toggle(EquipmentKind.Receiver));
            Assert.IsTrue(loadout.Toggle(EquipmentKind.Recorder));
            Assert.IsFalse(loadout.Toggle(EquipmentKind.CircuitMeter), "Um terceiro opcional deve ser recusado.");
            Assert.AreEqual(2, loadout.Items.Count);
            Assert.IsTrue(loadout.Toggle(EquipmentKind.Receiver), "Remover deve funcionar.");
            Assert.IsFalse(loadout.Has(EquipmentKind.Receiver));

            var copy = Loadout.FromMask(loadout.ToMask());
            Assert.IsTrue(copy.Has(EquipmentKind.Recorder));
            Assert.AreEqual(1, copy.Items.Count);
            Assert.AreEqual(2, Loadout.FromMask(0xF).Items.Count, "Máscara com quatro itens é limitada a dois.");
        }

        [Test]
        public void Difficulty_ScalesTimeAndThreat_AndKeepsLineBusyInRange()
        {
            var easy = DifficultyProfile.Get(DifficultyLevel.Investigation);
            var normal = DifficultyProfile.Get(DifficultyLevel.Operation);
            var hard = DifficultyProfile.Get(DifficultyLevel.Rupture);
            Assert.Greater(easy.CallDurationMultiplier, normal.CallDurationMultiplier);
            Assert.Less(hard.CallDurationMultiplier, normal.CallDurationMultiplier);
            Assert.Less(easy.ThreatWeightMultiplier, hard.ThreatWeightMultiplier);
            foreach (var p in new[] { easy, normal, hard })
            {
                Assert.GreaterOrEqual(p.LineBusySeconds, 15f);
                Assert.LessOrEqual(p.LineBusySeconds, 25f);
            }

            Assert.Greater(normal.CallDurationFor(20f, 1), normal.CallDurationFor(20f, 2), "Em solo, mais tempo.");
            Assert.IsTrue(normal.HintsFor(1), "Em solo, pistas redundantes.");
            Assert.IsFalse(normal.HintsFor(3));
        }

        [Test]
        public void SeedVariation_IsDeterministic_AndDistinct()
        {
            Assert.AreEqual(SeedVariation.Pick(42, "a", 6), SeedVariation.Pick(42, "a", 6));
            var picks = SeedVariation.PickDistinct(42, "boxes", 6, 3);
            Assert.AreEqual(3, picks.Length);
            CollectionAssert.AllItemsAreUnique(picks);
            CollectionAssert.AreEqual(picks, SeedVariation.PickDistinct(42, "boxes", 6, 3));
            foreach (var p in picks) Assert.That(p, Is.InRange(0, 5));

            var seen = new HashSet<string>();
            for (var seed = 0; seed < 50; seed++) seen.Add(string.Join(",", SeedVariation.PickDistinct(seed, "boxes", 6, 3)));
            Assert.Greater(seen.Count, 5, "Seeds diferentes devem gerar variações diferentes.");

            var shuffled = SeedVariation.Shuffle(7, "order", new[] { 205, 211, 216 });
            CollectionAssert.AreEquivalent(new[] { 205, 211, 216 }, shuffled);
            Assert.That(SeedVariation.NewSeed(), Is.GreaterThanOrEqualTo(0));
        }
    }
}
