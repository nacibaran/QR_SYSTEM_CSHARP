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

       // ... önceki kodlar ...

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


                    // QR kodundan elde edilen deðeri düzenleme iþlemi.
                    // Process.Start'i düzenlediðimiz yeniString ile çaðýralým.
                    Process.Start("explorer.exe", "/select, \"" + yeniString + "\"");

                    // QR kodundan elde edilen deðeri düzenleme iþlemi.

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
                MessageBox.Show("EN AZ BÝR ARAMA KRÝTERÝ GÝRÝN !", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string rootDirectory = "C:/Users/ASUS/Desktop/Projeler"; // Proje klasörünüzün gerçek yolu ile deðiþtirin.
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
                MessageBox.Show("Eþleþen dosya bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }
        private List<string> SearchFiles(string rootDirectory, string projectFolder, string drawingFolder, string innerFolder)
        {
            List<string> foundFiles = new List<string>();

            // Ýlk olarak projenin ana dizinine gidelim.
            string projectPath = Path.Combine(rootDirectory, projectFolder);
                
            // Senaryo 1: Tüm parametreler boþ, ana dizindeki tüm dosyalarý listele
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
                        // 2. parametredeki klasöre gir
                        string drawingPath = Path.Combine(projectPath, drawingFolder);
                        if (Directory.Exists(drawingPath))
                        {
                            // 2. parametredeki klasörlerin içine tek tek gir
                            foreach (var subfolder in Directory.GetDirectories(drawingPath))
                            {
                                string innerFolderPath = Path.Combine(subfolder, innerFolder);
                                if (Directory.Exists(innerFolderPath))
                                {
                                    // 3. parametredeki dosyanýn içine gir ve 4. parametreyi ara
                                    foundFiles.AddRange(Directory.GetFiles(innerFolderPath, "*", SearchOption.AllDirectories)
                                        .Where(filePath => filePath.Contains(innerFolder)));
                                }
                            }

                            if (foundFiles.Count == 0)
                            {
                                MessageBox.Show("4. parametre ile eþleþen dosya bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Çizim klasörü bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Proje klasörü bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    // Diðer senaryolar için hata mesajý verelim
                    MessageBox.Show("Geçersiz parametre kombinasyonu.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("Çizim klasörü bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Senaryo 6: 3. ve 4. parametreler dolu
            else if (string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
            {

                if (!string.IsNullOrEmpty(rootDirectory))
                {
                    // Root klasördeki klasörleri alýyoruz.
                    string[] subdirectories = Directory.GetDirectories(rootDirectory);

                    // Root klasörün altýndaki her klasör için iþlem yapalým.
                    foreach (string subdirectory in subdirectories)
                    {
                        // Drawing klasörünü içeren bir alt klasör buluyoruz.
                        string drawingPath = Path.Combine(subdirectory, drawingFolder,innerFolder);
                        if (Directory.Exists(drawingPath))
                        {
                            // Eðer drawing klasörü bulunduysa, içindeki dosyalarý listeye ekleyelim.
                            foundFiles.AddRange(Directory.GetFiles(drawingPath, "*", SearchOption.AllDirectories));
                        }
                        else
                        {
                            // Eðer drawing klasörü bulunamazsa, innerFolder aramaya gerek yok, sonraki klasöre geçelim.
                            continue;
                        }

                        // Drawing klasörünün altýnda innerFolder adýyla eþleþen bir klasör buluyoruz.
                        string innerFolderPath = Path.Combine(drawingPath, innerFolder);
                        if (Directory.Exists(innerFolderPath))
                        {
                            // Eðer innerFolder bulunduysa, içindeki dosyalarý listeye ekleyelim.
                            foundFiles.AddRange(Directory.GetFiles(innerFolderPath, "*", SearchOption.AllDirectories));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("rootDirectory deðeri boþ olamaz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }


            }

            // Senaryo 7: Sadece 4. parametre boþ
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
                    MessageBox.Show("Çizim klasörü ile iç klasör bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Senaryo 9: Tüm parametreler dolu
            else if (!string.IsNullOrEmpty(projectFolder) && !string.IsNullOrEmpty(drawingFolder) && !string.IsNullOrEmpty(innerFolder))
            {
                string innerFolderPath = Path.Combine(projectPath, drawingFolder, innerFolder);
                if (Directory.Exists(innerFolderPath))
                {
                    foundFiles.AddRange(Directory.GetFiles(innerFolderPath, "*", SearchOption.AllDirectories));
                }
                else
                {
                    MessageBox.Show("Proje, çizim ve iç klasör ile dosya bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            return foundFiles;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

       

        private void listBoxResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Listbox'da seçili bir öðe varsa, seçilen dosya yolundaki resmi PictureBox'a yükleyin.
            if (listBoxResults.SelectedIndex != -1)
            {
                string selectedFilePath = listBoxResults.SelectedItem.ToString();
                try
                {
                    // Dosya uzantýsýný küçük harfe çevirerek kontrol edelim.
                    string extension = Path.GetExtension(selectedFilePath).ToLower();

                    // Dosya uzantýsý png, jpg veya jpeg ise PictureBox'a yükleme yapalým.
                    if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
                    {

                        pdfViewer1.Visible = false;
                        pctr_search.Visible = true;
                        // Dosya yolunda belirtilen resmi PictureBox'a yükleme.
                        if (File.Exists(selectedFilePath))
                        {
                            pctr_search.Image = Image.FromFile(selectedFilePath);
                        }
                        else
                        {
                            MessageBox.Show("Dosya bulunamadý: " + selectedFilePath);
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
                    MessageBox.Show("Resim yüklenirken bir hata oluþtu: " + ex.Message);
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
