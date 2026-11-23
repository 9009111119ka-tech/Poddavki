using System;
using Poddavki.Core;
using Poddavki.AI;
using Poddavki.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Poddavki
{
    public partial class GameForm : Form
    {
        private bool isGameOver = false;
        private bool resultRecorded = false;
        private bool mustContinueCapture = false;
        private string capturePath = "";
        private bool isWhiteTurn = true;
        private bool isTwoPlayerMode;
        private bool isAIThinking = false;

        private int playerScore = 0;
        private int aiScore = 0;
        private int secondsElapsed = 0;
        private int selectedX = -1;
        private int selectedY = -1;
        private int cellSize = 60;

        private Timer gameTimer;
        private AIPlayer ai = new AIPlayer();
        private GameBoard board = new GameBoard();

        private readonly GameRepository gameRepository =
            new GameRepository();

        private readonly UserRepository userRepository =
            new UserRepository();

        public GameForm(bool twoPlayerMode = false)
        {
            InitializeComponent();
            FormClosing += GameForm_FormClosing;
            isTwoPlayerMode = twoPlayerMode;

            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            lstLog.Items.Clear();
            lstLog.Items.Add("Игра началась!");
            UpdateGameLabels();
            DrawBoard();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            secondsElapsed++;
            UpdateTimerLabel();
        }

        private void UpdateTimerLabel()
        {
            lblTimer.Text = $"Время: {secondsElapsed / 60:00}:{secondsElapsed % 60:00}";
        }

        private void UpdateGameLabels()
        {
            lblDifficulty.Text = isTwoPlayerMode
                ? "Режим: вдвоём"
                : "Сложность: " + GameSettings.DifficultyName;

            lblScore.Text = isTwoPlayerMode
                ? $"Счёт: Белые {playerScore} — {aiScore} Чёрные"
                : $"Счёт: Вы {playerScore} — {aiScore} ИИ";

            if (isGameOver)
                lblTurn.Text = "Игра окончена";
            else if (mustContinueCapture)
                lblTurn.Text = "Продолжите взятие: " + (isWhiteTurn ? "белые" : "чёрные");
            else if (!isWhiteTurn && !isTwoPlayerMode)
                lblTurn.Text = "Ход: ИИ";
            else
                lblTurn.Text = isWhiteTurn ? "Ход: Белые" : "Ход: Чёрные";

            UpdateTimerLabel();
        }

        private string GetCoordName(int x, int y)
        {
            char col = (char)('A' + y);
            return $"{col}{8 - x}";
        }

        private static bool Inside(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        private static bool IsOwnPiece(PieceType piece, bool white)
        {
            return white
                ? piece == PieceType.WhiteMan || piece == PieceType.WhiteKing
                : piece == PieceType.BlackMan || piece == PieceType.BlackKing;
        }

        private static bool IsKing(PieceType piece)
        {
            return piece == PieceType.WhiteKing || piece == PieceType.BlackKing;
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            if (isAIThinking)
                return;

            board.ResetBoard();
            isGameOver = false;
            resultRecorded = false;
            mustContinueCapture = false;
            capturePath = "";
            isWhiteTurn = true;
            playerScore = 0;
            aiScore = 0;
            secondsElapsed = 0;
            selectedX = -1;
            selectedY = -1;

            lstLog.Items.Clear();
            lstLog.Items.Add("Игра началась!");
            UpdateGameLabels();
            DrawBoard();
            gameTimer.Start();
        }

        private SaveState CreateSaveState(string name, bool autoSave)
        {
            SaveState save = new SaveState
            {
                SaveName = name,
                SaveDate = DateTime.Now,
                OwnerLogin = Program.CurrentUser == null ? "" : Program.CurrentUser.Login,
                IsAutoSave = autoSave,
                IsTwoPlayerMode = isTwoPlayerMode,
                IsWhiteTurn = isWhiteTurn,
                PlayerScore = playerScore,
                AiScore = aiScore,
                SecondsElapsed = secondsElapsed,
                MustContinueCapture = mustContinueCapture,
                CapturePath = capturePath,
                SelectedX = selectedX,
                SelectedY = selectedY
            };

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    save.BoardData.Add((int)board.Cells[i, j]);

            foreach (var item in lstLog.Items)
                save.LogData.Add(item.ToString());

            return save;
        }

        private List<SaveState> ReadSaves()
        {
            return gameRepository.GetAllSaves();
        }

        private void WriteSaves(List<SaveState> saves)
        {
            gameRepository.SaveAll(saves);
        }

        private void btnSaveGame_Click(object sender, EventArgs e)
        {
            if (isAIThinking)
            {
                MessageBox.Show("Дождитесь завершения хода компьютера.");
                return;
            }

            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите название для сохранения:",
                "Сохранение партии",
                "Моя партия").Trim();

            if (string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                List<SaveState> saves = ReadSaves();
                saves.Add(CreateSaveState(name, false));
                WriteSaves(saves);
                MessageBox.Show("Партия успешно сохранена!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить партию:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void SavePendingGame()
        {
            if (Program.CurrentUser == null)
                throw new InvalidOperationException(
                    "Пользователь не авторизован.");

            SaveState save = CreateSaveState(
                "Незавершённая партия", true);

            gameRepository.SavePendingGame(save);
        }

        private void RemovePendingGame()
        {
            if (Program.CurrentUser == null)
                return;

            try
            {
                gameRepository.RemovePendingGame(
                    Program.CurrentUser.Login,
                    isTwoPlayerMode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить завершённое автосохранение:\n" + ex.Message,
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        public bool RestorePendingGame(SaveState save)
        {
            if (save == null || save.BoardData == null || save.BoardData.Count != 64)
            {
                MessageBox.Show("Данные сохранения повреждены.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (save.IsTwoPlayerMode != isTwoPlayerMode)
            {
                MessageBox.Show(save.IsTwoPlayerMode
                    ? "Это партия для двух игроков. Откройте режим «Играть вдвоём»."
                    : "Это партия против ИИ. Откройте режим «Играть с ИИ».",
                    "Другой режим", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            for (int index = 0; index < 64; index++)
            {
                if (save.BoardData[index] < (int)PieceType.Empty ||
                    save.BoardData[index] > (int)PieceType.BlackKing)
                {
                    MessageBox.Show("В сохранении обнаружена неизвестная шашка.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            isTwoPlayerMode = save.IsTwoPlayerMode;
            isWhiteTurn = save.IsWhiteTurn;
            playerScore = save.PlayerScore;
            aiScore = save.AiScore;
            secondsElapsed = Math.Max(0, save.SecondsElapsed);
            mustContinueCapture = save.MustContinueCapture;
            capturePath = save.CapturePath ?? "";
            selectedX = save.SelectedX;
            selectedY = save.SelectedY;
            isGameOver = false;
            resultRecorded = false;

            int position = 0;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    board.Cells[i, j] = (PieceType)save.BoardData[position++];

            if (!mustContinueCapture)
            {
                selectedX = -1;
                selectedY = -1;
                capturePath = "";
            }
            else if (!Inside(selectedX, selectedY) ||
                !IsOwnPiece(board.Cells[selectedX, selectedY], isWhiteTurn) ||
                !HasFurtherCapture(selectedX, selectedY))
            {
                MessageBox.Show("Невозможно продолжить незавершённое взятие.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            lstLog.Items.Clear();
            if (save.LogData != null)
                foreach (string entry in save.LogData)
                    lstLog.Items.Add(entry);

            if (lstLog.Items.Count > 0)
                lstLog.TopIndex = lstLog.Items.Count - 1;

            UpdateGameLabels();
            DrawBoard();
            gameTimer.Start();
            return true;
        }

        private void btnLoadGame_Click(object sender, EventArgs e)
        {
            if (isAIThinking)
            {
                MessageBox.Show("Дождитесь завершения хода компьютера.");
                return;
            }

            try
            {
                List<SaveState> saves = ReadSaves();
                string login = Program.CurrentUser == null ? "" : Program.CurrentUser.Login;
                List<SaveState> visibleSaves = saves.FindAll(s =>
                    !s.IsAutoSave || string.Equals(s.OwnerLogin, login,
                        StringComparison.OrdinalIgnoreCase));

                if (visibleSaves.Count == 0)
                {
                    MessageBox.Show("Сохранённые партии не найдены.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (Form loadForm = new Form())
                {
                    loadForm.Text = "Загрузка партии";
                    loadForm.Size = new Size(390, 440);
                    loadForm.StartPosition = FormStartPosition.CenterParent;
                    loadForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    loadForm.MaximizeBox = false;
                    loadForm.MinimizeBox = false;

                    ListBox listBox = new ListBox();
                    listBox.Dock = DockStyle.Fill;

                    Action refreshList = () =>
                    {
                        listBox.Items.Clear();
                        foreach (SaveState item in visibleSaves)
                        {
                            string mode = item.IsTwoPlayerMode ? "Вдвоём" : "С ИИ";
                            string prefix = item.IsAutoSave ? "[Авто] " : "";
                            listBox.Items.Add($"{prefix}{item.SaveName} — {mode} ({item.SaveDate:dd.MM HH:mm})");
                        }
                        if (listBox.Items.Count > 0)
                            listBox.SelectedIndex = 0;
                    };
                    refreshList();

                    Panel buttonPanel = new Panel();
                    buttonPanel.Dock = DockStyle.Bottom;
                    buttonPanel.Height = 145;

                    Button btnLoadSelected = new Button();
                    btnLoadSelected.Text = "Загрузить";
                    btnLoadSelected.Dock = DockStyle.Bottom;
                    btnLoadSelected.Height = 45;
                    btnLoadSelected.Click += (s, ev) =>
                    {
                        if (listBox.SelectedIndex < 0)
                            return;

                        SaveState selectedSave = visibleSaves[listBox.SelectedIndex];
                        if (RestorePendingGame(selectedSave))
                        {
                            loadForm.Close();
                            MessageBox.Show($"Партия '{selectedSave.SaveName}' загружена!",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    };

                    Button btnDeleteSave = new Button();
                    btnDeleteSave.Text = "Удалить сохранение";
                    btnDeleteSave.Dock = DockStyle.Top;
                    btnDeleteSave.Height = 45;
                    btnDeleteSave.Click += (s, ev) =>
                    {
                        if (listBox.SelectedIndex < 0)
                            return;

                        SaveState selectedSave = visibleSaves[listBox.SelectedIndex];
                        if (MessageBox.Show($"Удалить сохранение '{selectedSave.SaveName}'?",
                            "Подтверждение", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question) != DialogResult.Yes)
                            return;

                        try
                        {
                            List<SaveState> updated = new List<SaveState>(saves);
                            updated.Remove(selectedSave);
                            WriteSaves(updated);
                            saves = updated;
                            visibleSaves.Remove(selectedSave);
                            refreshList();
                            MessageBox.Show("Сохранение удалено.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка удаления:\n" + ex.Message);
                        }
                    };

                    Button btnRenameSave = new Button();
                    btnRenameSave.Text = "Переименовать сохранение";
                    btnRenameSave.Dock = DockStyle.Top;
                    btnRenameSave.Height = 45;
                    btnRenameSave.Click += (s, ev) =>
                    {
                        if (listBox.SelectedIndex < 0)
                            return;

                        SaveState selectedSave = visibleSaves[listBox.SelectedIndex];
                        string newName = Microsoft.VisualBasic.Interaction.InputBox(
                            "Новое название:", "Переименование", selectedSave.SaveName).Trim();

                        if (string.IsNullOrWhiteSpace(newName))
                            return;

                        string oldName = selectedSave.SaveName;
                        try
                        {
                            selectedSave.SaveName = newName;
                            WriteSaves(saves);
                            refreshList();
                            MessageBox.Show("Сохранение переименовано.");
                        }
                        catch (Exception ex)
                        {
                            selectedSave.SaveName = oldName;
                            MessageBox.Show("Ошибка переименования:\n" + ex.Message);
                        }
                    };

                    buttonPanel.Controls.Add(btnLoadSelected);
                    buttonPanel.Controls.Add(btnDeleteSave);
                    buttonPanel.Controls.Add(btnRenameSave);
                    loadForm.Controls.Add(listBox);
                    loadForm.Controls.Add(buttonPanel);
                    loadForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка чтения сохранений:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBackToMenu_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isAIThinking)
            {
                e.Cancel = true;
                MessageBox.Show("Дождитесь завершения хода компьютера.");
                return;
            }

            if (!isGameOver)
            {
                DialogResult answer = MessageBox.Show(
                    "Хотите сохранить текущую партию перед выходом?",
                    "Сохранение партии", MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (answer == DialogResult.Yes)
                {
                    try
                    {
                        SavePendingGame();
                        MessageBox.Show("Партия сохранена в базе данных SQLite.",
                            "Сохранение выполнено", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        e.Cancel = true;
                        MessageBox.Show("Не удалось сохранить партию:\n" + ex.Message,
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (answer == DialogResult.No)
                {
                    RemovePendingGame();
                }
            }

            gameTimer.Stop();
        }

        private void DrawBoard()
        {
            if (boardPictureBox.Width <= 0 || boardPictureBox.Height <= 0)
                return;

            Bitmap bmp = new Bitmap(boardPictureBox.Width, boardPictureBox.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Brush cellBrush = (i + j) % 2 == 0
                            ? Brushes.Wheat : Brushes.SaddleBrown;
                        g.FillRectangle(cellBrush, j * cellSize, i * cellSize,
                            cellSize, cellSize);

                        PieceType piece = board.Cells[i, j];
                        if (piece == PieceType.Empty)
                            continue;

                        Brush pieceBrush = IsOwnPiece(piece, true)
                            ? Brushes.White : Brushes.Black;
                        g.FillEllipse(pieceBrush, j * cellSize + 5, i * cellSize + 5,
                            cellSize - 10, cellSize - 10);
                        g.DrawEllipse(Pens.Gray, j * cellSize + 5, i * cellSize + 5,
                            cellSize - 10, cellSize - 10);

                        if (IsKing(piece))
                        {
                            Rectangle kingRect = new Rectangle(
                                j * cellSize + 12, i * cellSize + 12,
                                cellSize - 24, cellSize - 24);

                            using (Pen goldPen = new Pen(Color.Goldenrod, 3))
                                g.DrawEllipse(goldPen, kingRect);

                            using (Font kingFont = new Font("Segoe UI Symbol", 22, FontStyle.Bold))
                            using (Brush goldBrush = new SolidBrush(Color.Gold))
                            using (StringFormat format = new StringFormat())
                            {
                                format.Alignment = StringAlignment.Center;
                                format.LineAlignment = StringAlignment.Center;
                                g.DrawString("♛", kingFont, goldBrush, kingRect, format);
                            }
                        }

                        if (i == selectedX && j == selectedY)
                        {
                            using (Pen highlightPen = new Pen(Color.LimeGreen, 3))
                                g.DrawEllipse(highlightPen, j * cellSize + 2,
                                    i * cellSize + 2, cellSize - 4, cellSize - 4);
                        }
                    }
                }
            }

            Image oldImage = boardPictureBox.Image;
            boardPictureBox.Image = bmp;
            if (oldImage != null)
                oldImage.Dispose();
        }

        private bool TryMakeMove(int fromX, int fromY, int toX, int toY,
            out bool wasCapture)
        {
            wasCapture = false;

            if (!Inside(fromX, fromY) || !Inside(toX, toY))
                return false;

            PieceType piece = board.Cells[fromX, fromY];
            if (!IsOwnPiece(piece, isWhiteTurn) ||
                board.Cells[toX, toY] != PieceType.Empty)
                return false;

            int dx = toX - fromX;
            int dy = toY - fromY;
            if (dx == 0 || Math.Abs(dx) != Math.Abs(dy))
                return false;

            int distance = Math.Abs(dx);
            int stepX = Math.Sign(dx);
            int stepY = Math.Sign(dy);
            int enemyX = -1;
            int enemyY = -1;
            int enemyCount = 0;

            for (int step = 1; step < distance; step++)
            {
                int x = fromX + stepX * step;
                int y = fromY + stepY * step;
                PieceType current = board.Cells[x, y];

                if (IsOwnPiece(current, isWhiteTurn))
                    return false;

                if (current != PieceType.Empty)
                {
                    enemyCount++;
                    enemyX = x;
                    enemyY = y;
                    if (enemyCount > 1)
                        return false;
                }
            }

            if (!IsKing(piece))
            {
                if (enemyCount == 0)
                {
                    int forward = isWhiteTurn ? -1 : 1;
                    if (distance != 1 || dx != forward)
                        return false;
                }
                else if (distance != 2)
                {
                    return false;
                }
            }

            if (enemyCount == 0 &&
                (mustContinueCapture || HasAnyCapture(isWhiteTurn)))
                return false;

            wasCapture = enemyCount == 1;
            if (wasCapture)
            {
                board.Cells[enemyX, enemyY] = PieceType.Empty;
                if (isWhiteTurn)
                    playerScore++;
                else
                    aiScore++;
            }

            board.Cells[fromX, fromY] = PieceType.Empty;
            if (piece == PieceType.WhiteMan && toX == 0)
                piece = PieceType.WhiteKing;
            else if (piece == PieceType.BlackMan && toX == 7)
                piece = PieceType.BlackKing;
            board.Cells[toX, toY] = piece;

            return true;
        }

        private bool HasFurtherCapture(int x, int y)
        {
            PieceType piece = board.Cells[x, y];
            if (piece == PieceType.Empty)
                return false;

            bool white = IsOwnPiece(piece, true);
            bool king = IsKing(piece);
            int[] directions = { -1, 1 };

            foreach (int dx in directions)
            {
                foreach (int dy in directions)
                {
                    int step = 1;
                    bool enemyFound = false;

                    while (true)
                    {
                        int nx = x + dx * step;
                        int ny = y + dy * step;
                        if (!Inside(nx, ny))
                            break;

                        PieceType current = board.Cells[nx, ny];
                        if (IsOwnPiece(current, white))
                            break;

                        if (current != PieceType.Empty)
                        {
                            if (enemyFound)
                                break;
                            enemyFound = true;
                        }
                        else if (enemyFound)
                        {
                            return true;
                        }

                        if (!king && step >= 2)
                            break;
                        step++;
                    }
                }
            }

            return false;
        }

        private bool HasAnyCapture(bool white)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (IsOwnPiece(board.Cells[i, j], white) &&
                        HasFurtherCapture(i, j))
                        return true;
            return false;
        }

        private bool HasAnyMove(bool white)
        {
            if (HasAnyCapture(white))
                return true;

            int[] directions = { -1, 1 };
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    PieceType piece = board.Cells[i, j];
                    if (!IsOwnPiece(piece, white))
                        continue;

                    foreach (int dx in directions)
                    {
                        if (!IsKing(piece) && dx != (white ? -1 : 1))
                            continue;

                        foreach (int dy in directions)
                        {
                            int nx = i + dx;
                            int ny = j + dy;
                            if (Inside(nx, ny) && board.Cells[nx, ny] == PieceType.Empty)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private void boardPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (isAIThinking || isGameOver || (!isTwoPlayerMode && !isWhiteTurn))
                return;

            int j = e.X / cellSize;
            int i = e.Y / cellSize;
            if (!Inside(i, j))
                return;

            PieceType clickedPiece = board.Cells[i, j];

            if (selectedX == -1)
            {
                if (!IsOwnPiece(clickedPiece, isWhiteTurn))
                    return;
                if (HasAnyCapture(isWhiteTurn) && !HasFurtherCapture(i, j))
                    return;

                selectedX = i;
                selectedY = j;
                DrawBoard();
                return;
            }

            if (selectedX == i && selectedY == j)
            {
                if (!mustContinueCapture)
                {
                    selectedX = -1;
                    selectedY = -1;
                    DrawBoard();
                }
                return;
            }

            if (!mustContinueCapture && IsOwnPiece(clickedPiece, isWhiteTurn))
            {
                if (!HasAnyCapture(isWhiteTurn) || HasFurtherCapture(i, j))
                {
                    selectedX = i;
                    selectedY = j;
                    DrawBoard();
                }
                return;
            }

            int oldX = selectedX;
            int oldY = selectedY;
            bool wasCapture;
            if (!TryMakeMove(oldX, oldY, i, j, out wasCapture))
                return;

            string mover = isWhiteTurn
                ? (isTwoPlayerMode ? "Белые" : "Вы")
                : "Чёрные";

            if (wasCapture)
            {
                if (string.IsNullOrEmpty(capturePath))
                    capturePath = GetCoordName(oldX, oldY);
                capturePath += " × " + GetCoordName(i, j);

                if (HasFurtherCapture(i, j))
                {
                    mustContinueCapture = true;
                    selectedX = i;
                    selectedY = j;
                    UpdateGameLabels();
                    DrawBoard();
                    return;
                }

                lstLog.Items.Add(mover + ": " + capturePath);
            }
            else
            {
                lstLog.Items.Add($"{mover}: {GetCoordName(oldX, oldY)} -> {GetCoordName(i, j)}");
            }

            if (lstLog.Items.Count > 0)
                lstLog.TopIndex = lstLog.Items.Count - 1;

            mustContinueCapture = false;
            capturePath = "";
            selectedX = -1;
            selectedY = -1;
            isWhiteTurn = !isWhiteTurn;

            UpdateGameLabels();
            DrawBoard();
            CheckGameOver();

            if (!isGameOver && !isTwoPlayerMode && !isWhiteTurn)
                MakeAIMove();
        }

        private async void MakeAIMove()
        {
            if (isAIThinking || isGameOver || isTwoPlayerMode || isWhiteTurn)
                return;

            isAIThinking = true;
            try
            {
                await Task.Delay(500);
                AIMove bestMove = ai.GetBestMove(board, GameSettings.SearchDepth);

                if (bestMove == null)
                {
                    EndGame(false, "У компьютера нет допустимых ходов.");
                    return;
                }

                PieceType piece = board.Cells[bestMove.FromX, bestMove.FromY];
                board.Cells[bestMove.FromX, bestMove.FromY] = PieceType.Empty;

                foreach (var captured in bestMove.Captured)
                    board.Cells[captured.X, captured.Y] = PieceType.Empty;

                foreach (var position in bestMove.Path)
                    if (piece == PieceType.BlackMan && position.X == 7)
                        piece = PieceType.BlackKing;

                board.Cells[bestMove.ToX, bestMove.ToY] = piece;
                aiScore += bestMove.Captured.Count;

                string moveText = "ИИ: " + GetCoordName(bestMove.FromX, bestMove.FromY);
                string separator = bestMove.Captured.Count > 0 ? " × " : " -> ";
                for (int i = 1; i < bestMove.Path.Count; i++)
                    moveText += separator + GetCoordName(
                        bestMove.Path[i].X, bestMove.Path[i].Y);

                lstLog.Items.Add(moveText);
                lstLog.TopIndex = lstLog.Items.Count - 1;

                isWhiteTurn = true;
                UpdateGameLabels();
                DrawBoard();
                CheckGameOver();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка хода компьютера:\n" + ex.Message,
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isAIThinking = false;
            }
        }

        private void CheckGameOver()
        {
            if (isGameOver)
                return;

            int whiteCount = 0;
            int blackCount = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (IsOwnPiece(board.Cells[i, j], true))
                        whiteCount++;
                    else if (IsOwnPiece(board.Cells[i, j], false))
                        blackCount++;
                }
            }

            if (whiteCount == 0)
                EndGame(true, "У белых не осталось шашек.");
            else if (blackCount == 0)
                EndGame(false, "У чёрных не осталось шашек.");
            else if (!HasAnyMove(isWhiteTurn))
                EndGame(isWhiteTurn, isWhiteTurn
                    ? "У белых нет допустимых ходов."
                    : "У чёрных нет допустимых ходов.");
        }


        private void EndGame(bool whiteWon, string reason)
        {
            if (isGameOver)
                return;

            isGameOver = true;
            gameTimer.Stop();

            RecordResult(whiteWon);

            string resultText;

            if (isTwoPlayerMode)
            {
                resultText = whiteWon
                    ? "Победа белых"
                    : "Победа чёрных";
            }
            else
            {
                resultText = whiteWon
                    ? "Победа"
                    : "Поражение";
            }

            // Записываем завершённую партию в историю.
            if (Program.CurrentUser != null)
            {
                try
                {
                    SaveState completedSave = CreateSaveState(
                        "Завершённая партия", false);

                    gameRepository.AddCompletedGame(
                        completedSave,
                        whiteWon,
                        resultText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось сохранить историю партии:\n" + ex.Message,
                        "Предупреждение",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }

            UpdateGameLabels();

            string message = isTwoPlayerMode
                ? (whiteWon ? "Победили белые! " : "Победили чёрные! ") + reason
                : (whiteWon ? "Вы победили! " : "Вы проиграли! ") + reason;

            lstLog.Items.Add(message);

            RemovePendingGame();

            MessageBox.Show(
                message,
                "Игра окончена",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }



        private void RecordResult(bool playerWon)
        {
            if (resultRecorded)
                return;

            resultRecorded = true;

            if (Program.CurrentUser == null)
                return;

            try
            {
                UserProfile currentUser = Program.CurrentUser;

                if (playerWon)
                    currentUser.Wins++;
                else
                    currentUser.Losses++;

                if (!userRepository.UpdateUser(currentUser))
                {
                    throw new InvalidOperationException(
                        "Профиль пользователя не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить статистику: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
