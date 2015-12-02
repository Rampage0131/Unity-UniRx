﻿using System;
using System.Threading;

namespace UniRx.Operators
{
    public abstract class OperatorObserverBase<TSource, TResult> : IDisposable, IObserver<TSource>, ISafeObserver
    {
        protected internal volatile IObserver<TResult> observer;
        IDisposable cancel;

        public OperatorObserverBase(IObserver<TResult> observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnNext(TSource value);

        public virtual void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }

        public virtual void OnCompleted()
        {
            try
            {
                observer.OnCompleted();
            }
            finally
            {
                Dispose();
            }
        }

        public virtual void Dispose()
        {
            observer = new UniRx.InternalUtil.EmptyObserver<TResult>();
            var target = System.Threading.Interlocked.Exchange(ref cancel, null);
            if (target != null)
            {
                target.Dispose();
            }
        }
    }

    public abstract class AutoDetachOperatorObserverBase<T> : IObserver<T>, IDisposable, ISafeObserver
    {
        protected internal volatile IObserver<T> observer;
        IDisposable cancel;

        int isStopped = 0;

        public AutoDetachOperatorObserverBase(IObserver<T> observer, IDisposable cancel)
        {
            this.observer = observer;
            this.cancel = cancel;
        }

        public abstract void OnNext(T value);

        public void OnError(Exception error)
        {
            if (Interlocked.Increment(ref isStopped) == 1)
            {
                try
                {
                    this.observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }
        }

        public void OnCompleted()
        {
            if (Interlocked.Increment(ref isStopped) == 1)
            {
                try
                {
                    this.observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            observer = new UniRx.InternalUtil.EmptyObserver<T>();
            var target = System.Threading.Interlocked.Exchange(ref cancel, null);
            if (target != null)
            {
                target.Dispose();
            }
        }
    }
}