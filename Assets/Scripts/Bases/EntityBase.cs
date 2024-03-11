using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBase : MonoBehaviour {

    public abstract int Hp { get; set; }
    public virtual bool Active
    {
        get { return gameObject.activeInHierarchy; }
        set { gameObject.SetActive(value); }
    }
    public abstract void AiFrame();
    public abstract EntityBase Spawn(string[] args, Vector2 spawnPos);
    public abstract void Despawn();
    public abstract void Kill(string[] args);
    public abstract string[] GenerateArgs();
}

public abstract class UnitBase : EntityBase
{
    public abstract bool PositionControlled { get; set; }
    public abstract bool RotationControlled { get; set; }
    public abstract bool IsLocal { get; set; }
    public abstract void SetTargetPosition(Vector2 targetPos);
    public abstract Vector2 GetTargetPosition();
}