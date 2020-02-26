using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathFinder
{
    public partial class MainForm : Form
    {
        //========================================================
        //Constructor
        public MainForm()
        {
            InitializeComponent();

            //Assign Button_Click handler to every button of the form
            foreach (var button in Controls.OfType<Button>())
                button.Click += Button_Click;

            //Setup radioButtons start position
            this.radioButtonGame.Checked = true;
        }

        //========================================================
        //Events
        public event Action<byte,byte> XYcellClick;
        public event Action<InterfaceMode> InterfaceModeChanged;
        public event Action<bool> ShowNumbersChanged;

        //========================================================
        //Handlers
        private void radioButtonGame_Click(object sender, EventArgs e)
        {
            InterfaceModeChanged?.Invoke(InterfaceMode.GAME);
        }

        private void radioButtonSetup_Click(object sender, EventArgs e)
        {
            InterfaceModeChanged?.Invoke(InterfaceMode.SETUP);
        }

        private void ShowNumbersCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ShowNumbersChanged?.Invoke( this.ShowNumbersCheckbox.Checked );
        }

        private void Button_Click(object sender, EventArgs e)
        {
            string buttonName = ((Button)sender).Name;
            byte X = byte.Parse(buttonName[7].ToString());
            byte Y = byte.Parse(buttonName[6].ToString());
            //Button name 'button75' will result in X 5 and Y 7

            XYcellClick?.Invoke(X,Y);
        }

        //========================================================
        //Methods
        public void PaintTheCell(byte X, byte Y, Color color)
        {
            GetButton(X,Y).BackColor = color;
        }

        public void PrintOnCell(byte X, byte Y, string value)
        {
            GetButton(X,Y).Text = value;
        }

        Button GetButton(byte X, byte Y)
        {
            string buttonName = $"button{Y}{X}";
            Type currenttype = this.GetType();
            FieldInfo fieldInfo = currenttype.GetField(buttonName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (Button)fieldInfo.GetValue(this);
        }
    }
}
