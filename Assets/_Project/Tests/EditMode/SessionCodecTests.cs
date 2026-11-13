using ExtensaoZero.Multiplayer;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class SessionCodecTests
    {
        [Test]
        public void Normalize_RemovesSpacesAndSymbols_AndUppercases()
        {
            Assert.AreEqual("EZ-214", SessionCodec.Normalize("  ez-214! "));
        }

        [Test]
        public void IsValid_RejectsShortAndEmptyKeys()
        {
            Assert.IsFalse(SessionCodec.IsValid(""));
            Assert.IsFalse(SessionCodec.IsValid("abc"));
            Assert.IsTrue(SessionCodec.IsValid("abcd"));
            Assert.IsTrue(SessionCodec.IsValid("EZ-214"));
        }

        [Test]
        public void Hash_IsDeterministic_AndCaseInsensitive()
        {
            Assert.AreEqual(SessionCodec.Hash("EZ-214"), SessionCodec.Hash("ez-214"));
            Assert.AreNotEqual(SessionCodec.Hash("EZ-214"), SessionCodec.Hash("EZ-215"));
        }

        [Test]
        public void HashHex_HasEightCharacters()
        {
            Assert.AreEqual(8, SessionCodec.HashHex("linha-zero").Length);
        }
    }
}
