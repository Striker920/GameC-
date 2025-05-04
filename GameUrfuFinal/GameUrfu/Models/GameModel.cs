using System;
using System.Collections.Generic;
using System.Drawing;

namespace BeaconGame.Models
{
    public class GameModel
    {
        // Константы
        public const int FieldCellSize = 20;
        public const int PlayerCellSize = 10;
        public const int CubeSize = 10;
        public const int EnemySize = 15;
        public const int BuildingSize = 4;
        public const int EnemyDamage = 10;
        public const int DamageInterval = 5000;

        public const int InitialAmmo = 10;
        public const int AmmoCost = 1;
        public const int ShootingRange = 200;
        public const int EnemySpawnInterval = 3000;
        public const int BarricadeHealth = 10;
        public const int WoodBarricadeCost = 10;
        public const int MetalBarricadeCost = 5;
        public const int StoneBarricadeCost = 5;
        public const int WoodHarvestAmount = 10;
        public const int MetalHarvestAmount = 5;
        public const int StoneHarvestAmount = 7;
        public const int MaxResources = 150;
        public const int AmmoHarvestAmount = 50;
        public const int MaxBeaconHealth = 100;
        public const int WoodRepairCost = 20;
        public const int MetalRepairCost = 10;

        // Константы для создания боеприпасов
        public const int WoodAmmoCost = 5;
        public const int MetalAmmoCost = 10;
        public const int StoneAmmoCost = 5;
        public const int AmmoCreateAmount = 20;
        public const int MaxAmmo = 100;

        public List<Point> PlayerBody { get; set; }
        public Direction CurrentDirection { get; set; }
        public bool GameOver { get; set; }
        public int Wood { get; set; }
        public int Metal { get; set; }
        public int Stone { get; set; }
        public int Ammo { get; set; }
        public int BeaconHealth { get; set; }
        public List<Point> Enemies { get; set; }
        public List<Barricade> Barricades { get; set; }
        public Random Random { get; set; }

        public int SpecialAreaStartX { get; set; }
        public int SpecialAreaStartY { get; set; }
        public int StoneAreaStartX { get; set; }
        public int StoneAreaStartY { get; set; }
        public int IronAreaStartX { get; set; }
        public int IronAreaStartY { get; set; }
        public int IronAreaWidth { get; set; }
        public int IronAreaHeight { get; set; }

        public GameModel()
        {
            PlayerBody = new List<Point>();
            Enemies = new List<Point>();
            Barricades = new List<Barricade>();
            Random = new Random();

            Wood = 100;
            Metal = 50;
            Stone = 75;
            Ammo = InitialAmmo;
            BeaconHealth = MaxBeaconHealth;
            IronAreaWidth = FieldCellSize;
            IronAreaHeight = FieldCellSize;
        }
        public GameModel(int param1, int param2) : this()
        {
        }
    }

    public class Barricade
    {
        public Point Location;
        public int Health;
        public Barricade(Point location)
        {
            Location = location;
            Health = GameModel.BarricadeHealth;
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}