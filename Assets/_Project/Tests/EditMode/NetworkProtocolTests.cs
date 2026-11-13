using ExtensaoZero.Multiplayer;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class NetworkProtocolTests
    {
        [Test]
        public void Invite_RoundTrips()
        {
            var invite = InviteCodec.Encode("192.168.0.12", 7777, "ab-2c3d");
            StringAssert.StartsWith(InviteCodec.Prefix, invite);
            Assert.IsTrue(InviteCodec.TryDecode(invite, out var data, out var error), error);
            Assert.AreEqual("192.168.0.12", data.Ip);
            Assert.AreEqual(7777, data.Port);
            Assert.AreEqual("AB-2C3D", data.Key);
            Assert.AreEqual(InviteCodec.Version, data.Version);
        }

        [Test]
        public void Invite_AcceptsIpv6Text_AndTrimsWhitespace()
        {
            var invite = "  " + InviteCodec.Encode("2001:db8::1", 40000, "KEYKEY") + "\n";
            Assert.IsTrue(InviteCodec.TryDecode(invite, out var data, out _));
            Assert.AreEqual("2001:db8::1", data.Ip);
            Assert.AreEqual(40000, data.Port);
        }

        [Test]
        public void Invite_RejectsGarbageWrongPrefixAndBadPort()
        {
            Assert.IsFalse(InviteCodec.TryDecode("", out _, out _));
            Assert.IsFalse(InviteCodec.TryDecode("XX1:abc", out _, out _));
            Assert.IsFalse(InviteCodec.TryDecode("EZ1:!!!", out _, out _));
            Assert.IsFalse(InviteCodec.TryDecode(InviteCodec.Encode("10.0.0.1", 80, "KEYKEY"), out _, out var error));
            StringAssert.Contains("Porta", error);
            Assert.IsFalse(InviteCodec.TryDecode(InviteCodec.Encode("10.0.0.1", 7777, "AB"), out _, out _), "Chave curta demais.");
        }

        [Test]
        public void Invite_RejectsOtherVersion()
        {
            var json = "{\"v\":2,\"ip\":\"10.0.0.1\",\"port\":7777,\"key\":\"KEYKEY\"}";
            var b64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            Assert.IsFalse(InviteCodec.TryDecode("EZ1:" + b64, out _, out var error));
            StringAssert.Contains("incompatível", error);
        }

        [Test]
        public void SessionKey_IsSixCharsFromUnambiguousAlphabet_AndValid()
        {
            for (var i = 0; i < 20; i++)
            {
                var key = SessionKeyGenerator.Generate();
                Assert.AreEqual(SessionKeyGenerator.Length, key.Length);
                foreach (var c in key) StringAssert.Contains(c.ToString(), SessionKeyGenerator.Alphabet);
                Assert.IsTrue(SessionCodec.IsValid(key));
            }
        }

        [Test]
        public void Beacon_RoundTrips_AndRejectsMalformed()
        {
            var beacon = new LanBeacon { Name = "Serra|do Silêncio", Port = 7777, PlayerCount = 2, MissionStarted = true, Fingerprint = "ABC123" };
            var text = beacon.Format();
            StringAssert.StartsWith("EZLAN1|", text);
            Assert.IsTrue(LanBeacon.TryParse(text, out var parsed));
            Assert.AreEqual(7777, parsed.Port);
            Assert.AreEqual(2, parsed.PlayerCount);
            Assert.IsTrue(parsed.MissionStarted);
            Assert.AreEqual("Serra/do Silêncio", parsed.Name, "O separador não pode vazar do nome.");

            Assert.IsFalse(LanBeacon.TryParse("EZLAN1|x|7777|2|1", out _));
            Assert.IsFalse(LanBeacon.TryParse("EZLAN2|x|7777|2|1|f", out _));
            Assert.IsFalse(LanBeacon.TryParse("EZLAN1|x|80|2|1|f", out _));
            Assert.IsFalse(LanBeacon.TryParse("EZLAN1|x|7777|9|1|f", out _));
            Assert.IsFalse(LanBeacon.TryParse("EZLAN1|x|7777|2|sim|f", out _));
        }

        [Test]
        public void Approval_ChecksVersionKeyCapacityAndMissionState()
        {
            var hash = SessionCodec.Hash("KEYKEY");
            var payload = ConnectionPayload.Build("keykey");
            Assert.AreEqual(ConnectionPayload.Verdict.Approved, ConnectionPayload.Evaluate(payload, hash, 1, false));
            Assert.AreEqual(ConnectionPayload.Verdict.WrongKey, ConnectionPayload.Evaluate(ConnectionPayload.Build("OUTRA"), hash, 1, false));
            Assert.AreEqual(ConnectionPayload.Verdict.SessionFull, ConnectionPayload.Evaluate(payload, hash, 4, false));
            Assert.AreEqual(ConnectionPayload.Verdict.MissionStarted, ConnectionPayload.Evaluate(payload, hash, 1, true));
            Assert.AreEqual(ConnectionPayload.Verdict.Malformed, ConnectionPayload.Evaluate(null, hash, 0, false));
            Assert.AreEqual(ConnectionPayload.Verdict.Malformed, ConnectionPayload.Evaluate(new byte[] { 1, 2, 3 }, hash, 0, false));

            var oldVersion = ConnectionPayload.Build("KEYKEY");
            oldVersion[2] = 1;
            oldVersion[3] = 0;
            Assert.AreEqual(ConnectionPayload.Verdict.VersionMismatch, ConnectionPayload.Evaluate(oldVersion, hash, 0, false));
            Assert.IsFalse(string.IsNullOrEmpty(ConnectionPayload.Reason(ConnectionPayload.Verdict.SessionFull)));
        }
    }
}
