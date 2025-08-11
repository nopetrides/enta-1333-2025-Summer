using UnityEngine;
using System;


public enum GameState // all possible high level game states
{
    MainMenu,
    Playing,
    Paused,
    Win,
    Lose
}

public class GameStateManager : MonoBehaviour  // singleton that tracks the current gamestate and notifies listeners when it changes



{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

   
    public event Action<GameState> OnGameStateChanged;

    void Awake()
    {
       
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

  
    public void SetState(GameState newState) // change the gamestate and notify subscribers
    {
        if (newState == CurrentState) return;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
