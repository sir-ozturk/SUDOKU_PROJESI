using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace SUDOKU_PROJESİ
{
    public partial class Form1 : Form
    {
        SudokuEngine motor = new SudokuEngine(); //oyuncuya gösterilecek soru (0’lar boş)
        TextBox[,] kutular = new TextBox[9, 9]; //81 tane textbox’ı satır-sütun mantığıyla saklamak için 2 boyutlu dizi.
        //aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa

        int gecenSure = 0;
        bool oyunBasladi = false;

        string connectionString = "Data Source=.;Initial Catalog=sudoku;Integrated Security=True"; //SQL Server’a bağlanmak için gerekli adres bilgisi.

        public Form1()
        {
            InitializeComponent();
        }

        private void Txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Backspace serbest
            if (char.IsControl(e.KeyChar))
                return;

            // 1-9 arası değilse engelle
            if (!char.IsDigit(e.KeyChar) || e.KeyChar == '0')
                e.Handled = true;
        }


        // ----------------------------
        // Oyuncu tahtasını oku
        // ----------------------------
        private int[,] OyuncuTahtasiniAl()
        {
            int[,] tablo = new int[9, 9]; //Oyuncunun yazdığı sudoku tahtası olacak.

            for (int i = 0; i < 9; i++) //satırları dolaş
                for (int j = 0; j < 9; j++) //sütunları dolaş
                    int.TryParse(kutular[i, j].Text, out tablo[i, j]); //TextBox’taki değeri al, sayıysa tabloya koy, değilse 0 yap

            return tablo;
        }

        // ----------------------------
        // SQL INSERT
        // ----------------------------
        private void VeritabaninaKaydet(string ad, string seviye, int sure)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sorgu = @"INSERT INTO siralama (oyuncuadi, seviye, bitirmesuresi, tarih) VALUES (@ad, @seviye, @sure, GETDATE())";

                SqlCommand cmd = new SqlCommand(sorgu, conn);
                cmd.Parameters.AddWithValue("@ad", ad);
                cmd.Parameters.AddWithValue("@seviye", seviye);
                cmd.Parameters.AddWithValue("@sure", sure);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ----------------------------
        // SQL SELECT + DataGridView
        // ----------------------------
        private void SkorlariGetir()
        {
            using (SqlConnection conn = new SqlConnection(connectionString)) //SQL Server’a bağlanmak için bağlantı nesnesi oluşturuyor
            {
                string sorgu = @"SELECT oyuncuadi, seviye, bitirmesuresi, tarih FROM siralama ORDER BY bitirmesuresi ASC";

                SqlDataAdapter da = new SqlDataAdapter(sorgu, conn); //Sonuçları DataTable içine doldurur
                DataTable dt = new DataTable(); //SQL’den gelen tabloyu RAM’de geçici bir tabloya yükle.
                da.Fill(dt);

                dt.Columns.Add("Süre");

                foreach (DataRow row in dt.Rows) //Her satırı dolaşıp bitirmesuresini alıp saate çevirip yeni kolana yazıyor
                {
                    int saniye = Convert.ToInt32(row["bitirmesuresi"]);
                    row["Süre"] = TimeSpan
                        .FromSeconds(saniye)
                        .ToString(@"hh\:mm\:ss");
                }

                dataGridView1.DataSource = dt; //DataTable’ı grid’e bağlar.
                dataGridView1.Columns["bitirmesuresi"].Visible = false; //Saniye cinsinden olan değeri gizliyor.
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls) //Formdaki kontrolleri geziyoruz
            {
                if (ctrl is TableLayoutPanel panel) //Sadece TableLayoutPanel olanları seçiyoruz
                {
                    foreach (Control c in panel.Controls) //O panelin içindeki kontrolleri geziyoruz
                    {
                        if (c is TextBox txt && txt.Name.StartsWith("txt_")) //Sadece txt_ ile başlayan TextBox’ları alıyoruz
                        {
                            string[] parca = txt.Name.Split('_'); //İsmi parçalayarak satır-sütun bilgisi çıkarıyoruz

                            int satir = int.Parse(parca[1]);
                            int sutun = int.Parse(parca[2]);

                            kutular[satir, sutun] = txt; // Dizide doğru yere koyuyoruz
                        }
                    }
                }
            }

            foreach (TextBox txt in kutular)
            {
                if (txt != null)
                {
                    txt.KeyPress += Txt_KeyPress;
                    txt.ShortcutsEnabled = false; // Ctrl + V engelleme
                    txt.Multiline = false;
                    txt.MaxLength = 1;
                    txt.Enabled = false; //textboxlar aktif değil
                }
            }

            SkorlariGetir();
        }

        // ----------------------------
        // BAŞLAT
        // ----------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            gecenSure = 0;
            label3.Text = "00:00:00";

            if (string.IsNullOrWhiteSpace(textBox82.Text))
            {
                MessageBox.Show("Lütfen oyuncu adı giriniz!");
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Lütfen zorluk seçiniz!");
                return;
            }

            motor.TahtayiDoldur();
            motor.SeviyeyiAyarla(comboBox1.SelectedItem.ToString());

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int sayi = motor.Tahta[i, j];

                    if (sayi != 0)
                    {
                        kutular[i, j].Text = sayi.ToString();
                        kutular[i, j].ReadOnly = true;
                        kutular[i, j].BackColor = Color.DeepSkyBlue;
                    }
                    else
                    {
                        kutular[i, j].Text = "";
                        kutular[i, j].ReadOnly = false;
                        kutular[i, j].Enabled = true;
                        kutular[i, j].BackColor = Color.LightGray;
                    }
                }
            }

            oyunBasladi = true;
            timer1.Start();
        }

        // ----------------------------
        // TIMER
        // ----------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            gecenSure++;
            label3.Text = TimeSpan.FromSeconds(gecenSure).ToString(@"hh\:mm\:ss");
        }

        // ----------------------------
        // KONTROL (canlı sudoku kontrol)
        // ----------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            if (!oyunBasladi) return; //"Başlat" demediyse kontrol yapılmaz.

            int[,] oyuncuTahta = OyuncuTahtasiniAl(); //TextBox'lardaki değerleri 9x9 int matrise çeviriyor
            bool hataVar = false;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (kutular[i, j].ReadOnly) //Oyunun başlangıçta verdiği sabit sayıları atla.
                        continue;

                    if (kutular[i, j].Text == "") //boşsa bir şey yapma
                    {
                        kutular[i, j].BackColor = Color.LightGray;
                        continue;
                    }

                    if (!motor.GecerliMi(oyuncuTahta, i, j)) //Hücre yanlışsa kırmızı yap.
                    {
                        kutular[i, j].BackColor = Color.Red;
                        hataVar = true;
                    }
                    else
                    {
                        kutular[i, j].BackColor = Color.DeepSkyBlue; //Hücre doğruysa mavi yap.
                    }
                }
            }
        }

        // ----------------------------
        // BİTİR
        // ----------------------------
        private void button3_Click(object sender, EventArgs e)
        {
            if (!oyunBasladi)
            {
                MessageBox.Show("Önce oyunu başlatın!");
                return;
            }

            int[,] oyuncuTahta = OyuncuTahtasiniAl(); //81 textbox’ı okur Sayıları int[,] matrise çevirir Boşları 0 yapar

            if (motor.KontrolEt(oyuncuTahta)) //Oyuncunun tablosu, motorun ürettiği gerçek çözümle birebir aynı mı?
            {
                timer1.Stop();
                oyunBasladi = false;

                MessageBox.Show("Tebrikler!" + label3.Text + "sürede bitirdiniz.");

                VeritabaninaKaydet(textBox82.Text, comboBox1.Text, gecenSure);

                SkorlariGetir(); //Skor tablosunu yenile
            }
            else
            {
                MessageBox.Show("Çözüm hatalı!");
            }
        }
    }
}
