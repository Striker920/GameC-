using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace GameUrfu
{
    public static class TextureGenerator
    {
        private static Random random = new Random();

        // Генерация всех текстур
        public static void GenerateAndSaveAllTextures(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (Bitmap playerTexture = GeneratePlayerTexture(16, 16))
            {
                playerTexture.Save(Path.Combine(directoryPath, "player.png"), ImageFormat.Png);
            }

            using (Bitmap enemyTexture = GenerateEnemyTexture(16, 16))
            {
                enemyTexture.Save(Path.Combine(directoryPath, "enemy.png"), ImageFormat.Png);
            }
        }

        // Генерация текстуры игрока
        public static Bitmap GeneratePlayerTexture(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // тело
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 100, 0))) // Темно-зеленый
                {
                    g.FillEllipse(brush, 0, 0, width, height);
                }

                // глаза
                int eyeSize = width / 5;
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.FillEllipse(brush, width / 3 - eyeSize / 2, height / 3 - eyeSize / 2, eyeSize, eyeSize);
                    g.FillEllipse(brush, width * 2 / 3 - eyeSize / 2, height / 3 - eyeSize / 2, eyeSize, eyeSize);
                }

                // зрачки
                int pupilSize = eyeSize / 2;
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(brush, width / 3 - pupilSize / 2, height / 3 - pupilSize / 2, pupilSize, pupilSize);
                    g.FillEllipse(brush, width * 2 / 3 - pupilSize / 2, height / 3 - pupilSize / 2, pupilSize, pupilSize);
                }

                // рот
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.DrawArc(pen, width / 4, height / 2, width / 2, height / 4, 0, 180);
                }
            }
            return bitmap;
        }

        // Генерация врага
        public static Bitmap GenerateEnemyTexture(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // тело врага
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, 0, 0))) // Красный
                {
                    g.FillEllipse(brush, 0, 0, width, height);
                }

                // глаза
                int eyeSize = width / 5;
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.FillEllipse(brush, width / 3 - eyeSize / 2, height / 3 - eyeSize / 2, eyeSize, eyeSize);
                    g.FillEllipse(brush, width * 2 / 3 - eyeSize / 2, height / 3 - eyeSize / 2, eyeSize, eyeSize);
                }

                // зрачки
                int pupilSize = eyeSize / 2;
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    g.FillEllipse(brush, width / 3 - pupilSize / 2, height / 3 - pupilSize / 2, pupilSize, pupilSize);
                    g.FillEllipse(brush, width * 2 / 3 - pupilSize / 2, height / 3 - pupilSize / 2, pupilSize, pupilSize);
                }

                //рот
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.DrawArc(pen, width / 4, height * 2 / 3, width / 2, height / 4, 180, 180);
                }

                //шипы
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, 0, 0)))
                {
                    g.FillPolygon(brush, new Point[] {
                        new Point(width/2, 0),
                        new Point(width/3, height/4),
                        new Point(width*2/3, height/4)
                    });
                }
            }
            return bitmap;
        }
    }
}