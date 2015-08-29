using System;
using System.Collections.Generic;
using System.Linq;

namespace StateWithEvents
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var player = Player.Register();

            player.Lock();

            Console.WriteLine(string.Format("State = {0}", player.State()));
            Console.WriteLine(string.Format("Events = {0}", string.Join(", ", player.Events().Select(e => e.GetType().Name))));

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

    public class Player : IStatefulAggregate<PlayerState>, IEventRecordingAggregate
    {
        private readonly PlayerState _state;
        private readonly List<object> _events = new List<object>();        

        private Player(Guid id)
        {
            _state = new PlayerState();
            Apply(new PlayerRegistered(id));
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

        public static Player Register()
        {
            return new Player(Guid.NewGuid());
        }
    }

    public class PlayerState
    {
        public Guid Id { get; set; }

        public bool Locked { get; set; }

        public override string ToString()
        {
            return string.Format("Id = {0} Locked = {1}", Id, Locked);
        }
    }

    public class PlayerRegistered
    {
        public PlayerRegistered(Guid playerId)
        {
            PlayerId = playerId;
        }

        public Guid PlayerId { get; set; }
    }

    public class PlayerLocked
    {
        public PlayerLocked(Guid playerId)
        {
            PlayerId = playerId;
        }

        public Guid PlayerId { get; set; }                
    }
}
