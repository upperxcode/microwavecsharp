using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Benner;
using Store;


namespace MicrowaveFormsApp
{
    public partial class MainForm : Form
    {
       
        private const String Ok = "Ok";
        public MainForm()
        {
            InitializeComponent();
        }

        private void atualizarTela()
        {
            lblPotency.Text = $"Potência: {MicroWaveStore.Potency}";
            lblStatus.Text = $"{MicroWaveStore.State}";
            lblTime.Text = $"{MicroWaveStore.TimeFormated()}";
        }

        private void addOutPutText(String text)
        {
            outputText.AppendText(text + Environment.NewLine);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }

            if (btnOpen.Text == Microwave.OpenDoor)
            {
                addOutPutText(MicroWaveStore.Open());
                btnOpen.Text = Microwave.ClosedDoor;
            }
            else
            {
                addOutPutText(MicroWaveStore.Close());
                btnOpen.Text = Microwave.OpenDoor;
            }

            atualizarTela();

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void addTime_Click(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            MicroWaveStore.AddSeconds();
            atualizarTela();
        }

        private void removeTime_Click(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            MicroWaveStore.RemoveSeconds();
            atualizarTela();
        }

        private void incPotency_Click(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            MicroWaveStore.IncrementPontency();
            atualizarTela();
        }

        private void decPotency_Click(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            MicroWaveStore.DecrementPontency();
            atualizarTela();
        }

        private void insertTime(object sender, MouseEventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            MicroWaveStore.StrTimeFormat((sender as Button).Text[0]);
            atualizarTela();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadSchedules();
            btnOpen.Text = Microwave.OpenDoor;

        }

        private void showSchedule_Click(object sender, EventArgs e)
        {
            String selectedSchedule = cmbSchedules.Text;
            addOutPutText(MicroWaveStore.ShowSchedule(selectedSchedule));
        }

        private void LoadSchedules()
        {
            List<String> list = MicroWaveStore.ScheduleNames();
            cmbSchedules.Items.Clear();
           
            list?.ForEach(item => cmbSchedules.Items.Add(item) );

            if (cmbSchedules.Items.Count > 0)
            {
                cmbSchedules.SelectedIndex = 0;
            }

            txtSchedule.Text = "";

        }

        private void showAllSchedule_Click(object sender, EventArgs e)
        {
            addOutPutText(MicroWaveStore.ShowAllSchedule());
        }

        private void addSchedule_Click(object sender, EventArgs e)
        {
            String mwName;
            String mwPreperation;
            int mwTime;
            int mwPotency;
            char mwCharacter;

            FrmAddSchedule frmSchedule = new();
            try
            {
                if (frmSchedule.ShowDialog(this) == DialogResult.OK)
                {
                    mwName = frmSchedule.MwName;
                    mwPreperation = frmSchedule.MwPreperation;
                    mwTime = int.Parse(frmSchedule.MwTime);         //a validação no form garante o tipo
                    mwPotency = int.Parse(frmSchedule.MwPotency);
                    mwCharacter = frmSchedule.MwCharacter;

                    MicroWaveStore.AddSchedule(mwName, mwPreperation, mwCharacter, mwTime, mwPotency);
                    LoadSchedules();
                }
            }
            finally
            {
                frmSchedule.Dispose();
            }


        }

        private void SelectSchedule(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            String str = (sender as Button).Text;
            if (str == Ok)
            {
                str = txtSchedule.Text;
            }
            try
            {
                addOutPutText(MicroWaveStore.SelectSchedule(str));
            }
            catch (FileErrorMicroWaveException ex)
            {
                ShowError(ex.Message);
            }

            atualizarTela();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
            const String errorMsg = "Ocorreu um erro na tentativa de aquecimento";
            btnStart.Enabled = false;
            if (MicroWaveStore.State != Microwave.StatePuased)
            {
                outputText.Clear();
            }

            try
            {
                MicroWaveStore.Start(
                (ret, status) =>
                {
                    outputText.AppendText(ret);
                    atualizarTela();
                    Application.DoEvents();
                    return status;
                }
            );

                outputText.AppendText($"\n {MicroWaveStore.ShowProgress()}\n");
                atualizarTela();

            }
            catch (StateErrorMicroWaveException ex)
            {
                ShowError($"{errorMsg}\n {ex.Message}");
            }
            catch (RangeErrorMicroWaveException ex)
            {
                ShowError($"{errorMsg}\n {ex.Message}");
            }
            catch (FileErrorMicroWaveException ex)
            {
                ShowError($"{errorMsg}\n {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocorreu um erro não tratado: {ex.Message}.");
            }
            finally
            {
                btnStart.Enabled = true;
            }

        }


        private void ShowError(String message)
        {
            MessageBox.Show(message, "Erro",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void button26_Click(object sender, EventArgs e)
        {
            outputText.Clear();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            MicroWaveStore.Interrupt(false);
        }

        private void button21_Click(object sender, EventArgs e)
        {
                MicroWaveStore.Interrupt(true);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (MicroWaveStore.InWarmingUp())
            {
                return;
            }
            MicroWaveStore.QuickStart();
            Start();
        }

        private void cmbSchedules_TextUpdate(object sender, EventArgs e)
        {}

        private void cmbSchedules_TextChanged(object sender, EventArgs e)
        {
            txtSchedule.Text = cmbSchedules.Text;
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}
