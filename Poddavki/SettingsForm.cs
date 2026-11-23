
using System;
using System.IO;
using System.Windows.Forms;

namespace Poddavki
{
    public static class GameSettings
    {
        private static string FilePath =>
            Path.Combine(
                Application.UserAppDataPath,
                "difficulty.txt");

        public static int Difficulty
        {
            get
            {
                try
                {
                    if (File.Exists(FilePath) &&
                        int.TryParse(File.ReadAllText(FilePath),
                            out int difficulty) &&
                        difficulty >= 0 && difficulty <= 2)
                    {
                        return difficulty;
                    }
                }
                catch
                {
                }

                return 1;
            }
            set
            {
                if (value < 0 || value > 2)
                    throw new ArgumentOutOfRangeException(nameof(value));

                Directory.CreateDirectory(
                    Application.UserAppDataPath);

                File.WriteAllText(FilePath, value.ToString());
            }
        }

        public static int SearchDepth
        {
            get
            {
                switch (Difficulty)
                {
                    case 0: return 2;
                    case 1: return 3;
                    case 2: return 5;
                    default: return 3;
                }
            }
        }

        public static string DifficultyName
        {
            get
            {
                switch (Difficulty)
                {
                    case 0: return "Легкая";
                    case 1: return "Средняя";
                    case 2: return "Сложная";
                    default: return "Средняя";
                }
            }
        }
    }

    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            cmbDifficulty.Items.Clear();
            cmbDifficulty.Items.AddRange(new object[]
            {
                "Легкая",
                "Средняя",
                "Сложная"
            });

            cmbDifficulty.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbDifficulty.SelectedIndex =
                GameSettings.Difficulty;

            chkHints.Parent?.Controls.Remove(chkHints);
            chkHints.Dispose();

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                GameSettings.Difficulty =
                    cmbDifficulty.SelectedIndex;

                MessageBox.Show(
                    "Настройки успешно сохранены!",
                    "Настройки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка сохранения настроек:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
