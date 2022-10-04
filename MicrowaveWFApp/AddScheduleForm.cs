using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicrowaveFormsApp
{
    

    public partial class FrmAddSchedule : Form
    {
        public String MwName { get; private set;}
        public String MwPreperation { get; private set; }
        public String MwTime { get; private set; }
        public String MwPotency { get; private set; }
        public char MwCharacter { get; private set; }


        public FrmAddSchedule()
        {
            InitializeComponent();
        }
        

        private void Save()
        {
            MwName = txtName.Text;
            MwPreperation = txtPreparation.Text;
            MwTime = nuTempo.Value.ToString();
            MwPotency = nuPotency.Value.ToString();
            MwCharacter = char.Parse(txtChar.Text.Substring(0, 1));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
            Close();
        }
    }
}
