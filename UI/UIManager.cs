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
            // _availableCharacters.Add("Образец модели", new ModelInfo("Образец", "Assets/Models/sample.obj", false)); // Removed character selection
            // _availableCharacters.Add("Сфера", new ModelInfo("Сфера", "sphere", true)); // Removed character selection
            
            // Default character if needed, though selection is removed
             _availableCharacters.Add("Стандартный", new ModelInfo("Стандартный", "Assets/Models/sample.obj", false));
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
            
            ImGui.Begin("Главное Меню", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse); // Title changed
            
            // Title
            ImGui.SetCursorPosX((windowSize.X - ImGui.CalcTextSize("Лабиринт").X) * 0.5f);
            ImGui.Text("Лабиринт");
            ImGui.Separator();
            
            // Stats display
            ImGui.Text($"Лучшее время: {_gameStateManager.GetFormattedSurvivalTime(_gameStateManager.LongestSurvivalTime)}"); // Changed text, removed score
            ImGui.Spacing();
            
            // Character selection REMOVED
            // ImGui.Text("Выберите персонажа:");
            // if (ImGui.BeginCombo("##CharacterCombo", _selectedCharacter))
            // {
            //     foreach (var character in _availableCharacters.Keys)
            //     {
            //         bool isSelected = (character == _selectedCharacter);
            //         if (ImGui.Selectable(character, isSelected))
            //         {
            //             _selectedCharacter = character;
            //         }
                    
            //         if (isSelected)
            //         {
            //             ImGui.SetItemDefaultFocus();
            //         }
            //     }
            //     ImGui.EndCombo();
            // }
            
            // Display character properties REMOVED
            // if (_availableCharacters.TryGetValue(_selectedCharacter, out ModelInfo modelInfo))
            // {
            //     ImGui.Text($"Движение: {(modelInfo.CanFly ? "Полет" : "Земля")}");
            //     ImGui.Text($"Путь к модели: {modelInfo.FilePath}");
            // }
            
            ImGui.Spacing();
            ImGui.Separator();
            
            // Start Game Button (centered)
            float buttonWidth = 200;
            ImGui.SetCursorPosX((windowSize.X - buttonWidth) * 0.5f);
            if (ImGui.Button("Начать Игру", new System.Numerics.Vector2(buttonWidth, 50))) // Changed text
            {
                _gameStateManager.ChangeState(GameState.Playing);
            }
            
            // Exit Button (centered)
            ImGui.SetCursorPosX((windowSize.X - buttonWidth) * 0.5f);
            if (ImGui.Button("Выход", new System.Numerics.Vector2(buttonWidth, 30)))
            {
                // This will be handled externally to close the application
                Environment.Exit(0);
            }
            
            ImGui.End();
        }
        
        // Render game UI during gameplay
        public void RenderGameUI()
        {
            // Убрано ImGuiWindowFlags.NoDecoration для отладки, чтобы видеть окно
            ImGuiWindowFlags windowFlags = ImGuiWindowFlags.AlwaysAutoResize | 
                                           ImGuiWindowFlags.NoSavedSettings | 
                                           ImGuiWindowFlags.NoFocusOnAppearing | 
                                           ImGuiWindowFlags.NoNav | 
                                           ImGuiWindowFlags.NoMove;
            
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10));
            ImGui.Begin("Игровой Интерфейс (Отладка)", windowFlags); // Изменен заголовок для ясности
            
            ImGui.Text("bombini gusine"); // Добавлена тестовая строка

            // Update and display survival time during gameplay
            _gameStateManager.UpdateSurvivalTime();
            ImGui.Text($"Время: {_gameStateManager.GetFormattedSurvivalTime(_gameStateManager.CurrentSurvivalTime)}");
            
            if (ImGui.Button("В Меню", new System.Numerics.Vector2(120, 0)))
            {
                _gameStateManager.ChangeState(GameState.GameOver); // Go to GameOver first to save stats
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
            
            ImGui.Begin("Игра Окончена", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse); // Title changed
            
            // Title
            ImGui.SetCursorPosX((windowSize.X - ImGui.CalcTextSize("Игра Окончена").X) * 0.5f); // Changed text
            ImGui.Text("Игра Окончена"); // Changed text
            ImGui.Separator();
            
            // Results
            ImGui.Text($"Время выживания: {_gameStateManager.GetFormattedSurvivalTime(_gameStateManager.CurrentSurvivalTime)}"); // Removed score
            
            ImGui.Spacing();
            ImGui.Separator();
            
            // Return to menu button
            float buttonWidth = 200;
            ImGui.SetCursorPosX((windowSize.X - buttonWidth) * 0.5f);
            if (ImGui.Button("В Меню", new System.Numerics.Vector2(buttonWidth, 40))) // Changed text
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