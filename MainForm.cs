
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;
using System.Data.Sql;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;



namespace MultiFaceRec
{

    //======================================================================================================================
    //             <<<<< WEB PROGRAMMING PROJECT-2017 ON Education Platform >>> Fcae Recognation Based Attendance System
    //======================================================================================================================
    public partial class FrmPrincipal : Form
    {
        //All the variables, vector, and the haarcascade 
        //----------------------------------------------

        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
       // string name, names = null;
        public static string name ="";
        public static string names = "";
        string atten = "PRESENT";



        //starting the 
        //------------
        public FrmPrincipal()
        {
            InitializeComponent();

            face = new HaarCascade("haarcascade_frontalface_default.xml");
            //eye = new HaarCascade("haarcascade_eye.xml");
            try
            {
                //Loading all the previous trained face data
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("Nothing in binary database, please add at least a face(Simply train the prototype with the Add Face Button).", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }


        //start the cemera process
        //------------------------
        public void button1_Click(object sender, EventArgs e)
        {
            if (txtYear.Text == "" && txtTerm.Text == "" && txtSubject.Text == "")
            {
                MessageBox.Show("Please enter all of the data for to start the cemera", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYear.Focus();
                return;
            }
            
            //Straing the cemera and will capture frame by frame 
            grabber = new Capture();
            grabber.QueryFrame();

            Application.Idle += new EventHandler(FrameGrabber);
            button1.Enabled = false; //cemra start button
      
           
            
              
            
            
            
        }




        //start the train ing process
        //---------------------------
        public void button2_Click(object sender, System.EventArgs e)
        {
            try
            {

                ContTrain = ContTrain + 1;

                //take the 320x240 picture from the cemera and make it 20x20 for TrainedFace
                gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                //TestImageBox.Image = gray;
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(20, 20)); // new size of pic only face


                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>(); //converting the pic to gray and save it to TranedFace
                    break;
                }

                //Trained image is been save to "Trainedface" variable
                TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainingImages.Add(TrainedFace);
                //adding text box train name to label
                labels.Add(textBox1.Text);


                imageBox1.Image = TrainedFace;


                File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");


                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                }

                MessageBox.Show(textBox1.Text + "´s face detected and added :)", "Training OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }






        //Matching caotured image with the TrainedFace
        //--------------------------------------------
        public void FrameGrabber(object sender, EventArgs e)
        {
            label3.Text = "0";

            NamePersons.Add("");


            // capture a frame form  device both face and all things on the image
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);


            gray = currentFrame.Convert<Gray, Byte>();

            //(TestImageBox.Image = currentFrame);

            //Result of haarCascade will be on the "MCvAvgComp"-facedetected (is it face or not )
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
          face,
          1.2,
          10,
          Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
          new Size(20, 20));


            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2); //Frame detect colour is 'read'


                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);


                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);

                    name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable

                    //the colour of  the face label name 
                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                }

                NamePersons[t - 1] = name;
                NamePersons.Add("");



                label3.Text = facesDetected[0].Length.ToString();



            }
            t = 0;


            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn] + ", ";
            }

            imageBoxFrameGrabber.Image = currentFrame;
            label4.Text = names;
            label6.Text = names;
         
            
            /*auto();
            cc.con = new SqlConnection(cs.DBConn);
            cc.con.Open();
            string cb = "Update Student set StudentID=@d2,Name=@d3,Year=@d4,Term=@d5,Subject=@d6,PresentAbsent=@d7,Photo=@d8 where C_ID=@d1";
            cc.cmd = new SqlCommand(cb);
            cc.cmd.Connection = cc.con;
            cc.cmd.Parameters.AddWithValue("@d1", txtID.Text);
            cc.cmd.Parameters.AddWithValue("@d2", txtStudentID.Text);
            cc.cmd.Parameters.AddWithValue("@d3", name);
            cc.cmd.Parameters.AddWithValue("@d4", txtYear.Text);
            cc.cmd.Parameters.AddWithValue("@d5", txtTerm.Text);
            cc.cmd.Parameters.AddWithValue("@d6", txtSubject.Text);
            cc.cmd.Parameters.AddWithValue("@d7", atten);
            MemoryStream ms = new MemoryStream();
            Bitmap bmpImage = new Bitmap(Picture.Image);
            bmpImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] data = ms.GetBuffer();
            SqlParameter p = new SqlParameter("@d8", SqlDbType.Image);
            p.Value = data;
            cc.cmd.Parameters.Add(p);
            cc.cmd.ExecuteReader();
            cc.con.Close();
            st1 = lblUser.Text;
            st2 = "updated the Student'" + name + "' having Student id '" + txtStudentID.Text + "'";
            cf.LogFunc(st1, System.DateTime.Now, st2);
            btnUpdate.Enabled = false;
            MessageBox.Show("Successfully updated", "Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
            */

            names = "";

            NamePersons.Clear();

        }


        /*public string giveData
        {
            get
            {
                return names;
            }
            set
            {

            }
        }*/


        //==========================================================
        //data base saving process ids done here
        //==========================================================

        ConnectionString cs = new ConnectionString();
        CommonClasses cc = new CommonClasses();
        clsFunc cf = new clsFunc();
        string st1;
        string st2;

        public void Reset()
        {
           
            txtSubject.Text = "";
            txtTerm.Text = "";
            txtYear.Text = "";
            txtStudentID.Text = "";
            txtID.Text = "";
            
            btnSave.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            //Picture.Image = Properties.Resources.photo;
            auto();
        }
        private void delete_records()
        {


            int RowsAffected = 0;
            cc.con = new SqlConnection(cs.DBConn);
            cc.con.Open();
            string ct = "delete from Student where C_ID=@d1";
            cc.cmd = new SqlCommand(ct);
            cc.cmd.Connection = cc.con;
            cc.cmd.Parameters.AddWithValue("@d1", txtID.Text);
            RowsAffected = cc.cmd.ExecuteNonQuery();
            if (RowsAffected > 0)
            {
                st1 = lblUser.Text;
                st2 = "deleted the Student'" + label6 + "' having Student id '" + txtStudentID.Text + "'";
                cf.LogFunc(st1, System.DateTime.Now, st2);
                MessageBox.Show("Successfully deleted", "Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Reset();
            }
            else
            {
                MessageBox.Show("No Record found", "Sorry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Reset();
            }
            if (cc.con.State == ConnectionState.Open)
            {
                cc.con.Close();
            }


            /* catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }*/
        }


        //auto  generate id
        //------------------
        public void auto()
        {

            int Num = 0;
            cc.con = new SqlConnection(cs.DBConn);
            cc.con.Open();
            string sql = "SELECT MAX(C_ID+1) FROM Student";
            cc.cmd = new SqlCommand(sql);
            cc.cmd.Connection = cc.con;
            if (Convert.IsDBNull(cc.cmd.ExecuteScalar()))
            {
                Num = 1;
                txtID.Text = Convert.ToString(Num);
                txtStudentID.Text = "C-" + Convert.ToString(Num);
            }
            else
            {
                Num = (int)(cc.cmd.ExecuteScalar());
                txtID.Text = Convert.ToString(Num);
                txtStudentID.Text = "C-" + Convert.ToString(Num);
            }
            cc.cmd.Dispose();
            cc.con.Close();
            cc.con.Dispose();
            /*
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }*/
        }

        public void FrmPrincipal_Load(object sender, EventArgs e)
        {

        }


        //New Button
        //-----------

        private void btnNew_Click(object sender, EventArgs e)
        {
            Reset();
        }



        //Save Button
        //-----------
        private void btnSave_Click(object sender, EventArgs e)
        {
            //Not Needed 
        }

       
        
        
        //Update Button
        //-------------
        private void btnUpdate_Click(object sender, EventArgs e)
        {

           //Not neeed
        }
       
        
        
        
        //For Image saving
        //----------------
        private void Browse_Click(object sender, EventArgs e)
        {

            var _with1 = openFileDialog1;

            _with1.Filter = ("Image Files |*.png; *.bmp; *.jpg;*.jpeg; *.gif;");
            _with1.FilterIndex = 4;
            //Reset the file name
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Picture.Image = Image.FromFile(openFileDialog1.FileName);
            }

            /*
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

       
        //Delect Button
        //-------------
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to delete this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                delete_records();
            }
        }



        //Showing Alll of the  Attendance Data
        //-------------------------------------

        private void btnGetData_Click(object sender, EventArgs e)
        {
            //this.Hide();

            Student_Entry_Record frm = new Student_Entry_Record();
            frm.Reset();
            frm.lblOperation.Text = "Student Master";
            frm.lblUser.Text = lblUser.Text;
            frm.Show();
        }


        //Button Close
        //-----------
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();

        }



        //Present Button and Present Will taken by it
        //-------------------------------------------

        private void btnPrsent_Click(object sender, EventArgs e)
        {
           
            if (txtYear.Text == "")
            {
                MessageBox.Show("Please enter Year", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYear.Focus();
                return;
            }
            if (txtTerm.Text == "")
            {
                MessageBox.Show("Please enter Term", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTerm.Focus();
                return;
            }
            if (txtSubject.Text == "")
            {
                MessageBox.Show("Please enter Sub name.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSubject.Focus();
                return;
            }


            //Main place to save data to the database

            cc.con = new SqlConnection(cs.DBConn);
            cc.con.Open();
            string cb = "insert into Student(C_ID,StudentID,Name,Year,Term,Subject,PresentAbsent,Photo) VALUES (@d1,@d2,@d3,@d4,@d5,@d6,@d7,@d8)";
            cc.cmd = new SqlCommand(cb);
            cc.cmd.Connection = cc.con;
            cc.cmd.Parameters.AddWithValue("@d1", txtID.Text);
            cc.cmd.Parameters.AddWithValue("@d2", txtStudentID.Text);
            cc.cmd.Parameters.AddWithValue("@d3", name);
            cc.cmd.Parameters.AddWithValue("@d4", txtYear.Text);
            cc.cmd.Parameters.AddWithValue("@d5", txtTerm.Text);
            cc.cmd.Parameters.AddWithValue("@d6", txtSubject.Text);
            cc.cmd.Parameters.AddWithValue("@d7", atten);
            MemoryStream ms = new MemoryStream();
            Bitmap bmpImage = new Bitmap(Picture.Image);
            bmpImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] data = ms.GetBuffer();
            SqlParameter p = new SqlParameter("@d8", SqlDbType.Image);
            p.Value = data;
            cc.cmd.Parameters.Add(p);
            cc.cmd.ExecuteReader();
            cc.con.Close();
            st1 = lblUser.Text;
            st2 = "added the new Student'" + name + "' having Student id '" + txtStudentID.Text + "'";
            cf.LogFunc(st1, System.DateTime.Now, st2);
            btnSave.Enabled = false;
            MessageBox.Show("Successfully saved", "Record", MessageBoxButtons.OK, MessageBoxIcon.Information);

            /*  catch (Exception ex)
              {
                  MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }*/
        }



        //Timer Method
        //------------
        public void timer1_Tick(object sender, EventArgs e)
        {

            
                //this.Hide();
            

        }



        //Excel data backUp
        //----------------
        private void ExelBackUp_Click(object sender, EventArgs e)
        {
            SqlConnection cnn;

            string connectionString = null;

            string sql = null;

            string data = null;

            int i = 0;

            int j = 0;



            Excel.Application xlApp;

            Excel.Workbook xlWorkBook;

            Excel.Worksheet xlWorkSheet;

            object misValue = System.Reflection.Missing.Value;



            xlApp = new Excel.Application();

            xlWorkBook = xlApp.Workbooks.Add(misValue);

            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);



            // connectionString = "data source=servername;initial catalog=databasename;user id=username;password=password;";
            connectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename=E:\EDUCATION\THEIRD YEAR\3rd year 2nd term\LAB\CSE-Web Programming Project-3200\FaceRecProOV (1)\FaceRecProOV\FaceRecProOV\FaceRecProOV\Attendance.mdf;Integrated Security=True";

            cnn = new SqlConnection(connectionString);

            cnn.Open();

            sql = "SELECT * FROM Student ";

            SqlDataAdapter dscmd = new SqlDataAdapter(sql, cnn);

            DataSet ds = new DataSet();

            dscmd.Fill(ds);



            for (i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
            {

                for (j = 0; j <= ds.Tables[0].Columns.Count - 1; j++)
                {

                    data = ds.Tables[0].Rows[i].ItemArray[j].ToString();

                    xlWorkSheet.Cells[i + 1, j + 1] = data;

                }

            }



            xlWorkBook.SaveAs("KHULNA-UNIVERSITY-CLASS-ATTENDANCE.xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

            xlWorkBook.Close(true, misValue, misValue);

            xlApp.Quit();



            releaseObject(xlWorkSheet);

            releaseObject(xlWorkBook);

            releaseObject(xlApp);



            MessageBox.Show("Excel file created , you can find the file c:\\KHULNA-UNIVERSITY-CLASS-ATTENDANCE");

        }

         private void releaseObject(object obj)

        {

            try

            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);

                obj = null;

            }

            catch (Exception ex)

            {

                obj = null;

                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());

            }

            finally

            {

                GC.Collect();

            }

        }



    }

       






    }



    