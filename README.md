# CadSVGParser
A plugin to export autocad entities to Svg.<br>
Support *line,polyline,ellipse,spline,arc,circle,hatch* entity.

# 1. How to build
1. Open CADParser.sln
2. In Microsoft Visual Studio, click View menu -> Solution Explorer to display the Solution Explorer if it is not already displayed.
3. In the Solution Explorer, on the toolbar along the top, click Show All Files.
4. Right-click the References node and click Add Reference.
5. Check ***acmgd.dll***,***acdbmgd.dll*** and ***accoremgd.dll*** is import from your AutoCAD install folder.<br>
   The default install location of AutoCAD is C:\Program Files\AutoCAD XXXX. 
6. In Microsoft Visual Studio, click Build menu -> Build CADParser.<br>
   The location of the ***SvgParser.dll*** file that is built is also displayed in the Output window.

# 2. How to use
## Load DLL:
1. Start AutoCAD if it is not already running.
2. In AutoCAD, at the Command prompt, enter ***netload*** and press Enter.
3. In the Choose .NET Assembly dialog box, browse to the location of the ***SvgParser.dll*** and select it. Click Open.

## Export svg:
- use command ***ParseSvg*** to parse current block data.
- use command ***ParseSvgBlocks*** to parse all block data.
- use command ***ParseSvgSelect*** to select and parse.
- use command ***(ParseSvg "filename")*** to select and parse. like (ParseSvg "C:\\output.svg")
- use command ***(ParseSvgBlocks "dictionaryname")*** to select and parse. like (ParseSvgBlocks "C:\\output\\")
- use command ***(ParseSvgSelect "filename")*** to select and parse. like (ParseSvgSelect "C:\\output.svg")
