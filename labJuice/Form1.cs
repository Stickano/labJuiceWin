using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using System.Windows.Forms.DataVisualization.Charting;
using lj;


namespace labJuice
{
    public partial class Form1 : Form
    {
        private static int numSample = 1000;
        private static int sampleRate = 2000;
        private static int magSecond;
        private static int magThird;
        private static double PHSecond;
        private static double PHThird;

        Complex[] sample = new Complex[numSample];

        public Form1()
        {
            InitializeComponent();
        }

        public static void ThrowErrorMessage(string msg, int errorCode)
        {
            StringBuilder errorString = new StringBuilder(50);
            LabJack.GetErrorString(errorCode, errorString);
            throw new Exception(msg + ":\r\n\r\n" + errorString);
        }

        //---------------------------------------------------------------------
        // This returns an array of all the local IDs which we use.
        //
        public static int[] GetAllLabJacks()
        {
            // Make sure we allocate space for what is passed
            int[] productIDList = new int[127];
            int[] serialNumList = new int[127];
            int[] localIDList = new int[127];
            int[] powerList = new int[127];
            int[,] calMatrix = new int[127, 20];
            int numFound = 0;
            int reserved1 = 0, reserved2 = 0;

            // Call the ListAll function.  We must use the keyword ref for parameters 
            // that aren't arrays that return data
            int result = LabJack.ListAll(productIDList, serialNumList, localIDList,
                powerList, calMatrix, ref numFound, ref reserved1, ref reserved2);
            if (result != 0)
                ThrowErrorMessage("Unable to enumerate controllers", result);

            int[] ljs = new int[numFound];
            int i = 0;

            // count how many we found and set
            // the array which will be returned
            // to contain valid IDs
            foreach (int id in localIDList)
            {
                if (id != 9999)
                {
                    ljs[i] = id;
                    ++i;
                }
            }

            // return that array
            return ljs;
        }

        //---------------------------------------------------------------------
        // This is our function that read's analog inputs
        //
        public static float ReadAnalogInput(int channel)
        {
            int ljID = -1;
            int overVoltage = 0;
            float voltage = 0.0f;

            int result = LabJack.EAnalogIn(ref ljID, 0, channel, 0, ref overVoltage, ref voltage);
            if (result != 0)
                ThrowErrorMessage("Error reading analog input", result);
            return voltage;
        }

        //---------------------------------------------------------------------
        // AIBurst
        //
        public static float[,] AIBurst(int numScans)
        {
            int ljID = -1;
            int stateIOin = 0;
            int[] channels = { 0, 0, 0, 0 };
            int[] gains = { 0, 0, 0, 0 };
            float scanRate = 2048;
            float[,] voltages = new float[4096, 4];
            int[] stateIOout = new int[4096];
            int overVoltage = 0;

            int result = LabJack.AIBurst(ref ljID, 0, stateIOin, 0,
                1, 1,
                channels,
                gains,
                ref scanRate,
                0, 0, 0,
                numScans, 5,
                voltages,
                stateIOout,
                ref overVoltage,
                2);

            if (result != 0)
                ThrowErrorMessage("Error setting digital output", result);

            return voltages;
        }

        //---------------------------------------------------------------------
        // AIStream
        //
        public void AIStream()
        {

            int error;
            int ljID = -1;
            int demo = 1, numChannels = 4, disableCal = 0;
            int[] channels = { 0, 1, 2, 3 };
            int[] gains = { 0, 0, 0, 0 };
            float sr = 250.0F;
            int numScans = 250, timeout = 10;
            float[,] voltages = new float[4096, 4];
            int[] stateIOout = new int[4096];
            int ljb = -1;
            int ov = -1;
            int numIts = 5;
            int reserved = 0;

            Console.WriteLine("AIStream");
            error = LabJack.AIStreamStart(ref ljID, demo, 0, 0, 1, numChannels, channels,
                gains, ref sr, disableCal, 0, 0);
            if (error != 0)
            {
                richTextBox3.AppendText("AIStreamStart Error: " + error + "\r\n");
            }

            int i = 0;
            while ((error == 0) && (i < numIts))
            {
                for (int j = 0; j < 400; j++)
                {
                    stateIOout[j] = 0;
                    voltages[j, 3] = 0;

                    //for (int k = 0; k < 4; k++)
                    //{
                    //}

                    double time = (double)j / numSample;
                    chart3.Series["Second Harmonic"].Points.AddXY(time, voltages[0, 2]);
                }

                error = LabJack.AIStreamRead(ljID, numScans, timeout, voltages, stateIOout, ref reserved, ref ljb, ref ov);
                if (error != 0)
                {
                    StringBuilder sb = new StringBuilder();
                    LabJack.GetErrorString(error, sb);
                    richTextBox3.AppendText("AIStreamRead Error: " + sb + "\r\n");
                }
                //richTextBox3.AppendText("1st Scan:  V1="+ voltages[0, 0]+", V2="+ voltages[0, 1]+", V3="+ voltages[0, 2]+", V4="+ voltages[0, 3]+"\r\n");
                richTextBox3.AppendText("1st Scan:  V3=" + voltages[0, 2] + "\r\n");
                richTextBox3.AppendText("LabJack Scan Backlog = " + ljb + "\r\n");
                i++;

                chart3.ChartAreas["ChartArea1"].AxisX.Minimum = 0;
                chart3.ChartAreas["ChartArea1"].AxisX.RoundAxisValues();

            }

            LabJack.AIStreamClear(ljID);

        }

        //---------------------------------------------------------------------
        // Set the analog outputs
        //
        public static void SetAnalogOutput(int channel, float voltage)
        {
            int ljID = -1;
            int result = 0;

            if (channel == 0)
                result = LabJack.EAnalogOut(ref ljID, 0, voltage, -1.0f);
            else if (channel == 1)
                result = LabJack.EAnalogOut(ref ljID, 0, -1.0f, voltage);
            else
                throw new Exception("Invalid analog output channel");

            if (result != 0)
                ThrowErrorMessage("Error reading analog input", result);
        }

        public void plotWaveform(int secondHarm, int thirdHarm, double secondPH, double thirdPH)
        {

            chart3.Series["Waveform"].Points.Clear();
            chart3.Series["Second Harmonic"].Points.Clear();
            chart3.Series["Third Harmonic"].Points.Clear();

            chart3.Series["Waveform"].LegendText = "Waveform";
            chart3.Series["Waveform"].ChartType = SeriesChartType.Line;

            chart3.ChartAreas["ChartArea1"].AxisX.TitleFont = new Font("Arial", 14f);
            chart3.ChartAreas["ChartArea1"].AxisX.Title = "Seconds";

            int fundamentalHz = Convert.ToInt32(numericUpDown3.Value);
            int secondHz = Convert.ToInt32(numericUpDown1.Value);
            int thirdHz = Convert.ToInt32(numericUpDown2.Value);

            // Fundamental, 2nd & 3rd harmonic waveforms
            double[] fundamental = Generate.Sinusoidal(numSample, sampleRate, fundamentalHz, 10.0);
            double[] second = Generate.Sinusoidal(numSample, sampleRate, secondHz, secondHarm, 0.0, secondPH);
            double[] third = Generate.Sinusoidal(numSample, sampleRate, thirdHz, thirdHarm, 0.0, thirdPH);

            // Add waveforms to get composite waveforms
            for (int i = 0; i < numSample; i++)
            {
                sample[i] = new Complex(fundamental[i] + second[i] + third[i], 0);
            }

            // Plot composite waveforms
            for (int i = 0; i < sample.Length / 5; i++)
            {
                double time = ((i + 1.0) / numSample) / 2;

                // Add second & third harmonic if checked
                if (checkBox2.Checked)
                    chart3.Series["Second Harmonic"].Points.AddXY(time, second[i]);
                if (checkBox3.Checked)
                    chart3.Series["Third Harmonic"].Points.AddXY(time, third[i]);

                // Add Waveform
                chart3.Series["Waveform"].Points.AddXY(time, sample[i].Real);
            }
        }

        public void plotFFT()
        {
            chart2.Series["Frequency"].Points.Clear();
            chart2.ChartAreas["ChartArea1"].AxisX.Title = "Hz";
            chart2.ChartAreas["ChartArea1"].AxisX.TitleFont = new Font("Arial", 14f);
            chart2.ChartAreas["ChartArea1"].AxisX.MinorTickMark.Enabled = true;

            double magnitude;

            // Forward Fourier converts time => frequency
            Fourier.Forward(sample, FourierOptions.NoScaling);

            // Plot frequency spectrum
            for (int i = 0; i < sample.Length / 10; i++)
            {
                // Get magnitude of each FFT sample
                // abs[sqrt(r^2 + i^2)]
                magnitude = (2 / numSample) * (Math.Abs(Math.Sqrt(Math.Pow(sample[i].Real, 2) + Math.Pow(sample[i].Imaginary, 2))));

                // hz by each sample
                double hzPerSample = sampleRate / numSample;

                chart2.Series["Frequency"].Points.AddXY(hzPerSample * i, magnitude);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // First call the GetAllLabJacks Function we Defined

            // List the IDs of LabJacks we found
            int[] idList = GetAllLabJacks();
            if (idList.Length == 0)
            {
                richTextBox3.AppendText("No LabJack device found! \r\n");
                return;
            }

            //SetAnalogOutput(0, 5.0f);
            //Task.Factory.StartNew(() => AIStream());
            AIStream();
            richTextBox3.AppendText("Device voltage: " + ReadAnalogInput(2) + "\r\n");

            foreach (int id in idList)
            {
                richTextBox3.AppendText("Connected with LabJack using ID: " + id + "\r\n");
            }

            // AIBurst
            richTextBox3.AppendText("AIBurst scans: " + numSample + "\r\n");
            float[,] v = AIBurst(numSample);
            for (int i = 0; i < numSample; i++)
            {
                sample[i] = new Complex(v[i, 0], 2);
            }

            chart3.ChartAreas["ChartArea1"].AxisX.Minimum = 0;
            chart3.ChartAreas["ChartArea1"].AxisX.RoundAxisValues();

            // 400 = samples to chart
            for (int i = 0; i < 400; i++)
            {
                double time = (double)i / numSample;
                chart3.Series["Waveform"].Points.AddXY(time, v[i, 0]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox3.AppendText("AIBurst \r\n");
            chart3.Series["Waveform"].Points.Clear();
            chart3.ChartAreas["ChartArea1"].AxisX.Minimum = 0;

            float[,] v = AIBurst(numSample);
            for (int i = 0; i < 400; i++)
            {
                double time = (double)i / numSample;
                chart3.Series["Waveform"].Points.AddXY(time, v[i, 0]);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            chart2.Series["Frequency"].Points.Clear();
            chart2.ChartAreas["ChartArea1"].AxisX.Title = "Hz";
            chart2.ChartAreas["ChartArea1"].AxisX.TitleFont = new Font("Arial", 14f);
            chart2.ChartAreas["ChartArea1"].AxisX.MinorTickMark.Enabled = true;

            double magnitude;

            // Forward Fourier converts time => frequency
            Fourier.Forward(sample, FourierOptions.NoScaling);

            // Plot frequency spectrum
            for (int i = 0; i < sample.Length / 10; i++)
            {
                // Get magnitude of each FFT sample
                // abs[sqrt(r^2 + i^2)]
                magnitude = (2.0 / numSample) * (Math.Abs(Math.Sqrt(Math.Pow(sample[i].Real, 2) + Math.Pow(sample[i].Imaginary, 2))));
                
                // hz by each sample
                double hzPerSample = sampleRate / numSample;

                chart2.Series["Frequency"].Points.AddXY(hzPerSample * i, magnitude);
            }
        }

        #region Demo Tools

        /// <summary>
        /// Clear the upper chart
        /// </summary>
        private void clearChar3()
        {
            chart3.Series["Waveform"].Points.Clear();
            chart3.Series["Second Harmonic"].Points.Clear();
            chart3.Series["Third Harmonic"].Points.Clear();
        }

        /// <summary>
        /// Enable demo tools
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                plotWaveform(0, 0, 0, 0);
            else
                clearChar3();
        }

        /// <summary>
        /// Magnitude for second harmonic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            magSecond = trackBar1.Value;
            textBox1.Text = trackBar1.Value.ToString();
            plotWaveform(magSecond, magThird, PHSecond, PHThird);
        }

        /// <summary>
        /// Enable plot of second harmonic (View in chart)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            plotWaveform(magSecond, magThird, PHSecond, PHThird);

        }

        /// <summary>
        /// Enable plot of third harmonic (view in chart)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            plotWaveform(magSecond, magThird, PHSecond, PHThird);

        }

        /// <summary>
        /// Waveform Hz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            plotWaveform(magSecond, magThird, PHSecond, PHThird);

        }

        /// <summary>
        /// Magnitude for third harmonic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            magThird = trackBar2.Value;
            textBox2.Text = trackBar2.Value.ToString();
            plotWaveform(magSecond, magThird, PHSecond, PHThird);
        }

        /// <summary>
        /// Hz for second harmonic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            plotWaveform(magSecond, magThird, PHSecond, PHThird);

        }

        /// <summary>
        /// Hz for third harmonic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            plotWaveform(magSecond, magThird, PHSecond, PHThird);

        }

        #endregion
        
    }
}
