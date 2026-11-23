using System;
using Poddavki.Data;
using System.Windows.Forms;

namespace Poddavki
{
    internal static class Program
    {
        public static UserProfile CurrentUser { get; set; }


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DatabaseInitializer.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка инициализации базы данных: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            Application.Run(new LoginForm());
        }

    }
}