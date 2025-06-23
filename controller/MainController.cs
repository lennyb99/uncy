using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.board;
using uncy.gui;
using uncy.model.board;
using uncy.view;

namespace uncy.controller
{
    public class MainController
    {
        public ViewInterface view;
        public BoardInterface model;
        public MainController(Form1 mainForm)
        {
            if(mainForm == null)
            {
                Console.WriteLine("Critical error with Main Form while initializing");
            }
            
            view = new ViewInterface(this, mainForm);
            model = new BoardInterface(this);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            model.SendBoardDataToController();
        }

        public void InputNewBoardData(HashSet<Coordinate> squares, Dictionary<Coordinate, Piece> piecePositions)
        {
            if(model == null)
            {
                Console.WriteLine("model is null");
            }
            view.SendNewBoardInformationToForm(ConvertBoardInformationData(squares,piecePositions), model.GetBoardDimensions());
        }

        private Dictionary<(int, int), char> ConvertBoardInformationData(HashSet<Coordinate> squares, Dictionary<Coordinate, Piece> piecePositions)
        {
            Dictionary<(int, int), char> boardInformation = new Dictionary<(int, int), char>();

            foreach(Coordinate coord in squares)
            {
                boardInformation.Add((coord.X, coord.Y), ' ');
            }

            foreach (var square in boardInformation)
            {
                if(piecePositions.ContainsKey(new Coordinate(square.Key.Item1, square.Key.Item2)))
                {
                    boardInformation[square.Key] = PieceFactory.GetPieceIdentifier(piecePositions[new Coordinate(square.Key.Item1, square.Key.Item2)]);
                }  
            }
            return boardInformation;
        }
    }
}
