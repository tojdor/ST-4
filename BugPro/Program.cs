using System;
using Stateless;

namespace BugPro
{
    public enum State
    {
        Open,
        InAnalysis,
        InProgress,
        Resolved,
        Closed,
        Reopened,
        Rejected,
        Deferred
    }

    public enum Trigger
    {
        Analyze,
        Assign,
        Resolve,
        Confirm,
        Reanalyze,
        Reject,
        Defer,
        Resume,
        Reopen
    }

    public class Bug
    {
        public const int MaxReopen = 3;

        private readonly StateMachine<State, Trigger> _machine;
        private int _reopenCount;

        public Bug()
        {
            _machine = new StateMachine<State, Trigger>(State.Open);

            _machine.Configure(State.Open)
                .Permit(Trigger.Analyze, State.InAnalysis);

            _machine.Configure(State.InAnalysis)
                .Permit(Trigger.Assign, State.InProgress)
                .Permit(Trigger.Defer, State.Deferred)
                .Permit(Trigger.Reject, State.Rejected);

            _machine.Configure(State.InProgress)
                .Permit(Trigger.Resolve, State.Resolved)
                .Permit(Trigger.Reanalyze, State.InAnalysis);

            _machine.Configure(State.Resolved)
                .Permit(Trigger.Confirm, State.Closed)
                .Permit(Trigger.Reanalyze, State.InAnalysis);

            _machine.Configure(State.Deferred)
                .Permit(Trigger.Resume, State.InAnalysis);

            _machine.Configure(State.Closed)
                .PermitIf(Trigger.Reopen, State.Reopened, () => _reopenCount < MaxReopen);

            _machine.Configure(State.Rejected)
                .PermitIf(Trigger.Reopen, State.Reopened, () => _reopenCount < MaxReopen);

            _machine.Configure(State.Reopened)
                .OnEntry(() => _reopenCount++)
                .Permit(Trigger.Analyze, State.InAnalysis);
        }

        public State CurrentState => _machine.State;

        public int ReopenCount => _reopenCount;

        public bool CanFire(Trigger trigger) => _machine.CanFire(trigger);

        public void Analyze() => _machine.Fire(Trigger.Analyze);
        public void Assign() => _machine.Fire(Trigger.Assign);
        public void Resolve() => _machine.Fire(Trigger.Resolve);
        public void Confirm() => _machine.Fire(Trigger.Confirm);
        public void Reanalyze() => _machine.Fire(Trigger.Reanalyze);
        public void Reject() => _machine.Fire(Trigger.Reject);
        public void Defer() => _machine.Fire(Trigger.Defer);
        public void Resume() => _machine.Fire(Trigger.Resume);
        public void Reopen() => _machine.Fire(Trigger.Reopen);
    }

    public static class Program
    {
        public static void Main()
        {
            var bug = new Bug();
            Console.WriteLine($"Start: {bug.CurrentState}");

            bug.Analyze();
            Console.WriteLine($"After Analyze: {bug.CurrentState}");

            bug.Assign();
            Console.WriteLine($"After Assign: {bug.CurrentState}");

            bug.Resolve();
            Console.WriteLine($"After Resolve: {bug.CurrentState}");

            bug.Confirm();
            Console.WriteLine($"After Confirm: {bug.CurrentState}");

            bug.Reopen();
            Console.WriteLine($"After Reopen: {bug.CurrentState} " +
                              $"(reopened {bug.ReopenCount} time(s))");

            bug.Analyze();
            Console.WriteLine($"After Analyze: {bug.CurrentState}");
        }
    }
}
