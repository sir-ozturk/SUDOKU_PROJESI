using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUDOKU_PROJESİ
{
    public class SudokuEngine
    {
        private int[,] tahta = new int[9, 9];
        private int[,] cozum = new int[9, 9];

        public int[,] Tahta => tahta; //tahta: Oyuncuya göstereceğin tahta (bazı yerleri 0 = boş)
        public int[,] Cozum => cozum; //cozum: Aynı bulmacanın tam doğru çözümü (hiç 0 yok)

        private static Random rnd = new Random();

        // -------------------------------------------------
        // Hücreye sayı koymak uygun mu? (Backtracking için)
        // -------------------------------------------------
        private bool UygunMu(int satir, int sutun, int sayi)
        {
            // SATIR
            for (int i = 0; i < 9; i++)
                if (tahta[satir, i] == sayi)
                    return false;

            // SÜTUN
            for (int i = 0; i < 9; i++)
                if (tahta[i, sutun] == sayi)
                    return false;

            // 3x3 BLOK Burada hücrenin bulunduğu 3x3 bloğun sol-üst köşesini buluyor. satır 7 ise 7/3=2, 2*3=6 → blok satırı 6’dan başlar sütun 4 ise 4/3=1, 1*3=3 → blok sütunu 3’ten başlar
            int basSatir = (satir / 3) * 3;
            int basSutun = (sutun / 3) * 3;

            for (int i = basSatir; i < basSatir + 3; i++)
                for (int j = basSutun; j < basSutun + 3; j++)
                    if (tahta[i, j] == sayi)
                        return false;

            return true;
        }

        // -------------------------------------------------
        // Oyuncu için kontrol (satır + sütun + blok)
        // -------------------------------------------------
        public bool GecerliMi(int[,] tablo, int satir, int sutun)
        {
            int sayi = tablo[satir, sutun];
            if (sayi == 0) return true;

            // SATIR
            for (int i = 0; i < 9; i++)
                if (i != sutun && tablo[satir, i] == sayi)
                    return false;

            // SÜTUN
            for (int i = 0; i < 9; i++)
                if (i != satir && tablo[i, sutun] == sayi)
                    return false;

            // BLOK
            int basSatir = (satir / 3) * 3;
            int basSutun = (sutun / 3) * 3;

            for (int i = basSatir; i < basSatir + 3; i++)
                for (int j = basSutun; j < basSutun + 3; j++)
                    if ((i != satir || j != sutun) && tablo[i, j] == sayi)
                        return false;

            return true;
        }

        // -------------------------------------------------
        // Tam çözüm üret
        // -------------------------------------------------
        public bool TahtayiDoldur()
        {
            // Tahtayı sıfırla
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    tahta[i, j] = 0;

            if (Solve())
            {
                Array.Copy(tahta, cozum, tahta.Length);
                return true;
            }

            return false;
        }

        // -------------------------------------------------
        // Backtracking çözüm algoritması
        // -------------------------------------------------
        private bool Solve()
        {
            for (int satir = 0; satir < 9; satir++)
            {
                for (int sutun = 0; sutun < 9; sutun++)
                {
                    if (tahta[satir, sutun] == 0)
                    {
                        int[] sayilar = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                        // Karıştır
                        for (int k = 0; k < 9; k++)
                        {
                            int r = rnd.Next(9);
                            int temp = sayilar[k];
                            sayilar[k] = sayilar[r];
                            sayilar[r] = temp;
                        }

                        foreach (int sayi in sayilar)
                        {
                            if (UygunMu(satir, sutun, sayi))
                            {
                                tahta[satir, sutun] = sayi;

                                if (Solve())
                                    return true;

                                tahta[satir, sutun] = 0;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        // -------------------------------------------------
        // Zorluk ayarla (hücre silme)
        // -------------------------------------------------
        public void SeviyeyiAyarla(string seviye)
        {
            int silinecek;

            switch (seviye)
            {
                case "Kolay":
                    silinecek = 30;
                    break;
                case "Orta":
                    silinecek = 45;
                    break;
                case "Zor":
                    silinecek = 60;
                    break;
                default:
                    silinecek = 30;
                    break;
            }

            while (silinecek > 0)
            {
                int satir = rnd.Next(0, 9);
                int sutun = rnd.Next(0, 9);

                if (tahta[satir, sutun] != 0)
                {
                    tahta[satir, sutun] = 0;
                    silinecek--;
                }
            }
        }

        // -------------------------------------------------
        // Oyuncu çözümü tamamen doğru mu?
        // -------------------------------------------------
        public bool KontrolEt(int[,] oyuncuTahta)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (oyuncuTahta[i, j] != cozum[i, j])
                        return false;

            return true;
        }
    }
}
