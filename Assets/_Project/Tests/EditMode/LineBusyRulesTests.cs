using ExtensaoZero.Anomaly;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class LineBusyRulesTests
    {
        [Test]
        public void Duration_IsAlwaysBetween15And25Seconds()
        {
            Assert.AreEqual(15f, LineBusyRules.ClampDuration(2f));
            Assert.AreEqual(25f, LineBusyRules.ClampDuration(90f));
            Assert.AreEqual(20f, LineBusyRules.ClampDuration(20f));
        }

        [Test]
        public void Trigger_OnlyWhenCrossingThreshold()
        {
            Assert.IsTrue(LineBusyRules.ShouldTrigger(10f, 70f, false));
            Assert.IsFalse(LineBusyRules.ShouldTrigger(60f, 70f, false), "Já estava acima do limiar.");
            Assert.IsFalse(LineBusyRules.ShouldTrigger(10f, 70f, true), "Já ocupada.");
            Assert.IsFalse(LineBusyRules.ShouldTrigger(10f, 40f, false));
        }

        [Test]
        public void ReturnFrequency_IsDeterministic_InRange_AndNotTheStart()
        {
            for (var seed = 0; seed < 40; seed++)
            {
                var f = LineBusyRules.ReturnFrequency(seed, 1, 0);
                Assert.That(f, Is.InRange(0, LineBusyRules.FrequencySteps - 1));
                Assert.AreNotEqual(0, f, "A frequência de retorno nunca é a inicial do sintonizador.");
                Assert.AreEqual(f, LineBusyRules.ReturnFrequency(seed, 1, 0));
            }
        }

        [Test]
        public void Step_WrapsBothWays()
        {
            Assert.AreEqual(0, LineBusyRules.Step(9, 1));
            Assert.AreEqual(9, LineBusyRules.Step(0, -1));
            Assert.AreEqual(4, LineBusyRules.Step(2, 2));
        }
    }
}
