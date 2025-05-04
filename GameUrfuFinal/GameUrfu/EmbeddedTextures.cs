using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace GameUrfu
{
    public static class EmbeddedTextures
    {
        public static void InitializeTextures()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "GameUrfuTextures");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // Генерация текстуры
            TextureGenerator.GenerateAndSaveAllTextures(tempDir);

            // Загрузка в память
            LoadTexturesFromDirectory(tempDir);

            // Удаление временных файлов
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Игнорор
            }
        }

        private static void LoadTexturesFromDirectory(string directory)
        {

            Water = new Bitmap(Path.Combine(directory, "water.png"));
            Land = new Bitmap(Path.Combine(directory, "land.png"));
            Wood = new Bitmap(Path.Combine(directory, "wood.png"));
            Iron = new Bitmap(Path.Combine(directory, "iron.png"));
            Stone = new Bitmap(Path.Combine(directory, "stone.png"));
            Beacon = new Bitmap(Path.Combine(directory, "beacon.png"));
            Player = new Bitmap(Path.Combine(directory, "player.png"));
            Enemy = new Bitmap(Path.Combine(directory, "enemy.png"));
            Barricade = new Bitmap(Path.Combine(directory, "barricade.png"));
        }

        public static Bitmap Water { get; private set; }
        public static Bitmap Land { get; private set; }
        public static Bitmap Wood { get; private set; }
        public static Bitmap Iron { get; private set; }
        public static Bitmap Stone { get; private set; }
        public static Bitmap Beacon { get; private set; }
        public static Bitmap Player { get; private set; }
        public static Bitmap Enemy { get; private set; }
        public static Bitmap Barricade { get; private set; }

        public static void Dispose()
        {
            Water?.Dispose();
            Land?.Dispose();
            Wood?.Dispose();
            Iron?.Dispose();
            Stone?.Dispose();
            Beacon?.Dispose();
            Player?.Dispose();
            Enemy?.Dispose();
            Barricade?.Dispose();
        }
    }
}