using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BeaconGame.Models;
using BeaconGame.Views;

namespace BeaconGame.Controllers
{
    public class GameController : IDisposable
    {
        private GameModel _model; 
        private System.Windows.Forms.Timer _gameTimer; 
        private System.Windows.Forms.Timer _damageTimer; 
        private System.Windows.Forms.Timer _enemySpawnTimer; 
        private System.Windows.Forms.Timer _resourceHarvestTimer; 
        private int _clientWidth; 
        private int _clientHeight;
        internal Action<object, EventArgs> GameOver; 

        public event EventHandler GameUpdated; 
        public event EventHandler GameOverEvent; 

        public GameController(GameModel model)
        {
            _model = model;
            InitializeTimers(); 
        }

        // Инициализация игровых таймеров
        private void InitializeTimers()
        {
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 150; // Интервал основного цикла
            _gameTimer.Tick += GameTimer_Tick;

            _damageTimer = new System.Windows.Forms.Timer();
            _damageTimer.Interval = GameModel.DamageInterval; // Интервал нанесения урона
            _damageTimer.Tick += DamageTimer_Tick;

            _enemySpawnTimer = new System.Windows.Forms.Timer();
            _enemySpawnTimer.Interval = GameModel.EnemySpawnInterval; // Интервал спавна врагов
            _enemySpawnTimer.Tick += EnemySpawnTimer_Tick;

            _resourceHarvestTimer = new System.Windows.Forms.Timer();
            _resourceHarvestTimer.Interval = 1000; // Интервал сбора ресурсов
        }

        public void StartGame(int clientWidth, int clientHeight)
        {
            _clientWidth = clientWidth;
            _clientHeight = clientHeight;
            InitializeGame(); 
            _enemySpawnTimer.Start(); 
            _gameTimer.Start();
            _damageTimer.Start();
            _resourceHarvestTimer.Start();
            OnGameUpdated();
        }

        private void InitializeGame()
        {
            _model.PlayerBody.Clear();
            _model.Enemies.Clear(); 
            _model.BeaconHealth = GameModel.MaxBeaconHealth; 
            _model.Ammo = GameModel.InitialAmmo; 
            _model.Barricades.Clear(); 

            int gameWidth = _clientWidth * 2 / 3;
            int centerX = (gameWidth / GameModel.FieldCellSize) / 2;
            int centerY = (_clientHeight / GameModel.FieldCellSize) / 2;

            int startX = centerX * GameModel.FieldCellSize;
            int startY = centerY * GameModel.FieldCellSize;

            // позиции зон сбора ресурсов
            _model.SpecialAreaStartX = startX + GameModel.FieldCellSize * 0;
            _model.SpecialAreaStartY = startY + GameModel.FieldCellSize * 3;
            _model.StoneAreaStartX = startX + GameModel.FieldCellSize * 3;
            _model.StoneAreaStartY = startY + GameModel.FieldCellSize * 0;
            _model.IronAreaStartX = startX + GameModel.FieldCellSize * 2;
            _model.IronAreaStartY = startY + GameModel.FieldCellSize * 0;

            //начальная позиции игрока
            int playerX = startX + (GameModel.FieldCellSize - GameModel.PlayerCellSize) / 2;
            int playerY = startY + (GameModel.FieldCellSize - GameModel.PlayerCellSize) / 2;

            _model.PlayerBody.Add(new Point(playerX, playerY));
            _model.CurrentDirection = Direction.Right; 
            _model.GameOver = false; 
        }

        // Обработчик основного игрового цикла
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!_model.GameOver)
            {
                CheckCollision(); 
                MoveEnemies(); 
                CheckEnemyCollisions(); 
                OnGameUpdated(); 
            }
        }

        // Обработчик нанесения урона
        private void DamageTimer_Tick(object sender, EventArgs e)
        {
            ApplyBuildingDamage();
            ApplyBarricadeDamage(); 
        }

        // Обработчик спавна врагов
        private void EnemySpawnTimer_Tick(object sender, EventArgs e)
        {
            AddEnemy();
        }

        // Обработка клавиш
        public void HandleKeyDown(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    MovePlayer(Direction.Up);
                    break;
                case Keys.Down:
                    MovePlayer(Direction.Down);
                    break;
                case Keys.Left:
                    MovePlayer(Direction.Left);
                    break;
                case Keys.Right:
                    MovePlayer(Direction.Right);
                    break;
            }
        }

        // Движение игрока
        public void MovePlayer(Direction direction)
        {
            if (_model.PlayerBody.Count == 0) return;

            _model.CurrentDirection = direction;
            Point head = _model.PlayerBody[0];
            Point newHead;

            switch (direction)
            {
                case Direction.Up:
                    newHead = new Point(head.X, head.Y - GameModel.FieldCellSize);
                    break;
                case Direction.Down:
                    newHead = new Point(head.X, head.Y + GameModel.FieldCellSize);
                    break;
                case Direction.Left:
                    newHead = new Point(head.X - GameModel.FieldCellSize, head.Y);
                    break;
                case Direction.Right:
                    newHead = new Point(head.X + GameModel.FieldCellSize, head.Y);
                    break;
                default:
                    newHead = head;
                    break;
            }

            int gameWidth = GetGameWidth();
            newHead.X = Math.Max(0, Math.Min(newHead.X, gameWidth - GameModel.PlayerCellSize));
            newHead.Y = Math.Max(0, Math.Min(newHead.Y, GetClientHeight() - GameModel.PlayerCellSize));

            _model.PlayerBody[0] = newHead;
            CheckCollision(); 
            OnGameUpdated(); 
        }

        // Добавление нового врага
        private void AddEnemy()
        {
            int gameWidth = GetGameWidth();
            int numCellsX = gameWidth / GameModel.FieldCellSize;
            int numCellsY = GetClientHeight() / GameModel.FieldCellSize;
            int centerX = numCellsX / 2;
            int centerY = numCellsY / 2;

            int landStartX = (centerX - (GameModel.CubeSize / 2)) * GameModel.FieldCellSize;
            int landStartY = (centerY - (GameModel.CubeSize / 2)) * GameModel.FieldCellSize;
            int landEndX = landStartX + GameModel.CubeSize * GameModel.FieldCellSize;
            int landEndY = landStartY + GameModel.CubeSize * GameModel.FieldCellSize;

            int enemyX, enemyY;
            // Генерация спавна врага
            do
            {
                enemyX = _model.Random.Next(0, gameWidth / GameModel.FieldCellSize) * GameModel.FieldCellSize;
                enemyY = _model.Random.Next(0, GetClientHeight() / GameModel.FieldCellSize) * GameModel.FieldCellSize;
            } while (enemyX >= landStartX && enemyX < landEndX && enemyY >= landStartY && enemyY < landEndY);

            _model.Enemies.Add(new Point(enemyX, enemyY));
            OnGameUpdated();
        }

        // Движение врагов
        private void MoveEnemies()
        {
            int gameWidth = GetGameWidth();
            int numCellsX = gameWidth / GameModel.FieldCellSize;
            int numCellsY = GetClientHeight() / GameModel.FieldCellSize;
            int centerX = numCellsX / 2;
            int centerY = numCellsY / 2;

            // Расчет цели для врагов
            int buildingStartX = (centerX - (GameModel.CubeSize / 2) + (GameModel.CubeSize / 2 - GameModel.BuildingSize / 2)) * GameModel.FieldCellSize;
            int buildingStartY = (centerY - (GameModel.CubeSize / 2) + (GameModel.CubeSize / 2 - GameModel.BuildingSize / 2)) * GameModel.FieldCellSize;
            int buildingEndX = buildingStartX + GameModel.BuildingSize * GameModel.FieldCellSize;
            int buildingEndY = buildingStartY + GameModel.BuildingSize * GameModel.FieldCellSize;

            for (int i = 0; i < _model.Enemies.Count; i++)
            {
                Point enemy = _model.Enemies[i];

                // Проверка, заблокан ли враг баррикадой
                bool blocked = false;
                foreach (var barricade in _model.Barricades)
                {
                    if (IsEnemyNearBarricade(enemy, barricade))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                {
                    continue;
                }

                int targetX = buildingStartX + (GameModel.BuildingSize * GameModel.FieldCellSize) / 2;
                int targetY = buildingStartY + (GameModel.BuildingSize * GameModel.FieldCellSize) / 2;

                float directionX = targetX - enemy.X;
                float directionY = targetY - enemy.Y;

                float distance = (float)Math.Sqrt(directionX * directionX + directionY * directionY);

                if (distance <= GameModel.BuildingSize * GameModel.FieldCellSize)
                {
                    continue;
                }

                float length = (float)Math.Sqrt(directionX * directionX + directionY * directionY);
                if (length > 0)
                {
                    directionX /= length;
                    directionY /= length;
                }

                int newX = enemy.X + (int)(directionX * (GameModel.FieldCellSize / 4));
                int newY = enemy.Y + (int)(directionY * (GameModel.FieldCellSize / 4));

                newX = Math.Max(0, Math.Min(newX, gameWidth - GameModel.EnemySize));
                newY = Math.Max(0, Math.Min(newY, GetClientHeight() - GameModel.EnemySize));

                _model.Enemies[i] = new Point(newX, newY);
            }
        }

        private bool IsEnemyNearBarricade(Point enemy, Barricade barricade)
        {
            return Math.Abs(enemy.X - barricade.Location.X) < GameModel.FieldCellSize &&
                   Math.Abs(enemy.Y - barricade.Location.Y) < GameModel.FieldCellSize;
        }

        // Нанесение урона зданию
        private void ApplyBuildingDamage()
        {
            int gameWidth = GetGameWidth();
            int numCellsX = gameWidth / GameModel.FieldCellSize;
            int numCellsY = GetClientHeight() / GameModel.FieldCellSize;
            int centerX = numCellsX / 2;
            int centerY = numCellsY / 2;

            int buildingStartX = (centerX - (GameModel.CubeSize / 2) + (GameModel.CubeSize / 2 - GameModel.BuildingSize / 2)) * GameModel.FieldCellSize;
            int buildingStartY = (centerY - (GameModel.CubeSize / 2) + (GameModel.CubeSize / 2 - GameModel.BuildingSize / 2)) * GameModel.FieldCellSize;

            bool damageApplied = false;

            // Проверка всех врагов на близость к зданию
            foreach (Point enemy in _model.Enemies)
            {
                float distanceX = (buildingStartX + (GameModel.BuildingSize * GameModel.FieldCellSize) / 2) - enemy.X;
                float distanceY = (buildingStartY + (GameModel.BuildingSize * GameModel.FieldCellSize) / 2) - enemy.Y;
                float distance = (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                if (distance <= GameModel.BuildingSize * GameModel.FieldCellSize && !damageApplied)
                {
                    _model.BeaconHealth -= GameModel.EnemyDamage;
                    damageApplied = true;
                    break;
                }
            }

            // Проверка на хп здания
            if (_model.BeaconHealth <= 0)
            {
                EndGame();
                return;
            }
            OnGameUpdated();
        }

        // Нанесение урона баррикадам
        private void ApplyBarricadeDamage()
        {
            List<Barricade> barricadesToRemove = new List<Barricade>();

            foreach (var enemy in _model.Enemies)
            {
                foreach (var barricade in _model.Barricades)
                {
                    if (IsEnemyNearBarricade(enemy, barricade))
                    {
                        barricade.Health -= GameModel.EnemyDamage;
                        if (barricade.Health <= 0)
                        {
                            barricadesToRemove.Add(barricade);
                        }
                        break;
                    }
                }
            }

            // Удаление баррикад
            foreach (var barricade in barricadesToRemove)
            {
                _model.Barricades.Remove(barricade);
            }
            OnGameUpdated();
        }

        private void CheckCollision()
        {
            if (_model.PlayerBody.Count == 0) return;

            Point head = _model.PlayerBody[0];

            int gameWidth = GetGameWidth();
            int numCellsX = gameWidth / GameModel.FieldCellSize;
            int numCellsY = GetClientHeight() / GameModel.FieldCellSize;
            int centerX = numCellsX / 2;
            int centerY = numCellsY / 2;

            int startX = (centerX - (GameModel.CubeSize / 2)) * GameModel.FieldCellSize;
            int startY = (centerY - (GameModel.CubeSize / 2)) * GameModel.FieldCellSize;
            int endX = startX + GameModel.CubeSize * GameModel.FieldCellSize;
            int endY = startY + GameModel.CubeSize * GameModel.FieldCellSize;

            if (head.X < startX || head.X >= endX || head.Y < startY || head.Y >= endY)
            {
                EndGame();
                return;
            }
        }

        private void CheckEnemyCollisions()
        {
            if (_model.PlayerBody.Count == 0) return;

            Point head = _model.PlayerBody[0];

            foreach (Point enemy in _model.Enemies)
            {
                int distanceX = Math.Abs(head.X - enemy.X);
                int distanceY = Math.Abs(head.Y - enemy.Y);

                if (distanceX < (GameModel.FieldCellSize / 2 + GameModel.EnemySize / 2) &&
                    distanceY < (GameModel.FieldCellSize / 2 + GameModel.EnemySize / 2))
                {
                    EndGame();
                    return;
                }
            }
        }

        // Окончание игры
        public void EndGame()
        {
            _model.GameOver = true;
            _gameTimer.Stop();
            _damageTimer.Stop();
            _enemySpawnTimer.Stop();
            _resourceHarvestTimer.Stop();
            OnGameOverEvent();
        }

        // Ремонт маяка
        public void RepairBeacon()
        {
            // Проверка наличия ресурсов для ремонта
            if (_model.Wood >= GameModel.WoodRepairCost && _model.Metal >= GameModel.MetalRepairCost)
            {
                _model.Wood -= GameModel.WoodRepairCost;
                _model.Metal -= GameModel.MetalRepairCost;
                _model.BeaconHealth = Math.Min(_model.BeaconHealth + 20, GameModel.MaxBeaconHealth);
                OnGameUpdated();
            }
        }

        // Стрельба
        public void ShootEnemy()
        {
            if (_model.Enemies.Count == 0 || _model.Ammo <= 0) return;

            Point playerHead = _model.PlayerBody[0];
            Point nearestEnemy = Point.Empty;
            double minDistance = double.MaxValue;

            // Поиск ближайшего врага
            foreach (Point enemy in _model.Enemies)
            {
                double distanceX = playerHead.X - enemy.X;
                double distanceY = playerHead.Y - enemy.Y;
                double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (minDistance <= GameModel.ShootingRange)
            {
                _model.Enemies.Remove(nearestEnemy);
                _model.Ammo -= GameModel.AmmoCost;
                OnGameUpdated();
            }
        }

        // Постройка баррикады
        public void BuildBarricade()
        {
            // Проверка наличия ресов
            if (_model.Wood >= GameModel.WoodBarricadeCost &&
                _model.Metal >= GameModel.MetalBarricadeCost &&
                _model.Stone >= GameModel.StoneBarricadeCost)
            {
                Point playerHead = _model.PlayerBody[0];
                Point barricadeLocation = new Point(playerHead.X, playerHead.Y);

                bool canPlace = true;
                foreach (var barricade in _model.Barricades)
                {
                    if (barricade.Location == barricadeLocation)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    _model.Barricades.Add(new Barricade(barricadeLocation));
                    _model.Wood -= GameModel.WoodBarricadeCost;
                    _model.Metal -= GameModel.MetalBarricadeCost;
                    _model.Stone -= GameModel.StoneBarricadeCost;
                    OnGameUpdated();
                }
            }
        }

        // Сбор ресурсов
        public void HarvestResources()
        {
            // Сбор дерева
            if (IsNearHarvestArea())
            {
                if (_model.Wood + GameModel.WoodHarvestAmount <= GameModel.MaxResources)
                    _model.Wood += GameModel.WoodHarvestAmount;
                else
                    _model.Wood = GameModel.MaxResources;
                OnGameUpdated();
            }

            // Сбор камня
            if (IsNearStoneArea())
            {
                if (_model.Stone + GameModel.StoneHarvestAmount <= GameModel.MaxResources)
                    _model.Stone += GameModel.StoneHarvestAmount;
                else
                    _model.Stone = GameModel.MaxResources;
                OnGameUpdated();
            }

            // Сбор металла
            if (IsNearIronArea())
            {
                if (_model.Metal + GameModel.MetalHarvestAmount <= GameModel.MaxResources)
                    _model.Metal += GameModel.MetalHarvestAmount;
                else
                    _model.Metal = GameModel.MaxResources;
                OnGameUpdated();
            }
        }

        // Проверка нахождения в зоне сбора дерева
        private bool IsNearHarvestArea()
        {
            if (_model.PlayerBody.Count == 0) return false;

            return _model.PlayerBody[0].X >= _model.SpecialAreaStartX &&
                   _model.PlayerBody[0].X < _model.SpecialAreaStartX + _model.IronAreaWidth &&
                   _model.PlayerBody[0].Y >= _model.SpecialAreaStartY &&
                   _model.PlayerBody[0].Y < _model.SpecialAreaStartY + _model.IronAreaHeight;
        }

        // Проверка нахождения в зоне сбора камня
        private bool IsNearStoneArea()
        {
            if (_model.PlayerBody.Count == 0) return false;

            return _model.PlayerBody[0].X >= _model.StoneAreaStartX &&
                   _model.PlayerBody[0].X < _model.StoneAreaStartX + _model.IronAreaWidth &&
                   _model.PlayerBody[0].Y >= _model.StoneAreaStartY &&
                   _model.PlayerBody[0].Y < _model.StoneAreaStartY + _model.IronAreaHeight;
        }

        // Проверка нахождения в зоне сбора металла
        private bool IsNearIronArea()
        {
            if (_model.PlayerBody.Count == 0) return false;

            return _model.PlayerBody[0].X >= _model.IronAreaStartX &&
                   _model.PlayerBody[0].X < _model.IronAreaStartX + _model.IronAreaWidth &&
                   _model.PlayerBody[0].Y >= _model.IronAreaStartY &&
                   _model.PlayerBody[0].Y < _model.IronAreaStartY + _model.IronAreaHeight;
        }

        private int GetGameWidth()
        {
            return _clientWidth * 2 / 3;
        }

        private int GetClientHeight()
        {
            return _clientHeight;
        }

        // обновления игры
        protected virtual void OnGameUpdated()
        {
            GameUpdated?.Invoke(this, EventArgs.Empty);
        }

        //окончания игры
        protected virtual void OnGameOverEvent()
        {
            GameOverEvent?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _gameTimer?.Dispose();
            _damageTimer?.Dispose();
            _enemySpawnTimer?.Dispose();
            _resourceHarvestTimer?.Dispose();
        }

        internal void StartGame()
        {
            throw new NotImplementedException();
        }

        // Создание боеприпасов
        public void CreateAmmo()
        {
            // Проверка достаточно ли ресурсов для создания боеприпасов
            if (_model.Wood >= GameModel.WoodAmmoCost &&
                _model.Metal >= GameModel.MetalAmmoCost &&
                _model.Stone >= GameModel.StoneAmmoCost)
            {
                _model.Wood -= GameModel.WoodAmmoCost;
                _model.Metal -= GameModel.MetalAmmoCost;
                _model.Stone -= GameModel.StoneAmmoCost;

                // Добавление боеприпасов
                _model.Ammo = Math.Min(_model.Ammo + GameModel.AmmoCreateAmount, GameModel.MaxAmmo);

                OnGameUpdated();
            }
        }
    }
}