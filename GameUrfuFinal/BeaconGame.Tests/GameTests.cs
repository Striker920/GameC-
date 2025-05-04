using System;
using System.Drawing;
using System.Collections.Generic;
using NUnit.Framework;
using System.Reflection;
using GameUrfu; // Используйте фактическое пространство имен вашего проекта

namespace BeaconGame.Tests
{
    [TestFixture]
    public class GameTests
    {
        private GameModel _model;
        private GameController _controller;
        private const int TestClientWidth = 800;
        private const int TestClientHeight = 600;

        [SetUp]
        public void Setup()
        {
            _model = new GameModel();
            _controller = new GameController(_model);
            _controller.StartGame(TestClientWidth, TestClientHeight);
        }

        [TearDown]
        public void Cleanup()
        {
            _controller.Dispose();
        }

        [Test]
        public void TestGameInitialization()
        {
            // Проверяем, что игра инициализирована корректно
            Assert.AreEqual(GameModel.MaxBeaconHealth, _model.BeaconHealth);
            Assert.AreEqual(GameModel.InitialAmmo, _model.Ammo);
            Assert.AreEqual(1, _model.PlayerBody.Count);
            Assert.AreEqual(0, _model.Enemies.Count);
            Assert.AreEqual(0, _model.Barricades.Count);
            Assert.IsFalse(_model.GameOver);
        }

        [Test]
        public void TestPlayerMovement()
        {
            // Запоминаем начальную позицию игрока
            Point initialPosition = _model.PlayerBody[0];

            // Двигаем игрока вправо
            _controller.MovePlayer(Direction.Right);
            Point newPosition = _model.PlayerBody[0];

            // Проверяем, что игрок сдвинулся вправо на размер клетки
            Assert.AreEqual(initialPosition.X + GameModel.FieldCellSize, newPosition.X);
            Assert.AreEqual(initialPosition.Y, newPosition.Y);

            // Двигаем игрока вниз
            _controller.MovePlayer(Direction.Down);
            newPosition = _model.PlayerBody[0];

            // Проверяем, что игрок сдвинулся вниз на размер клетки
            Assert.AreEqual(initialPosition.X + GameModel.FieldCellSize, newPosition.X);
            Assert.AreEqual(initialPosition.Y + GameModel.FieldCellSize, newPosition.Y);
        }

        [Test]
        public void TestEnemyAddition()
        {
            // Запоминаем начальное количество врагов
            int initialEnemyCount = _model.Enemies.Count;

            // Добавляем врага через приватный метод с помощью рефлексии
            var addEnemyMethod = typeof(GameController).GetMethod("AddEnemy",
                BindingFlags.NonPublic | BindingFlags.Instance);
            addEnemyMethod.Invoke(_controller, null);

            // Проверяем, что враг добавлен
            Assert.AreEqual(initialEnemyCount + 1, _model.Enemies.Count);
        }

        [Test]
        public void TestResourceHarvesting()
        {
            // Запоминаем начальные ресурсы
            int initialWood = _model.Wood;
            int initialMetal = _model.Metal;
            int initialStone = _model.Stone;

            // Перемещаем игрока в зону добычи дерева
            _model.PlayerBody[0] = new Point(_model.SpecialAreaStartX, _model.SpecialAreaStartY);
            _controller.HarvestResources();

            // Проверяем, что дерево добыто
            Assert.Greater(_model.Wood, initialWood);
            Assert.AreEqual(initialMetal, _model.Metal);
            Assert.AreEqual(initialStone, _model.Stone);

            // Перемещаем игрока в зону добычи металла
            _model.PlayerBody[0] = new Point(_model.IronAreaStartX, _model.IronAreaStartY);
            _controller.HarvestResources();

            // Проверяем, что металл добыт
            Assert.Greater(_model.Metal, initialMetal);

            // Перемещаем игрока в зону добычи камня
            _model.PlayerBody[0] = new Point(_model.StoneAreaStartX, _model.StoneAreaStartY);
            _controller.HarvestResources();

            // Проверяем, что камень добыт
            Assert.Greater(_model.Stone, initialStone);
        }

        [Test]
        public void TestBarricadeBuilding()
        {
            // Запоминаем начальные ресурсы и количество баррикад
            int initialWood = _model.Wood;
            int initialMetal = _model.Metal;
            int initialStone = _model.Stone;
            int initialBarricadeCount = _model.Barricades.Count;

            // Строим баррикаду
            _controller.BuildBarricade();

            // Проверяем, что баррикада построена и ресурсы потрачены
            Assert.AreEqual(initialBarricadeCount + 1, _model.Barricades.Count);
            Assert.AreEqual(initialWood - GameModel.WoodBarricadeCost, _model.Wood);
            Assert.AreEqual(initialMetal - GameModel.MetalBarricadeCost, _model.Metal);
            Assert.AreEqual(initialStone - GameModel.StoneBarricadeCost, _model.Stone);

            // Проверяем, что баррикада построена в позиции игрока
            Assert.AreEqual(_model.PlayerBody[0], _model.Barricades[0].Location);
        }

        [Test]
        public void TestEnemyShooting()
        {
            // Добавляем врага рядом с игроком
            Point playerPosition = _model.PlayerBody[0];
            Point enemyPosition = new Point(playerPosition.X + GameModel.FieldCellSize, playerPosition.Y);
            _model.Enemies.Add(enemyPosition);

            // Запоминаем начальное количество врагов и боеприпасов
            int initialEnemyCount = _model.Enemies.Count;
            int initialAmmo = _model.Ammo;

            // Стреляем по врагу
            _controller.ShootEnemy();

            // Проверяем, что враг уничтожен и потрачен боеприпас
            Assert.AreEqual(initialEnemyCount - 1, _model.Enemies.Count);
            Assert.AreEqual(initialAmmo - GameModel.AmmoCost, _model.Ammo);
        }

        [Test]
        public void TestBeaconRepair()
        {
            // Уменьшаем здоровье маяка
            _model.BeaconHealth = GameModel.MaxBeaconHealth / 2;

            // Запоминаем начальные ресурсы и здоровье маяка
            int initialWood = _model.Wood;
            int initialMetal = _model.Metal;
            int initialHealth = _model.BeaconHealth;

            // Ремонтируем маяк
            _controller.RepairBeacon();

            // Проверяем, что здоровье маяка увеличилось и ресурсы потрачены
            Assert.Greater(_model.BeaconHealth, initialHealth);
            Assert.AreEqual(initialWood - GameModel.WoodRepairCost, _model.Wood);
            Assert.AreEqual(initialMetal - GameModel.MetalRepairCost, _model.Metal);
        }

        [Test]
        public void TestGameOver()
        {
            // Проверяем, что игра не окончена
            Assert.IsFalse(_model.GameOver);

            // Вызываем метод окончания игры
            _controller.EndGame();

            // Проверяем, что игра окончена
            Assert.IsTrue(_model.GameOver);
        }

        [Test]
        public void TestEnemyCollision()
        {
            // Добавляем врага в позицию игрока
            _model.Enemies.Add(_model.PlayerBody[0]);

            // Вызываем приватный метод проверки столкновений с помощью рефлексии
            var checkEnemyCollisionsMethod = typeof(GameController).GetMethod("CheckEnemyCollisions",
                BindingFlags.NonPublic | BindingFlags.Instance);
            checkEnemyCollisionsMethod.Invoke(_controller, null);

            // Проверяем, что игра окончена из-за столкновения с врагом
            Assert.IsTrue(_model.GameOver);
        }

        [Test]
        public void TestMaxResources()
        {
            // Устанавливаем ресурсы близко к максимуму
            _model.Wood = GameModel.MaxResources - 5;

            // Перемещаем игрока в зону добычи дерева
            _model.PlayerBody[0] = new Point(_model.SpecialAreaStartX, _model.SpecialAreaStartY);

            // Добываем ресурсы
            _controller.HarvestResources();

            // Проверяем, что количество ресурсов не превышает максимум
            Assert.AreEqual(GameModel.MaxResources, _model.Wood);
        }

        [Test]
        public void TestBarricadeDamage()
        {
            // Добавляем баррикаду
            Point barricadePosition = new Point(100, 100);
            _model.Barricades.Add(new Barricade(barricadePosition));

            // Добавляем врага рядом с баррикадой
            _model.Enemies.Add(barricadePosition);

            // Запоминаем начальное здоровье баррикады
            int initialHealth = _model.Barricades[0].Health;

            // Вызываем приватный метод нанесения урона баррикадам с помощью рефлексии
            var applyBarricadeDamageMethod = typeof(GameController).GetMethod("ApplyBarricadeDamage",
                BindingFlags.NonPublic | BindingFlags.Instance);
            applyBarricadeDamageMethod.Invoke(_controller, null);

            // Проверяем, что здоровье баррикады уменьшилось
            Assert.Less(_model.Barricades[0].Health, initialHealth);
        }

        [Test]
        public void TestCreateAmmo()
        {
            // Запоминаем начальные ресурсы и количество боеприпасов
            int initialWood = _model.Wood;
            int initialMetal = _model.Metal;
            int initialStone = _model.Stone;
            int initialAmmo = _model.Ammo;

            // Создаем боеприпасы
            _controller.CreateAmmo();

            // Проверяем, что боеприпасы созданы и ресурсы потрачены
            Assert.AreEqual(initialAmmo + GameModel.AmmoCreateAmount, _model.Ammo);
            Assert.AreEqual(initialWood - GameModel.WoodAmmoCost, _model.Wood);
            Assert.AreEqual(initialMetal - GameModel.MetalAmmoCost, _model.Metal);
            Assert.AreEqual(initialStone - GameModel.StoneAmmoCost, _model.Stone);
        }

        [Test]
        public void TestMaxAmmo()
        {
            // Устанавливаем боеприпасы близко к максимуму
            _model.Ammo = GameModel.MaxAmmo - 5;

            // Запоминаем начальное количество боеприпасов
            int initialAmmo = _model.Ammo;

            // Создаем боеприпасы
            _controller.CreateAmmo();

            // Проверяем, что количество боеприпасов не превышает максимум
            Assert.AreEqual(GameModel.MaxAmmo, _model.Ammo);
        }

        [Test]
        public void TestResetAfterGameOver()
        {
            // Изменяем значения ресурсов
            _model.Wood = 10;
            _model.Metal = 5;
            _model.Stone = 15;
            _model.Ammo = 3;
            _model.BeaconHealth = 30;

            // Добавляем врагов и баррикады
            _model.Enemies.Add(new Point(100, 100));
            _model.Barricades.Add(new Barricade(new Point(200, 200)));

            // Запоминаем начальные значения
            int initialWood = GameModel.InitialWood;
            int initialMetal = GameModel.InitialMetal;
            int initialStone = GameModel.InitialStone;
            int initialAmmo = GameModel.InitialAmmo;
            int initialHealth = GameModel.MaxBeaconHealth;

            // Завершаем игру
            _controller.EndGame();

            // Перезапускаем игру
            _controller.StartGame(TestClientWidth, TestClientHeight);

            // Проверяем, что все значения сброшены к начальным
            Assert.AreEqual(initialWood, _model.Wood);
            Assert.AreEqual(initialMetal, _model.Metal);
            Assert.AreEqual(initialStone, _model.Stone);
            Assert.AreEqual(initialAmmo, _model.Ammo);
            Assert.AreEqual(initialHealth, _model.BeaconHealth);
            Assert.AreEqual(1, _model.PlayerBody.Count); // Должен быть только один элемент - голова игрока
            Assert.AreEqual(0, _model.Enemies.Count); // Враги должны быть удалены
            Assert.AreEqual(0, _model.Barricades.Count); // Баррикады должны быть удалены
            Assert.IsFalse(_model.GameOver); // Флаг GameOver должен быть сброшен
        }
    }
}