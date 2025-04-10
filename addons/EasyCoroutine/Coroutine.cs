#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Godot;

namespace EasyCoroutine;

public class Coroutine
{
    private static readonly PriorityQueue<Coroutine, ulong> WaitTimeCoroutines = new();
    private static Queue<Coroutine> _waitPhysicsFrameCoroutines = new();
    private static Coroutine? _contextCoroutine;
    private static ulong _nowTime;
    public static void Time(ulong time)
    {
        _nowTime = time;
        while (WaitTimeCoroutines.Count > 0 && WaitTimeCoroutines.Peek().FreeTime < time)
        {
            var coroutine = WaitTimeCoroutines.Dequeue();
            HandleWait(coroutine);
        }
    }

    public static void PhysicsFrame()
    {
        int count = _waitPhysicsFrameCoroutines.Count;
        while (count-- > 0)
        {
            var coroutine = _waitPhysicsFrameCoroutines.Dequeue();
            HandleWait(coroutine);
        }
    }
    /// <summary>
    /// 获取当前上下文协程
    /// </summary>
    /// <returns></returns>
    public static Coroutine GetCurrentCoroutine()
    {
        return _contextCoroutine!;
    }
    public static Coroutine StartCoroutine(IEnumerator<ulong> routine)
    {
        var coroutine = new Coroutine(routine);
        HandleWait(coroutine);
        return coroutine;
    }
    /// <summary>
    /// 创建一个锁,协程等待锁直到解锁
    /// </summary>
    /// <param name="isLocked"></param>
    /// <returns></returns>
    public static Lock CreateLock(bool isLocked = true)
    { 
        var cLock = new Lock();
        if(!isLocked) cLock.Unlock();
        return cLock;
    }
    public static void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine.IsDone) return;
        RaiseCoroutineStop(coroutine);
    }
    private static void HandleWait(Coroutine coroutine)
    {
        //已经完成就退出,在等待时使用Stop可能发生这种情况
        if (coroutine.IsDone) return;
        //如果移动到结束
        if (!MoveNext(coroutine))
        {
            //引发结束
            RaiseCoroutineStop(coroutine);
            return;
        }
        //获取当前的等待类型
        var currentWaitType = coroutine.Enumerator.Current != ulong.MaxValue ? WaitType.WaitTime : WaitType.WaitLazy;
        coroutine.WaitingType = currentWaitType;
        //设置参数
        if (coroutine.WaitingType == WaitType.WaitTime)
        {
            coroutine.FreeTime = _nowTime + coroutine.Enumerator.Current;
            WaitTimeCoroutines.Enqueue(coroutine, coroutine.FreeTime);
        }
    }
    private static void RaiseCoroutineStop(Coroutine coroutine)
    {
        coroutine.IsDone = true;
        coroutine.End?.Invoke(coroutine, EventArgs.Empty);
    }
    public static ulong WaitForSeconds([Range(0,float.MaxValue)] float seconds)
    {
        return (ulong)(seconds * 1000);
    }

    public static ulong WaitForTimeSpan(TimeSpan timeSpan)
    {
        return (ulong)timeSpan.TotalMilliseconds;
    }
    
    public static ulong WaitForSignal(GodotObject obj, string signalName)
    {
        var content = GetCurrentCoroutine();
        obj.Connect(signalName, Callable.From(OnSignal), (uint)GodotObject.ConnectFlags.OneShot);
        return ulong.MaxValue;

        void OnSignal()
        {
            HandleWait(content);
        }
    }

    public static ulong WaitForUnlock(Lock cLock)
    {
        if (cLock.Unlocked) return 0;
        var content = GetCurrentCoroutine();
        cLock.UnlockAction += UnlockAction;
        return ulong.MaxValue;
        
        void UnlockAction()
        {
            cLock.UnlockAction -= UnlockAction;
            HandleWait(content);
        }
    }

    public static ulong WaitForNextFrame()
    {
        return 0;
    }
    public static ulong WaitForNextPhysicsFrame()
    {
        var content = GetCurrentCoroutine();
        _waitPhysicsFrameCoroutines.Enqueue(content);
        return ulong.MaxValue;
    }
    public static ulong WaitForOtherCoroutine(Coroutine coroutine)
    {
        if (coroutine.IsDone) return 0;
        var content = GetCurrentCoroutine();
        coroutine.End += CoroutineOnEnd;
        return ulong.MaxValue;

        void CoroutineOnEnd(object? sender, EventArgs e)
        {
            coroutine.End -= CoroutineOnEnd;
            HandleWait(content);
        }
    }

    public static ulong WaitForTask(Task task)
    {
        if(task.IsCompleted) return 0;
        var content = GetCurrentCoroutine();
        task.ContinueWith(t =>
        {
            HandleWait(content);
        });
        return ulong.MaxValue;
    }
    
    public static ulong WaitForTween(Tween tween)
    {
        var content = GetCurrentCoroutine();
        tween.Finished += TweenOnFinished;
        return ulong.MaxValue;
        
        void TweenOnFinished()
        {
            tween.Finished -= TweenOnFinished;
            HandleWait(content);
        }
    }
    
    private static bool MoveNext(Coroutine coroutine)
    {
        var prev = _contextCoroutine;
        _contextCoroutine = coroutine;
        bool r = coroutine.Enumerator.MoveNext();
        _contextCoroutine = prev;
        return r;
    }


    private Coroutine(IEnumerator<ulong> enumerator)
    {
        Enumerator = enumerator;
    }
    private IEnumerator<ulong> Enumerator { get; }
    public WaitType WaitingType { get; private set; }
    public ulong FreeTime { get; private set; }
    public bool IsDone { get; private set; }
    public event EventHandler? End;
    
    public enum WaitType
    {
        WaitTime,
        WaitLazy,
    }
    
    public class Lock
    {
        internal Lock(){}
        public bool Unlocked { get; private set; } 
        internal Action? UnlockAction { get; set; }
        public void Unlock()
        {
            if (Unlocked) return;
            Unlocked = true;
            UnlockAction?.Invoke();
        }
        /// <summary>
        /// 重置锁,注意会清除所有的解锁事件
        /// </summary>
        public void Reset()
        {
            Unlocked = false;
            UnlockAction = null;
        }
    }
}