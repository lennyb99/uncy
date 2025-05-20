using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;


public partial class MainForm : Form
{
    private Panel mainContentPanel;
    private Panel rightSidebarPanel;
    private TextBox inputTextBox;
    private Button submitButton;

    private HashSet<Coordinate> _activeSquares;
    private int _boardWidth = 8;  // Standardwert, wird dynamisch angepasst
    private int _boardHeight = 8; // Standardwert, wird dynamisch angepasst
    private Color _lightSquareColor = Color.FromArgb(240, 217, 181);
    private Color _darkSquareColor = Color.FromArgb(181, 136, 99);

    public MainForm(HashSet<Coordinate> activeSquares)
    {
        _activeSquares = activeSquares ?? new HashSet<Coordinate>();
        CalculateBoardDimensions(); // Berechnet die Board-Größe basierend auf den activeSquares
        InitializeComponentCustom();
    }

    public MainForm() : this(new HashSet<Coordinate>())
    {
    }

    // Neue Methode, um die Board-Dimensionen zu bestimmen
    private void CalculateBoardDimensions()
    {
        if (_activeSquares == null || !_activeSquares.Any())
        {
            _boardWidth = 1; // Mindestens 1x1, um Division durch Null zu vermeiden, wenn leer
            _boardHeight = 1;
            return;
        }

        // Finde die maximalen X- und Y-Werte in den aktiven Quadraten
        // Da die Koordinaten 0-basiert sind, ist die Dimension max + 1
        _boardWidth = _activeSquares.Max(p => p.X) + 1;
        _boardHeight = _activeSquares.Max(p => p.Y) + 1;
    }


    private void InitializeComponentCustom()
    {
        this.SuspendLayout();
        this.Text = "Uncy Chess Engine";
        this.Size = new Size(800, 600);
        this.MinimumSize = new Size(600, 400);
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.StartPosition = FormStartPosition.CenterScreen;

        rightSidebarPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 200,
            BackColor = Color.FromArgb(45, 45, 48),
            Padding = new Padding(10)
        };
        this.Controls.Add(rightSidebarPanel);

        inputTextBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Height = 100,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 5, 0, 0)
        };
        inputTextBox.HandleCreated += (sender, e) => {
            NativeMethods.SetWindowTheme(inputTextBox.Handle, "", "");
        };
        rightSidebarPanel.Controls.Add(inputTextBox);

        submitButton = new Button
        {
            Text = "Make Move",
            Dock = DockStyle.Bottom,
            Height = 40,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 5)
        };
        submitButton.FlatAppearance.BorderSize = 0;
        submitButton.Click += SubmitButton_Click;
        rightSidebarPanel.Controls.Add(submitButton);

        mainContentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 35),
            Padding = new Padding(10)
        };
        mainContentPanel.Paint += MainContentPanel_Paint;
        mainContentPanel.Resize += (s, e) => mainContentPanel.Invalidate();
        this.Controls.Add(mainContentPanel);

        this.ResumeLayout(false);
    }

    private void MainContentPanel_Paint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        Panel panel = sender as Panel;
        if (panel == null) return;

        int availableWidth = panel.ClientSize.Width - panel.Padding.Left - panel.Padding.Right;
        int availableHeight = panel.ClientSize.Height - panel.Padding.Top - panel.Padding.Bottom;

        if (availableWidth <= 0 || availableHeight <= 0 || _boardWidth <= 0 || _boardHeight <= 0) return;

        // Quadratgröße basierend auf den dynamischen Board-Dimensionen berechnen
        int squareWidth = availableWidth / _boardWidth;
        int squareHeight = availableHeight / _boardHeight;
        int squareSize = Math.Min(squareWidth, squareHeight); // Um quadratische Felder sicherzustellen

        if (squareSize <= 0) return;

        int totalBoardPixelWidth = squareSize * _boardWidth;
        int totalBoardPixelHeight = squareSize * _boardHeight;
        int offsetX = panel.Padding.Left + (availableWidth - totalBoardPixelWidth) / 2;
        int offsetY = panel.Padding.Top + (availableHeight - totalBoardPixelHeight) / 2;

        // Schleife von 0 bis boardHeight-1 und 0 bis boardWidth-1
        for (int y = 0; y < _boardHeight; y++) // Logische Y-Koordinate (0 ist unten)
        {
            for (int x = 0; x < _boardWidth; x++) // Logische X-Koordinate (0 ist links)
            {
                Coordinate currentLogicalPoint = new Coordinate(x, y);

                if (_activeSquares.Contains(currentLogicalPoint))
                {
                    // Farblogik (0,0) ist normalerweise dunkel bei Schach, wenn A1 dunkel ist.
                    // Wenn (0,0) logisch unten links ist, und wir wollen, dass es dunkel ist,
                    // dann ist (x+y) % 2 != 0 für dunkel.
                    // Standard Schach: (0,0) bzw. A1 ist dunkel. (0+0)%2 = 0 -> hell.
                    // Um A1 dunkel zu machen, wenn (0,0) = A1: (x+y)%2 == 0 -> dunkel.
                    // Wir passen die Logik an, damit das visuell untere linke Feld (logisch 0,0)
                    // die Farbe bekommt, die es hätte, wenn es A1 wäre.
                    // (x + y) % 2 == 0 für das Feld A1 (0,0) würde es hell machen.
                    // Um es dunkel zu machen: (x + y) % 2 != 0
                    // Oder, wenn wir die Farben invertieren:
                    Color squareColor;
                    if ((x + y) % 2 == 0) // Felder wie A1, C1, B2 etc.
                    {
                        squareColor = _darkSquareColor; // Machen wir (0,0) dunkel
                    }
                    else // Felder wie B1, A2, C2 etc.
                    {
                        squareColor = _lightSquareColor;
                    }


                    // Y-Koordinate für die Darstellung umkehren:
                    // panelHeight - (logischesY + 1) * squareSize + offsetY
                    // Die logische Koordinate y=0 soll am unteren Rand des Zeichenbereichs gezeichnet werden.
                    int drawingY = offsetY + (_boardHeight - 1 - y) * squareSize;
                    int drawingX = offsetX + x * squareSize;

                    using (SolidBrush brush = new SolidBrush(squareColor))
                    {
                        g.FillRectangle(brush, drawingX, drawingY, squareSize, squareSize);
                    }
                }
            }
        }
    }


    private void SubmitButton_Click(object sender, EventArgs e)
    {
        MessageBox.Show($"Input: {inputTextBox.Text}", "Move Submitted");
        // Um das Board zu aktualisieren (z.B. nach einer Eingabe):
        // 1. _activeSquares modifizieren
        // 2. CalculateBoardDimensions(); // Wenn sich die Dimensionen ändern könnten
        // 3. mainContentPanel.Invalidate(); // Neu zeichnen
    }

    static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
    }
}

