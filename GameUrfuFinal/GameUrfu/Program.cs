using BeaconGame.Controllers;
using BeaconGame.Models;
using BeaconGame.Views;
using System;
using System.Windows.Forms;

namespace GameUrfu
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // �������������
                TextureManager.Initialize();

                GameModel model = new GameModel();
                GameController controller = new GameController(model);
                GameView view = new GameView(model, controller);

                Application.Run(view);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������� ������: {ex.Message}\n\n{ex.StackTrace}",
                    "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                TextureManager.Dispose();
            }
        }
    }
}