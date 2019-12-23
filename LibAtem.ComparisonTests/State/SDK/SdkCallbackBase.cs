using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LibAtem.Util;

namespace LibAtem.ComparisonTests.State.SDK
{
    public static class SdkCallbackUtil
    {
        public static Action<string> AppendChange(Action<string> upstream, string mid)
        {
            return suffix =>
            {
                if (suffix == null)
                {
                    upstream(mid);
                }
                else if (mid == null)
                {
                    upstream(suffix);
                }
                else
                {
                    upstream($"{mid}.{suffix}");
                }
            };
        }
    }

    public interface INotify<in T>
    {
        void Notify(T eventType);
    }

    public abstract class SdkCallbackBase<T> : IDisposable
    {
        protected readonly List<IDisposable> Children = new List<IDisposable>();
        protected readonly T Props;
        protected readonly Action<string> OnChange;

        internal SdkCallbackBase(T props, Action<string> onChange)
        {
            Props = props;
            OnChange = onChange;

            MethodInfo addCallback = typeof(T).GetMethod("AddCallback");
            addCallback.Invoke(Props, new object[] {this});
        }

        public virtual void Dispose()
        {
            DisposeMany(Children);

            MethodInfo removeCallback = typeof(T).GetMethod("RemoveCallback");
            removeCallback.Invoke(Props, new object[] { this });
        }

        protected static void DisposeMany(IEnumerable<IDisposable> objs)
        {
            foreach (IDisposable obj in objs)
                obj.Dispose();
        }

        public Action<string> AppendChange(string mid) => SdkCallbackUtil.AppendChange(OnChange, mid);

    }

    public abstract class SdkCallbackBaseNotify<T, Te> : SdkCallbackBase<T>, INotify<Te>
    {
        internal SdkCallbackBaseNotify(T props, Action<string> onChange) : base(props, onChange)
        {
        }

        public abstract void Notify(Te eventType);

        protected void TriggerAllChanged(params Te[] skip)
        {
            Enum.GetValues(typeof(Te)).OfType<Te>().Where(v => !skip.Contains(v)).ForEach(Notify);
        }

    }
}