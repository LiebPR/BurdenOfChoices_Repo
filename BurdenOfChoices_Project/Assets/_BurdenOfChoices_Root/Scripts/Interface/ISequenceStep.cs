using System;

public interface ISequenceStep
{
    void Play(Action onFinished);
}
