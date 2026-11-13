using ExtensaoZero.Investigation;
using NUnit.Framework;

namespace ExtensaoZero.Tests
{
    public sealed class NotebookModelTests
    {
        [Test]
        public void Register_DeduplicatesById()
        {
            var model = new NotebookModel();
            Assert.IsTrue(model.Register(new EvidenceEntry("a", "A", "corpo", EvidenceCategory.Document)));
            Assert.IsFalse(model.Register(new EvidenceEntry("a", "Outro", "corpo", EvidenceCategory.Anomaly)));
            Assert.AreEqual(1, model.Count);
        }

        [Test]
        public void Register_RejectsEmptyId()
        {
            var model = new NotebookModel();
            Assert.IsFalse(model.Register(new EvidenceEntry("", "Sem id", "corpo", EvidenceCategory.Document)));
            Assert.IsFalse(model.Register(null));
        }

        [Test]
        public void Get_ReturnsEntryAndContainsReflectsState()
        {
            var model = new NotebookModel();
            model.Register(new EvidenceEntry("cable", "Cabo", "214", EvidenceCategory.Extension));
            Assert.IsTrue(model.Contains("cable"));
            Assert.AreEqual("Cabo", model.Get("cable").Title);
            Assert.IsNull(model.Get("missing"));
        }

        [Test]
        public void Notes_AreLocalAndIndependent()
        {
            var model = new NotebookModel { Notes = "hipótese local" };
            Assert.AreEqual("hipótese local", model.Notes);
            model.Clear();
            Assert.AreEqual(string.Empty, model.Notes);
            Assert.AreEqual(0, model.Count);
        }
    }
}
