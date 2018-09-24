using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private GameState m_GameState = GameState.Play;
    public enum GameState { Play, GameOver };
    public delegate void OnStateChangeDelegate(GameState state);
    public OnStateChangeDelegate OnStateChange;

    private int m_Score = 0;
    private UnityEngine.UI.Text m_ScoreReadout;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
        m_ScoreReadout = GameObject.FindGameObjectWithTag("Score").GetComponent<UnityEngine.UI.Text>();
    }

    public void ModifyScore(int change)
    {
        m_Score++;
        if (m_ScoreReadout != null)
            m_ScoreReadout.text = m_Score.ToString(); 
    }

    public int GetScore()
    {
        return m_Score;
    }

    public void SetGameState(GameState newState)
    {
        m_GameState = newState;
        if (OnStateChange != null)
        {
            OnStateChange(m_GameState);
            if (m_GameState == GameState.Play)
                m_Score = 0;
        }

    }
    public GameState GetGameState()
    {
        return m_GameState;
    }
}