using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BugPro;

namespace BugTests;

[TestClass]
public class BugWorkflowTests
{
    [TestMethod]
    public void InitialStateIsNew()
    {
        var bug = new Bug();
        Assert.AreEqual(BugState.New, bug.State);
    }

    [TestMethod]
    public void StartTriageFromNewMovesToTriage()
    {
        var bug = new Bug();
        bug.StartTriage();
        Assert.AreEqual(BugState.Triage, bug.State);
    }

    [TestMethod]
    public void StartFixFromTriageMovesToFixing()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.StartFix();
        Assert.AreEqual(BugState.Fixing, bug.State);
    }

    [TestMethod]
    public void NeedInfoFromTriageMovesToNeedInfo()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.NeedInfo();
        Assert.AreEqual(BugState.NeedInfo, bug.State);
    }

    [TestMethod]
    public void OtherProductFromTriageMovesToOtherProduct()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.OtherProduct();
        Assert.AreEqual(BugState.OtherProduct, bug.State);
    }

    [TestMethod]
    public void NeedDecisionLaterFromTriageMovesToNeedDecisionLater()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.NeedDecisionLater();
        Assert.AreEqual(BugState.NeedDecisionLater, bug.State);
    }

    [TestMethod]
    public void MarkNotBugFromTriageMovesToNotABug()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkNotBug();
        Assert.AreEqual(BugState.NotABug, bug.State);
    }

    [TestMethod]
    public void MarkWontFixFromTriageMovesToWontFix()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkWontFix();
        Assert.AreEqual(BugState.WontFix, bug.State);
    }

    [TestMethod]
    public void MarkDuplicateFromTriageMovesToDuplicate()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkDuplicate();
        Assert.AreEqual(BugState.Duplicate, bug.State);
    }

    [TestMethod]
    public void MarkNotReproFromTriageMovesToNotReproducible()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkNotRepro();
        Assert.AreEqual(BugState.NotReproducible, bug.State);
    }

    [TestMethod]
    public void ReturnFromNeedInfoMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.NeedInfo();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReturnFromOtherProductMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.OtherProduct();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReturnFromNeedDecisionLaterMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.NeedDecisionLater();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReturnFromNotABugMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkNotBug();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReturnFromWontFixMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkWontFix();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReturnFromDuplicateMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkDuplicate();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReturnFromNotReproMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.MarkNotRepro();
        bug.Return();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void StartTriageFromReturnedMovesToTriage()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.NeedInfo();
        bug.Return();
        bug.StartTriage();
        Assert.AreEqual(BugState.Triage, bug.State);
    }

    [TestMethod]
    public void MarkFixedFromFixingMovesToVerification()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.StartFix();
        bug.MarkFixed();
        Assert.AreEqual(BugState.Verification, bug.State);
    }

    [TestMethod]
    public void ApproveFromVerificationMovesToClosed()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.StartFix();
        bug.MarkFixed();
        bug.Approve();
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void RejectFromVerificationMovesToReturned()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.StartFix();
        bug.MarkFixed();
        bug.Reject();
        Assert.AreEqual(BugState.Returned, bug.State);
    }

    [TestMethod]
    public void ReopenFromClosedMovesToReopened()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.StartFix();
        bug.MarkFixed();
        bug.Approve();
        bug.Reopen();
        Assert.AreEqual(BugState.Reopened, bug.State);
    }

    [TestMethod]
    public void StartTriageFromReopenedMovesToTriage()
    {
        var bug = new Bug();
        bug.StartTriage();
        bug.StartFix();
        bug.MarkFixed();
        bug.Approve();
        bug.Reopen();
        bug.StartTriage();
        Assert.AreEqual(BugState.Triage, bug.State);
    }

    [TestMethod]
    public void ApproveInNewThrows()
    {
        var bug = new Bug();
        Assert.ThrowsException<InvalidOperationException>(() => bug.Approve());
    }

    [TestMethod]
    public void ReturnInTriageThrows()
    {
        var bug = new Bug();
        bug.StartTriage();
        Assert.ThrowsException<InvalidOperationException>(() => bug.Return());
    }
}
