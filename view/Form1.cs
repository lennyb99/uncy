using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uncy.controller;

namespace uncy.gui
{
    public partial class Form1 : Form
    {
        private Dictionary<char, Image> images = new Dictionary<char, Image>();
        private Dictionary<(int, int), char> boardInformation;
        private (int, int) boardDimensions;

        private int squareSize = 100;

        MainController controller;
        
        public Form1()
        {
            boardInformation = new Dictionary<(int, int), char>();
            boardInformation.Add((0, 0), 'Q');
            boardInformation.Add((0, 1), 'K');
            boardInformation.Add((1, 0), 'q');
            boardInformation.Add((1, 1), 'k');
            boardInformation.Add((2, 0), ' ');
            boardInformation.Add((2, 1), ' ');

            InitializeComponent();
            LoadImages();
            RedrawMainPanel();
        }

        public void RedrawMainPanel()
        {
            mainPanel.Paint += mainPanel_Paint;
        }

        public void LoadNewBoardInformation(Dictionary<(int,int),char> boardInformation, (int,int) boardDimensions)
        {
            this.boardDimensions = boardDimensions; 
            this.boardInformation = boardInformation;
            CalculateSquareSize(boardDimensions);
            RedrawMainPanel();
        }

        private void LoadImages()
        {
            string imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "images","pieces");
            if (!Directory.Exists(imageFolder))
            {
                Console.WriteLine("Bilderordner nicht gefunden: " + imageFolder);
                return;
            }

            foreach(string file in Directory.GetFiles(imageFolder))
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    Image img = Image.FromFile(file);
                    images.Add(GetPieceImageIdentifier(fileName), img);
                }
                catch(Exception ex) {
                        Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        private char GetPieceImageIdentifier(string pieceName)
        {
            switch(pieceName)
            {
                case "whitepawn":
                    return 'P';
                case "whiteknight":
                    return 'N';
                case "whitebishop":
                    return 'B';
                case "whiterook":
                    return 'R';
                case "whitequeen":
                    return 'Q';
                case "whiteking":
                    return 'K';
                case "blackpawn":
                    return 'p';
                case "blackknight":
                    return 'n';
                case "blackbishop":
                    return 'b';
                case "blackrook":
                    return 'r';
                case "blackqueen":
                    return 'q';
                case "blackking":
                    return 'k';
                default:
                    Console.WriteLine("Unidentified image object");
                    break;
            }
            return 'x';
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            CreateAllSquares(g);
        }

        private void CalculateSquareSize((int,int) boardDimensions)
        {

        }

        private void CreateAllSquares(Graphics g)
        {
            if (boardInformation == null || boardInformation.Count == 0)
            {
                Console.WriteLine("No information on the board");
                return;
            }

            foreach (var kvp in boardInformation)
            {
                Rectangle rect = new Rectangle(kvp.Key.Item1*squareSize, Math.Abs(kvp.Key.Item2*squareSize-(boardDimensions.Item2*squareSize)), squareSize, squareSize);
                CreateSquare(g, rect, IsBrightSquare(kvp.Key.Item1, kvp.Key.Item2));
                if(kvp.Value != ' ') { 
                    DrawPiece(g, rect, kvp.Value);
                }
            }
        }

        private bool IsBrightSquare(int x, int y) {
            return (x + y) % 2 == 0;
        }


        private void DrawPiece(Graphics g, Rectangle rect, char pieceType)
        {
            g.DrawImage(images[pieceType], rect);
        }

        private void CreateSquare(Graphics g, Rectangle rect, bool bright)
        {
            DrawSquare(g, rect, bright);
        }

        private void DrawSquare(Graphics g, Rectangle rect, bool bright)
        {
            if(bright)
            {
                using (Pen pen = new Pen(Color.FromArgb(255, 240, 217, 181)))
                using(SolidBrush brush = new SolidBrush(Color.FromArgb(255, 240, 217, 181)))
                {
                    g.DrawRectangle(pen, rect);
                    g.FillRectangle(brush, rect);
                }
            }
            else
            {
                using (Pen pen = new Pen(Color.FromArgb(255, 181, 136, 99)))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 181, 136, 99)))
                {
                    g.DrawRectangle(pen,rect);
                    g.FillRectangle(brush, rect);
                }
            }
        }
    }
}
