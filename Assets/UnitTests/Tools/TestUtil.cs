using System;
using System.Collections.Generic;

namespace UniRx.Tests
{
    public static class TestUtil
    {
        public static T[] ToArrayWait<T>(this IObservable<T> source)
        {
            return source.ToArray().Wait();
        }

        public static RecordObserver<T> Record<T>(this IObservable<T> source)
        {
            var d = new SingleAssignmentDisposable();
            var observer = new RecordObserver<T>(d);
            d.Disposable = source.Subscribe(observer);

            return observer;
        }
    }

    public class RecordObserver<T> : IObserver<T>
    {
        readonly object gate = new object();
        readonly IDisposable subscription;

        public List<T> Values { get; set; }
        public List<Notification<T>> Notifications { get; set; }

        public RecordObserver(IDisposable subscription)
        {
            this.subscription = subscription;
            this.Values = new List<T>();
            this.Notifications = new List<Notification<T>>();
        }

        public void DisposeSubscription()
        {
            subscription.Dispose();
        }

        public void OnNext(T value)
        {
            lock (gate)
            {
                Values.Add(value);
                Notifications.Add(Notification.CreateOnNext<T>(value));
            }
        }

        public void OnError(Exception error)
        {
            lock (gate)
            {
                Notifications.Add(Notification.CreateOnError<T>(error));
            }
        }
        public void OnCompleted()
        {
            lock (gate)
            {
                Notifications.Add(Notification.CreateOnCompleted<T>());
            }
        }
    }
}
