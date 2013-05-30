using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReasonCam
{
    public interface IObserver
    {
        Type Listener { get; }
    }

    public class Observer<T> : IObserver
    {
        public Type Listener { get; private set; }
        public Action<T> Action { get; private set; }

        public Observer(Type listener, Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");

            Listener = listener;
            Action = action;
        }
    }

    public class Observer : IObserver
    {
        public Type Listener { get; private set; }
        public Action Action { get; private set; }

        public Observer(Type listener, Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            Listener = listener;
            Action = action;
        }
    }

    /// <summary>
    /// usage:
    /// NotificationCenter.DefaultCenter.AddObserver<type>(this, delegate, CONSTANT_NAME);
    /// NotificationCenter.DefaultCenter.RemoveObservers(this); // removes all "my" listeners
    /// NotificationCenter.DefaultCenter.PostNofication(CONSTANT_NAME,parameter);
    /// </summary>
    public class NotificationCenter
    {
        private readonly IDictionary<string, IList<IObserver>> observers = new Dictionary<string, IList<IObserver>>();

        private static NotificationCenter defaultCenter = null;
        static readonly object padlock = new object();

        public static NotificationCenter DefaultCenter
        {
            get
            {
                lock (padlock)
                {
                    if (defaultCenter == null)
                        defaultCenter = new NotificationCenter();
                    return defaultCenter;
                }
            }
        }

        public void AddObserver<T>(object listener, Action<T> action, string name)
        {
            Type listenerType = listener.GetType();
            var observer = new Observer<T>(listenerType, action);

            if (observers.ContainsKey(name))
                observers[name].Add(observer);
            else
                observers.Add(name, new List<IObserver> { observer });
        }

        public void AddObserver(object listener, Action action, string name)
        {
            Type listenerType = listener.GetType();
            var observer = new Observer(listenerType, action);

            if (observers.ContainsKey(name))
                observers[name].Add(observer);
            else
                observers.Add(name, new List<IObserver> { observer });
        }

        public void RemoveObserver(object listener, string name)
        {
            Type listenerType = listener.GetType();

            if (observers.ContainsKey(name))
            {
                foreach (var notificationObserver in new List<IObserver>(observers[name]))
                {
                    if (notificationObserver.Listener == listenerType)
                        observers[name].Remove(notificationObserver);
                }
            }
        }

        public void RemoveObservers(object listener)
        {
            foreach (var name in observers.Keys)
            {
                RemoveObserver(listener, name);
            }
        }

        public void PostNofication<T>(string name, T parameter)
        {
            if (observers.ContainsKey(name))
            {
                foreach (var observer in observers[name].Cast<Observer<T>>())
                {
                    observer.Action(parameter);
                }
            }
        }

        public void PostNotification(string name)
        {
            if (observers.ContainsKey(name))
            {
                foreach (var observer in observers[name].Cast<Observer>())
                {
                    observer.Action();
                }
            }
        }
    }
}
