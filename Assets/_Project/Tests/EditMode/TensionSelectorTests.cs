using System;
using System.Collections.Generic;
using ExtensaoZero.Anomaly;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace ExtensaoZero.Tests
{
    public sealed class TensionSelectorTests
    {
        private readonly List<TensionEventDefinition> m_Events = new List<TensionEventDefinition>();

        [TearDown]
        public void TearDown()
        {
            foreach (var definition in m_Events)
            {
                if (definition != null) UnityEngine.Object.DestroyImmediate(definition);
            }

            m_Events.Clear();
        }

        private TensionEventDefinition Add(string id, float minTension, float maxTension, float cooldown, int minStep = 0)
        {
            var definition = TensionEventDefinition.Create(id, TensionCategory.Presence, TensionEventKind.LightFlicker,
                minTension, maxTension, cooldown, 1f, 0.2f, minStep, "");
            m_Events.Add(definition);
            return definition;
        }

        [Test]
        public void Select_RespectsTensionRange()
        {
            Add("low", 0f, 0.5f, 60f);
            Add("high", 0.6f, 1f, 60f);
            var selector = new TensionSelector();
            var result = selector.Select(m_Events, 0.2f, 0, 0f, "", new Random(1));
            Assert.AreEqual("low", result.id);
        }

        [Test]
        public void Select_RespectsMissionStep()
        {
            Add("late", 0f, 1f, 60f, 5);
            var selector = new TensionSelector();
            Assert.IsNull(selector.Select(m_Events, 0.5f, 2, 0f, "", new Random(1)));
        }

        [Test]
        public void Select_NeverRepeatsLastEvent()
        {
            Add("a", 0f, 1f, 0f);
            Add("b", 0f, 1f, 0f);
            var selector = new TensionSelector();
            var result = selector.Select(m_Events, 0.5f, 0, 0f, "a", new Random(7));
            Assert.AreEqual("b", result.id);
        }

        [Test]
        public void Select_RespectsPerEventCooldown()
        {
            Add("a", 0f, 1f, 60f);
            var selector = new TensionSelector();
            selector.MarkFired("a", 0f);
            Assert.IsNull(selector.Select(m_Events, 0.5f, 0, 10f, "", new Random(1)));
            Assert.IsNotNull(selector.Select(m_Events, 0.5f, 0, 61f, "", new Random(1)));
        }

        [Test]
        public void Select_ReturnsNullWhenNoEventEligible()
        {
            Add("threat", 0.7f, 1f, 60f, 4);
            var selector = new TensionSelector();
            Assert.IsNull(selector.Select(m_Events, 0.1f, 0, 0f, "", new Random(1)));
            Assert.IsNull(selector.Select(m_Events, 0.9f, 0, 0f, "", new Random(1)));
        }
    }
}
