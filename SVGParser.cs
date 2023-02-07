using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using SVGParser.SvgParser.core;
using SVGParser.Utils;

namespace SVGParser
{
    public class SVGParser
    {
        private string _version = "2023020710";
        private bool _showAlertDialog = true;
        private bool _showMessage = true;
        private bool _outputPng = false;

        private bool _isLoadContextMenu = false;

        public SVGParser()
        {
            MsgManager mm = new MsgManager(true, false);
            mm.Show("Welcome to use CADSVGParser, it's create by @vincentmmc.");
            mm.Show("use command 'ParseSvgHelp' to show help.");
            mm.Show("version:" + this._version);
        }

        #region Command/Lisp Functions

        [CommandMethod("ParseSvgLoadMenu")]
        public void CommandParseSvgLoad()
        {
            if (_isLoadContextMenu)
            {
                return;
            }
            ContextMenuExtension contextMenu = new ContextMenuExtension();
            contextMenu.Title = "ParseSvgTool";

            // add menu items
            List<Tuple<string, Action>> functions = getFunctions();
            foreach (Tuple<string, Action> f in functions)
            {
                string itemName = f.Item1;
                Action action = f.Item2;
                MenuItem item = new MenuItem(itemName);
                item.Click += new EventHandler((object sender, EventArgs e) =>
                {
                    action.Invoke();
                });
                contextMenu.MenuItems.Add(item);
            }

            // add check items
            {
                // enable/disable png output
                {
                    MenuItem item = new MenuItem("PNG Output");
                    item.Checked = _outputPng;
                    item.Click += new EventHandler((object sender, EventArgs e) =>
                    {
                        item.Checked = !item.Checked;
                        _outputPng = item.Checked;
                        string tips = _outputPng ? "Enable PNG Output" : "Disable PNG Output";
                        new MsgManager(true, false).Show(tips);
                    });
                    contextMenu.MenuItems.Add(item);

                }

                // enable/disable color flip
                {
                    MenuItem item = new MenuItem("Color Flip");
                    item.Checked = ColorUtils.FLIP_WHITE_COLOR;
                    item.Click += new EventHandler((object sender, EventArgs e) =>
                    {
                        item.Checked = !item.Checked;
                        ColorUtils.FLIP_WHITE_COLOR = item.Checked;
                        string tips = ColorUtils.FLIP_WHITE_COLOR ? "Enable Color Flip" : "Disable Color Flip";
                        new MsgManager(true, false).Show(tips);
                    });
                    contextMenu.MenuItems.Add(item);
                }
            }
            Application.AddDefaultContextMenuExtension(contextMenu);
            _isLoadContextMenu = true;
        }

        [CommandMethod("ParseSvgHelp")]
        public void CommandParseSvgHelp()
        {
            this.ShowHelpMessage();
        }

        [CommandMethod("ParseSvg")]
        public void CommandParseSvg()
        {
            this.initMessageParam();
            this.ParseSvg(null);
        }

        [CommandMethod("ParseSvgSelect")]
        public void CommandParseSvgSelect()
        {
            this.initMessageParam();
            this.ParseSvgSelect(null);
        }

        [CommandMethod("ParseSvgBlocks")]
        public void CommandParseSvgBlocks()
        {
            this.initMessageParam();
            this.ParseSvgBlocks(null);
        }

        [CommandMethod("ParseSvgEnablePng")]
        public void CommandParseEnablePng()
        {
            this._outputPng = true;
            new MsgManager(true, false).Show("EnablePng");
        }

        [CommandMethod("ParseSvgDisablePng")]
        public void CommandParseDisablePng()
        {
            this._outputPng = false;
            new MsgManager(true, false).Show("DisablePng");
        }

        [CommandMethod("ParseSvgEnableFlipColor")]
        public void CommandParseEnableFlipColor()
        {
            ColorUtils.FLIP_WHITE_COLOR = true;
            new MsgManager(true, false).Show("EnableFlipColor");
        }

        [CommandMethod("ParseSvgDisableFlipColor")]
        public void CommandParseDisableFlipColor()
        {
            ColorUtils.FLIP_WHITE_COLOR = false;
            new MsgManager(true, false).Show("DisableFlipColor");
        }

        [LispFunction("ParseSvg")]
        public void LispParseSvg(ResultBuffer rbArgs)
        {
            List<string> fParams = this.getLispFunctionParams(rbArgs);
            if (fParams.Count > 0)
            {
                if (fParams.Count > 1 && fParams[1] == "false")
                {
                    this._showAlertDialog = false;
                }
                this.ParseSvg(fParams[0]);
            }
        }

        [LispFunction("ParseSvgSelect")]
        public void LispParseSvgSelect(ResultBuffer rbArgs)
        {
            List<string> fParams = this.getLispFunctionParams(rbArgs);
            if (fParams.Count > 0)
            {
                if (fParams.Count > 1 && fParams[1] == "false")
                {
                    this._showAlertDialog = false;
                }
                this.ParseSvgSelect(fParams[0]);
            }
        }

        [LispFunction("ParseSvgBlocks")]
        public void LispParseSvgBlocks(ResultBuffer rbArgs)
        {
            List<string> fParams = this.getLispFunctionParams(rbArgs);
            if (fParams.Count > 0)
            {
                if (fParams.Count > 1 && fParams[1] == "false")
                {
                    this._showAlertDialog = false;
                }
                this.ParseSvgBlocks(fParams[0]);
            }
        }

        [LispFunction("ParseSvgHelp")]
        public void LispParseSvgHelp(ResultBuffer rbArgs)
        {
            this.ShowHelpMessage();
        }



        #endregion

        #region private function

        #region core functions

        private void ParseSvg(string saveFilePath = null)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            MsgManager mm = new MsgManager(this._showMessage, this._showAlertDialog);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<Entity> supportedEntities = new List<Entity>();
                // collect layerinfo
                Dictionary<ObjectId, LayerInfo> layerColorDict = getLayerInfoDictionary(trans, db);

                // collect entities
                {
                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    foreach (ObjectId item in acBlkTblRec)
                    {
                        DBObject entity = item.GetObject(OpenMode.ForRead, false, false);
                        if (entity is BlockReference)
                        {
                            BlockReference blockRef = entity as BlockReference;
                            this.parseBlockReference(trans, blockRef, null, supportedEntities);
                        }
                        else if (entity is Entity)
                        {
                            supportedEntities.Add(entity.Clone() as Entity);
                        }
                    }
                }

                if (supportedEntities.Count > 0)
                {
                    SvgConverter converter = new SvgConverter(supportedEntities, layerColorDict);
                    string data = converter.Parse();
                    if (saveFilePath != null)
                    {
                        FileUtils.SaveFile(saveFilePath, data);
                        if (this._outputPng)
                        {
                            FileUtils.SaveFile(Path.ChangeExtension(saveFilePath, ".png"), converter.ParseToBitmap());
                        }
                    }
                    else
                    {
                        FileUtils.OpenDialogToSaveFile(data);
                        if (this._outputPng)
                        {
                            FileUtils.OpenDialogToSaveFile(converter.ParseToBitmap());
                        }
                    }
                    mm.Show("ParseSvg Done");
                }
                else
                {
                    mm.Show("No Support Objects");
                }
                trans.Commit(); //提交事务处理
            }
        }

        private void ParseSvgSelect(string saveFilePath = null)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            MsgManager mm = new MsgManager(this._showMessage, this._showAlertDialog);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                List<Entity> supportedEntities = new List<Entity>();
                // collect layerinfo
                Dictionary<ObjectId, LayerInfo> layerColorDict = getLayerInfoDictionary(trans, db);

                // collect entities
                {
                    PromptSelectionResult acSSPrompt = ed.GetSelection();
                    if (acSSPrompt.Status == PromptStatus.OK)
                    {
                        SelectionSet acSSet = acSSPrompt.Value;
                        foreach (SelectedObject acSSObj in acSSet)
                        {
                            if (acSSObj == null)
                            {
                                continue;
                            }
                            DBObject entity = trans.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as Entity;
                            if (entity is BlockReference)
                            {
                                BlockReference blockRef = entity as BlockReference;
                                this.parseBlockReference(trans, blockRef, null, supportedEntities);
                            }
                            else if (entity is Entity)
                            {
                                supportedEntities.Add(entity.Clone() as Entity);
                            }
                        }
                    }
                }

                // output
                if (supportedEntities.Count > 0)
                {
                    SvgConverter converter = new SvgConverter(supportedEntities, layerColorDict);
                    string data = converter.Parse();

                    if (saveFilePath != null)
                    {
                        FileUtils.SaveFile(saveFilePath, data);
                        if (this._outputPng)
                        {
                            FileUtils.SaveFile(Path.ChangeExtension(saveFilePath, ".png"), converter.ParseToBitmap());
                        }
                    }
                    else
                    {
                        FileUtils.OpenDialogToSaveFile(data);
                        if (this._outputPng)
                        {
                            FileUtils.OpenDialogToSaveFile(converter.ParseToBitmap());
                        }
                    }
                    mm.Show("ParseSvgSelect Done");
                }
                else
                {
                    mm.Show("No Selected Objects Or No Support Objects");
                }

                trans.Commit();
            }
        }

        private void ParseSvgBlocks(string saveFilePath = null)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Database db = HostApplicationServices.WorkingDatabase;
            MsgManager mm = new MsgManager(this._showMessage, this._showAlertDialog);
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                string saveFolder = "";
                if (saveFilePath != null)
                {
                    saveFolder = Path.GetDirectoryName(saveFilePath);
                }
                else
                {
                    saveFolder = FileUtils.GetSaveFolder();
                }
                if (saveFolder == "")
                {
                    mm.Show("Please Select Folder To Save");
                    return;
                }

                // collect layerinfo
                Dictionary<ObjectId, LayerInfo> layerColorDict = getLayerInfoDictionary(trans, db);

                // collect all block datas
                {
                    BlockTable acBlkTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //以读方式打开块表
                    ObjectId modelSpaceTable = acBlkTbl[BlockTableRecord.ModelSpace];
                    foreach (ObjectId blockId in acBlkTbl)
                    {
                        if (blockId.Equals(modelSpaceTable))
                        {
                            continue;
                        }
                        DBObject blockObject = trans.GetObject(blockId, OpenMode.ForRead);
                        List<Entity> supportedEntities = new List<Entity>();
                        BlockTableRecord acBlkTblRec = blockId.GetObject(OpenMode.ForRead, false, false) as BlockTableRecord;
                        foreach (ObjectId item in acBlkTblRec)
                        {
                            DBObject entity = item.GetObject(OpenMode.ForRead, false, false);
                            if (entity is BlockReference)
                            {
                                BlockReference blockRef = entity as BlockReference;
                                this.parseBlockReference(trans, blockRef, null, supportedEntities);
                            }
                            else if (entity is Entity)
                            {
                                supportedEntities.Add(entity.Clone() as Entity);
                            }
                        }
                        if (supportedEntities.Count > 0)
                        {
                            SvgConverter converter = new SvgConverter(supportedEntities, layerColorDict);
                            string data = converter.Parse();
                            string svgFileName = acBlkTblRec.Name + ".svg";
                            FileUtils.SaveFile(Path.Combine(saveFolder, svgFileName), data);

                            if (this._outputPng)
                            {
                                string pngFileName = acBlkTblRec.Name + ".png";
                                FileUtils.SaveFile(Path.Combine(saveFolder, pngFileName), converter.ParseToBitmap());
                            }

                            mm.ShowMsg(string.Format("Parse Block:{0} Done", svgFileName));
                        }
                    }
                }

                // done
                mm.Show("ParseSvgBlocks Done");
                trans.Commit();
            }
        }

        private void ShowHelpMessage()
        {
            MsgManager mm = new MsgManager(true, false);
            mm.Show("use command 'ParseSvg' to parse current block data");
            mm.Show("use command 'ParseSvgBlocks' to parse all block data");
            mm.Show("use command 'ParseSvgSelect' to select and parse");
            mm.Show("use command 'ParseSvgHelp' to show help");
            mm.Show("use command 'ParseSvgEnablePng' to enable output png, default is disable");
            mm.Show("use command 'ParseSvgEnableFlipColor' to enable flip white color to black, default is disable");
            mm.Show("use command 'ParseSvgSelect' to select and parse");
            mm.Show("use command '(ParseSvg \"filename\")' to select and parse. like (ParseSvg \"C:\\\\output.svg\")");
            mm.Show("use command '(ParseSvgBlocks \"dictionaryname\")' to select and parse. like (ParseSvgBlocks \"C:\\\\output\\\\\")");
            mm.Show("use command '(ParseSvgSelect \"filename\")' to select and parse. like (ParseSvgSelect \"C:\\\\output.svg\")");
        }

        #endregion


        /// <summary>
        /// parse block
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="blockRef"></param>
        /// <param name="parentMatrix"></param>
        /// <param name="targets"></param>
        private void parseBlockReference(Transaction trans, BlockReference blockRef, Matrix3d? parentMatrix, List<Entity> targets)
        {
            BlockTableRecord bfTableRecord = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId bfItem in bfTableRecord)
            {
                Matrix3d finalMatrix = parentMatrix.HasValue ? parentMatrix.Value.PostMultiplyBy(blockRef.BlockTransform) : blockRef.BlockTransform;
                DBObject bfEntity = bfItem.GetObject(OpenMode.ForRead, false, false);
                if (bfEntity is BlockReference)
                {
                    this.parseBlockReference(trans, bfEntity as BlockReference, finalMatrix, targets);
                }
                else if (bfEntity is Entity)
                {
                    Entity entity = null;
                    try
                    {
                        entity = bfEntity.Clone() as Entity;
                        entity.TransformBy(finalMatrix);
                        targets.Add(entity);
                    }
                    catch (System.Exception ex)
                    {
                        // process NonUniformly scale
                        if (ex.Message == "eCannotScaleNonUniformly" && entity != null)
                        {
                            List<Entity> lines = EntityUtils.ConvertByNonUniformMatrix(entity, finalMatrix);
                            foreach (Entity line in lines)
                            {
                                targets.Add(line);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// get layer dict
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private Dictionary<ObjectId, LayerInfo> getLayerInfoDictionary(Transaction trans, Database db)
        {
            Dictionary<ObjectId, LayerInfo> layerInfoDict = new Dictionary<ObjectId, LayerInfo>();
            int layerOrder = 0;
            LayerTable layerTable = trans.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            foreach (ObjectId item in layerTable)
            {
                LayerTableRecord entity = (LayerTableRecord)item.GetObject(OpenMode.ForRead, false, false);
                LayerInfo lf = new LayerInfo();
                lf.Name = entity.Name;
                lf.Color = ColorUtils.GetColorStr(entity.Color);
                lf.Order = layerOrder++;
                layerInfoDict.Add(entity.Id, lf);
            }
            return layerInfoDict;
        }

        /// <summary>
        /// get lisp function params
        /// </summary>
        /// <param name="rbArgs"></param>
        /// <returns></returns>
        private List<string> getLispFunctionParams(ResultBuffer rbArgs)
        {
            List<string> functionParams = new List<string>();

            foreach (TypedValue rb in rbArgs)
            {
                string str = "";
                if (rb.TypeCode == (int)LispDataType.Text)
                {
                    str = rb.Value.ToString();
                }
                functionParams.Add(str);
            }
            return functionParams;
        }

        /// <summary>
        /// init
        /// </summary>
        private void initMessageParam()
        {
            this._showMessage = true;
            this._showAlertDialog = true;
        }

        private List<Tuple<string, Action>> getFunctions()
        {
            List<Tuple<string, Action>> functionDatas = new List<Tuple<string, Action>>();
            functionDatas.Add(new Tuple<string, Action>("Parse Select", () =>
            {
                this.initMessageParam();
                this.ParseSvgSelect(null);
            }));
            functionDatas.Add(new Tuple<string, Action>("Parse Current", () =>
            {
                this.initMessageParam();
                this.ParseSvg(null);
            }));
            functionDatas.Add(new Tuple<string, Action>("Parse Blocks", () =>
            {
                this.initMessageParam();
                this.ParseSvgBlocks(null);
            }));
            return functionDatas;
        }

        #endregion
    }
}
