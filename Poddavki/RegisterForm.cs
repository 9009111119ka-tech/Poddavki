using System;
using Poddavki.Data;
using System.Windows.Forms;

namespace Poddavki
{
    public partial class RegisterForm : Form
    {

        public RegisterForm()
        {
            InitializeComponent();
        }

        private readonly UserRepository userRepository = new UserRepository();

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            string login = txtRegLogin.Text.Trim();
            string password = txtRegPassword.Text.Trim();
            string confirmPassword = txtRegConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (login.Length < 3 || login.Length > 20)
            {
                MessageBox.Show("Логин должен содержать от 3 до 20 символов.",
                    "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 6 || password.Length > 20)
            {
                MessageBox.Show("Пароль должен содержать от 6 до 20 символов.",
                    "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UserProfile newUser = new UserProfile
            {
                Login = login,
                Wins = 0,
                Losses = 0,
                Draws = 0
            };

            try
            {
                if (!userRepository.Register(newUser, password))
                {
                    MessageBox.Show(
                        "Пользователь с таким логином уже существует!",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Аккаунт успешно создан! Теперь вы можете войти.",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnCancelReg_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}