using System;
using Microsoft.Extensions.Logging;

namespace Simple3DGame.Core
{
    // Enum defining the possible game states
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
    
    // Class responsible for managing the game state and transitions between states
    public class GameStateManager
    {
        // Current game state
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        
        // Event triggered when game state changes - marked as nullable
        public event Action<GameState>? OnGameStateChanged;
        
        // Game statistics
        public int CurrentScore { get; private set; } = 0;
        public TimeSpan CurrentSurvivalTime { get; private set; } = TimeSpan.Zero;
        public TimeSpan LongestSurvivalTime { get; private set; } = TimeSpan.Zero;
        public int HighScore { get; private set; } = 0;
        
        // Timing
        private DateTime _gameStartTime;
        private readonly ILogger _logger;
        
        // Updated constructor to accept ILogger instead of specific generic type
        public GameStateManager(ILogger logger)
        {
            _logger = logger;
            // Load high score and longest survival time from config or file if needed
            // For now, we'll just initialize them to 0
        }
        
        // Change the current game state and trigger the OnGameStateChanged event
        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState) return;
            
            GameState oldState = CurrentState;
            CurrentState = newState;
            
            // Handle state-specific logic
            switch (newState)
            {
                case GameState.MainMenu:
                    // If coming from gameplay, update statistics
                    if (oldState == GameState.Playing || oldState == GameState.GameOver)
                    {
                        UpdateGameStatistics();
                    }
                    break;
                    
                case GameState.Playing:
                    // Reset current game statistics and start timer
                    CurrentScore = 0;
                    CurrentSurvivalTime = TimeSpan.Zero;
                    _gameStartTime = DateTime.Now;
                    break;
                    
                case GameState.GameOver:
                    // Calculate final survival time
                    UpdateSurvivalTime();
                    break;
            }
            
            _logger.LogInformation($"Game state changed from {oldState} to {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
        
        // Update the current survival time
        public void UpdateSurvivalTime()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentSurvivalTime = DateTime.Now - _gameStartTime;
            }
        }
        
        // Increment the score when collecting a yellow sphere
        public void IncrementScore(int amount = 1)
        {
            CurrentScore += amount;
            _logger.LogInformation($"Score increased by {amount}. New score: {CurrentScore}");
        }
        
        // Update game statistics (high score, longest survival time)
        private void UpdateGameStatistics()
        {
            // Update high score if current score is higher
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                _logger.LogInformation($"New high score: {HighScore}");
            }
            
            // Update longest survival time if current time is longer
            if (CurrentSurvivalTime > LongestSurvivalTime)
            {
                LongestSurvivalTime = CurrentSurvivalTime;
                _logger.LogInformation($"New longest survival time: {LongestSurvivalTime}");
            }
            
            // Here you could also save the statistics to a file or database
        }
        
        // Get formatted survival time string (for display)
        public string GetFormattedSurvivalTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}.{time.Milliseconds/10:D2}";
        }
    }
}