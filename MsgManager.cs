using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using SVGParser.SvgParser.core;
using SVGParser.Utils;

namespace SVGParser
{
    class MsgManager
    {
        private bool _showMessage = true;
        private bool _showAlertDialog = true;

        public MsgManager(bool showMsg, bool showDialog)
        {
            this._showMessage = showMsg;
            this._showAlertDialog = showDialog;
        }

        public void Show(string msg, bool force = false)
        {
            if (force || this._showMessage)
            {
                this.ShowMsg(msg);
            }

            if (force || this._showAlertDialog)
            {
                this.showDialog(msg);
            }
        }

        public void ShowMsg(string msg)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + msg + "\n");
        }

        public void showDialog(string msg)
        {
            Application.ShowAlertDialog(msg);
        }
    }
}
