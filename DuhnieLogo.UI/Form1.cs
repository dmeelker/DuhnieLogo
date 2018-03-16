﻿using DuhnieLogo.Core.Interpreter;
using DuhnieLogo.Core.Tokens;
using DuhnieLogo.UI.Model;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuhnieLogo.UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            drawTimer.Elapsed += DrawTimer_Elapsed;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private BufferedGraphicsContext bufferedGraphicsContext = new BufferedGraphicsContext();
        private BufferedGraphics bufferedGraphics;

        private Scintilla sourceField;

        private System.Timers.Timer drawTimer = new System.Timers.Timer(10);
        private Bitmap drawingImage;
        private readonly object bufferLock = new object();

        private readonly InputProvider inputProvider = new InputProvider();

        private void RecreateBuffer()
        {
            lock (bufferLock)
            {
                if (bufferedGraphics != null)
                {
                    bufferedGraphics.Graphics.Dispose();
                    bufferedGraphics.Dispose();
                }
                bufferedGraphics = bufferedGraphicsContext.Allocate(panViewport.CreateGraphics(), panViewport.DisplayRectangle);
            }
        }
        
        private void DrawTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DrawViewport();
        }

        private void DrawViewport()
        {
            lock (bufferLock)
            {
                if (bufferedGraphics == null)
                    RecreateBuffer();

                var g = bufferedGraphics.Graphics;
                g.DrawImageUnscaled(drawingImage, 0, 0);

                foreach (var turtle in turtles)
                {
                    g.TranslateTransform((int)turtle.Location.X, (int)turtle.Location.Y);

                    if(!turtle.VariantModus)
                        g.RotateTransform(turtle.Orientation);

                    //g.DrawImageUnscaled(turtle.Image, -(turtle.Image.Width / 2), -turtle.Image.Height);
                    g.DrawImageUnscaled(turtle.Image, 0, 0);
                    g.ResetTransform();
                }

                bufferedGraphics.Render();
            }
        }

        private List<Turtle> turtles = new List<Turtle>();
        private LinkedList<Turtle> activeTurtles = new LinkedList<Turtle>();
        private Dictionary<string, Turtle> turtleIndex = new Dictionary<string, Turtle>();

        private void ClearTurtles()
        {
            turtles.Clear();
            turtleIndex.Clear();
            activeTurtles.Clear();
        }

        private void RegisterTurtle(Turtle turtle)
        {
            turtles.Add(turtle);
            turtleIndex[turtle.Name.ToLower()] = turtle;
            activeTurtles.Remove(turtle);
        }

        private void ActivateTurtle(Turtle turtle)
        {
            //activeTurtles.Clear();
            activeTurtles.AddLast(turtle);
        }

        private Turtle GetTurtle(string name)
        {
            if (turtleIndex.ContainsKey(name.ToLower()))
                return turtleIndex[name.ToLower()];

            throw new ScriptException($"Er bestaat geen turtle met naam '{name}'");
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            EnableRunningMode();

            ClearConsole();
            ClearTurtles();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync(sourceField.Text);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                WriteToConsole($"Onverwachte fout: {e.Error}");

            drawTimer.Stop();
            DrawViewport();

            DisableRunningMode();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            RunScript(e.Argument as string);
        }

        private void RunScript(string script)
        {
            drawingImage = new Bitmap(panViewport.Width, panViewport.Height);

            using (var g = panViewport.CreateGraphics())
            using (var bufferG = Graphics.FromImage(drawingImage))
            {
                g.Clear(Color.White);
                bufferG.Clear(Color.White);
                
                var tokens = Core.Tokens.Lexer.Tokenize(script).ToArray();

                var mainTurtle = new Turtle("0")
                {
                    GraphicsContext = bufferG,
                    Location = new Model.Point(panViewport.Width / 2, panViewport.Height / 2),
                    Image = Image.FromFile("images/turtle.png")
                };

                RegisterTurtle(mainTurtle);
                ActivateTurtle(mainTurtle);

                var interpreter = new Interpreter();
                RegisterBuiltInProcedures(interpreter, bufferG);

                drawTimer.Start();

                try
                {
                    interpreter.Interpret(tokens);
                }
                catch (ScriptException ex)
                {
                    if (ex.Token != null)
                        WriteToConsole($"Er is een fout opgetreden: {ex.Message} (Regel: {ex.Token.Location.Row + 1}, Teken: {ex.Token.Location.Column + 1})");
                    else
                        WriteToConsole($"Er is een fout opgetreden: {ex.Message}");
                }
                catch (Exception ex)
                {
                    WriteToConsole($"Er is een fout opgetreden: {ex.Message}");
                }
            }
        }

        private void EnableRunningMode()
        {
            sourceField.Enabled = false;
            btnRun.Enabled = false;
        }

        private void DisableRunningMode()
        {
            sourceField.Enabled = true;
            btnRun.Enabled = true;
        }

        private void RegisterBuiltInProcedures(Interpreter interpreter, Graphics bufferG)
        {
            interpreter.RegisterFunction("print", new string[] { "message" }, (_globalMemory, _arguments) =>
            {
                var stringBuilder = new StringBuilder();

                foreach (var arg in _arguments)
                {
                    if (arg is ListVariable)
                        stringBuilder.Append(string.Join(" ", arg as ListVariable));
                    else
                        stringBuilder.Append(arg.ToString());
                }

                WriteToConsole(stringBuilder.ToString());
                
                return null;
            });

            

            interpreter.RegisterFunction("maakturtle", new string[] { "naam", "opties" }, (_memorySpace, _arguments) =>
            {
                var turtle = new Turtle(_arguments[0].ToString())
                {
                    GraphicsContext = bufferG,
                    Location = new Model.Point(panViewport.Width / 2, panViewport.Height / 2),
                    Image = Image.FromFile("images/turtle.png")
                };

                var options = _arguments[1] as ListVariable;
                if(options.Count >= 3)
                {
                    turtle.Location.X = Convert.ToInt32(options[0]);
                    turtle.Location.Y = Convert.ToInt32(options[1]);
                    turtle.Orientation = Convert.ToInt32(options[2]);
                }

                RegisterTurtle(turtle);

                return null;
            });

            interpreter.RegisterFunction("gebruik", new string[] { "turtles" }, (_memorySpace, _arguments) =>
            {
                activeTurtles.Clear();

                if(_arguments[0] is ListVariable)
                {
                    foreach(var name in _arguments[0] as ListVariable)
                    {
                        var turtle = GetTurtle(name);
                        ActivateTurtle(turtle);
                    }
                }
                else
                {
                    var turtle = GetTurtle(_arguments[0].ToString());
                    ActivateTurtle(turtle);
                }

                return null;
            });

            interpreter.RegisterFunction("overlap?", new string[] { "turtles" }, (_memorySpace, _arguments) =>
            {
                List<string> turtleNames;

                if (_arguments[0] is ListVariable)
                    turtleNames = (_arguments[0] as ListVariable);
                else
                    turtleNames = new List<string>() { _arguments[0].ToString() };

                var activeTurtle = activeTurtles.FirstOrDefault();
                if (activeTurtle == null)
                    throw new ScriptException("Er is geen active turtle");

                var activeTurtleBounds = activeTurtle.BoundingBox;

                foreach(var name in turtleNames)
                {
                    var turtle = GetTurtle(name);

                    if (turtle.BoundingBox.IntersectsWith(activeTurtleBounds))
                        return true;
                }

                return false;
            });

            interpreter.RegisterFunction("vooruit", new string[] { "stappen" }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.Forward((int)_arguments[0]);
                return null;
            });

            interpreter.RegisterFunction("achteruit", new string[] { "stappen" }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.Forward(-(int)_arguments[0]);
                return null;
            });

            interpreter.RegisterFunction("links", new string[] { "stappen" }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.Left((int)_arguments[0]);
                return null;
            });

            interpreter.RegisterFunction("rechts", new string[] { "stappen" }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.Right((int)_arguments[0]);
                return null;
            });

            interpreter.RegisterFunction("penop", new string[] { }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.PenDown = false;
                return null;
            });

            interpreter.RegisterFunction("penneer", new string[] { }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.PenDown = true;
                return null;
            });

            interpreter.RegisterFunction("zetpendikte", new string[] { "dikte" }, (_memorySpace, _arguments) =>
            {
                foreach (var turtle in activeTurtles)
                    turtle.PenWidth = (int)_arguments[0];
                return null;
            });

            interpreter.RegisterFunction("zetpenkleur", new string[] { "kleur" }, (_memorySpace, _arguments) =>
            {
                var values = (ListVariable)_arguments[0];

                foreach (var turtle in activeTurtles)
                    turtle.PenColor = Color.FromArgb(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]), Convert.ToInt32(values[2]));
                return null;
            });

            interpreter.RegisterFunction("zetvariantmodus", new string[] { "aan" }, (_memorySpace, _arguments) =>
            {
                var enabled = Convert.ToBoolean(_arguments[0]);

                foreach (var turtle in activeTurtles)
                    turtle.VariantModus = enabled;
                return null;
            });

            interpreter.RegisterFunction("zetvorm", new string[] { "afbeelding" }, (_memorySpace, _arguments) =>
            {
                var image = _arguments[0] as Image;

                foreach (var turtle in activeTurtles)
                    turtle.Image = image;

                return null;
            });

            interpreter.RegisterFunction("wistekening", new string[] { }, (_memorySpace, _arguments) =>
            {
                bufferG.Clear(Color.White);
                return null;
            });

            interpreter.RegisterFunction("printnaartv", new string[] { "tekst"}, (_memorySpace, _arguments) =>
            {
                var stringBuilder = new StringBuilder();

                foreach (var arg in _arguments)
                {
                    if (arg is ListVariable)
                        stringBuilder.Append(string.Join(" ", arg as ListVariable));
                    else
                        stringBuilder.Append(arg.ToString());
                }

                var text = stringBuilder.ToString();
                foreach (var turtle in activeTurtles)
                    turtle.Print(text);

                return null;
            });

            interpreter.RegisterFunction("toets?", new string[] { }, (_memorySpace, _arguments) =>
            {
                return inputProvider.KeyPressAvailable;
            });

            interpreter.RegisterFunction("leestoets", new string[] { }, (_memorySpace, _arguments) =>
            {
                var key = inputProvider.WaitForKeyPress();
                return (int) key;
            });
        }

        private void ClearConsole()
        {
            txtOutput.Text = "";
        }

        private void WriteToConsole(string message)
        {
            AppendText action = AppendToConsole;
            var text = (txtOutput.Text.Length > 0 ? Environment.NewLine : "") + message;

            if (txtOutput.InvokeRequired)
                txtOutput.Invoke(action, text);
            else
                action(text);
        }

        private delegate void AppendText(string text);

        private void AppendToConsole(string text)
        {
            txtOutput.AppendText(text);
        }

        private void panViewport_Resize(object sender, EventArgs e)
        {
            RecreateBuffer();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            inputProvider.KeyDown(e.KeyCode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sourceField = new Scintilla();
            panEditor.Controls.Add(sourceField);

            sourceField.Dock = DockStyle.Fill;

            var nums = sourceField.Margins[1];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            sourceField.StyleResetDefault();
            sourceField.Styles[Style.Default].Font = "Consolas";
            sourceField.Styles[Style.Default].Size = 10;

            SetupHighlighting();
        }

        private const int STYLE_KEYWORD = 1;
        private const int STYLE_IDENTIFIER = 2;
        private const int STYLE_STRINGLITERAL = 3;

        private void SetupHighlighting()
        {
            sourceField.Lexer = ScintillaNET.Lexer.Container;

            sourceField.Styles[STYLE_KEYWORD].ForeColor = Color.Blue;
            sourceField.Styles[STYLE_IDENTIFIER].ForeColor = Color.Green;
            sourceField.Styles[STYLE_STRINGLITERAL].ForeColor = Color.Red;

            sourceField.StyleNeeded += SourceField_StyleNeeded;
        }

        private void SourceField_StyleNeeded(object sender, StyleNeededEventArgs e)
        {
            StyleSyntax(e);
        }

        private void StyleSyntax(StyleNeededEventArgs e)
        {
            var fromIndex = sourceField.GetEndStyled();
            var toIndex = e.Position;

            try
            {
                var tokens = Core.Tokens.Lexer.Tokenize(sourceField.GetTextRange(fromIndex, toIndex - fromIndex));

                foreach (var token in tokens)
                {
                    var location = token.Location.Character + fromIndex;

                    if (IsKeyword(token))
                    {
                        sourceField.StartStyling(location);
                        sourceField.SetStyling(token.LiteralValue.Length, STYLE_KEYWORD);
                    }
                    if (token.Type == TokenType.Identifier)
                    {
                        sourceField.StartStyling(location);
                        sourceField.SetStyling(token.LiteralValue.Length, STYLE_IDENTIFIER);
                    }
                    if (token.Type == TokenType.StringLiteral)
                    {
                        sourceField.StartStyling(location);
                        sourceField.SetStyling(token.LiteralValue.Length, STYLE_STRINGLITERAL);
                    }


                }
            }
            catch(Exception ex)
            {

            }

        }

        private bool IsKeyword(Token token)
        {
            return
                token.Type == TokenType.Learn ||
                token.Type == TokenType.End ||
                token.Type == TokenType.Make ||
                token.Type == TokenType.Local ||
                token.Type == TokenType.True ||
                token.Type == TokenType.False;
        }
    }
}
