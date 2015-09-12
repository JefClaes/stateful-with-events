using System;
using System.Collections.Generic;
using System.Linq;

namespace StateWithEvents
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var player = Player.Register("1", "jef");

            player.Lock();

            var state = player.State();
            var events = player.Events();

            Console.WriteLine(string.Format("State = {0}", state));
            Console.WriteLine(string.Format("Events = {0}", string.Join(", ", events.Select(e => e.GetType().Name))));

            new Scenario<Player, PlayerState>()
                .Given(() => Player.Register("1", "jef"))
                .When(sut => sut.Lock())
                .ThenState(new PlayerState()
                    { Id = "1", Locked = true, Nickname = "jef" })
                .ThenEvent(new PlayerRegistered("1", "jef"))
                .ThenEvent(new PlayerLocked("1"))
                .Run();

            Console.ReadLine();
        }
    }

    public interface IStatefulAggregate<TState>
    {
        TState State();
    }

    public interface IEventRecordingAggregate
    {
        IEnumerable<object> Events();
    }

    public class Scenario<TAggregate, TState> where TAggregate : IStatefulAggregate<TState>, IEventRecordingAggregate
    {
        private Func<TAggregate> _factory;
        private Action<TAggregate> _when;
        private object _expectedState;
        private List<object> _expectedEvents = new List<object>();

        public Scenario<TAggregate, TState> Given(Func<TAggregate> factory)
        {
            _factory = factory;

            return this;
        }

        public Scenario<TAggregate, TState> When(Action<TAggregate> when)
        {
            _when = when;

            return this;
        }

        public Scenario<TAggregate, TState> ThenState(object state)
        {
            _expectedState = state;

            return this;
        }

        public Scenario<TAggregate, TState> ThenEvent(object @event) 
        {
            _expectedEvents.Add(@event);

            return this;
        }

        public void Run()
        {
            var sut = _factory();

            _when(sut);

            var state = sut.State();
            var events = sut.Events();

            if (state.Equals(_expectedState)) throw new InvalidOperationException("State assertion failed");
            if (events.Count() != _expectedEvents.Count()) throw new InvalidOperationException("Events assertion failed");
        }
    }

    public class Player : IStatefulAggregate<PlayerState>, IEventRecordingAggregate
    {
        private readonly PlayerState _state;
        private readonly List<object> _events = new List<object>();        

        private Player(string id, string nickname)
        {
            _state = new PlayerState();
            Apply(new PlayerRegistered(id, nickname));
        }

        public Player(PlayerState state)
        {
            _state = state;
        }    

        public void Lock()
        {
            if (!_state.Locked)
                Apply(new PlayerLocked(_state.Id));
        }     

        private void When(PlayerLocked e)
        {
            _state.Locked = true;
        }

        private void When(PlayerRegistered e)
        {
            _state.Id = e.PlayerId;
            _state.Nickname = e.Nickname;
        }

        public PlayerState State()
        {
            return _state;
        }

        private void Apply(dynamic e)
        {
            _events.Add(e);
            When(e);
        }

        public IEnumerable<object> Events()
        {
            return _events;
        }

        public static Player Register(string id, string nickname)
        {
            return new Player(id, nickname);
        }
    }

    public class PlayerState
    {
        public string Id { get; set; }

        public string Nickname { get; set; }

        public bool Locked { get; set; }

        public override string ToString()
        {
            return string.Format("Id = {0} Nickname = {1} Locked = {2}", Id, Nickname, Locked);
        }

        public override bool Equals(object obj)
        {
            if (obj != null) return false;
            if (obj.GetType() != typeof(PlayerState)) return false;

            var other = (PlayerState)obj;

            return other.Id == Id && other.Nickname == Nickname && other.Locked == Locked;
        }

        public override int GetHashCode()
        {
            return 1;            
        }
    }

    public class PlayerRegistered
    {
        public PlayerRegistered(string playerId, string nickname)
        {
            PlayerId = playerId;
            Nickname = nickname;
        }

        public string PlayerId { get; set; }

        public string Nickname { get; set; }
    }

    public class PlayerLocked
    {
        public PlayerLocked(string playerId)
        {
            PlayerId = playerId;
        }

        public string PlayerId { get; set; }                
    }  
}
