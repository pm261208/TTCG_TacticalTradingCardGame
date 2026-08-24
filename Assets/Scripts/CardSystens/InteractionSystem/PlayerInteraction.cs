using System.Collections;
using UnityEngine;

public abstract class PlayerInteraction {

    public bool IsFinished { get; protected set; }
    public bool IsCanceled { get; protected set; }
    public bool CanCancel { get; protected set; }

    public abstract void OnEnter();
    public abstract void OnExit();

    public abstract void OnClickCard(Card card);
    public abstract void OnClickZone(Tile tile);
    public abstract void OnClickButton(TempButton button);
    public abstract void TryCancel();

    public virtual IEnumerator WaitForFinish() {
        yield return new WaitUntil(() => IsFinished);
    }

    protected void Finish() {
        InteractionSystem.Instance.StartInteraction(null);
        IsFinished = true;
    }
}
