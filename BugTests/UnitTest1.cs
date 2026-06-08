using System.Linq;
using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests
{
    [TestClass]
    public class BugWorkflowTests
    {
        private static Bug NewBug() => new Bug();

        private static Bug AtBuilding()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            bug.Do(Move.Accept);
            bug.Do(Move.Build);
            return bug;
        }

        private static Bug AtQa()
        {
            var bug = AtBuilding();
            bug.Do(Move.ToQa);
            return bug;
        }

        private static Bug AtDone()
        {
            var bug = AtQa();
            bug.Do(Move.Close);
            return bug;
        }

        [TestMethod]
        public void NewBug_StartsRegistered() =>
            Assert.AreEqual(Stage.Registered, NewBug().Current);

        [TestMethod]
        public void NewBug_HasZeroReopens() =>
            Assert.AreEqual(0, NewBug().Reopens);

        [TestMethod]
        public void Triage_FromRegistered_GoesTriage()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            Assert.AreEqual(Stage.Triage, bug.Current);
        }

        [TestMethod]
        public void Accept_FromTriage_GoesAccepted()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            bug.Do(Move.Accept);
            Assert.AreEqual(Stage.Accepted, bug.Current);
        }

        [TestMethod]
        public void Decline_FromTriage_GoesDeclined()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            bug.Do(Move.Decline);
            Assert.AreEqual(Stage.Declined, bug.Current);
        }

        [TestMethod]
        public void Build_FromAccepted_GoesBuilding() =>
            Assert.AreEqual(Stage.Building, AtBuilding().Current);

        [TestMethod]
        public void Pause_FromBuilding_GoesPaused()
        {
            var bug = AtBuilding();
            bug.Do(Move.Pause);
            Assert.AreEqual(Stage.Paused, bug.Current);
        }

        [TestMethod]
        public void Unpause_FromPaused_GoesBuilding()
        {
            var bug = AtBuilding();
            bug.Do(Move.Pause);
            bug.Do(Move.Unpause);
            Assert.AreEqual(Stage.Building, bug.Current);
        }

        [TestMethod]
        public void ToQa_FromBuilding_GoesQa() =>
            Assert.AreEqual(Stage.QA, AtQa().Current);

        [TestMethod]
        public void Bounce_FromQa_GoesBuilding()
        {
            var bug = AtQa();
            bug.Do(Move.Bounce);
            Assert.AreEqual(Stage.Building, bug.Current);
        }

        [TestMethod]
        public void Close_FromQa_GoesDone() =>
            Assert.AreEqual(Stage.Done, AtDone().Current);

        [TestMethod]
        public void Reopen_FromDone_GoesReopened_AndCounts()
        {
            var bug = AtDone();
            bug.Do(Move.Reopen);
            Assert.AreEqual(Stage.Reopened, bug.Current);
            Assert.AreEqual(1, bug.Reopens);
        }

        [TestMethod]
        public void Reopen_FromDeclined_GoesReopened()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            bug.Do(Move.Decline);
            bug.Do(Move.Reopen);
            Assert.AreEqual(Stage.Reopened, bug.Current);
        }

        [TestMethod]
        public void Reopened_Triage_GoesBackToTriage()
        {
            var bug = AtDone();
            bug.Do(Move.Reopen);
            bug.Do(Move.Triage);
            Assert.AreEqual(Stage.Triage, bug.Current);
        }

        [TestMethod]
        public void Reopen_BeyondLimit_Throws()
        {
            var bug = AtDone();
            bug.Do(Move.Reopen);    // переоткрытие #1
            bug.Do(Move.Triage);
            bug.Do(Move.Decline);
            bug.Do(Move.Reopen);    // переоткрытие #2 — лимит исчерпан
            bug.Do(Move.Triage);
            bug.Do(Move.Decline);
            Assert.AreEqual(2, bug.Reopens);
            Assert.IsFalse(bug.Allows(Move.Reopen));
            Assert.ThrowsException<WorkflowViolationException>(() => bug.Do(Move.Reopen));
        }

        [TestMethod]
        public void Allows_ReflectsPermittedMoves()
        {
            var bug = NewBug();
            Assert.IsTrue(bug.Allows(Move.Triage));
            Assert.IsFalse(bug.Allows(Move.Close));
        }

        [TestMethod]
        public void Options_AtTriage_AreCorrect()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            var options = bug.Options.ToList();
            CollectionAssert.Contains(options, Move.Accept);
            CollectionAssert.Contains(options, Move.Decline);
            Assert.IsFalse(options.Contains(Move.Build));
        }

        [TestMethod]
        public void Log_RecordsEachTransition()
        {
            var bug = NewBug();
            bug.Do(Move.Triage);
            bug.Do(Move.Accept);
            Assert.AreEqual(2, bug.Log.Count);
            StringAssert.Contains(bug.Log[0], "Registered");
        }

        [DataTestMethod]
        [DataRow(Move.Accept)]
        [DataRow(Move.Build)]
        [DataRow(Move.Pause)]
        [DataRow(Move.ToQa)]
        [DataRow(Move.Close)]
        [DataRow(Move.Reopen)]
        public void Registered_InvalidMoves_Throw(Move move)
        {
            var bug = NewBug();
            Assert.ThrowsException<WorkflowViolationException>(() => bug.Do(move));
        }

        [DataTestMethod]
        [DataRow(Move.Triage)]
        [DataRow(Move.Accept)]
        [DataRow(Move.Build)]
        [DataRow(Move.ToQa)]
        [DataRow(Move.Close)]
        public void Done_InvalidMoves_Throw(Move move)
        {
            var bug = AtDone();
            Assert.ThrowsException<WorkflowViolationException>(() => bug.Do(move));
        }
    }
}
