using System.Collections.Concurrent;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Stores touches to apply them once a frame for platforms that dispatch touches asynchronously
    /// while user code is running.
    /// </summary>
    internal class TouchQueue
    {
        private readonly ConcurrentQueue<TouchEvent> _queue = new ConcurrentQueue<TouchEvent>();

        public void Enqueue(int id, TouchLocationState state, Vector2 pos)
        {
            _queue.Enqueue(new TouchEvent(id, state, pos));
        }

        public void ProcessQueued()
        {
            TouchEvent ev;
            while (_queue.TryDequeue(out ev))                
                TouchPanel.Current.AddEvent(ev.Id, ev.State, ev.Pos);
        }

        private struct TouchEvent
        {
            public readonly int Id;
            public readonly TouchLocationState State;
            public readonly Vector2 Pos;

            public TouchEvent(int id, TouchLocationState state, Vector2 pos)
            {
                Id = id;
                State = state;
                Pos = pos;
            }
        }

    }
}
