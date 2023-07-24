using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.VisualBasic.Devices;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ZXing;
using PdfiumViewer;
using System.Drawing.Imaging;
using System.IO;

namespace QR_ELK
{
    public partial class Form1 : Form
    {
        public Form1()
        {

           
            InitializeComponent();
        }

        

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice captureDevice;

        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            foreach (FilterInfo filterInfo in filterInfoCollection)
                cboDevice.Items.Add(filterInfo.Name);
            cboDevice.SelectedIndex = 0;
        }

        private void btn_tara_Click(object sender, EventArgs e)
        {
            captureDevice = new VideoCaptureDevice(filterInfoCollection[cboDevice.SelectedIndex].MonikerString);
            captureDevice.NewFrame += CaptureDevice_NewFrame;
            captureDevice.Start();
            timer1.Start();
        }

        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (captureDevice != null && captureDevice.IsRunning)
            {
                timer1.Stop();
                captureDevice.SignalToStop();       
            }
        }

       // ... �nceki kodlar ...

private void timer1_Tick(object sender, EventArgs e)
{
    if (pictureBox.Image != null)
    {
        BarcodeReader barcodeReader = new BarcodeReader();
        Result result = barcodeReader.Decode((Bitmap)pictureBox.Image);

        if (result != null)
        {
                    String s = result.Text.ToString();

                    string yeniString = s.Replace("/", "\\");
                    yeniString += "\\";


                    // QR kodundan elde edilen de�eri d�zenleme i�lemi.
                    // Process.Start'i d�zenledi�imiz yeniString ile �a��ral�m.
                    Process.Start("explorer.exe", "/select, \"" + yeniString + "\"");

                    // QR kodundan elde edilen de�eri d�zenleme i�lemi.

                    timer1.Stop();
                    txtQRCode.Text = yeniString;
                   


        }




            }
}

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            string projectCode = txtProjectCode.Text.Trim();    
            string drawingCode = txtDrawingCode.Text.Trim();
            string countrycode = txtCountryCode.Text.Trim();

            if (string.IsNullOrEmpty(projectCode) && string.IsNullOrEmpty(drawingCode) && string.IsNullOrEmpty(countrycode))
            {
                MessageBox.Show("EN AZ B�R ARAMA KR�TER� G�R�N !", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rootDirectory = "C:/Users/ASUS/Desktop/Projeler"; // Proje klas�r�n�z�n ger�ek yolu ile de�i�tirin.
            List<string> foundFiles = SearchFiles(rootDirectory, projectCode, drawingCode, countrycode);

            if (foundFiles.Count > 0)
            {
                listBoxResults.Items.Clear();
                foreach (string filePath in foundFiles)
                {
                    listBoxResults.Items.Add(filePath);
                }
            }
            else
            {
                listBoxResults.Items.Clear();
                MessageBox.Show("E�le�en dosya bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }
        private List<string> SearchFiles(string rootDirectory, string projectFolder, string drawingFolder, string innerFolder)
        {
            List<string> foundFiles = new List<string>();

            // �lk olarak projenin ana dizinine gidelim.
            string projectPath = Path.Combine(rootDirectory, projectFolder);
                
            // Senaryo 1: T�m parametreler bo�, ana dizindeki t�m dosyalar� listele
            if (string.IsNullOrEmpty(projectFolder) && string.IsNullOrEmpty(drawingFolder) && string.IsNullOrEmpty(innerFolder))
            {
                foundFiles.AddRange(Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories));
            }

            // Senaryo 2: Sadece 4. parametre dolu
            else if (string.IsNullOrEmpty(projectFolder) && string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
            {
                foundFiles.AddRange(Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories)
                    .Where(filePath => filePath.Contains(innerFolder)));
            }

            // Senaryo 3: Sadece 2. parametre dolu
            else if (!string.IsNullOrEmpty(projectFolder) && string.IsNullOrEmpty(drawingFolder) && string.IsNullOrEmpty(innerFolder))
            {
                foundFiles.AddRange(Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories));
            }

        // Senaryo 4: 2. ve 4. parametreler dolu
            else if (!string.IsNullOrEmpty(projectFolder) && string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
            {


                if (!string.IsNullOrEmpty(projectFolder) && string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
                {
                    if (Directory.Exists(projectPath))
                    {
                        // 2. parametredeki klas�re gir
                        string drawingPath = Path.Combine(projectPath, drawingFolder);
                        if (Directory.Exists(drawingPath))
                        {
                            // 2. parametredeki klas�rlerin i�ine tek tek gir
                            foreach (var subfolder in Directory.GetDirectories(drawingPath))
                            {
                                string innerFolderPath = Path.Combine(subfolder, innerFolder);
                                if (Directory.Exists(innerFolderPath))
                                {
                                    // 3. parametredeki dosyan�n i�ine gir ve 4. parametreyi ara
                                    foundFiles.AddRange(Directory.GetFiles(innerFolderPath, "*", SearchOption.AllDirectories)
                                        .Where(filePath => filePath.Contains(innerFolder)));
                                }
                            }

                            if (foundFiles.Count == 0)
                            {
                                MessageBox.Show("4. parametre ile e�le�en dosya bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("�izim klas�r� bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Proje klas�r� bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // Di�er senaryolar i�in hata mesaj� verelim
                    MessageBox.Show("Ge�ersiz parametre kombinasyonu.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }



            }

            // Senaryo 5: Sadece 3. parametre dolu
            else if (!string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && string.IsNullOrEmpty(innerFolder))
            {
                string drawingPath = Path.Combine(projectPath, drawingFolder);
                if (Directory.Exists(drawingPath))
                {
                    foundFiles.AddRange(Directory.GetFiles(drawingPath, "*", SearchOption.AllDirectories));
                }
                else
                {
                    MessageBox.Show("�izim klas�r� bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Senaryo 6: 3. ve 4. parametreler dolu
            else if (string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
            {

                if (!string.IsNullOrEmpty(rootDirectory))
                {
                    // Root klas�rdeki klas�rleri al�yoruz.
                    string[] subdirectories = Directory.GetDirectories(rootDirectory);

                    // Root klas�r�n alt�ndaki her klas�r i�in i�lem yapal�m.
                    foreach (string subdirectory in subdirectories)
                    {
                        // Drawing klas�r�n� i�eren bir alt klas�r buluyoruz.
                        string drawingPath = Path.Combine(subdirectory, drawingFolder,innerFolder);
                        if (Directory.Exists(drawingPath))
                        {
                            // E�er drawing klas�r� bulunduysa, i�indeki dosyalar� listeye ekleyelim.
                            foundFiles.AddRange(Directory.GetFiles(drawingPath, "*", SearchOption.AllDirectories));
                        }
                        else
                        {
                            // E�er drawing klas�r� bulunamazsa, innerFolder aramaya gerek yok, sonraki klas�re ge�elim.
                            continue;
                        }

                        // Drawing klas�r�n�n alt�nda innerFolder ad�yla e�le�en bir klas�r buluyoruz.
                        string innerFolderPath = Path.Combine(drawingPath, innerFolder);
                        if (Directory.Exists(innerFolderPath))
                        {
                            // E�er innerFolder bulunduysa, i�indeki dosyalar� listeye ekleyelim.
                            foundFiles.AddRange(Directory.GetFiles(innerFolderPath, "*", SearchOption.AllDirectories));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("rootDirectory de�eri bo� olamaz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }


            }

            // Senaryo 7: Sadece 4. parametre bo�
            else if (string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && string.IsNullOrEmpty(innerFolder))
            {
                foundFiles.AddRange(Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories)
                    .Where(filePath => filePath.Contains(drawingFolder)));
            }

            // Senaryo 8: 2. ve 3. parametreler dolu
            else if (!string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && string.IsNullOrEmpty(innerFolder))
            {
                string drawingPath = Path.Combine(projectPath, drawingFolder);
                if (Directory.Exists(drawingPath))
                {
                    foundFiles.AddRange(Directory.GetFiles(drawingPath, "*", SearchOption.AllDirectories)
                        .Where(filePath => filePath.Contains(innerFolder)));
                }
                else
                {
                    MessageBox.Show("�izim klas�r� ile i� klas�r bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Senaryo 9: T�m parametreler dolu
            else if (!string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
            {
                string innerFolderPath = Path.Combine(projectPath, drawingFolder, innerFolder);
                if (Directory.Exists(innerFolderPath))
                {
                    foundFiles.AddRange(Directory.GetFiles(innerFolderPath, "*", SearchOption.AllDirectories));
                }
                else
                {
                    MessageBox.Show("Proje, �izim ve i� klas�r ile dosya bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            return foundFiles;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

       

        private void listBoxResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Listbox'da se�ili bir ��e varsa, se�ilen dosya yolundaki resmi PictureBox'a y�kleyin.
            if (listBoxResults.SelectedIndex != -1)
            {
                string selectedFilePath = listBoxResults.SelectedItem.ToString();
                try
                {
                    // Dosya uzant�s�n� k���k harfe �evirerek kontrol edelim.
                    string extension = Path.GetExtension(selectedFilePath).ToLower();

                    // Dosya uzant�s� png, jpg veya jpeg ise PictureBox'a y�kleme yapal�m.
                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                    {

                        pdfViewer1.Visible = false;
                        pctr_search.Visible = true;
                        // Dosya yolunda belirtilen resmi PictureBox'a y�kleme.
                        if (File.Exists(selectedFilePath))
                        {
                            pctr_search.Image = Image.FromFile(selectedFilePath);
                        }
                        else
                        {
                            MessageBox.Show("Dosya bulunamad�: " + selectedFilePath);
                        }
                    }
                    else if (extension == ".pdf")

                        
                    {
                        pctr_search.Visible = false;
                        pdfViewer1.Visible= true;
                        


                            byte[] bytes = System.IO.File.ReadAllBytes(selectedFilePath);
                            var Stream = new MemoryStream(bytes);
                            PdfDocument pdfDocument = PdfDocument.Load(Stream);
                            pdfViewer1.Document=pdfDocument;


                        
                     
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim y�klenirken bir hata olu�tu: " + ex.Message);
                }
            }

        }


        

        private void txtQRCode_TextChanged(object sender, EventArgs e)
        {

        }

        private void pctr_search_Click(object sender, EventArgs e)
        {

        }





        // ... sonraki kodlar ...

    }
}
