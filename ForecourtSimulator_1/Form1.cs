using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForecourtSimulator_1
{
    public partial class Form1 : Form
    {

        #region Member variables

        #region Settings
        public int pumpFieldCount { get; set; }
        private int numberOfRows;

        private Size baseSize;
        private Point baseLocation;
        private Size baseGradeButtonSize;
        private Size baseNozzleHandleButtonSize;

        private double baseAmount = 0.00;
        private int transactionID = 1;
        private bool emergencyStop = false;
        int numberOfConcurrentTransactions = 2;

        ForecourtCommunication fcCommunication;

        //Last grade-button checked
        private List<CheckBox> lastChecked = new List<CheckBox>();

        //How many transactions each pump has
        private List<int> numberOfTransactions = new List<int>();

        //List of grades
        private List<Grade> grades = new List<Grade>();

        //Thread objects, used for doing automatic simulation
        private List<Thread> pumpThreads = new List<Thread>();
        //Thread locks, one for each pump
        private List<object> threadLocks = new List<object>();
        //Thread pause signals, used when "handle" button is unchecked
        private List<bool> threadPauseSignals = new List<bool>();
        //Thread stop signals, used when "nozzle" button is unchecked
        private List<bool> threadStopSignals = new List<bool>();

        #endregion

        #region Images

        private static System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

        private Bitmap imageAuthorized = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.authorized.png"));
        private Bitmap imageCalling = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.calling.png"));
        private Bitmap imageClosed = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.closed.png"));
        private Bitmap imageError = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.error.png"));
        private Bitmap imageOffline = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.offline.png"));
        private Bitmap imageFuelling = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.pumping.png"));
        private Bitmap imageReserved = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.reserved.png"));
        private Bitmap imageStop = new Bitmap(assembly.GetManifestResourceStream("ForecourtSimulator_1.Images.stop.png"));
        #endregion

        #region Control names
        private string baseContainerName = "panelPumpContainer";
        private string baseLabelPumpIDName = "labelPumpIDName";
        private string baseStatusName = "labelStatus";
        private string baseGroupBoxName = "groupBoxGrades";
        private string baseButtonGradeName = "buttonGrade";
        private string baseButtonNozzleName = "buttonNozzle";
        private string baseButtonHandleName = "buttonHandle";
        private string baseAmountName = "labelAmount";
        private string baseVolumeName = "labelVolume";
        private string basePriceName = "labelPrice";
        private string baseAmountNumberName = "labelAmountDouble";
        private string baseVolumeNumberName = "labelVolumeDouble";
        private string basePriceNumberName = "labelPriceDouble";
        private string basePictureBoxIconName = "pictureBoxIcon";
        private string baseListBoxTransactionsName = "listTransactions";
        private string baseLabelTransactionsName = "labelTransactions";

        #endregion

        #endregion


        public Form1()
        {
            InitializeComponent();

            pumpFieldCount = 0;
            numberOfRows = 0;
            baseSize = new Size(530, 180);
            baseLocation = new Point(13, 60);
            baseGradeButtonSize = new Size(30, 23);
            baseNozzleHandleButtonSize = new Size(75, 25);


            //Set grades 
            grades.Add(new Grade(1, "95 okt", 1.5));  //95 okt, price = 1, can be changed
            grades.Add(new Grade(2, "98 okt", 2.0));
            grades.Add(new Grade(3, "Diesel", 1.0));

            StartServerAndClient();
        }

        public void StartServerAndClient()
        {
            //Create an instance of Forecourt Communication, start the Simulator server and create a thread to wait until the client connects to the Forecourt server
            ForecourtCommunication fcc = new ForecourtCommunication(this);
            fcCommunication = fcc;
            fcc.StartPipeServer();

            Thread FCThread = new Thread(() => fcc.CreateClient());
            FCThread.IsBackground = true;
            FCThread.Start();

        }

        #region Adding pump field to UI 
        private void buttonAddPump_Click(object sender, System.EventArgs e)
        {

            //Create the new panel
            Panel newPumpField = new Panel();
            newPumpField.BackColor = System.Drawing.Color.White;
            newPumpField.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            newPumpField.Name = baseContainerName + (pumpFieldCount + 1).ToString();
            newPumpField.Size = baseSize;
            newPumpField.TabIndex = pumpFieldCount;

            //Set the location of the panel appropriately 
            //If there are two fields in the row, create a new row and place the new field on the left
            if (pumpFieldCount % 2 == 0)
            {
                newPumpField.Location = new System.Drawing.Point(baseLocation.X, (numberOfRows * (baseSize.Height + 10)) + baseLocation.Y);

                numberOfRows++;
            }
            //If there is one field in the row, place the new field to the right of the already existing plane 
            else
            {
                newPumpField.Location = new System.Drawing.Point(baseLocation.X + baseSize.Width + 10, ((numberOfRows - 1) * (baseSize.Height + 10)) + baseLocation.Y);
            }

            pumpFieldCount++;
            this.Controls.Add(newPumpField);

            //Add into thread-lists
            //pumpThreads, threadLocks, threadStopSignals 
            pumpThreads.Add(null);
            threadLocks.Add(new object());
            threadPauseSignals.Add(false);
            threadStopSignals.Add(false);

            //Add into the transaction-list
            numberOfTransactions.Add(0);

            //Add controls inside of the panel 

            //Temporarily suspend control layout logic while inserting all controls
            newPumpField.SuspendLayout();

            //Pump label
            AddLabel(newPumpField);

            //Buttons on bottom
            AddButtons(newPumpField);

            //Add text fields for amount, volume and price
            AddText(newPumpField);

            //Add image according to status
            AddImage(newPumpField);

            //Add list of transactions
            AddTransactions(newPumpField);

            //Resume layout control
            newPumpField.ResumeLayout(false);
            newPumpField.PerformLayout();
        }

        #region Labels
        private void AddLabel(Panel newPumpField)
        {

            //Add numeric lable in the top-left corner
            Label newLabel = new Label();
            newPumpField.Controls.Add(newLabel);
            newLabel.AutoSize = true;
            newLabel.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newLabel.Location = new Point(15, 4);
            newLabel.Name = baseLabelPumpIDName + pumpFieldCount.ToString();
            newLabel.Size = new Size(32, 36);
            newLabel.TabIndex = 0;
            newLabel.Text = pumpFieldCount.ToString();

            //Add status of pump, displayed on the top
            Label newStatus = new Label();
            newPumpField.Controls.Add(newStatus);
            newStatus.AutoSize = true;
            newStatus.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newStatus.Location = new Point(50, 4);
            newStatus.Name = baseStatusName + pumpFieldCount.ToString();
            newStatus.Size = new Size(32, 36);
            newStatus.TabIndex = 0;
            newStatus.Text = "Authorized";
            newStatus.TextChanged += new System.EventHandler(StatusLabelTextChanged);
        }
        #endregion

        #region Buttons
        private void AddButtons(Panel newPumpField)
        {

            //Add handle button
            CheckBox newHandleButton = new CheckBox();
            newPumpField.Controls.Add(newHandleButton);
            newHandleButton.Appearance = Appearance.Button;
            newHandleButton.AutoSize = true;
            newHandleButton.Location = new Point(170, 150);
            newHandleButton.Name = baseButtonHandleName + pumpFieldCount.ToString();
            newHandleButton.Size = baseNozzleHandleButtonSize;
            newHandleButton.TabIndex = 4;
            newHandleButton.Text = "Handle";
            newHandleButton.UseVisualStyleBackColor = true;
            newHandleButton.Click += new System.EventHandler(HandleButton_Click);

            // Add nozzle button
            CheckBox newNozzleButton = new CheckBox();
            newPumpField.Controls.Add(newNozzleButton);
            newNozzleButton.Appearance = Appearance.Button;
            newNozzleButton.AutoSize = true;
            newNozzleButton.Location = new Point(117, 150);
            newNozzleButton.Name = baseButtonNozzleName + pumpFieldCount.ToString();
            newNozzleButton.Size = baseNozzleHandleButtonSize;
            newNozzleButton.TabIndex = 4;
            newNozzleButton.Text = "Nozzle";
            newNozzleButton.UseVisualStyleBackColor = true;
            newNozzleButton.Click += new System.EventHandler(NozzleButton_Click);

            //Create new groupBox for all the grade buttons
            GroupBox newGroupBox = new GroupBox();
            newGroupBox.SuspendLayout();
            newPumpField.Controls.Add(newGroupBox);

            //Create grade buttons
            CheckBox newGrade1 = new CheckBox();
            CheckBox newGrade2 = new CheckBox();
            CheckBox newGrade3 = new CheckBox();
            newGroupBox.Controls.Add(newGrade1);
            newGroupBox.Controls.Add(newGrade2);
            newGroupBox.Controls.Add(newGrade3);

            //Settings for groupBox
            newGroupBox.Location = new Point(3, 133);
            newGroupBox.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newGroupBox.Name = baseGroupBoxName + pumpFieldCount.ToString();
            newGroupBox.Size = new Size(107, 43);
            newGroupBox.TabIndex = 0;
            newGroupBox.TabStop = false;
            newGroupBox.Text = "Grades:";

            //Names of grade buttons: buttonGrade + pump + grade 
            //Example, pump 4, grade 2: buttonGrade42
            //To get pump ID from name: Fetch second to last char from chararray, or length-2
            //To get grade ID from name: Fetch last char from chararray, or length-1

            //New Grade 1 button
            newGrade1.Appearance = Appearance.Button;
            newGrade1.AutoSize = true;
            newGrade1.Location = new Point(4, 16);
            newGrade1.Name = baseButtonGradeName + pumpFieldCount.ToString() + "1";
            newGrade1.Size = baseGradeButtonSize; /*new Size(86, 27);*/
            newGrade1.TabIndex = 4;
            newGrade1.Text = "G1";
            newGrade1.UseVisualStyleBackColor = true;
            newGrade1.Click += new System.EventHandler(GradeButtons_Click);

            //New Grade 2 button
            newGrade2.Appearance = Appearance.Button;
            newGrade2.AutoSize = true;
            newGrade2.Location = new Point(38, 16);
            newGrade2.Name = baseButtonGradeName + pumpFieldCount.ToString() + "2";
            newGrade2.Size = baseGradeButtonSize; /*new Size(86, 27);*/
            newGrade2.TabIndex = 4;
            newGrade2.Text = "G2";
            newGrade2.UseVisualStyleBackColor = true;
            newGrade2.Click += new System.EventHandler(GradeButtons_Click);

            //New Grade 3 button
            newGrade3.Appearance = Appearance.Button;
            newGrade3.AutoSize = true;
            newGrade3.Location = new Point(72, 16);
            newGrade3.Name = baseButtonGradeName + pumpFieldCount.ToString() + "3";
            newGrade3.Size = baseGradeButtonSize; /*new Size(86, 27);*/
            newGrade3.TabIndex = 4;
            newGrade3.Text = "G3";
            newGrade3.UseVisualStyleBackColor = true;
            newGrade3.Click += new System.EventHandler(GradeButtons_Click);

            lastChecked.Add(null);

            newGroupBox.ResumeLayout(false);
            newGroupBox.PerformLayout();
        }
        #endregion

        #region Text for Amount, Volume and Price
        private void AddText(Panel newPumpField)
        {

            var textSize = 9F;

            //Create Amount, Volume and Price labels
            Label newAmount = new Label();
            Label newVolume = new Label();
            Label newPrice = new Label();
            newPumpField.Controls.Add(newAmount);
            newPumpField.Controls.Add(newVolume);
            newPumpField.Controls.Add(newPrice);

            //Create number fields
            Label newNumAmount = new Label();
            Label newNumVolume = new Label();
            Label newNumPrice = new Label();
            newPumpField.Controls.Add(newNumAmount);
            newPumpField.Controls.Add(newNumVolume);
            newPumpField.Controls.Add(newNumPrice);

            //Amount 
            newAmount.AutoSize = true;
            newAmount.Location = new Point(10, 48);
            newAmount.Font = new Font("Microsoft Sans Serif", textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newAmount.Name = baseAmountName + pumpFieldCount.ToString();
            newAmount.Size = new Size(64, 17);
            newAmount.TabIndex = 0;
            newAmount.Text = "Amount: ";

            //Amount num
            newNumAmount.AutoSize = true;
            newNumAmount.Location = new Point(90, 48);
            newNumAmount.Font = new Font("Microsoft Sans Serif", textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newNumAmount.Name = baseAmountNumberName + pumpFieldCount.ToString();
            newNumAmount.Size = new Size(68, 17);
            newNumAmount.TabIndex = 0;
            newNumAmount.Text = baseAmount.ToString("N2");

            //Volume
            newVolume.AutoSize = true;
            newVolume.Location = new Point(10, 74);
            newVolume.Font = new Font("Microsoft Sans Serif", textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newVolume.Name = baseVolumeName + pumpFieldCount.ToString();
            newVolume.Size = new Size(64, 17);
            newVolume.TabIndex = 0;
            newVolume.Text = "Volume: ";

            //Volume num
            newNumVolume.AutoSize = true;
            newNumVolume.Location = new Point(90, 74);
            newNumVolume.Font = new Font("Microsoft Sans Serif", textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newNumVolume.Name = baseVolumeNumberName + pumpFieldCount.ToString();
            newNumVolume.Size = new Size(68, 17);
            newNumVolume.TabIndex = 0;
            newNumVolume.Text = baseAmount.ToString("N2");

            //Price
            newPrice.AutoSize = true;
            newPrice.Location = new Point(10, 100);
            newPrice.Font = new Font("Microsoft Sans Serif", textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newPrice.Name = basePriceName + pumpFieldCount.ToString();
            newPrice.Size = new Size(64, 17);
            newPrice.TabIndex = 0;
            newPrice.Text = "Price: ";

            //Price num
            newNumPrice.AutoSize = true;
            newNumPrice.Location = new Point(90, 100);
            newNumPrice.Font = new Font("Microsoft Sans Serif", textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newNumPrice.Name = basePriceNumberName + pumpFieldCount.ToString();
            newNumPrice.Size = new Size(68, 17);
            newNumPrice.TabIndex = 0;
            newNumPrice.Text = baseAmount.ToString("N2");


        }
        #endregion

        #region Images

        private void AddImage(Panel newPumpField)
        {

            //Create a new panel that will contain the picture box
            Panel newPanel = new Panel()
            {
                Name = "formPictureBox" + pumpFieldCount.ToString(),
                Size = new Size(40, 40),
                Location = new Point(485, 5),
                Visible = true
            };
            newPumpField.Controls.Add(newPanel);


            //Create a picture box, place it inside panel, add bitmap as Image property 
            PictureBox newPictureBox = new PictureBox();
            newPanel.Controls.Add(newPictureBox);

            newPictureBox.Name = basePictureBoxIconName + pumpFieldCount.ToString();
            newPictureBox.TabIndex = 0;
            newPictureBox.Image = imageAuthorized;
            newPictureBox.Dock = DockStyle.Fill;
            newPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

        }

        #endregion

        #region Transactions 

        public void AddTransactions(Panel newPumpField)
        {

            //Add label to clarify that this is a list of previous transactions
            Label newTransLabel = new Label();
            newPumpField.Controls.Add(newTransLabel);

            newTransLabel.AutoSize = true;
            newTransLabel.Location = new Point(234, 47);
            newTransLabel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            newTransLabel.Name = baseLabelTransactionsName + pumpFieldCount.ToString();
            newTransLabel.Size = new Size(64, 17);
            newTransLabel.TabIndex = 0;
            newTransLabel.Text = "Transactions: ";

            //Create a ListView, that will display transaction data in a table
            ListView newList = new ListView();
            newPumpField.Controls.Add(newList);

            //Create the table headers
            newList.View = View.Details;
            newList.Columns.Add("ID", 30, HorizontalAlignment.Center);
            newList.Columns.Add("Amount", 63, HorizontalAlignment.Center);
            newList.Columns.Add("Volume", 63, HorizontalAlignment.Center);
            newList.Columns.Add("Price", 63, HorizontalAlignment.Center);
            newList.Columns.Add("Grade", 63, HorizontalAlignment.Center);

            newList.Location = new Point(237, 65);
            newList.Size = new Size(286, 110);
            newList.Name = baseListBoxTransactionsName + pumpFieldCount;
            newList.Alignment = ListViewAlignment.SnapToGrid;

            //Load transactions into the table
            //Currently only using fabricated test data, will improve on this 
            //var transactions = CreateTestData();
            //foreach (Transaction t in transactions)
            //{
            //    if (pumpFieldCount == t.PumpID)
            //    {
            //        //Transform the Transaction-object into a ListViewItem object, and then add it to the ListView
            //        var toAdd = new ListViewItem(new[] { t.ID.ToString(), t.Amount.ToString("N2"), t.Volume.ToString("N2"), t.Price.ToString("N2"), t.GradeID.ToString() });
            //        newList.Items.Add(toAdd);
            //    }
            //}

        }

        #endregion


        #endregion

        //Remove the last pump from the UI (the one with the hightest ID)
        private void buttonRemovePump_Click(object sender, EventArgs e)
        {
            Panel toRemove = Controls.Find(baseContainerName + pumpFieldCount.ToString(), true).FirstOrDefault() as Panel;

            if (toRemove != null)
                this.Controls.Remove(toRemove);

            if (pumpFieldCount % 2 != 0 && numberOfRows > 0)
                numberOfRows--;
            if (pumpFieldCount > 0)
                pumpFieldCount--;

        }

        //Make sure that only one object inside of the groupBox is checked
        private void GradeButtons_Click(object sender, EventArgs e)
        {
            CheckBox activeCheckBox = sender as CheckBox;
            if (activeCheckBox == null)
                return;

            int pumpID = int.Parse(activeCheckBox.Name[activeCheckBox.Name.Length - 2].ToString());
            int index = pumpID - 1;
            Label status = this.Controls.Find(baseStatusName + pumpID, true).FirstOrDefault() as Label;

            //If fuelling has started, the grade-buttons cannot change 
            if (status.Text == "Fuelling" || status.Text == "Paused" || status.Text == "Starting")
            {
                activeCheckBox.Checked = lastChecked[index] == activeCheckBox;
                return;
            }


            //Find the index of the pump. 
            //This is done so that there can only be one checkbox active in each groupbox, but multiple groupboxes can have active checkboxes

            if (lastChecked[index] != null)
            {
                lastChecked[index].Checked = false;
            }

            lastChecked[index] = activeCheckBox.Checked ? activeCheckBox : null;

            //Set the price number field with the price corresponding to the grade 
            int gradeID = int.Parse(activeCheckBox.Name[activeCheckBox.Name.Length - 1].ToString());
            Label priceNum = this.Controls.Find(basePriceNumberName + pumpID, true).FirstOrDefault() as Label;

            Grade gradeOfButton = grades[gradeID - 1];
            if (priceNum != null)
                priceNum.Text = gradeOfButton.price.ToString("N2");

            //Every time that a button is pressed, send the grade to Forecourt Manager
            fcCommunication.SetPumpGrade(pumpID - 1, gradeID - 1);
        }

        //Desc
        private void HandleButton_Click(object sender, EventArgs e)
        {
            CheckBox handle = sender as CheckBox;
            if (handle == null)
                return;

            int pumpID = int.Parse(handle.Name[handle.Name.Length - 1].ToString());

            CheckBox nozzle = this.Controls.Find(baseButtonNozzleName + pumpID, true).FirstOrDefault() as CheckBox;

            //If the corresponding nozzle button is not checked, then nothing should happen to the handle button
            if (nozzle?.Checked == false)
            {
                handle.Checked = false;
                return;
            }

            Label status = this.Controls.Find(baseStatusName + pumpID, true).FirstOrDefault() as Label;

            if (status != null)
            {
                if (status.Text == "Calling")
                    ;  //What to do? 
                else if (status.Text == "Starting")
                    StatusStartingToFuelling(pumpID, status);
                else if (status.Text == "Fuelling")
                    StatusFuellingToPaused(pumpID, status);
                else if (status.Text == "Paused")
                    StatusPausedToFuelling(pumpID, status);
            }

        }

        private void NozzleButton_Click(object sender, EventArgs e)
        {
            CheckBox nozzle = sender as CheckBox;
            if (nozzle == null)
                return;

            int pumpID = int.Parse(nozzle.Name[nozzle.Name.Length - 1].ToString());

            //Check if there is a grade-button chosen. If not, then nothing happens. 
            if (lastChecked[pumpID - 1] == null)
            {
                nozzle.Checked = false;
                return;
            }

            //Get the state of the pump 
            Label status = this.Controls.Find(baseStatusName + pumpID, true).FirstOrDefault() as Label;

            if (status.Text == "Authorized")
                StatusAuthorizedToStarting(pumpID, status);
            else if (status.Text == "Idle")
                StatusIdleToCalling(pumpID, status);
            else if (status.Text == "Starting")
                StatusStartingToAuthorized(pumpID, status);
            else if (status.Text == "Fuelling" || status.Text == "Paused")
                FuellingFinished(pumpID, status);
        }

        #region Change Status of Pumps
        private void StatusIdleToCalling(int pumpID, Label status)
        {
            status.Text = "Calling";
            fcCommunication.PumpToCalling(pumpID);

            ChangeImage(pumpID, imageCalling);
        }

        private void StatusAuthorizedToStarting(int pumpID, Label status)
        {

            status.Text = "Starting";
            fcCommunication.PumpToStarting(pumpID);

            //No image for starting? 
        }

        private void StatusStartingToFuelling(int pumpID, Label status)
        {

            status.Text = "Fuelling";

            fcCommunication.PumpToFuelling(pumpID);

            //Fetch the amount and volume lables, and reset them to 0.0 
            Label volume = this.Controls.Find(baseVolumeNumberName + pumpID, true).FirstOrDefault() as Label;
            volume.Text = baseAmount.ToString("N2");
            Label amount = this.Controls.Find(baseAmountNumberName + pumpID, true).FirstOrDefault() as Label;
            amount.Text = baseAmount.ToString("N2");

            ChangeImage(pumpID, imageFuelling);
        }

        void StatusStartingToAuthorized(int pumpID, Label status)
        {

            status.Text = "Authorized";
            fcCommunication.PumpToAuthorized(pumpID);

            ChangeImage(pumpID, imageAuthorized);
        }

        private void StatusFuellingToPaused(int pumpID, Label status)
        {

            status.Text = "Paused";
            fcCommunication.PumpToPaused(pumpID);

            threadPauseSignals[pumpID - 1] = true;

            //Paused image? 
        }

        private void StatusPausedToFuelling(int pumpID, Label status)
        {

            status.Text = "Fuelling";
            fcCommunication.PumpToFuelling(pumpID);

            ChangeImage(pumpID, imageFuelling);

            int index = pumpID - 1;
        }

        public void StatusAuthorizedToIdle(int pumpID)
        {
            Label status = this.Controls.Find(baseStatusName + pumpID, true).FirstOrDefault() as Label;
            ThreadHelper.SetText(this, status, "Idle");

            //Image idle missing?
        }

        public void StatusEmergencyStopToAuthorized(int pumpID)
        {
            Label status = this.Controls.Find(baseStatusName + pumpID, true).FirstOrDefault() as Label;
            ThreadHelper.SetText(this, status, "Authorized");

            ChangeImage(pumpID, imageAuthorized);

        }

        private void FuellingFinished(int pumpID, Label status)
        {

            status.Text = "Authorized";
            fcCommunication.PumpToAuthorized(pumpID);

            threadStopSignals[pumpID - 1] = true;

            ChangeImage(pumpID, imageAuthorized);

            //Reset buttons 
            CheckBox nozzle = this.Controls.Find(baseButtonNozzleName + pumpID, true).FirstOrDefault() as CheckBox;
            CheckBox handle = this.Controls.Find(baseButtonHandleName + pumpID, true).FirstOrDefault() as CheckBox;
            CheckBox grade = lastChecked[pumpID - 1];

            nozzle.Checked = false;
            handle.Checked = false;
            grade.Checked = false;

            //Create Transaction
            CreateTransaction(pumpID);
        }

        private void ChangeImage(int pumpID, Bitmap image)
        {

            PictureBox imageToChange = this.Controls.Find(basePictureBoxIconName + pumpID, true).FirstOrDefault() as PictureBox;

            if (imageToChange == null)
                return;

            imageToChange.Image = image;
        }

        #endregion

        #region Fuelling Simulation

        public void CreateTransaction(int pumpID)
        {

            Label volume = this.Controls.Find(baseVolumeNumberName + pumpID, true).FirstOrDefault() as Label;
            Label amount = this.Controls.Find(baseAmountNumberName + pumpID, true).FirstOrDefault() as Label;
            Label price = this.Controls.Find(basePriceNumberName + pumpID, true).FirstOrDefault() as Label;
            CheckBox gradeChosen = lastChecked[pumpID - 1];
            int grade = int.Parse(gradeChosen.Text[gradeChosen.Text.Length - 1].ToString());

            double amountNum = double.Parse(amount.Text);
            double volumeNum = double.Parse(volume.Text);
            double priceNum = double.Parse(price.Text);

            Transaction t = new Transaction(transactionID, grade, amountNum, priceNum, volumeNum, pumpID);

            ListView currentBox = this.Controls.Find(baseListBoxTransactionsName + pumpID, true).FirstOrDefault() as ListView;

            var toAdd = new ListViewItem(new[] { t.ID.ToString(), t.Amount.ToString("N2"), t.Volume.ToString("N2"), t.Price.ToString("N2"), grades[t.GradeID - 1].name });
            currentBox.Items.Add(toAdd);

            transactionID++;
            lastChecked[pumpID - 1] = null;

            numberOfTransactions[pumpID - 1]++;
        }

        public void StatusLabelTextChanged(object sender, EventArgs e)
        {

            Label status = sender as Label;
            if (status == null)
                return;

            //Only change values if pump is fuelling
            if (status.Text != "Fuelling")
                return;

            //Fetch the labels to change
            int pumpID = int.Parse(status.Name[status.Name.Length - 1].ToString());
            int index = pumpID - 1;

            //Do not allow the third transaction to start
            if (numberOfTransactions[index] >= numberOfConcurrentTransactions)
            {
                status.Text = "Authorized";
                fcCommunication.PumpToAuthorized(pumpID);

                ChangeImage(pumpID, imageAuthorized);

                //Reset the buttons
                CheckBox nozzle = this.Controls.Find(baseButtonNozzleName + pumpID, true).FirstOrDefault() as CheckBox;
                CheckBox handle = this.Controls.Find(baseButtonHandleName + pumpID, true).FirstOrDefault() as CheckBox;
                CheckBox grade = lastChecked[pumpID - 1];

                nozzle.Checked = false;
                handle.Checked = false;
                grade.Checked = false;
                return;
            }

            //Reset signal arrays, so that the pump will run the simulation
            threadStopSignals[index] = false;
            threadPauseSignals[index] = false;

            //Start thread  
            pumpThreads[index] = new Thread(() => AutomaticFuellingThread(pumpID)) { IsBackground = true };
            pumpThreads[index].Start();
        }

        private void AutomaticFuellingThread(int pumpID)
        {
            int index = pumpID - 1;


            Random random = new Random();

            while (threadPauseSignals[index] == false && threadStopSignals[index] == false && emergencyStop == false) {
                lock (threadLocks[index]) {

                    //Set the text of the volume text field 
                    Label volume = this.Controls.Find(baseVolumeNumberName + pumpID, true).FirstOrDefault() as Label;
                    double newVolumeNum = double.Parse(volume.Text) + (random.NextDouble() * (3 - 1) + 1);
                    string newVolume = newVolumeNum.ToString("N2");
                    ThreadHelper.SetText(this, volume, newVolume);

                    //Set the text of the amount text field, calculated using the grade price
                    Label price = this.Controls.Find(basePriceNumberName + pumpID, true).FirstOrDefault() as Label;
                    double priceOfUnit = double.Parse(price.Text);

                    Label amount = this.Controls.Find(baseAmountNumberName + pumpID, true).FirstOrDefault() as Label;
                    double newAmountNum = double.Parse(newVolume) * priceOfUnit;
                    string newAmount = newAmountNum.ToString("N2");
                    ThreadHelper.SetText(this, amount, newAmount);

                    //Send this fuelling step to the Forecourt Manager
                    fcCommunication.FuellingStep(pumpID, newVolumeNum, newAmountNum);

                    Thread.Sleep(700);
                }
            }

            fcCommunication.StopFuelling(pumpID);

            //Clean up after thread
            Thread toRemove = pumpThreads[index];
            toRemove.Join();
        }


        #endregion

        #region Test Data
        private List<Transaction> CreateTestData()
        {

            List<Transaction> transactions = new List<Transaction>();

            Transaction trans = new Transaction()
            {
                ID = transactionID,
                GradeID = 95,
                Amount = 100,
                Price = 1,
                Volume = 100,
                PumpID = 1
            };
            transactions.Add(trans);
            transactionID++;

            trans = new Transaction()
            {
                ID = transactionID,
                GradeID = 95,
                Amount = 200,
                Price = 1,
                Volume = 200,
                PumpID = 1
            };

            transactions.Add(trans);
            transactionID++;

            trans = new Transaction()
            {
                ID = transactionID,
                GradeID = 95,
                Amount = 300,
                Price = 1,
                Volume = 300,
                PumpID = 2
            };
            transactions.Add(trans);
            transactionID++;

            return transactions;
        }
        #endregion

        public string GetFuelPrices()
        {
            string prices = "";

            //Add all the grade prices to the back of 'prices', with an additional comma to seperate them
            foreach (Grade g in grades)
            {
                prices += g.price.ToString() + ",";
            }

            //Remove the last comma
            prices.TrimEnd(prices[prices.Length - 1]);

            return prices;
        }

        public void SetEmergencyStop(bool value)
        {
            emergencyStop = value;

            for (int i = 1; i <= pumpFieldCount; i++)
            {
                //Change the text to stopped
                Label status = this.Controls.Find(baseStatusName + i.ToString(), true).FirstOrDefault() as Label;
                ThreadHelper.SetText(this, status, "Stopped");

                //Change the image to stopped
                PictureBox imageToChange = this.Controls.Find(basePictureBoxIconName + i, true).FirstOrDefault() as PictureBox;
                imageToChange.Image = imageStop;
            }
        }

        public void ClosePump(int pumpID)
        {
            Label status = this.Controls.Find(baseStatusName + pumpID.ToString(), true).FirstOrDefault() as Label;
            if (status.Text == "Authorized" || status.Text == "Idle")
            {
                ThreadHelper.SetText(this, status, "Closed");
                PictureBox imageToChange = this.Controls.Find(basePictureBoxIconName + pumpID, true).FirstOrDefault() as PictureBox;
                imageToChange.Image = imageClosed;
            }
        }

        public void OpenPump(int pumpID)
        {
            Label status = this.Controls.Find(baseStatusName + pumpID.ToString(), true).FirstOrDefault() as Label;
            if (status.Text == "Closed")
            {
                ThreadHelper.SetText(this, status, "Idle");
                PictureBox imageToChange = this.Controls.Find(basePictureBoxIconName + pumpID, true).FirstOrDefault() as PictureBox;
                imageToChange.Image = imageAuthorized;
            }
        }

        public void DisableAddCloseButtons()
        {
            Button add = this.Controls.Find("buttonAddPump", true).FirstOrDefault() as Button;
            Button remove = this.Controls.Find("buttonRemovePump", true).FirstOrDefault() as Button;

            ThreadHelper.SetButton(this, add, false);
            ThreadHelper.SetButton(this, remove, false);
        }
    }
}
