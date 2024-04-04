using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonPersistance<T> : MonoBehaviour, ISingletonPresistance where T : MonoBehaviour
{
    public GameObject GetGameObject
    {
        get
        {
            return gameObject;
        }
    }
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }
    protected static T _instance;
    protected bool _isInit;
    protected virtual void Awake()
    {
        _isInit = false;
        if (Instance == null)
        {
            _instance = this as T;
            _isInit = true;
            DontDestroyOnLoad(this);
        }
        else
        {
            _isInit = false;
            Destroy(this.gameObject);
            return;
        }
    }
    protected virtual void OnDestroy()
    {

    }
    public abstract IEnumerator DoActionOnAwake(System.Action onCompleted = null);


}
public interface ISingletonPresistance
{
    IEnumerator DoActionOnAwake(Action onCompleted = null);
    GameObject GetGameObject { get; }
}
