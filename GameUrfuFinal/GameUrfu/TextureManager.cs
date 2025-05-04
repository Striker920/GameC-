using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GameUrfu
{
    public static class TextureManager
    {
        // Константы
        public const string WATER_TEXTURE = "water";
        public const string LAND_TEXTURE = "land";
        public const string WOOD_TEXTURE = "wood";
        public const string IRON_TEXTURE = "iron";
        public const string STONE_TEXTURE = "stone";
        public const string BEACON_TEXTURE = "beacon";
        public const string PLAYER_TEXTURE = "player";
        public const string ENEMY_TEXTURE = "enemy";
        public const string BARRICADE_TEXTURE = "barricade";

        //хранение текстур
        private static Dictionary<string, Bitmap> _textures = new Dictionary<string, Bitmap>();
        
        private static string _texturePath;

        // Инициализация текстур
        public static void Initialize()
        {
            try
            {
                _texturePath = Path.Combine(Application.StartupPath, "Textures");

                if (!Directory.Exists(_texturePath))
                {
                    Directory.CreateDirectory(_texturePath);
                }

                EnsureTexturesExist();

                LoadTextures();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации текстур: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                CreateFallbackTextures();
            }
        }

        private static void EnsureTexturesExist()
        {
            bool needToGenerateTextures = false;

            // Список необходимых текстур
            string[] requiredTextures = new string[]
            {
                $"{WATER_TEXTURE}.png",
                $"{LAND_TEXTURE}.png",
                $"{WOOD_TEXTURE}.png",
                $"{IRON_TEXTURE}.png",
                $"{STONE_TEXTURE}.png",
                $"{BEACON_TEXTURE}.png",
                $"{PLAYER_TEXTURE}.png",
                $"{ENEMY_TEXTURE}.png",
                $"{BARRICADE_TEXTURE}.png"
            };

            // Проверка текстур
            foreach (string texture in requiredTextures)
            {
                if (!File.Exists(Path.Combine(_texturePath, texture)))
                {
                    needToGenerateTextures = true;
                    break;
                }
            }

            if (needToGenerateTextures)
            {
                GenerateTextures();
            }
        }

        // Генерация текстур
        private static void GenerateTextures()
        {
            try
            {
                TextureGenerator.GenerateAndSaveAllTextures(_texturePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации текстур: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private static void LoadTextures()
        {
            try
            {
                // Чистка словаря текстур
                foreach (var texture in _textures.Values)
                {
                    texture.Dispose();
                }
                _textures.Clear();
                
                // Загрузка всех текстуры
                _textures[WATER_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{WATER_TEXTURE}.png"));
                _textures[LAND_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{LAND_TEXTURE}.png"));
                _textures[WOOD_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{WOOD_TEXTURE}.png"));
                _textures[IRON_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{IRON_TEXTURE}.png"));
                _textures[STONE_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{STONE_TEXTURE}.png"));
                _textures[BEACON_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{BEACON_TEXTURE}.png"));
                _textures[PLAYER_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{PLAYER_TEXTURE}.png"));
                _textures[ENEMY_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{ENEMY_TEXTURE}.png"));
                _textures[BARRICADE_TEXTURE] = new Bitmap(Path.Combine(_texturePath, $"{BARRICADE_TEXTURE}.png"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке текстур: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // Создание заглушек для всех текстур
        private static void CreateFallbackTextures()
        {
            _textures.Clear();
            
            _textures[WATER_TEXTURE] = CreateFallbackTexture(Color.Blue);
            _textures[LAND_TEXTURE] = CreateFallbackTexture(Color.Brown);
            _textures[WOOD_TEXTURE] = CreateFallbackTexture(Color.Yellow);
            _textures[IRON_TEXTURE] = CreateFallbackTexture(Color.Purple);
            _textures[STONE_TEXTURE] = CreateFallbackTexture(Color.Orange);
            _textures[BEACON_TEXTURE] = CreateFallbackTexture(Color.White);
            _textures[PLAYER_TEXTURE] = CreateFallbackTexture(Color.Green);
            _textures[ENEMY_TEXTURE] = CreateFallbackTexture(Color.Red);
            _textures[BARRICADE_TEXTURE] = CreateFallbackTexture(Color.Gray);
        }

        public static Bitmap GetTexture(string textureName)
        {
            if (_textures.ContainsKey(textureName))
            {
                return _textures[textureName];
            }
            
            return CreateFallbackTexture(Color.Magenta);
        }

        // Создание текстуры-заглушки
        private static Bitmap CreateFallbackTexture(Color baseColor)
        {
            Bitmap fallback = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(fallback))
            {
                g.Clear(baseColor);
                
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.DrawLine(pen, 0, 0, 32, 32);
                    g.DrawLine(pen, 0, 32, 32, 0);
                }
            }
            return fallback;
        }

        public static void Dispose()
        {
            foreach (var texture in _textures.Values)
            {
                texture.Dispose();
            }
            _textures.Clear();
        }
    }
}