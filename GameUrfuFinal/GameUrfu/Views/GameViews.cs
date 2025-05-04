using System;
using System.Drawing;
using System.Windows.Forms;
using BeaconGame.Models;
using BeaconGame.Controllers;
using GameUrfu;

namespace BeaconGame.Views
{
    public class GameView : Form
    {
        private GameModel _model;
        private GameController _controller;
        private Button _repairButton;
        private Button _shootButton;
        private Button _buildBarricadeButton;
        private Button _harvestButton;
        private Button _createAmmoButton;
        private Panel _gameCanvas;
        private Panel _resourcesPanel;
        private Label _woodLabel;
        private Label _metalLabel;
        private Label _stoneLabel;
        private Label _ammoLabel;
        private Label _healthLabel;
        private ProgressBar _healthBar;
        private Panel _healthPanel;
        private Label _healthValueLabel;

        public GameView(GameModel model, GameController controller)
        {
            _model = model;
            _controller = controller;

            InitializeComponent();
            InitializeUI();

            _controller.GameUpdated += (s, e) =>
            {
                _gameCanvas.Invalidate();
                UpdateResourcesDisplay();
                UpdateButtonsState();
                UpdateHealthDisplay();
            };

            _controller.GameOverEvent += (s, e) =>
            {
                MessageBox.Show("Игра окончена!", "Маяк");
                _controller.StartGame(ClientSize.Width, ClientSize.Height);
            };

            this.Load += (s, e) => _controller.StartGame(ClientSize.Width, ClientSize.Height);
        }

        private void InitializeComponent()
        {
            DoubleBuffered = true;
            KeyPreview = true;
            Activated += (s, ev) => { if (!Focused) Focus(); };

            ClientSize = new Size(800, 600);
            Text = "Маяк";

            _gameCanvas = new GameCanvas(_model);
            _gameCanvas.Dock = DockStyle.Left;
            _gameCanvas.Width = ClientSize.Width * 2 / 3;
            Controls.Add(_gameCanvas);

            // панель для отображения ресурсов
            _resourcesPanel = new Panel();
            _resourcesPanel.Location = new Point(ClientSize.Width * 2 / 3 + 10, 10);
            _resourcesPanel.Size = new Size(ClientSize.Width / 3 - 20, 100);
            _resourcesPanel.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(_resourcesPanel);

            // метки для отображения ресурсов
            Font resourceFont = new Font("Arial", 10, FontStyle.Bold);

            _woodLabel = new Label();
            _woodLabel.AutoSize = true;
            _woodLabel.Location = new Point(10, 10);
            _woodLabel.Font = resourceFont;
            _resourcesPanel.Controls.Add(_woodLabel);

            _metalLabel = new Label();
            _metalLabel.AutoSize = true;
            _metalLabel.Location = new Point(10, 30);
            _metalLabel.Font = resourceFont;
            _resourcesPanel.Controls.Add(_metalLabel);

            _stoneLabel = new Label();
            _stoneLabel.AutoSize = true;
            _stoneLabel.Location = new Point(10, 50);
            _stoneLabel.Font = resourceFont;
            _resourcesPanel.Controls.Add(_stoneLabel);

            _ammoLabel = new Label();
            _ammoLabel.AutoSize = true;
            _ammoLabel.Location = new Point(10, 70);
            _ammoLabel.Font = resourceFont;
            _resourcesPanel.Controls.Add(_ammoLabel);

            // панель здоровья маяка
            _healthPanel = new Panel();
            _healthPanel.Location = new Point(ClientSize.Width * 2 / 3 + 10, 120);
            _healthPanel.Size = new Size(ClientSize.Width / 3 - 20, 80);
            _healthPanel.BorderStyle = BorderStyle.FixedSingle;
            _healthPanel.BackColor = Color.FromArgb(240, 240, 240);
            Controls.Add(_healthPanel);

            // заголовок панели здоровья
            Label healthTitleLabel = new Label();
            healthTitleLabel.Text = "ЗДОРОВЬЕ МАЯКА";
            healthTitleLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            healthTitleLabel.AutoSize = false;
            healthTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            healthTitleLabel.Dock = DockStyle.Top;
            healthTitleLabel.Height = 25;
            healthTitleLabel.BackColor = Color.SteelBlue;
            healthTitleLabel.ForeColor = Color.White;
            _healthPanel.Controls.Add(healthTitleLabel);

            // числовое значение здоровья
            _healthValueLabel = new Label();
            _healthValueLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            _healthValueLabel.AutoSize = false;
            _healthValueLabel.TextAlign = ContentAlignment.MiddleCenter;
            _healthValueLabel.Location = new Point(10, 30);
            _healthValueLabel.Size = new Size(_healthPanel.Width - 20, 20);
            _healthPanel.Controls.Add(_healthValueLabel);

            _healthBar = new ProgressBar();
            _healthBar.Location = new Point(10, 55);
            _healthBar.Size = new Size(_healthPanel.Width - 20, 15);
            _healthBar.Minimum = 0;
            _healthBar.Maximum = GameModel.MaxBeaconHealth;
            _healthBar.Style = ProgressBarStyle.Continuous;
            _healthPanel.Controls.Add(_healthBar);

            UpdateResourcesDisplay();
            UpdateHealthDisplay();
        }

        private void UpdateResourcesDisplay()
        {
            _woodLabel.Text = $"Дерево: {_model.Wood}";
            _metalLabel.Text = $"Металл: {_model.Metal}";
            _stoneLabel.Text = $"Камень: {_model.Stone}";
            _ammoLabel.Text = $"Боеприпасы: {_model.Ammo}/{GameModel.MaxAmmo}";
        }

        private void UpdateHealthDisplay()
        {
            // Обновление числового значение здоровья
            _healthValueLabel.Text = $"{_model.BeaconHealth} / {GameModel.MaxBeaconHealth}";

            _healthBar.Value = Math.Max(0, Math.Min(_model.BeaconHealth, GameModel.MaxBeaconHealth));

            //цвет текста от уровня здоровья
            if (_model.BeaconHealth > GameModel.MaxBeaconHealth * 0.7)
            {
                _healthValueLabel.ForeColor = Color.Green;
            }
            else if (_model.BeaconHealth > GameModel.MaxBeaconHealth * 0.3)
            {
                _healthValueLabel.ForeColor = Color.Orange;
            }
            else
            {
                _healthValueLabel.ForeColor = Color.Red;
            }
        }

        private void UpdateButtonsState()
        {
            // Обновление кнопок в зависимости от наличия ресурсов
            _repairButton.Enabled = _model.Wood >= GameModel.WoodRepairCost &&
                                   _model.Metal >= GameModel.MetalRepairCost;

            _buildBarricadeButton.Enabled = _model.Wood >= GameModel.WoodBarricadeCost &&
                                           _model.Metal >= GameModel.MetalBarricadeCost &&
                                           _model.Stone >= GameModel.StoneBarricadeCost;

            _shootButton.Enabled = _model.Ammo > 0 && _model.Enemies.Count > 0;

            _createAmmoButton.Enabled = _model.Wood >= GameModel.WoodAmmoCost &&
                                       _model.Metal >= GameModel.MetalAmmoCost &&
                                       _model.Stone >= GameModel.StoneAmmoCost &&
                                       _model.Ammo < GameModel.MaxAmmo;
        }

        private void InitializeUI()
        {
            _repairButton = new Button();
            _repairButton.Text = "Ремонт";
            _repairButton.Location = new Point(ClientSize.Width * 2 / 3 + 10, 210);
            _repairButton.Size = new Size(120, 30);
            _repairButton.Click += (s, e) => _controller.RepairBeacon();
            Controls.Add(_repairButton);

            _shootButton = new Button();
            _shootButton.Text = "Стрелять";
            _shootButton.Location = new Point(ClientSize.Width * 2 / 3 + 10, 250);
            _shootButton.Size = new Size(120, 30);
            _shootButton.Click += (s, e) => _controller.ShootEnemy();
            Controls.Add(_shootButton);

            _buildBarricadeButton = new Button();
            _buildBarricadeButton.Text = "Построить баррикаду";
            _buildBarricadeButton.Location = new Point(ClientSize.Width * 2 / 3 + 10, 290);
            _buildBarricadeButton.Size = new Size(120, 30);
            _buildBarricadeButton.Click += (s, e) => _controller.BuildBarricade();
            Controls.Add(_buildBarricadeButton);

            _harvestButton = new Button();
            _harvestButton.Text = "Добыть";
            _harvestButton.Location = new Point(ClientSize.Width * 2 / 3 + 10, 330);
            _harvestButton.Size = new Size(120, 30);
            _harvestButton.Click += (s, e) => _controller.HarvestResources();
            Controls.Add(_harvestButton);

            // кнопка для создания боеприпасов
            _createAmmoButton = new Button();
            _createAmmoButton.Text = "Создать боеприпасы";
            _createAmmoButton.Location = new Point(ClientSize.Width * 2 / 3 + 10, 370);
            _createAmmoButton.Size = new Size(120, 30);
            _createAmmoButton.Click += (s, e) => _controller.CreateAmmo();
            Controls.Add(_createAmmoButton);

            //подсказка о стоимости создания боеприпасов
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(_createAmmoButton,
                $"Стоимость: Дерево: {GameModel.WoodAmmoCost}, " +
                $"Металл: {GameModel.MetalAmmoCost}, " +
                $"Камень: {GameModel.StoneAmmoCost}\n" +
                $"Создает {GameModel.AmmoCreateAmount} боеприпасов");

            //подсказка о стоимости ремонта
            toolTip.SetToolTip(_repairButton,
                $"Стоимость: Дерево: {GameModel.WoodRepairCost}, " +
                $"Металл: {GameModel.MetalRepairCost}\n" +
                $"Восстанавливает 20 единиц здоровья");

            //подсказка о стоимости баррикады
            toolTip.SetToolTip(_buildBarricadeButton,
                $"Стоимость: Дерево: {GameModel.WoodBarricadeCost}, " +
                $"Металл: {GameModel.MetalBarricadeCost}, " +
                $"Камень: {GameModel.StoneBarricadeCost}");

            UpdateButtonsState();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            _controller.HandleKeyDown(keyData);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _controller.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}