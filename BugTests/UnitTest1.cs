using System;
using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests
{
    [TestClass]
    public class BugWorkflowTests
    {
        private static Bug FreshBug() => new Bug();

        [TestMethod]
        public void NewBug_StartsInOpen()
        {
            var bug = FreshBug();
            Assert.AreEqual(State.Open, bug.CurrentState);
        }

        [TestMethod]
        public void NewBug_HasZeroReopens()
        {
            var bug = FreshBug();
            Assert.AreEqual(0, bug.ReopenCount);
        }

        [TestMethod]
        public void Analyze_MovesOpenToAnalysis()
        {
            var bug = FreshBug();
            bug.Analyze();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void Assign_MovesAnalysisToInProgress()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            Assert.AreEqual(State.InProgress, bug.CurrentState);
        }

        [TestMethod]
        public void Resolve_MovesInProgressToResolved()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            bug.Resolve();
            Assert.AreEqual(State.Resolved, bug.CurrentState);
        }

        [TestMethod]
        public void Confirm_MovesResolvedToClosed()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            bug.Resolve();
            bug.Confirm();
            Assert.AreEqual(State.Closed, bug.CurrentState);
        }

        [TestMethod]
        public void Reject_MovesAnalysisToRejected()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Reject();
            Assert.AreEqual(State.Rejected, bug.CurrentState);
        }

        [TestMethod]
        public void Defer_MovesAnalysisToDeferred()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Defer();
            Assert.AreEqual(State.Deferred, bug.CurrentState);
        }

        [TestMethod]
        public void Resume_MovesDeferredBackToAnalysis()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Defer();
            bug.Resume();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void Reanalyze_FromInProgress_ReturnsToAnalysis()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            bug.Reanalyze();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void Reanalyze_FromResolved_ReturnsToAnalysis()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            bug.Resolve();
            bug.Reanalyze();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void Reopen_FromClosed_MovesToReopened()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            bug.Resolve();
            bug.Confirm();
            bug.Reopen();
            Assert.AreEqual(State.Reopened, bug.CurrentState);
        }

        [TestMethod]
        public void Reopen_FromRejected_MovesToReopened()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Reject();
            bug.Reopen();
            Assert.AreEqual(State.Reopened, bug.CurrentState);
        }

        [TestMethod]
        public void Reopen_IncrementsReopenCount()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Reject();
            bug.Reopen();
            Assert.AreEqual(1, bug.ReopenCount);
        }

        [TestMethod]
        public void Reopened_Analyze_ReturnsToAnalysis()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Reject();
            bug.Reopen();
            bug.Analyze();
            Assert.AreEqual(State.InAnalysis, bug.CurrentState);
        }

        [TestMethod]
        public void CanFire_Analyze_TrueInOpen()
        {
            var bug = FreshBug();
            Assert.IsTrue(bug.CanFire(Trigger.Analyze));
        }

        [TestMethod]
        public void CanFire_Assign_FalseInOpen()
        {
            var bug = FreshBug();
            Assert.IsFalse(bug.CanFire(Trigger.Assign));
        }

        [TestMethod]
        public void Assign_FromOpen_Throws()
        {
            var bug = FreshBug();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Assign());
        }

        [TestMethod]
        public void Confirm_FromOpen_Throws()
        {
            var bug = FreshBug();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Confirm());
        }

        [TestMethod]
        public void Resolve_FromAnalysis_Throws()
        {
            var bug = FreshBug();
            bug.Analyze();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Resolve());
        }

        [TestMethod]
        public void Reopen_FromOpen_Throws()
        {
            var bug = FreshBug();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Reopen());
        }

        [TestMethod]
        public void Resume_FromClosed_Throws()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Assign();
            bug.Resolve();
            bug.Confirm();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Resume());
        }

        [TestMethod]
        public void Reopen_BeyondLimit_Throws()
        {
            var bug = FreshBug();
            bug.Analyze();
            bug.Reject();

            for (int i = 0; i < Bug.MaxReopen; i++)
            {
                bug.Reopen();
                bug.Analyze();
                bug.Reject();
            }

            Assert.AreEqual(Bug.MaxReopen, bug.ReopenCount);
            Assert.IsFalse(bug.CanFire(Trigger.Reopen));
            Assert.ThrowsException<InvalidOperationException>(() => bug.Reopen());
        }

        [TestMethod]
        public void DoubleAnalyze_Throws()
        {
            var bug = FreshBug();
            bug.Analyze();
            Assert.ThrowsException<InvalidOperationException>(() => bug.Analyze());
        }
    }
}
