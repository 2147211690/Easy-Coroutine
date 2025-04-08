using System;
using System.Collections.Generic;
using Godot;

namespace EasyCoroutine.Example;

public class CoroutineExample
{
    public Action PlayerHit;
    public Node2D Player { get; init; }
    //等待指定秒数
    IEnumerator<ulong> Test1()
    {
        GD.Print("Hello I am Test1");
        yield return Coroutine.WaitForSeconds(1);
        GD.Print("Test1 is done");
    }
    //等待其他协程完成
    IEnumerator<ulong> Test2()
    {
        GD.Print("Hello I am Test2");
        yield return Coroutine.WaitForSeconds(1);
        yield return Coroutine.WaitForOtherCoroutine(Coroutine.StartCoroutine(Test1()));
        GD.Print("Test2 is done");
    }
    //通过锁等待事件
    IEnumerator<ulong> Test3()
    {
        GD.Print("Hello I am Test3");
        var lock1 = Coroutine.CreateLock();
        PlayerHit += OnPlayerHit;
        yield return Coroutine.WaitForUnlock(lock1);
        GD.Print("Test3 is done");
            
        void OnPlayerHit()
        {
            GD.Print("PlayerHit");
            PlayerHit -= PlayerHit;
            lock1.Unlock();
        }
    }
    //等待Tween完成
    IEnumerator<ulong> Test4()
    {
        GD.Print("Hello I am Test4");
        var tween = Player.CreateTween();
        tween.TweenProperty(Player, "position", new Vector2(100, 100), 2);
        yield return Coroutine.WaitForTween(tween);
        GD.Print("Test4 is done");
    }
    //等待下一帧
    IEnumerator<ulong> Test5()
    {
        GD.Print("Hello I am Test5");
        yield return Coroutine.WaitForNextFrame();
        GD.Print("Test5 is done");
    }
    //等待物理帧
    IEnumerator<ulong> Test6()
    {
        GD.Print("Hello I am Test6");
        yield return Coroutine.WaitForNextPhysicsFrame();
        GD.Print("Test6 is done");
    }
}