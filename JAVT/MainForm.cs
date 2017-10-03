/*
 * Created by SharpDevelop.
 * User: IRU-OAS
 * Date: 25/07/2017
 * Time: 10:54 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

//Ref: http://www.c-sharpcorner.com/UploadFile/1e050f/text-to-speech-converter-in-C-Sharp/
//Ref: http://techqa.info/programming/tag/speechsynthesizer
//Ref: http://ellismis.com/2012/03/17/converting-or-transcribing-audio-to-text-using-c-and-net-system-speech/
//Ref: https://msdn.microsoft.com/en-us/library/ms723627(VS.85).aspx


namespace JAVT
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		
		private SpeechRecognitionEngine sr;
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			
		}
		
		void BrowseButtonClick(object sender, EventArgs e)
		{
			try {
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			DialogResult res = fbd.ShowDialog();
			
			if(res == DialogResult.OK) {
				
				textBox1.Text = fbd.SelectedPath;
				
				string[] fil = Directory.GetFiles(fbd.SelectedPath, "*" + textBox2.Text);
				
				int i = 0;
				foreach (string f in fil) {
					if(i>0) richTextBox1.Text += "\n" + f;
					else richTextBox1.Text += f;
					i++;
				}
				
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		void ConvButtonClick(object sender, EventArgs e)
		{
			try {
			string[] fil = richTextBox1.Text.Split('\n');
			
			foreach(string f in fil) {
				//MessageBox.Show(Directory.GetCurrentDirectory() + "\\ffmpeg\\bin\\ffmpeg.exe -i " + f + " -ab 160k -ac 2 -ar 44100 -vn " + f + ".wav");
				
				string arg = "-i " + f;
				if(v2aRadioButton.Checked) {
					arg += " -ab " + abTextBox.Text + " -ac 2 -ar 44100 -vn " + f + toTextBox.Text;
				}
				
				else if(v2vRadioButton.Checked) {
					arg += " " + f + toTextBox.Text;
				}
				
				else {
					arg += " " + f + toTextBox.Text + " -ab " + abTextBox.Text;
				}
				
				
				if(otherTextBox.Text != "") {
					arg += " " + otherTextBox.Text;
					arg = arg.Replace("  ", " ");
				}
				
				
				Process process = new Process();
				process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\JAVT\\ffmpeg\\bin\\ffmpeg.exe";
				//process.StartInfo.Arguments = "-i " + f + " -ab 160k -ac 2 -ar 44100 -vn " + f + ".wav";
				process.StartInfo.Arguments = arg;
				process.StartInfo.ErrorDialog = true;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
				process.Start();
				process.WaitForExit();
				
				//var proc = Process.Start(Directory.GetCurrentDirectory() + "\\ffmpeg\\bin\\ffmpeg.exe -i " + f + " -ab 160k -ac 2 -ar 44100 -vn " + f + ".wav");
				//proc.WaitForExit();
			}
			
			MessageBox.Show("Completed. ");
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		void BrowseButton2Click(object sender, EventArgs e)
		{
			try{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Audio Files (.Wav)|*.wav";
			
			DialogResult res = ofd.ShowDialog();
			
			if(res == DialogResult.OK) {
				
				textBox3.Text = ofd.FileName;
				
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		DictationGrammar dictation;
		void ConvButton2Click(object sender, EventArgs e)
		{
			try{
				
				if(engineComboBox.Text == "Microsoft SAPI") {
					richTextBox3.Text = "";
					dictation = new DictationGrammar();
					sr = new SpeechRecognitionEngine();
					sr.LoadGrammar(dictation);
			
					if(radioButton1.Checked)
						sr.SetInputToWaveFile(textBox3.Text);
					else
						sr.SetInputToDefaultAudioDevice();
			
					sr.RecognizeAsync(RecognizeMode.Multiple);
			
					//sr.SpeechHypothesized -= new EventHandler<SpeechHypothesizedEventArgs>(SpeechHypothesizing);
					sr.SpeechRecognized -= new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            		sr.EmulateRecognizeCompleted -= new EventHandler<EmulateRecognizeCompletedEventArgs>(EmulateRecognizeCompletedHandler);
			
            		//sr.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(SpeechHypothesizing);
            		sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);
            		sr.EmulateRecognizeCompleted += new EventHandler<EmulateRecognizeCompletedEventArgs>(EmulateRecognizeCompletedHandler);
				}
				
				else {
					
					string cmd = "";
					if(radioButton2.Checked) cmd = "-mic " + Directory.GetCurrentDirectory() + "\\JAVT\\cmu_sphinx\\temp.txt";
					else cmd = "-i " + textBox3.Text + " " + Directory.GetCurrentDirectory() + "\\JAVT\\cmu_sphinx\\temp.txt";
					
					StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + "\\JAVT\\cmu_sphinx\\cmd.txt");
					sw.WriteLine(cmd);
					sw.Close();
					
					Process process = new Process();
					process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\JAVT\\cmu_sphinx\\main.exe";
					//process.StartInfo.Arguments = "-i " + f + " -ab 160k -ac 2 -ar 44100 -vn " + f + ".wav";
					process.StartInfo.Arguments = cmd;
					process.StartInfo.ErrorDialog = true;
					process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					process.Start();
					process.WaitForExit();
					
					StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\JAVT\\cmu_sphinx\\temp.txt");
					string line = reader.ReadLine();
					
					int y = 0;
					while(line != null) {
						if(y > 0) this.richTextBox3.Text += "\n" + line;
						this.richTextBox3.Text = line;
						
						line = reader.ReadLine();
						y++;
					}
					
					reader.Close();
					
				}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		/*
		string realTimeResult;
		private void SpeechHypothesizing(object sender, SpeechHypothesizedEventArgs e) {
			realTimeResult = e.Result.Text;
		}*/
		
		string finalResult;
		private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
			try{
			finalResult = e.Result.Text;
			richTextBox3.Text += " " + finalResult;
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		bool isCompleted = false;
		private void EmulateRecognizeCompletedHandler(object sender, EmulateRecognizeCompletedEventArgs e) {
			try{
			isCompleted = true;
			
			sr.UnloadGrammar(dictation);
            sr.RecognizeAsyncStop();
            
            richTextBox3.Text += "\n\nCompleted. \n";
            MessageBox.Show("Completed. ");
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
            
		}
		void RadioButton2CheckedChanged(object sender, EventArgs e)
		{
			try{
			textBox3.Enabled = false;
			browseButton2.Enabled = false;
			convButton2.Text = "Start";
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void RadioButton1CheckedChanged(object sender, EventArgs e)
		{
			try{
			textBox3.Enabled = true;
			browseButton2.Enabled = true;
			convButton2.Text = "Convert";
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void SavButtonClick(object sender, EventArgs e)
		{
			try{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "text files (*.txt)|*.txt";
			DialogResult res = sfd.ShowDialog();
			
			if(res == DialogResult.OK) {
				richTextBox3.SaveFile(sfd.FileName);
			}
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
			
		}
		
		SpeechSynthesizer speaker;
		void SpeakButtonClick(object sender, EventArgs e)
		{
			try{
			speaker = new SpeechSynthesizer();
			speaker.Rate = int.Parse(rateTextBox.Text);
			speaker.Volume = int.Parse(volTextBox.Text);
			
			if(comboBox1.Text.ToLower().Trim() == "female") speaker.SelectVoiceByHints(VoiceGender.Female);
			if(comboBox1.Text.ToLower().Trim() == "male") speaker.SelectVoiceByHints(VoiceGender.Male);
			if(comboBox1.Text.ToLower().Trim() == "neutral") speaker.SelectVoiceByHints(VoiceGender.Neutral);
			if(comboBox1.Text.ToLower().Trim() == "") speaker.SelectVoiceByHints(VoiceGender.NotSet);
			
            speaker.SpeakAsync(richTextBox2.Text);
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void V2aRadioButtonCheckedChanged(object sender, EventArgs e)
		{
			try{
			abTextBox.Enabled = true;
			toTextBox.Text = ".wav";
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void V2vRadioButtonCheckedChanged(object sender, EventArgs e)
		{
			try{
			abTextBox.Enabled = false;
			toTextBox.Text = ".mp4";
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void A2aRadioButtonCheckedChanged(object sender, EventArgs e)
		{
			try{
			abTextBox.Enabled = true;
			toTextBox.Text = ".wav";
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void HelpButtonClick(object sender, EventArgs e)
		{
			try{
			System.Diagnostics.Process.Start(@"https://ffmpeg.org/ffmpeg.html");
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void RefreshButtonClick(object sender, EventArgs e)
		{
			try{
			string[] fil = Directory.GetFiles(textBox1.Text, "*" + textBox2.Text);
			
			richTextBox1.Text = "";
			
			int i = 0;
			foreach (string f in fil) {
				if(i>0) richTextBox1.Text += "\n" + f;
				else richTextBox1.Text += f;
				i++;
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void PauseButtonClick(object sender, EventArgs e)
		{
			try {
			speaker.Pause();
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void ResumeButtonClick(object sender, EventArgs e)
		{
			try {
			speaker.Resume();
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void StopButtonClick(object sender, EventArgs e)
		{
			try {
			speaker.Dispose();
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void Save2FileButtonClick(object sender, EventArgs e)
		{
			try {
			SaveFileDialog sfd = new SaveFileDialog();

			sfd.Filter = "wav files (*.wav)|*.wav";

			if (sfd.ShowDialog() == DialogResult.OK)
			{
     			FileStream fs = new FileStream(sfd.FileName,FileMode.Create,FileAccess.Write);
     			speaker = new SpeechSynthesizer();
				speaker.Rate = int.Parse(rateTextBox.Text);
				speaker.Volume = int.Parse(volTextBox.Text);
			
				if(comboBox1.Text.ToLower().Trim() == "female") speaker.SelectVoiceByHints(VoiceGender.Female);
				if(comboBox1.Text.ToLower().Trim() == "male") speaker.SelectVoiceByHints(VoiceGender.Male);
				if(comboBox1.Text.ToLower().Trim() == "neutral") speaker.SelectVoiceByHints(VoiceGender.Neutral);
				if(comboBox1.Text.ToLower().Trim() == "") speaker.SelectVoiceByHints(VoiceGender.NotSet);
     			
				//fileName, new SpeechAudioFormatInfo(22050, AudioBitsPerSample.Eight, AudioChannel.Mono
				speaker.SetOutputToWaveStream(fs);
     			speaker.Speak(richTextBox1.Text);
     			speaker.SetOutputToNull();
     			speaker.Dispose();
     			fs.Close();
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void OpenButtonClick(object sender, EventArgs e)
		{
			try {
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "text files (*.txt)|*.txt";
			
			DialogResult res = ofd.ShowDialog();
			
			if(res == DialogResult.OK) {
				richTextBox2.Text = "";
				
				StreamReader reader = new StreamReader(ofd.FileName);
				string line = reader.ReadLine();
				
				int i = 0;
				while(line != null) {
					
					if(i > 0) richTextBox2.Text += line;
					else richTextBox2.Text += line;
					
					line = reader.ReadLine();
					
					i++;
				}
				
				reader.Close();
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void RBrowseButtonClick(object sender, EventArgs e)
		{
			try {
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			DialogResult res = fbd.ShowDialog();
			
			if(res == DialogResult.OK) {
				
				rDirTextBox.Text = fbd.SelectedPath;
				
				string[] fil = Directory.GetFiles(fbd.SelectedPath, "*" + rExtTextBox.Text);
				
				int i = 0;
				foreach (string f in fil) {
					if(i>0) rRichTextBox.Text += "\n" + f;
					else rRichTextBox.Text += f;
					i++;
				}
				
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		void RRenameButtonClick(object sender, EventArgs e)
		{
			try {
				
			DialogResult res = MessageBox.Show("This may take some time. Continue?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			
			if(res == DialogResult.Yes) {
			string[] files = rRichTextBox.Text.Split('\n');
			
			int i = 1;
			foreach(string f in files) {
				
				string newFileName = Path.GetFileName(f);
				string newFileExt = Path.GetExtension(newFileName);
				
				if(rRCheckBox.Checked) {
					newFileName = newFileName.Replace(rRTextBox1.Text, rRTextBox2.Text);
				}
				
				if(rPCheckBox.Checked) {
					newFileName = rPTextBox.Text + newFileName;
					
					if(rPSCheckBox.Checked)
						newFileName = i.ToString() + newFileName;
				}
				
				if(rSCheckBox.Checked) {
					newFileName += rSTextBox.Text;
					
					if(rSSCheckBox.Checked)
						newFileName += i.ToString();
				}
				
				if(rCCheckBox.Checked) {
					if(rCComboBox.Text.ToLower() == "lowercase")
						newFileName = newFileName.ToLower();
					
					else if(rCComboBox.Text.ToLower() == "uppercase")
						newFileName = newFileName.ToUpper();
				}
				
				if(rECheckBox.Checked) {
					newFileName = newFileName.Replace(newFileExt, rETextBox.Text);
				}
				
				newFileName = rDirTextBox.Text + "\\" + newFileName;
				
				File.Copy(f, newFileName);
				if(rAllCheckBox.Checked) File.Delete(f);
				
				i++;
				
			}
			
			MessageBox.Show("Completed. ");
			}
			}
			
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
			
		}
		
		
	}
}
