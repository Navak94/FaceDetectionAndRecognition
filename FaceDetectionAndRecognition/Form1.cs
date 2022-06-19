using System;
using Emgu.CV;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Structure;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.IO.Ports;
using System.Collections;
//using System.Random;

namespace FaceDetectionAndRecognition

{


    public partial class Form1 : Form
    {
        //Declare Variables
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr,Byte> Frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace= null;
        Image<Gray, byte> grayface= null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels= new List<string>();
        List<string> Users= new List<string>();
        string name, names = null;
        int Count, NumLabels, t;
        string faceInfoForArduinoTargeting;
        SerialPort port;
        List<string> VocabUlaryRememBered = new List<string>();
        SpeechSynthesizer synth = new SpeechSynthesizer();
        System.Collections.Generic.IEnumerable<String> Lines; //= File.ReadLines("C:\\Users\\Nate Hindman\\source\\repos\\FaceDetectionAndRecognition\\FaceDetectionAndRecognition\\bin\\Debug\\20k.txt");
        List<string> VocaBularY = new List<string>();
        SpeechRecognitionEngine Speechreco = new SpeechRecognitionEngine();
        SpeechSynthesizer Voice = new SpeechSynthesizer();
       
        void getPortNames()
        {
            
            String[] ports = SerialPort.GetPortNames();
            port = new SerialPort();

            try
            {
                port.PortName = ports[0];
                port.BaudRate = 9600;
                port.Open();
            }
           
            catch (Exception e1)
            {
                MessageBox.Show("No arduino connected  >:(");
            }
            
        }

       SpeechRecognitionEngine sE = new SpeechRecognitionEngine();
        
        private void Form1_Load(object sender, EventArgs e)
        {

            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
            getPortNames();
            
        }
       

        public Form1()
        {

            Speechreco.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Speechreco_SpeechRecognized);
            LoadGrammar();
            Speechreco.SetInputToDefaultAudioDevice();
            Speechreco.RecognizeAsync(RecognizeMode.Multiple);
            InitializeComponent();
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(',');   
                NumLabels = Convert.ToUInt16(Labels[0]);
                string FacesLoad;
                for(int i=1;i<NumLabels + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/Faces.txt"));
                    labels.Add(Labels[i]);
                }


            }
            catch(Exception ex)
            {
              
            }
        }

        private void Speechreco_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine(e.Result.Text);
        }

        private void LoadGrammar()
        {
            Choices texts = new Choices();
            string[] lines = File.ReadAllLines(Environment.CurrentDirectory+ "\\20k.txt");
            texts.Add(lines);
            Grammar wordlist = new Grammar(new GrammarBuilder(texts));
            Speechreco.LoadGrammar(wordlist);
        }
        private  void LoadGrammar1()
        {

            DictationGrammar defaultDictationGrammar = new DictationGrammar();
            defaultDictationGrammar.Name = "default dictation";
            defaultDictationGrammar.Enabled = true;
            SpeechRecognitionEngine recoEngine = new SpeechRecognitionEngine();
            recoEngine.LoadGrammar(defaultDictationGrammar);
            Console.WriteLine(recoEngine);


        }

        private void FrameProcedure(object sender, EventArgs e)
        {
        
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayface = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayface.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,new Size(20,20));
           
            foreach(MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray,Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                
                float xinfo = f.rect.X;
                float yinfo = f.rect.Y;
                try
                {
                    if (port.IsOpen)
                    {
                        port.WriteLine(xinfo + ":" + yinfo + "\n");
                    }
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }

                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(Count, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                }
            
                Users.Add("");
            }

            CameraBox.Image = Frame;
            names = "";
            Users.Clear();
        }
         
    }

}