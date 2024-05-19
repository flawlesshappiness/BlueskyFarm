using System;
using System.Collections;

public class Coroutine : CustomYieldInstruction
{
    public Guid Id { get; set; }

    public IEnumerator Enumerator { get; set; }

    public bool HasCompleted { get; set; }
    public bool HasEnded { get; set; }

    public override bool KeepWaiting => !HasEnded;

    public Coroutine(IEnumerator enumerator)
    {
        Enumerator = enumerator;
    }

    public static Coroutine Start(IEnumerator enumerator)
    {
        var coroutine = new Coroutine(enumerator);
        CoroutineHandler.Instance.AddCoroutine(coroutine);
        return coroutine;
    }

    public static Coroutine Start(Func<IEnumerator> enumerator) =>
        Start(enumerator());

    public static bool Stop(Coroutine coroutine)
    {
        if (coroutine == null) return true;

        coroutine.HasEnded = true;
        CoroutineHandler.Instance.RemoveCoroutine(coroutine);
        return true;
    }

    public void UpdateFrame()
    {
        if (!TryMoveEnumerator(Enumerator))
        {
            HasEnded = true;
            HasCompleted = true;
        }
    }

    private bool TryMoveEnumerator(IEnumerator enumerator)
    {
        var new_enumerator = enumerator.Current != null ? enumerator.Current as IEnumerator : null;
        if (new_enumerator != null)
        {
            if (TryMoveEnumerator(new_enumerator))
            {
                return true;
            }
        }

        return enumerator.MoveNext();
    }

}
