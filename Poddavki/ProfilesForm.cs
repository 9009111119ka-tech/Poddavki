
using System;
using System.Collections.Generic;
using Poddavki.Data;
using System.Windows.Forms;

namespace Poddavki
{
    public partial class ProfilesForm : Form
    {
        private readonly UserRepository userRepository =
            new UserRepository();

        private List<UserProfile> users =
            new List<UserProfile>();

        public ProfilesForm()
        {
            InitializeComponent();

            btnBack.Click += btnBack_Click;
            btnCreateProfile.Click += btnCreateProfile_Click;
            btnDeleteProfile.Click += btnDeleteProfile_Click;

            dgvProfiles.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvProfiles.MultiSelect = false;
            dgvProfiles.AllowUserToAddRows = false;
            dgvProfiles.RowHeadersVisible = false;

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            try
            {
                users = userRepository.GetAllUsers();

                dgvProfiles.Rows.Clear();

                foreach (UserProfile user in users)
                {
                    dgvProfiles.Rows.Add(
                        user.Login,
                        user.Wins,
                        user.Losses,
                        user.Wins + user.Losses + user.Draws);
                }

                dgvProfiles.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить профили: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCreateProfile_Click(
            object sender, EventArgs e)
        {
            using (RegisterForm registration = new RegisterForm())
            {
                registration.ShowDialog(this);
            }

            LoadProfiles();
        }

        private void btnDeleteProfile_Click(
            object sender, EventArgs e)
        {
            if (dgvProfiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите профиль.");
                return;
            }

            int index = dgvProfiles.SelectedRows[0].Index;

            if (index < 0 || index >= users.Count)
                return;

            UserProfile selected = users[index];

            if (Program.CurrentUser == null ||
                !string.Equals(
                    selected.Login,
                    Program.CurrentUser.Login,
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Удалять можно только собственный профиль.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Удалить свой профиль и всю его статистику?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                if (!userRepository.DeleteUser(selected.Login))
                {
                    MessageBox.Show("Профиль не найден.");
                    return;
                }

                Program.CurrentUser = null;

                MessageBox.Show(
                    "Профиль удалён. Приложение будет перезапущено.");

                Application.Restart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить профиль: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
