using UnityEngine;

public enum TriggerType {
    OnDraw,
    OnSummon,
    OnSet,
    OnDestroy,
    OnSentToGY,
    OnAttack,
    OnAttacked,
    OnMove,
    OnCardAdd,
    OnTakeDamage,
    OnEffectActivate,

    OnTurnStart,
    OnTurnEnd,

    OnCardTargeted,

    Passive,
    NoTrigger,
}
