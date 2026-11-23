using System;
using Poddavki.Data;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Poddavki
{
    public partial class MainMenuForm : Form
    {
        private readonly GameRepository gameRepository =
            new GameRepository();
        public MainMenuForm()
        {
            InitializeComponent();
            btnProfiles.Click += btnProfiles_Click;
            btnPlayHotseat.Click += btnPlayHotseat_Click;
            Shown += MainMenuForm_Shown;
        }

        public MainMenuForm(UserProfile user)
        {
            InitializeComponent();
            btnProfiles.Click += btnProfiles_Click;
            btnPlayHotseat.Click += btnPlayHotseat_Click;
            lblWelcome.Text = "Добро пожаловать!";
            Shown += MainMenuForm_Shown;
        }

        private void MainMenuForm_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(OfferResumeGame));
        }


        private void OfferResumeGame()
        {
            if (Program.CurrentUser == null)
                return;

            try
            {
                List<SaveState> pending = gameRepository.GetAllSaves()
                    .FindAll(s => s.IsAutoSave &&
                        string.Equals(
                            s.OwnerLogin,
                            Program.CurrentUser.Login,
                            StringComparison.OrdinalIgnoreCase));

                pending.Sort((a, b) =>
                    b.SaveDate.CompareTo(a.SaveDate));

                foreach (SaveState save in pending)
                {
                    string mode = save.IsTwoPlayerMode
                        ? "вдвоём"
                        : "против ИИ";

                    DialogResult answer = MessageBox.Show(
                        $"Есть незавершённая партия ({mode}) " +
                        $"от {save.SaveDate:dd.MM.yyyy HH:mm}.\n" +
                        "Продолжить её?",
                        "Продолжение партии",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (answer == DialogResult.No)
                    {
                        gameRepository.RemovePendingGame(
                            save.OwnerLogin,
                            save.IsTwoPlayerMode);

                        continue;
                    }

                    bool restored;

                    using (GameForm game =
                        new GameForm(save.IsTwoPlayerMode))
                    {
                        restored = game.RestorePendingGame(save);

                        if (restored)
                            game.ShowDialog(this);
                    }

                    if (restored)
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось проверить сохранённые партии:\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void btnPlayAI_Click(object sender, EventArgs e)
        {
            using (GameForm game = new GameForm())
                game.ShowDialog(this);
        }

        private void btnPlayHotseat_Click(object sender, EventArgs e)
        {
            using (GameForm game = new GameForm(true))
                game.ShowDialog(this);
        }

        private void btnProfiles_Click(object sender, EventArgs e)
        {
            using (ProfilesForm profiles = new ProfilesForm())
                profiles.ShowDialog(this);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm settings = new SettingsForm())
                settings.ShowDialog(this);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnHistory_Click_1(object sender, EventArgs e)
        {
            HistoryForm historyForm = new HistoryForm();
            historyForm.ShowDialog(this);
        }
    }
}
