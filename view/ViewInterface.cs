using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uncy.controller;
using uncy.gui;

namespace uncy.view
{
    public class ViewInterface
    {
        Form1 form;
        public ViewInterface(MainController cont, Form1 mainForm)
        {
            form = mainForm;
        }

        public void SendNewBoardInformationToForm(Dictionary<(int, int), char> boardInformation, (int, int) boardDimensions)
        {
            form.LoadNewBoardInformation(boardInformation, boardDimensions);
        }

    }
}
