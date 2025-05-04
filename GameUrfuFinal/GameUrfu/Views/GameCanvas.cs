using System;
using System.Drawing;
using System.Windows.Forms;
using BeaconGame.Models;

namespace GameUrfu
{
    public class GameCanvas : Panel
    {
        private GameModel _model;

        public GameCanvas(GameModel model)
        {
            _model = model;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            int gameWidth = Width;

            // вода
            for (int x = 0; x < gameWidth; x += GameModel.FieldCellSize)
            {
                for (int y = 0; y < Height; y += GameModel.FieldCellSize)
                {
                    g.DrawImage(TextureManager.GetTexture(TextureManager.WATER_TEXTURE),
                        x, y, GameModel.FieldCellSize, GameModel.FieldCellSize);
                }
            }

            int numCellsX = gameWidth / GameModel.FieldCellSize;
            int numCellsY = Height / GameModel.FieldCellSize;
            int centerX = numCellsX / 2;
            int centerY = numCellsY / 2;
            int startX = (centerX - (GameModel.CubeSize / 2)) * GameModel.FieldCellSize;
            int startY = (centerY - (GameModel.CubeSize / 2)) * GameModel.FieldCellSize;

            // земля
            for (int x = 0; x < GameModel.CubeSize; x++)
            {
                for (int y = 0; y < GameModel.CubeSize; y++)
                {
                    g.DrawImage(TextureManager.GetTexture(TextureManager.LAND_TEXTURE),
                        startX + x * GameModel.FieldCellSize,
                        startY + y * GameModel.FieldCellSize,
                        GameModel.FieldCellSize, GameModel.FieldCellSize);
                }
            }

            // Дерево
            g.DrawImage(TextureManager.GetTexture(TextureManager.WOOD_TEXTURE),
                _model.SpecialAreaStartX, _model.SpecialAreaStartY,
                _model.IronAreaWidth, _model.IronAreaHeight);

            // Камень
            g.DrawImage(TextureManager.GetTexture(TextureManager.STONE_TEXTURE),
                _model.StoneAreaStartX, _model.StoneAreaStartY,
                _model.IronAreaWidth, _model.IronAreaHeight);

            // Железо
            g.DrawImage(TextureManager.GetTexture(TextureManager.IRON_TEXTURE),
                _model.IronAreaStartX, _model.IronAreaStartY,
                _model.IronAreaWidth, _model.IronAreaHeight);

            // маяк
            int buildingStartX = startX + (GameModel.CubeSize / 2 - GameModel.BuildingSize / 2) * GameModel.FieldCellSize;
            int buildingStartY = startY + (GameModel.CubeSize / 2 - GameModel.BuildingSize / 2) * GameModel.FieldCellSize;
            g.DrawImage(TextureManager.GetTexture(TextureManager.BEACON_TEXTURE),
                buildingStartX, buildingStartY,
                GameModel.BuildingSize * GameModel.FieldCellSize,
                GameModel.BuildingSize * GameModel.FieldCellSize);

            // сетка
            using (Pen gridPen = new Pen(Color.FromArgb(50, Color.Gray)))
            {
                for (int x = 0; x < gameWidth; x += GameModel.FieldCellSize)
                {
                    g.DrawLine(gridPen, x, 0, x, Height);
                }
                for (int y = 0; y < Height; y += GameModel.FieldCellSize)
                {
                    g.DrawLine(gridPen, 0, y, gameWidth, y);
                }
            }

            // игрок
            if (_model.PlayerBody.Count > 0)
            {
                Point head = _model.PlayerBody[0];
                int offsetX = (GameModel.FieldCellSize - GameModel.PlayerCellSize) / 2;
                int offsetY = (GameModel.FieldCellSize - GameModel.PlayerCellSize) / 2;
                g.DrawImage(TextureManager.GetTexture(TextureManager.PLAYER_TEXTURE),
                    head.X + offsetX, head.Y + offsetY,
                    GameModel.PlayerCellSize, GameModel.PlayerCellSize);
            }

            // враги
            foreach (Point enemy in _model.Enemies)
            {
                g.DrawImage(TextureManager.GetTexture(TextureManager.ENEMY_TEXTURE),
                    enemy.X, enemy.Y, GameModel.EnemySize, GameModel.EnemySize);
            }

            // баррикады
            foreach (var barricade in _model.Barricades)
            {
                g.DrawImage(TextureManager.GetTexture(TextureManager.BARRICADE_TEXTURE),
                    barricade.Location.X, barricade.Location.Y,
                    GameModel.FieldCellSize, GameModel.FieldCellSize);
            }
        }
    }
}