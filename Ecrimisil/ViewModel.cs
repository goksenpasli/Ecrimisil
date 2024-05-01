using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Ecrimisil
{
    public class ViewModel : INotifyPropertyChanged
    {
        public static readonly string xmldatapath = $@"{Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath)}\Data.xml";

        public ViewModel()
        {
            PropertyChanged += ViewModel_PropertyChanged;
            YdoListe = GetYdoListe();
            AddYdo = new RelayCommand<object>(
                parameter =>
                {
                    if (YdoListe?.Any(z => z.Yıl == SeçiliYıl) == true)
                    {
                        _ = MessageBox.Show("Bu Yıla Ait Veri Var Gerekirse Düzeltme Yapın", "ECRİMİSİL");
                        return;
                    }
                    YdoListe?.Add(new Ydo() { Oran = SeçiliOran, Yıl = SeçiliYıl });
                    Güncelle.Execute(null);
                },
                parameter => SeçiliOran > 0);

            Hesapla = new RelayCommand<object>(
                parameter =>
                {
                    TutarListe = EcrimisilHesapla(Başlangıç, Bitiş, BazFiyat, Yüzölçüm);
                    ToplamTutar = TutarListe?.Sum(z => z.EcrimisilTutarı) ?? 0;
                },
                parameter => Yüzölçüm > 0 && BazFiyat > 0 && Başlangıç < Bitiş && YdoYılDoğrula());

            Ayarla = new RelayCommand<object>(
                parameter =>
                {
                    BazFiyat = BazFiyat * AyarlanacakTutar / ToplamTutar;
                    if (Hesapla?.CanExecute(null) == true)
                    {
                        Hesapla.Execute(null);
                    }
                },
                parameter => AyarlanacakTutar > 0 && BazFiyat > 0 && ToplamTutar > 0);

            Güncelle = new RelayCommand<object>(parameter => YdoListe.Serialize(), parameter => true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand<object> AddYdo { get; }

        public RelayCommand<object> Ayarla { get; }

        public decimal AyarlanacakTutar { get; set; }

        public DateTime Başlangıç { get; set; } = DateTime.Today;

        public decimal BazFiyat { get; set; }

        public DateTime Bitiş { get; set; } = DateTime.Today;

        public int Fark { get; set; }

        public string FarkAçıklama { get; set; }

        public RelayCommand<object> Güncelle { get; }

        public RelayCommand<object> Hesapla { get; }

        public decimal SeçiliOran { get; set; }

        public int SeçiliYıl { get; set; } = DateTime.Now.Year - 1;

        public decimal ToplamTutar { get; set; }

        public ObservableCollection<EcrimisilVeri> TutarListe { get; set; }

        [XmlElement(ElementName = "Ydo")]
        public ObservableCollection<Ydo> YdoListe { get; set; } = [];

        public IEnumerable<int> Yıllar { get; set; } = Enumerable.Range(DateTime.Now.Year - 25, 25);

        public decimal Yüzölçüm { get; set; }

        private bool AllElementsExist<T>(List<T> list1, List<T> list2) => list2.All(list1.Contains);

        private ObservableCollection<EcrimisilVeri> EcrimisilHesapla(DateTime başlangıç, DateTime bitiş, decimal bazfiyat, decimal yüzölçüm)
        {
            int ilkyılgünfarkı = GünFarkı(başlangıç, new DateTime(başlangıç.Year, 12, 31));
            int sonyılgünfarkı = GünFarkı(new DateTime(bitiş.Year, 1, 1), bitiş);
            ObservableCollection<EcrimisilVeri> liste = [];
            decimal tutar;
            for (int şuankiyıl = bitiş.Year; şuankiyıl >= başlangıç.Year; şuankiyıl--)
            {
                if (başlangıç.Year == bitiş.Year)
                {
                    int daycount = GünFarkı(başlangıç, bitiş);
                    tutar = bazfiyat * daycount / 365;
                    liste.Add(
                        new EcrimisilVeri()
                        {
                            Aralık = $"{başlangıç.ToShortDateString()}\n{bitiş.ToShortDateString()}",
                            Gün = daycount,
                            BirimFiyat = tutar / yüzölçüm * 365 / daycount,
                            EcrimisilYüzölçümü = yüzölçüm,
                            Yıl = şuankiyıl,
                            EcrimisilTutarı = tutar
                        });
                    break;
                }
                if (şuankiyıl == bitiş.Year)
                {
                    tutar = bazfiyat * sonyılgünfarkı / 365;
                    liste.Add(
                        new EcrimisilVeri()
                        {
                            Aralık = $"{new DateTime(şuankiyıl, 1, 1).ToShortDateString()}\n{bitiş.ToShortDateString()}",
                            Gün = sonyılgünfarkı,
                            BirimFiyat = tutar / yüzölçüm * 365 / sonyılgünfarkı,
                            EcrimisilYüzölçümü = yüzölçüm,
                            Yıl = şuankiyıl,
                            EcrimisilTutarı = tutar
                        });
                    continue;
                }
                decimal oran = YdoListe.FirstOrDefault(z => z.Yıl == şuankiyıl)?.Oran ?? 0;
                decimal ydoartırım = 1 + (oran / 100);
                decimal sonecrimisiltutarı = liste.Last().EcrimisilTutarı;
                if (şuankiyıl == başlangıç.Year)
                {
                    tutar = sonecrimisiltutarı / ydoartırım * ilkyılgünfarkı / 365;
                    liste.Add(
                        new EcrimisilVeri()
                        {
                            Aralık = $"{başlangıç.ToShortDateString()}\n{new DateTime(şuankiyıl, 12, 31).ToShortDateString()}",
                            Gün = ilkyılgünfarkı,
                            BirimFiyat = tutar / yüzölçüm * 365 / ilkyılgünfarkı,
                            EcrimisilYüzölçümü = yüzölçüm,
                            Yıl = şuankiyıl,
                            EcrimisilTutarı = tutar
                        });
                }
                else
                {
                    tutar = şuankiyıl == bitiş.Year - 1 ? bazfiyat / ydoartırım : sonecrimisiltutarı / ydoartırım;
                    liste.Add(
                        new EcrimisilVeri()
                        {
                            Aralık = $"{new DateTime(şuankiyıl, 1, 1).ToShortDateString()}\n{new DateTime(şuankiyıl, 12, 31).ToShortDateString()}",
                            Gün = 365,
                            BirimFiyat = tutar / yüzölçüm,
                            EcrimisilYüzölçümü = yüzölçüm,
                            Yıl = şuankiyıl,
                            EcrimisilTutarı = tutar
                        });
                }
            }
            return liste;
        }

        private ObservableCollection<Ydo> GetYdoListe() => DesignerProperties.GetIsInDesignMode(new DependencyObject()) ? null : File.Exists(xmldatapath) ? xmldatapath.DeSerialize<ObservableCollection<Ydo>>() : [];

        private int GünFarkı(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                return 0;
            }
            TimeSpan difference = endDate - startDate;
            if (difference.Days / 365d > 5)
            {
                _ = MessageBox.Show("Dikkat 5 yılı aşıyor.", "ECRİMİSİL");
            }
            return difference.Days;
        }

        private string GünFarkıAçıklama(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                return "BİTİŞ BAŞLANGIÇTAN KÜÇÜK VEYA EŞİT OLMAZ";
            }
            TimeSpan difference = endDate - startDate;

            int years = (int)(difference.TotalDays / 365);
            int months = (int)(difference.TotalDays % 365 / 30);
            int days = (int)(difference.TotalDays % 365 % 30);

            return $"{years} YIL, {months} AY, {days} GÜN";
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "Başlangıç" or "Bitiş")
            {
                if (Başlangıç > Bitiş)
                {
                    Başlangıç = Bitiş;
                }
                FarkAçıklama = GünFarkıAçıklama(Başlangıç, Bitiş);
                Fark = GünFarkı(Başlangıç, Bitiş);
            }
        }

        private bool YdoYılDoğrula()
        {
            List<int> yıllar = Enumerable.Range(Başlangıç.Year, Bitiş.Year - Başlangıç.Year).ToList();
            bool allExist = AllElementsExist(YdoListe.Select(z => z.Yıl).ToList(), yıllar);
            if (!allExist)
            {
                FarkAçıklama = "Belirtilen yıl aralığına ait her yıla ait YDO girilmemiş";
            }
            return allExist;
        }
    }
}
