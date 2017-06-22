using System.Diagnostics;
using HciLab.Utilities;
using HciLab.Utilities.Mash3D;
using HciLab.KoBeLU.InterfacesAndDataModel;
using KoBeLUAdmin.Backend;
using KoBeLUAdmin.GUI.TypeEditor;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Threading.Tasks;
using KoBeLUAdmin.Network;
using System.Net;
using System.Windows.Forms;
using System.Collections.Generic;
using KoBeLUAdmin.Model.Process;
using System.Windows;
using System.Threading;
using KoBeLUAdmin.ContentProviders;

namespace KoBeLUAdmin.Scene
{
    [Serializable()]
    class SceneExternalVisualization : SceneItem, ISerializable
    {
        private int m_SerVersion = 1;

        private string m_Filename;

        private ImageSource m_ImageSource = null;

        private System.Collections.Generic.LinkedList<int> previousTries;
        private SceneRect[] previousRects;
        private SceneRect averageRect;
        private SceneRect[] pyramidRects;
        private SceneRect[] avatarRects;
        private SceneRect[] cupRects;
        private SceneRect[] borderRects;
        private SceneCircle avatarHead;
        private Color[] rectColor;
        private int[] indices;
        private int indexSum = 0;
        private int assemblyCount = 0;
        private System.IO.StreamWriter file;
        private String fileName;
        private String folder;

        private Color[] colorRange;

        private int assemblyNumber = 1;
        private System.Windows.Threading.DispatcherTimer redrawTimer = new System.Windows.Threading.DispatcherTimer();
        private long[] durations;
        private bool[] errors;

        private bool isInitialized = false;

        // calibration
        private const int calibrationAmount = 3;
        private int currentCalibration = 0;
        private long[,] calibrationTimes;
        private long[] meanDerivation;
        private long[] meanEstimator;
        private bool isCalibrating = true;
        private bool calibrationStarted = false;

        private string ipAdressGamification;
        private int portGamification = 20000;

        private int screenChecker; 

        public SceneExternalVisualization()
            : base()
        {
        }

        public SceneExternalVisualization(double x, double y, double w, double h, string pFilename, double rotation = 0, double rotationCenterX = 0, double rotationCenterY = 0, double scale = 1.0f, int amount = 10)
            : base(x, y, w, h, rotation, rotationCenterX, rotationCenterY, scale)
        {
            string visualizationPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\" + "Unity\\run2.exe";
            if (!System.IO.File.Exists(visualizationPath))
            {

                Debug.WriteLine("WARNING! PATH DOES NOT EXIST. NOT ADDING VISUALIZATION COMPONENT");
                //TODO: It is internally added so we can see it in editor but it's just an empty object. Delete that!
                return;
            }

            //Get IP Adress
            ipAdressGamification = SettingsManager.Instance.Settings.UDPIPTarget;
            Debug.WriteLine("IP Adress: " + ipAdressGamification);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //TODO: Load dynamically from KoBeLU GUI Interface via an open... dialogue
            //TODO: Insert a Command via System.IO for finding the path separator dynamically and system-dependent --> This is just for stable coding
            startInfo.FileName = visualizationPath;
            Process.Start(startInfo);

            if (pFilename == null || pFilename.Equals(String.Empty))
            {
                m_ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                 Properties.Resources.placeholder.GetHbitmap(),
                   IntPtr.Zero,
                   System.Windows.Int32Rect.Empty,
                   BitmapSizeOptions.FromWidthAndHeight(Properties.Resources.placeholder.Width, Properties.Resources.placeholder.Height));
            }
            else
                m_ImageSource = new BitmapImage(new Uri(m_Filename));

            Width = Properties.Resources.placeholder.Width;
            Debug.WriteLine("VISUALIZATION WIDTH: ---------------- " + Width);
            Height = Properties.Resources.placeholder.Height;
            initGamification();


        }


        protected SceneExternalVisualization(SerializationInfo pInfo, StreamingContext context)
            : base(pInfo, context)
        {
            int pSerVersion = pInfo.GetInt32("m_SerVersion");

            if (pSerVersion < 1)
                return;

            m_Filename = pInfo.GetString("m_Filename");

            ipAdressGamification = SettingsManager.Instance.Settings.UDPIPTarget;

            initGamification();
        }

        

        

        private void initGamification()
        {

            this.redrawTimer.Interval = new System.TimeSpan(1000000);
            this.redrawTimer.Start();
            this.redrawTimer.Tick += this.UpdateGameStatus;

            previousTries = new System.Collections.Generic.LinkedList<int>();
            previousRects = new SceneRect[10];
            avatarRects = new SceneRect[2];
            cupRects = new SceneRect[4];
            borderRects = new SceneRect[5];

            reconstrctDrawable();
            initializeColorRange();

            WorkflowManager.Instance.WorkingStepStarted += this.workingStepStarted;
            WorkflowManager.Instance.WorkingStepCompleted += this.updateStepCompleted;
            StateManager.Instance.StateChange += this.cancelAssembly;

            writeInitSettings();

            NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, "ic:0");
            Thread.Sleep(50);
            NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, "sc:" + getStepCount().ToString());

            



        }

        private void initGameStatus()
        {


            if (!calibrationStarted)
            {
                calibrationTimes = new long[getStepCount(), calibrationAmount];
                meanDerivation = new long[getStepCount()];
                meanEstimator = new long[getStepCount()];
                calibrationStarted = true;

                writeLogHeader(getStepCount());
            }

            if ((!isInitialized) && StateManager.Instance.State == AllEnums.State.WORKFLOW_PLAYING)
            {
                int stepCount = getStepCount();

                isInitialized = true;
                durations = new long[stepCount];
                rectColor = new Color[stepCount];
                indices = new int[stepCount];
                for (int i = 0; i < rectColor.Length; i++)
                {
                    rectColor[i] = Color.FromRgb(0, 255, 0);
                }
                errors = new bool[stepCount];
                for (int i = 0; i < errors.Length; i++)
                {
                    errors[i] = false;
                }

            }
            else if (StateManager.Instance.State != AllEnums.State.WORKFLOW_PLAYING)
            {
                isInitialized = false;
            }

        }

        private int addPosition = 0;

        public Point? GetLocationWithinScreen(Form form)
        {
            foreach (Screen screen in Screen.AllScreens)
                if (screen.Bounds.Contains(form.Location))
                    return new Point(form.Location.X - screen.Bounds.Left,
                                     form.Location.Y - screen.Bounds.Top);

            return null;
        }



        public void writeInitSettings()
        {
            fileName = "settings.txt";
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Settings\\";//@"C:\Users\iMotions Lab PC\Documents\Settings\";

            string[] lines = System.IO.File.ReadAllLines(folder + fileName);

            if (lines == null || !lines[0].ToLower().Contains("screen"))
                screenChecker = 0;
            else
            {
                if (lines[0] == "ScreenChecker:0")
                {
                    screenChecker = 0;
                }
                else if (lines[0] == "ScreenChecker:1")
                {
                    screenChecker = 1;
                }
            }

            Screen[] screens = Screen.AllScreens;
            System.Drawing.Rectangle bounds = screens[screenChecker].Bounds;
            double xrect = bounds.X;
            double yrect = bounds.Y;
            Debug.WriteLine("BOUNDS OF RECTANGLE: " + bounds);


            if(ScreenManager.isSecondScreenConnected()){
                addPosition = Screen.PrimaryScreen.Bounds.Width;
            }
            
            try
            {

                using (file = new System.IO.StreamWriter(folder + fileName, false))
                {
                    file.WriteLine("ScreenChecker:" + screenChecker);
                    file.WriteLine("ic:1");                    
                    file.WriteLine("po:" + (xrect + Convert.ToInt32(this.Y)) + ";" + (yrect + Convert.ToInt32(this.X)));
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ERROR");
            }
        }

        private void writeLogHeader(int stepCount)
        {
            fileName = "Assembly_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm", System.Globalization.CultureInfo.GetCultureInfo("de")) + ".csv";
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Logs\\";//@"C:\Users\Peter Muschick\Documents\Logs\";
            Debug.WriteLine(fileName);
            Debug.WriteLine(folder);

            String timeStrings = "";
            String errorStrings = "";
            for (int i = 0; i < stepCount; i++)
            {
                timeStrings += "Time#" + i + ";";
                errorStrings += "Error#" + i + ";";
            }

            try
            {
                using (file = new System.IO.StreamWriter(folder + fileName, true))
                {
                    file.AutoFlush = true;
                    file.WriteLine("Assembly#;" + timeStrings + errorStrings + "Calibrating;" + "totalTime;" + "errorCount;");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("ERROR");
            }
        }

        private void writeLogLine()
        {
            String timeStrings = "";
            String errorStrings = "";
            long totalTime = 0;
            int errorCount = 0;
            for (int i = 0; i < getStepCount(); i++)
            {
                timeStrings += durations[i] + ";";
                totalTime += durations[i];
                errorStrings += errors[i] + ";";
                if (errors[i])
                {
                    errorCount++;
                }
            }
            try
            {
                using (file = new System.IO.StreamWriter(folder + fileName, true))
                {
                    file.AutoFlush = true;
                    file.WriteLine(assemblyNumber + ";" + timeStrings + errorStrings + isCalibrating + ";" + totalTime + ";" + errorCount + ";");
                }
            }
            catch (Exception)
            {

            }
            assemblyNumber++;
        }

        private void UpdateGameStatus(object sender, EventArgs e)
        {

            if (isCalibrating)
            {
                //return;
            }

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                try
                {
                    int amount = getStepCount();

                    removeOldRects(SceneManager.Instance.CurrentScene);

                    if (StateManager.Instance.State == AllEnums.State.WORKFLOW_PLAYING && SceneManager.Instance.CurrentScene.ToString().Contains(this.ToString()))
                    {
                        pyramidRects = new SceneRect[amount];
                        calcCurrentRectColor();
                        
                        //MotionEAP Gamification, delete comments to start 
                        generatePyramid(amount);
                        generatePreviousRects();
                        generateAvatar();
                        generateCup();

                        if (WorkflowManager.Instance.CurrentWorkingStepNumber < durations.Length)
                        {
                            durations[WorkflowManager.Instance.CurrentWorkingStepNumber] += (long)redrawTimer.Interval.Milliseconds;
                        }
                    }

                }
                catch (Exception)
                {

                }
            }));
        }

        private void workingStepStarted(object sender, WorkingStepStartedEventArgs e)
        {
            initGameStatus();

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                try
                {
                    removeOldRects(SceneManager.Instance.CurrentScene);
                    Debug.WriteLine("START STEP");
                }
                catch (Exception)
                {

                }
            }));
            if (!isCalibrating)
            {
                if (WorkflowManager.Instance.CurrentWorkingStepNumber.ToString() != "0")
                {
                    NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, "ls:" + WorkflowManager.Instance.CurrentWorkingStepNumber.ToString());
                }
            }
        }

        private void updateStepCompleted(object sender, WorkingStepCompletedEventArgs e)
        {
            if (e.WorkingStepNumber < durations.Length)
            {
                durations[e.WorkingStepNumber] = e.StepDurationTime;
                Debug.WriteLine("STEP COMPLETED");
            }

            if (isCalibrating)
            {
                if (e.WorkingStepNumber < getStepCount())
                {
                    calibrationTimes[e.WorkingStepNumber, currentCalibration] = e.StepDurationTime;
                }
            }

            



            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                try
                {
                    if (SceneManager.Instance.CurrentScene.ToString().Contains(this.ToString()))
                    {
                        removeOldRects(SceneManager.Instance.CurrentScene);

                        if (e.WorkingStepNumber < getStepCount())
                        {
                            Scene nextScene = e.LoadedWorkflow.WorkingSteps[e.WorkingStepNumber + 1].getAdaptiveScene(WorkflowManager.Instance.AdaptivityLevelId).Scene;
                            nextScene.Add(this);
                        }
                        if (e.WorkingStepNumber != 0)
                        {
                            SceneManager.Instance.CurrentScene.Remove(this);
                        }

                        if (e.WorkingStepNumber == WorkflowManager.Instance.LoadedWorkflow.WorkingSteps.Count - 1)
                        {
                            writeLogLine();
                            isInitialized = false;
                            if (isCalibrating)
                            {
                                currentCalibration++;
                                if (currentCalibration >= calibrationAmount)
                                {
                                    isCalibrating = false;
                                    calcTimes();
                                }
                            }
                            else
                            {
                                addTimeToPreviousTries();
                            }
                            
                            
                        }
                    }

                }
                catch (Exception)
                {

                }
            }));
        }

        private void addTimeToPreviousTries()
        {
            if (previousTries.Count >= 10)
            {
                previousTries.RemoveLast();
            }

            for (int i = 0; i < errors.Length; i++)
            {
                if (errors[i])
                {
                    previousTries.AddFirst(9);
                    indexSum += 9;
                    assemblyCount++;
                    NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, "av:9");  
                    return;
                }
            }

            double average = 0.0;
            for (int i = 0; i < indices.Length; i++)
            {
                average += indices[i];
            }
            average = Math.Round(average / (double)indices.Length);
            previousTries.AddFirst((int)average);

            Debug.WriteLine("average: " + average);
            NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, "av:" + average);  

            indexSum += (int)average;
            assemblyCount++;

        }

        private void calcTimes()
        {
            for (int i = 0; i < getStepCount(); i++)
            {
                for (int j = 0; j < calibrationAmount; j++)
                {
                    meanEstimator[i] += calibrationTimes[i, j];
                }
                meanEstimator[i] /= calibrationAmount;
            }

            string meanEstimatorString = "me:";
            for (int i = 0; i < getStepCount(); i++)
            {
                meanEstimatorString = string.Concat(meanEstimatorString, string.Concat(meanEstimator[i].ToString(), ";"));
            }

            NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, meanEstimatorString);


            for (int i = 0; i < getStepCount(); i++)
            {
                for (int j = 0; j < calibrationAmount; j++)
                {
                    meanDerivation[i] += (calibrationTimes[i, j] - meanEstimator[i]) * (calibrationTimes[i, j] - meanEstimator[i]) / calibrationAmount;
                }
                meanDerivation[i] = (long)Math.Sqrt(meanDerivation[i]);

                if (meanDerivation[i] == 0)
                {
                    meanDerivation[i] = 1;
                }

            }

        }

        private void cancelAssembly(object sender, AllEnums.State state)
        {
            if (state != AllEnums.State.WORKFLOW_PLAYING)
            {
                if (WorkflowManager.Instance.CurrentWorkingStepNumber != 0)
                {
                    SceneManager.Instance.CurrentScene.Remove(this);
                }
            }
        }

        private void failCurrentStep(object sender, EventArgs e)
        {
            NetworkManager.Instance.SendDataOverUDP(ipAdressGamification, portGamification, "error");
            errors[WorkflowManager.Instance.CurrentWorkingStepNumber] = true;
        }

        private void removeOldRects(Scene scene)
        {
            try
            {
                if (pyramidRects != null)
                {
                    for (int i = 0; i < pyramidRects.Length; i++)
                    {
                        if (pyramidRects[i] != null)
                        {
                            scene.Remove(pyramidRects[i]);
                            pyramidRects[i] = null;
                        }
                    }
                }

                if (previousRects != null)
                {
                    for (int i = 0; i < previousRects.Length; i++)
                    {
                        if (previousRects[i] != null)
                        {
                            scene.Remove(previousRects[i]);
                            previousRects[i] = null;
                        }
                    }
                }

                if (averageRect != null)
                {
                    scene.Remove(averageRect);
                }

                if (avatarRects != null)
                {
                    for (int i = 0; i < avatarRects.Length; i++)
                    {
                        if (avatarRects[i] != null)
                        {
                            scene.Remove(avatarRects[i]);
                            avatarRects[i] = null;
                        }
                    }
                }

                if (avatarHead != null)
                {
                    scene.Remove(avatarHead);
                }

                if (cupRects != null)
                {
                    for (int i = 0; i < cupRects.Length; i++)
                    {
                        if (cupRects[i] != null)
                        {
                            scene.Remove(cupRects[i]);
                            cupRects[i] = null;
                        }
                    }
                }

                if (borderRects != null)
                {
                    for (int i = 0; i < borderRects.Length; i++)
                    {
                        if (borderRects[i] != null)
                        {
                            scene.Remove(borderRects[i]);
                            borderRects[i] = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private int getStepCount()
        {
            try
            {

                int count = WorkflowManager.Instance.LoadedWorkflow.WorkingSteps.Count;
                if (WorkflowManager.Instance.LoadedWorkflow.WorkingSteps[count - 1].Mode == AllEnums.PBD_Mode.END_CONDITION)
                {
                    return count - 1;
                }
                return count;
            }
            catch (Exception e)
            {
                return 3;
            }
        }

        public new void GetObjectData(SerializationInfo pInfo, StreamingContext pContext)
        {
            base.GetObjectData(pInfo, pContext);

            pInfo.AddValue("m_SerVersion", m_SerVersion);

            pInfo.AddValue("m_Filename", m_Filename);
        }

        protected override void reconstrctDrawable()
        {
            //this.Visual3DModel = Image3DGeo.Image(X, Y, Width, Height, m_ImageSource, 0);

            if (StateManager.Instance.State != AllEnums.State.WORKFLOW_PLAYING)
            {
                this.Visual3DModel = HciLab.Utilities.Mash3D.Rectangle3DGeo.Rect(X, Y, 100, 50, Color.FromRgb(0, 0, 255), Z);
            }
            else if (isCalibrating)
            {
                this.Visual3DModel = HciLab.Utilities.Mash3D.Text3DGeo.CreateTextLabel(X + 40, Y + 20, "Kalibration" + currentCalibration, Color.FromRgb(255, 255, 255), 12, new System.Windows.Media.FontFamily("Arial"));
            }
            else
            {
                this.Visual3DModel = null;
            }
            //HciLab.Utilities.Mash3D.PolygonMash3D.

            isFlashing();
            //calcPyramidRects();
            //UpdateGameStatus();
        }


        private void calcCurrentRectColor()
        {
            try
            {
                if (WorkflowManager.Instance.CurrentWorkingStep.Mode == AllEnums.PBD_Mode.END_CONDITION)
                {
                    return;
                }

                int curStep = WorkflowManager.Instance.CurrentWorkingStepNumber;

                if (errors[curStep])
                {
                    rectColor[curStep] = colorRange[9];
                    indices[curStep] = 9;
                    return;
                }

                double diff = meanEstimator[curStep] - durations[curStep];
                Debug.WriteLine("Mean Estimator [curStep]: " + meanEstimator[curStep]);
                Debug.WriteLine("Durations [curStep]" + durations[curStep]);
                diff /= (meanDerivation[curStep] * 2);

                long index = (long)(10 - (diff + 1) * 5);
                Debug.WriteLine("Diff: " + diff);
                Debug.WriteLine("INDEX: " + index);

                if (index < 0)
                {
                    index = 0;
                }
                else if (index > 9)
                {
                    index = 9;
                }
                rectColor[curStep] = colorRange[index];
                indices[curStep] = (int)index;
            }
            catch (Exception e)
            {

            }
        }

        private void generatePreviousRects()
        {
            int width = 15;
            int height = 15;
            int deltaX = -10;
            int deltaY = 100;

            int counter = 9;
            foreach (int item in previousTries)
            {
                previousRects[counter] = new SceneRect(X + deltaX + (counter % 5) * width, Y + deltaY - (counter / 5) * height, width - 1, height - 1, colorRange[item]);
                previousRects[counter].Visibility = System.Windows.Visibility.Visible;
                SceneManager.Instance.CurrentScene.Add(previousRects[counter]);
                counter--;
            }

            if (assemblyCount > 0)
            {
                double avg = Math.Round((double)indexSum / (double)assemblyCount);
                averageRect = new SceneRect(X + deltaX + 5 * width + 2, Y + deltaY - height, 2 * width - 3, 2 * height - 1, colorRange[(int)avg]);
                averageRect.Visibility = System.Windows.Visibility.Visible;
                SceneManager.Instance.CurrentScene.Add(averageRect);

                borderRects[0] = new SceneRect(X + deltaX - 1, Y + deltaY + height, 7 * width + 1, 1, Color.FromRgb(255, 255, 255));
                borderRects[1] = new SceneRect(X + deltaX - 1, Y + deltaY - height - 2, 7 * width + 1, 1, Color.FromRgb(255, 255, 255));
                borderRects[2] = new SceneRect(X + deltaX - 2, Y + deltaY - height - 2, 1, 2 * height + 3, Color.FromRgb(255, 255, 255));
                borderRects[3] = new SceneRect(X + deltaX + 5 * width, Y + deltaY - height - 2, 1, 2 * height + 3, Color.FromRgb(255, 255, 255));
                borderRects[4] = new SceneRect(X + deltaX + 7 * width, Y + deltaY - height - 2, 1, 2 * height + 3, Color.FromRgb(255, 255, 255));

                foreach (SceneRect rect in borderRects)
                {
                    rect.Visibility = System.Windows.Visibility.Visible;
                    SceneManager.Instance.CurrentScene.Add(rect);
                }

            }
        }

        private void generateAvatar()
        {
            try
            {
                int amount = getStepCount();
                double height = 50 / amount;
                double maxWidth = 100;
                double delta = maxWidth / (amount * 2);
                int currentStep = WorkflowManager.Instance.CurrentWorkingStepNumber;
                double x = X + maxWidth - delta * currentStep;
                double y = Y + height * currentStep;
                avatarRects[0] = new SceneRect(x + 6.5, y, 1.50, 5, Color.FromRgb(255, 255, 255));
                avatarRects[1] = new SceneRect(x + 5.5, y + 5, 3.5, 5, Color.FromRgb(255, 255, 255));
                avatarHead = new SceneCircle(x + 7.4, y + 12, 1.25, 0, 0, Color.FromRgb(255, 255, 255));

                avatarHead.Visibility = System.Windows.Visibility.Visible;
                SceneManager.Instance.CurrentScene.Add(avatarHead);

                for (int i = 0; i < avatarRects.Length; i++)
                {
                    avatarRects[i].Visibility = System.Windows.Visibility.Visible;
                    SceneManager.Instance.CurrentScene.Add(avatarRects[i]);
                }
            }
            catch (Exception)
            {

            }
        }

        private void generateCup()
        {
            for (int i = 0; i < errors.Length; i++)
            {
                if (errors[i])
                {
                    return;
                }
            }
            int offsetX = 40;
            int offsetY = 50;
            cupRects[0] = new SceneRect(X + offsetX + 2, Y + offsetY + 0, 10, 3, Color.FromRgb(255, 215, 0));
            cupRects[1] = new SceneRect(X + offsetX + 6, Y + offsetY + 3, 2, 3, Color.FromRgb(255, 215, 0));
            cupRects[2] = new SceneRect(X + offsetX + 4, Y + offsetY + 6, 6, 2, Color.FromRgb(255, 215, 0));
            cupRects[3] = new SceneRect(X + offsetX + 0, Y + offsetY + 8, 14, 5, Color.FromRgb(255, 215, 0));

            for (int i = 0; i < cupRects.Length; i++)
            {
                cupRects[i].Visibility = System.Windows.Visibility.Visible;
                SceneManager.Instance.CurrentScene.Add(cupRects[i]);
            }
        }

        /// <summary>
        /// Rectangles to be displayed in Gamification mode
        /// </summary>
        /// <param name="amount"></param>
        private void generatePyramid(int amount)
        {
            try
            {
                int height = 50 / amount;
                int maxWidth = 100;
                int delta = maxWidth / (amount * 2);

                for (int i = 0; i < amount; i++)
                {
                    if (rectColor[i] == null)
                    {
                        rectColor[i] = Color.FromRgb(0, 255, 0);
                    }

                    pyramidRects[i] = new SceneRect(X + delta * i, Y + height * i, maxWidth - 2 * delta * i, height - 1, rectColor[i]);

                    //pyramidRects[i].resetRect(rectColor[i]);

                    pyramidRects[i].Visibility = System.Windows.Visibility.Visible;

                    SceneManager.Instance.CurrentScene.Add(pyramidRects[i]);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Color Range of the gamification rects.
        /// </summary>
        private void initializeColorRange()
        {
            colorRange = new Color[10];
            colorRange[0] = Color.FromRgb(0, 255, 0);
            colorRange[1] = Color.FromRgb(75, 255, 0);
            colorRange[2] = Color.FromRgb(150, 255, 0);
            colorRange[3] = Color.FromRgb(200, 255, 0);
            colorRange[4] = Color.FromRgb(255, 255, 0);
            colorRange[5] = Color.FromRgb(255, 200, 0);
            colorRange[6] = Color.FromRgb(255, 150, 0);
            colorRange[7] = Color.FromRgb(255, 100, 0);
            colorRange[8] = Color.FromRgb(255, 75, 0);
            colorRange[9] = Color.FromRgb(255, 0, 0);
        }

        public override string Name
        {
            get
            {
                return "Gamification " + this.Id;
            }
        }

        [Category("Source")]
        [DisplayName("File Path")]
        [Description("The path to the image file")]
        [EditorAttribute(typeof(ImageBrowseTypeEditor), typeof(ImageBrowseTypeEditor))]
        public String FileName
        {
            get { return m_Filename; }
            set
            {
                m_Filename = value;
                try
                {
                    m_ImageSource = new BitmapImage(new Uri(m_Filename));
                }
                catch
                {
                    m_ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                       Properties.Resources.placeholder.GetHbitmap(),
                         IntPtr.Zero,
                         System.Windows.Int32Rect.Empty,
                         BitmapSizeOptions.FromWidthAndHeight(Properties.Resources.placeholder.Width, Properties.Resources.placeholder.Height));
                }

                NotifyPropertyChanged("FileName");
            }
        }
    }
}
