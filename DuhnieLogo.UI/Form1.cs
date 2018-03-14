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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuhnieLogo.UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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

        private void btnRun_Click(object sender, EventArgs e)
        {
            var buffer = new Bitmap(panViewport.Width, panViewport.Height);

            using (var g = panViewport.CreateGraphics())
            using(var bufferG = Graphics.FromImage(buffer))
            {
                g.Clear(Color.White);
                bufferG.Clear(Color.White);
                ClearConsole();

                var tokens = Lexer.Tokenize(txtInput.Text).ToArray();
                var turtle = new Turtle() {
                    GraphicsContext = g,
                    BufferBitmap = buffer,
                    BufferContext = bufferG,
                    Location = new Model.Point(panViewport.Width / 2, panViewport.Height / 2),
                    Image = Image.FromFile("images/turtle.png")
                };

                var interpreter = new Interpreter();

                interpreter.RegisterFunction("print", new string[] { "message" }, (_globalMemory, _arguments) => {
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

                interpreter.RegisterFunction("vooruit", new string[] { "stappen" }, (_memorySpace, _arguments) => {
                    turtle.Forward((int)_arguments[0]);
                    return null;
                });

                interpreter.RegisterFunction("achteruit", new string[] { "stappen" }, (_memorySpace, _arguments) => {
                    turtle.Forward(-(int)_arguments[0]);
                    return null;
                });

                interpreter.RegisterFunction("links", new string[] { "stappen" }, (_memorySpace, _arguments) => {
                    turtle.Left((int)_arguments[0]);
                    return null;
                });

                interpreter.RegisterFunction("rechts", new string[] { "stappen" }, (_memorySpace, _arguments) => {
                    turtle.Right((int)_arguments[0]);
                    return null;
                });

                interpreter.RegisterFunction("penop", new string[] {}, (_memorySpace, _arguments) => {
                    turtle.PenDown = false;
                    return null;
                });

                interpreter.RegisterFunction("penneer", new string[] { }, (_memorySpace, _arguments) => {
                    turtle.PenDown = true;
                    return null;
                });

                interpreter.RegisterFunction("zetpendikte", new string[] { "dikte" }, (_memorySpace, _arguments) => {
                    turtle.PenWidth = (int)_arguments[0];
                    return null;
                });

                interpreter.RegisterFunction("zetpenkleur", new string[] { "kleur" }, (_memorySpace, _arguments) => {
                    var values = (List<string>)_arguments[0];

                    turtle.PenColor = Color.FromArgb(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]), Convert.ToInt32(values[2]));
                    return null;
                });

                interpreter.RegisterFunction("wistekening", new string[] { }, (_memorySpace, _arguments) => {
                    g.Clear(Color.White);
                    bufferG.Clear(Color.White);
                    return null;
                });

                try
                {
                    interpreter.Interpret(tokens);
                }
                catch(ScriptException ex)
                {
                    if(ex.Token != null)
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
    }
}
