
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Poddavki.Data;

namespace Poddavki
{
    public partial class HistoryForm : Form
    {
        private readonly GameRepository gameRepository =
            new GameRepository();

        private List<CompletedGame> completedGames =
            new List<CompletedGame>();

        public HistoryForm()
        {
            InitializeComponent();

            ConfigureGrids();

            dgvGames.SelectionChanged += dgvGames_SelectionChanged;
            btnClose.Click += btnClose_Click;
            btnExport.Click += btnExport_Click;

            btnExport.Enabled = false;

            LoadGames();
        }

        private void ConfigureGrids()
        {
            dgvGames.ReadOnly = true;
            dgvGames.AllowUserToAddRows = false;
            dgvGames.AllowUserToDeleteRows = false;
            dgvGames.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvGames.MultiSelect = false;
            dgvGames.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvGames.RowHeadersVisible = false;
            dgvGames.Columns.Clear();

            dgvGames.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                Visible = false
            });

            dgvGames.Columns.Add("colDate", "Дата");
            dgvGames.Columns.Add("colMode", "Режим");
            dgvGames.Columns.Add("colResult", "Результат");
            dgvGames.Columns.Add("colDuration", "Время");
            dgvGames.Columns.Add("colMoves", "Ходы");

            dgvMoves.ReadOnly = true;
            dgvMoves.AllowUserToAddRows = false;
            dgvMoves.AllowUserToDeleteRows = false;
            dgvMoves.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvMoves.MultiSelect = false;
            dgvMoves.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvMoves.RowHeadersVisible = false;
            dgvMoves.Columns.Clear();

            dgvMoves.Columns.Add("colNumber", "№");
            dgvMoves.Columns.Add("colMoveText", "Ход");
        }

        private void LoadGames()
        {
            if (Program.CurrentUser == null)
                return;

            try
            {
                completedGames =
                    gameRepository.GetCompletedGames(
                        Program.CurrentUser.Login);

                dgvGames.Rows.Clear();
                dgvMoves.Rows.Clear();

                foreach (CompletedGame game in completedGames)
                {
                    int index = dgvGames.Rows.Add(
                        game.Id,
                        game.CompletedAt.ToString("dd.MM.yyyy HH:mm"),
                        game.IsTwoPlayerMode ? "Вдвоём" : "С ИИ",
                        game.ResultText,
                        TimeSpan.FromSeconds(
                            Math.Max(0, game.SecondsElapsed))
                            .ToString(@"hh\:mm\:ss"),
                        game.MoveCount);

                    dgvGames.Rows[index].Tag = game;
                }

                if (dgvGames.Rows.Count > 0)
                {
                    dgvGames.Rows[0].Selected = true;
                    dgvGames.CurrentCell =
                        dgvGames.Rows[0].Cells[1];

                    LoadSelectedGame();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки истории:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void dgvGames_SelectionChanged(
            object sender, EventArgs e)
        {
            LoadSelectedGame();
        }

        private CompletedGame GetSelectedGame()
        {
            if (dgvGames.SelectedRows.Count == 0)
                return null;

            return dgvGames.SelectedRows[0].Tag as CompletedGame;
        }

        private void LoadSelectedGame()
        {
            CompletedGame game = GetSelectedGame();

            dgvMoves.Rows.Clear();

            if (game == null)
            {
                btnExport.Enabled = false;
                return;
            }

            btnExport.Enabled = true;

            try
            {
                List<string> moves =
                    gameRepository.GetCompletedMoves(game.Id);

                for (int i = 0; i < moves.Count; i++)
                {
                    dgvMoves.Rows.Add(i + 1, moves[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки ходов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            CompletedGame game = GetSelectedGame();

            if (game == null)
            {
                MessageBox.Show("Выберите партию для экспорта.");
                return;
            }

            try
            {
                List<string> moves =
                    gameRepository.GetCompletedMoves(game.Id);

                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Текстовый файл (*.txt)|*.txt";
                    dialog.FileName =
                        "Партия_" + game.Id + ".txt";

                    if (dialog.ShowDialog() != DialogResult.OK)
                        return;

                    using (StreamWriter writer =
                        new StreamWriter(
                            dialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("ИСТОРИЯ ПАРТИИ");
                        writer.WriteLine("------------------------------");
                        writer.WriteLine(
                            "Дата: " +
                            game.CompletedAt.ToString("dd.MM.yyyy HH:mm"));

                        writer.WriteLine(
                            "Режим: " +
                            (game.IsTwoPlayerMode ? "Вдвоём" : "С ИИ"));

                        writer.WriteLine(
                            "Результат: " + game.ResultText);

                        writer.WriteLine(
                            "Продолжительность: " +
                            TimeSpan.FromSeconds(
                                Math.Max(0, game.SecondsElapsed))
                                .ToString(@"hh\:mm\:ss"));

                        writer.WriteLine(
                            "Количество ходов: " + game.MoveCount);

                        writer.WriteLine();
                        writer.WriteLine("ПОСЛЕДОВАТЕЛЬНОСТЬ ХОДОВ");
                        writer.WriteLine("------------------------------");

                        for (int i = 0; i < moves.Count; i++)
                        {
                            writer.WriteLine(
                                (i + 1) + ". " + moves[i]);
                        }
                    }
                }

                MessageBox.Show(
                    "История партии успешно экспортирована!",
                    "Экспорт",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка экспорта:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}
