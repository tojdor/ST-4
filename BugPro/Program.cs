using System;
using System.Collections.Generic;
using System.Linq;
using Stateless;

namespace BugPro
{
    public sealed class WorkflowViolationException : InvalidOperationException
    {
        public WorkflowViolationException(string message) : base(message) { }
    }

    public enum Stage
    {
        Registered,
        Triage,
        Accepted,
        Building,
        Paused,
        QA,
        Done,
        Declined,
        Reopened
    }

    public enum Move
    {
        Triage,
        Accept,
        Decline,
        Build,
        Pause,
        Unpause,
        ToQa,
        Bounce,
        Close,
        Reopen
    }

    public sealed class Bug
    {
        public const int MaxReopens = 2;

        private sealed record Edge(Stage From, Move On, Stage To, Func<Bug, bool>? When = null);

        private static readonly Edge[] Edges =
        {
            new(Stage.Registered, Move.Triage,  Stage.Triage),
            new(Stage.Triage,     Move.Accept,  Stage.Accepted),
            new(Stage.Triage,     Move.Decline, Stage.Declined),
            new(Stage.Accepted,   Move.Build,   Stage.Building),
            new(Stage.Building,   Move.Pause,   Stage.Paused),
            new(Stage.Paused,     Move.Unpause, Stage.Building),
            new(Stage.Building,   Move.ToQa,    Stage.QA),
            new(Stage.QA,         Move.Bounce,  Stage.Building),
            new(Stage.QA,         Move.Close,   Stage.Done),
            new(Stage.Done,       Move.Reopen,  Stage.Reopened, b => b._reopens < MaxReopens),
            new(Stage.Declined,   Move.Reopen,  Stage.Reopened, b => b._reopens < MaxReopens),
            new(Stage.Reopened,   Move.Triage,  Stage.Triage),
        };

        private readonly StateMachine<Stage, Move> _sm;
        private readonly List<string> _log = new();
        private int _reopens;

        public Bug()
        {
            _sm = new StateMachine<Stage, Move>(Stage.Registered);

            _sm.OnUnhandledTrigger((stage, move) =>
                throw new WorkflowViolationException(
                    $"Действие {move} запрещено на стадии {stage}."));

            _sm.Configure(Stage.Reopened).OnEntry(() => _reopens++);

            foreach (var group in Edges.GroupBy(e => e.From))
            {
                var cfg = _sm.Configure(group.Key);
                foreach (var edge in group)
                {
                    if (edge.When is null)
                        cfg.Permit(edge.On, edge.To);
                    else
                        cfg.PermitIf(edge.On, edge.To, () => edge.When(this));
                }
            }

            _sm.OnTransitioned(t => _log.Add($"{t.Source}:{t.Trigger}->{t.Destination}"));
        }

        public Stage Current => _sm.State;
        public int Reopens => _reopens;
        public IReadOnlyList<string> Log => _log;
        public bool Allows(Move move) => _sm.CanFire(move);
        public IEnumerable<Move> Options => _sm.PermittedTriggers;
        public void Do(Move move) => _sm.Fire(move);
    }

    public static class Program
    {
        public static void Main()
        {
            var bug = new Bug();
            Console.WriteLine($"Старт: {bug.Current}");

            foreach (var move in new[] { Move.Triage, Move.Accept, Move.Build, Move.ToQa, Move.Close })
            {
                bug.Do(move);
                Console.WriteLine($"После {move}: {bug.Current}");
            }

            bug.Do(Move.Reopen);
            Console.WriteLine($"После Reopen: {bug.Current} (переоткрытий: {bug.Reopens})");

            Console.WriteLine("\nЖурнал переходов:");
            foreach (var line in bug.Log)
                Console.WriteLine($"  {line}");
        }
    }
}
