using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Adding System.Linq for LINQ extension methods
using ImGuiNET;
using OpenTK.Mathematics;
using Simple3DGame.Core;
using Simple3DGame.Models;
using System.Numerics;

namespace Simple3DGame.UI
{
    public class UIManager
    {
        private readonly GameStateManager _gameStateManager;
        private readonly Dictionary<string, ModelInfo> _availableCharacters = new Dictionary<string, ModelInfo>();
        private string _selectedCharacter;
        
        // Structure to store model information
        public class ModelInfo
        {
            public string Name { get; set; }
            public string FilePath { get; set; }
            public bool CanFly { get; set; }
            
            public ModelInfo(string name, string filePath, bool canFly)
            {
                Name = name;
                FilePath = filePath;
                CanFly = canFly;
            }
        }
        
        public UIManager(GameStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
            LoadAvailableCharacters();
            
            // Select the first character by default
            if (_availableCharacters.Count > 0)
            {
                _selectedCharacter = _availableCharacters.Keys.First();
            }
            else
            {
                _selectedCharacter = string.Empty;
            }
        }
        
        // Load available character models from the models directory
        private void LoadAvailableCharacters()
        {
            // These would typically be loaded from a configuration file or by scanning a directory
            // For now, let's add some predefined models
            _availableCharacters.Add("Sample Model", new ModelInfo("Sample", "Assets/Models/sample.obj", false));
            _availableCharacters.Add("Sphere", new ModelInfo("Sphere", "sphere", true));
            
            // You would scan a directory like this:
            // Directory.GetFiles("Assets/Models", "*.obj").ForEach(file => 
            //     _availableCharacters.Add(Path.GetFileNameWithoutExtension(file), 
            //     new ModelInfo(Path.GetFileNameWithoutExtension(file), file, false)));
        }
        
        // Render the main menu UI
        public void RenderMainMenu()
        {
            // Center the window on the screen
            ImGuiIOPtr io = ImGui.GetIO();
            System.Numerics.Vector2 windowSize = new System.Numerics.Vector2(400, 400);
            System.Numerics.Vector2 windowPos = new System.Numerics.Vector2(
                (io.DisplaySize.X - windowSize.X) * 0.5f,
                (io.DisplaySize.Y - windowSize.Y) * 0.5f
            );
            
            ImGui.SetNextWindowPos(windowPos);
            ImGui.SetNextWindowSize(windowSize);
            
            ImGui.Begin("Maze Runner", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
            
            // Title
            ImGui.SetCursorPosX((windowSize.X - ImGui.CalcTextSize("MAZE RUNNER").X) * 0.5f);
            ImGui.Text("MAZE RUNNER");
            ImGui.Separator();
            
            // Stats display
            ImGui.Text($"High Score: {_gameStateManager.HighScore}");
            ImGui.Text($"Longest Survival: {_gameStateManager.GetFormattedSurvivalTime(_gameStateManager.LongestSurvivalTime)}");
            ImGui.Spacing();
            
            // Character selection
            ImGui.Text("Select Character:");
            if (ImGui.BeginCombo("##CharacterCombo", _selectedCharacter))
            {
                foreach (var character in _availableCharacters.Keys)
                {
                    bool isSelected = (character == _selectedCharacter);
                    if (ImGui.Selectable(character, isSelected))
                    {
                        _selectedCharacter = character;
                    }
                    
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
            
            // Display character properties
            if (_availableCharacters.TryGetValue(_selectedCharacter, out ModelInfo modelInfo))
            {
                ImGui.Text($"Movement: {(modelInfo.CanFly ? "Flying" : "Ground")}");
                ImGui.Text($"Model Path: {modelInfo.FilePath}");
            }
            
            ImGui.Spacing();
            ImGui.Separator();
            
            // Start Game Button (centered)
            float buttonWidth = 200;
            ImGui.SetCursorPosX((windowSize.X - buttonWidth) * 0.5f);
            if (ImGui.Button("Start Game", new System.Numerics.Vector2(buttonWidth, 50)))
            {
                _gameStateManager.ChangeState(GameState.Playing);
            }
            
            // Exit Button (centered)
            ImGui.SetCursorPosX((windowSize.X - buttonWidth) * 0.5f);
            if (ImGui.Button("Exit", new System.Numerics.Vector2(buttonWidth, 30)))
            {
                // This will be handled externally to close the application
                Environment.Exit(0);
            }
            
            ImGui.End();
        }
        
        // Render game UI during gameplay
        public void RenderGameUI()
        {
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | 
                                           ImGuiWindowFlags.AlwaysAutoResize | 
                                           ImGuiWindowFlags.NoSavedSettings | 
                                           ImGuiWindowFlags.NoFocusOnAppearing | 
                                           ImGuiWindowFlags.NoNav | 
                                           ImGuiWindowFlags.NoMove;
            
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10));
            ImGui.Begin("Game Info", windowFlags);
            ImGui.Text($"Score: {_gameStateManager.CurrentScore}");
            
            // Update and display survival time during gameplay
            _gameStateManager.UpdateSurvivalTime();
            ImGui.Text($"Time: {_gameStateManager.GetFormattedSurvivalTime(_gameStateManager.CurrentSurvivalTime)}");
            
            if (ImGui.Button("Return to Menu"))
            {
                _gameStateManager.ChangeState(GameState.GameOver);
                _gameStateManager.ChangeState(GameState.MainMenu);
            }
            
            ImGui.End();
        }
        
        // Render game over UI
        public void RenderGameOverUI()
        {
            // Center the window on the screen
            ImGuiIOPtr io = ImGui.GetIO();
            System.Numerics.Vector2 windowSize = new System.Numerics.Vector2(400, 300);
            System.Numerics.Vector2 windowPos = new System.Numerics.Vector2(
                (io.DisplaySize.X - windowSize.X) * 0.5f,
                (io.DisplaySize.Y - windowSize.Y) * 0.5f
            );
            
            ImGui.SetNextWindowPos(windowPos);
            ImGui.SetNextWindowSize(windowSize);
            
            ImGui.Begin("Game Over", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
            
            // Title
            ImGui.SetCursorPosX((windowSize.X - ImGui.CalcTextSize("GAME OVER").X) * 0.5f);
            ImGui.Text("GAME OVER");
            ImGui.Separator();
            
            // Results
            ImGui.Text($"Final Score: {_gameStateManager.CurrentScore}");
            ImGui.Text($"Survival Time: {_gameStateManager.GetFormattedSurvivalTime(_gameStateManager.CurrentSurvivalTime)}");
            
            ImGui.Spacing();
            ImGui.Separator();
            
            // Return to menu button
            float buttonWidth = 200;
            ImGui.SetCursorPosX((windowSize.X - buttonWidth) * 0.5f);
            if (ImGui.Button("Return to Menu", new System.Numerics.Vector2(buttonWidth, 40)))
            {
                _gameStateManager.ChangeState(GameState.MainMenu);
            }
            
            ImGui.End();
        }
        
        // Render UI based on current game state
        public void RenderUI()
        {
            switch (_gameStateManager.CurrentState)
            {
                case GameState.MainMenu:
                    RenderMainMenu();
                    break;
                
                case GameState.Playing:
                    RenderGameUI();
                    break;
                
                case GameState.GameOver:
                    RenderGameOverUI();
                    break;
            }
        }
        
        // Get the currently selected character model info
        public ModelInfo GetSelectedCharacter()
        {
            if (_availableCharacters.TryGetValue(_selectedCharacter, out ModelInfo modelInfo))
            {
                return modelInfo;
            }
            
            // Fallback to the first character or a default
            return _availableCharacters.Values.FirstOrDefault() ?? new ModelInfo("Default", "Assets/Models/sample.obj", false);
        }
    }
}