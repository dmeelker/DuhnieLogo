using DuhnieLogo.Core.Interpreter;
using DuhnieLogo.Core.Tokens;
using DuhnieLogo.UI.Model;
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

        private System.Timers.Timer drawTimer = new System.Timers.Timer(10);
        private Bitmap drawingImage;
        private readonly object bufferLock = new object();

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
                    g.RotateTransform(turtle.Orientation);

                    g.DrawImageUnscaled(turtle.Image, -(turtle.Image.Width / 2), -turtle.Image.Height);
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
            turtleIndex[turtle.Name] = turtle;
            activeTurtles.Remove(turtle);
        }

        private void ActivateTurtle(Turtle turtle)
        {
            //activeTurtles.Clear();
            activeTurtles.AddLast(turtle);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            drawingImage = new Bitmap(panViewport.Width, panViewport.Height);

            using (var g = panViewport.CreateGraphics())
            using(var bufferG = Graphics.FromImage(drawingImage))
            {
                g.Clear(Color.White);
                bufferG.Clear(Color.White);
                ClearConsole();

                var tokens = Lexer.Tokenize(txtInput.Text).ToArray();

                ClearTurtles();

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

                //try
                //{
                interpreter.Interpret(tokens);
                //}
                //catch(ScriptException ex)
                //{
                //    if(ex.Token != null)
                //        WriteToConsole($"Er is een fout opgetreden: {ex.Message} (Regel: {ex.Token.Location.Row + 1}, Teken: {ex.Token.Location.Column + 1})");
                //    else
                //        WriteToConsole($"Er is een fout opgetreden: {ex.Message}");
                //}
                //catch (Exception ex)
                //{
                //    WriteToConsole($"Er is een fout opgetreden: {ex.Message}");
                //}

                drawTimer.Stop();
                DrawViewport();
            }
        }

        private void RegisterBuiltInProcedures(Interpreter interpreter, Graphics bufferG)
        {
            interpreter.RegisterFunction("print", new string[] { "message" }, (_globalMemory, _arguments) =>
            {
                var stringBuilder = new StringBuilder();

                foreach (var arg in _arguments)
                {
                    if (arg is List<string>)
                        stringBuilder.Append(string.Join(" ", arg as List<string>));
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

                var options = _arguments[1] as List<string>;
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

                if(_arguments[0] is List<string>)
                {
                    foreach(var name in _arguments[0] as List<string>)
                    {
                        var turtle = turtleIndex[name];
                        ActivateTurtle(turtle);
                    }
                }
                else
                {
                    var turtle = turtleIndex[_arguments[0].ToString()];
                    ActivateTurtle(turtle);
                }

                return null;
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
                var values = (List<string>)_arguments[0];

                foreach (var turtle in activeTurtles)
                    turtle.PenColor = Color.FromArgb(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]), Convert.ToInt32(values[2]));
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
                    if (arg is List<string>)
                        stringBuilder.Append(string.Join(" ", arg as List<string>));
                    else
                        stringBuilder.Append(arg.ToString());
                }

                var text = stringBuilder.ToString();
                foreach (var turtle in activeTurtles)
                    turtle.Print(text);

                return null;
            });
        }

        private void ClearConsole()
        {
            txtOutput.Text = "";
        }

        private void WriteToConsole(string message)
        {
            if (txtOutput.Text.Length > 0)
                txtOutput.AppendText(Environment.NewLine);

            txtOutput.AppendText(message);
        }

        private void panViewport_Resize(object sender, EventArgs e)
        {
            RecreateBuffer();
        }
    }
}
