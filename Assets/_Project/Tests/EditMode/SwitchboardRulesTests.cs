using ExtensaoZero.Telephone;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class SwitchboardRulesTests
    {
        [Test]
        public void ValidExtensions_IncludeTrunkLineAndPanelRange()
        {
            Assert.IsTrue(SwitchboardRules.IsValidExtension(0));
            Assert.IsTrue(SwitchboardRules.IsValidExtension(9));
            Assert.IsTrue(SwitchboardRules.IsValidExtension(200));
            Assert.IsTrue(SwitchboardRules.IsValidExtension(219));
            Assert.IsFalse(SwitchboardRules.IsValidExtension(199));
            Assert.IsFalse(SwitchboardRules.IsValidExtension(220));
            Assert.IsFalse(SwitchboardRules.IsValidExtension(7));
        }

        [Test]
        public void SameExtension_IsRejected()
        {
            Assert.IsFalse(SwitchboardRules.Validate(214, 214, out var message));
            Assert.IsNotEmpty(message);
        }

        [Test]
        public void Solution_Is214ToTrunkZero_InEitherDirection()
        {
            Assert.IsTrue(SwitchboardRules.IsSolution(214, 0));
            Assert.IsTrue(SwitchboardRules.IsSolution(0, 214));
            Assert.IsFalse(SwitchboardRules.IsSolution(214, 9));
        }

        [Test]
        public void LineNine_IsContaminated()
        {
            Assert.IsTrue(SwitchboardRules.IsContaminated(214, 9));
            Assert.IsTrue(SwitchboardRules.IsContaminated(9, 0));
            Assert.IsFalse(SwitchboardRules.IsContaminated(214, 0));
        }
    }
}
