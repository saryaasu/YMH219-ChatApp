using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YMH219_ChatApp
{
    public partial class Form1 : Form
    {
        private ChatServer _server;
        private ChatClient _client;

        // Sunucu Kontrolleri
        private TextBox txtServerPort;
        private Button btnSunucuBaslat;
        private Button btnSunucuDurdur;
        private Label lblSunucuDurum;
        private ListBox lstBagliIstemciler;

        // İstemci Kontrolleri
        private TextBox txtIp;
        private TextBox txtClientPort;
        private TextBox txtKullaniciAdi;
        private Button btnBaglan;
        private Button btnBaglantiyiKes;
        private Label lblIstemciDurum;
        private ListBox lstMesajlar;
        private TextBox txtMesaj;
        private Button btnGonder;

        public Form1()
        {
            ArayuzuOlustur();
        }

        private void ArayuzuOlustur()
        {
            Text = "YMH 219 - İstemci-Sunucu Chat Uygulaması";
            Width = 860;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += Form1_FormClosing;

            // --- SOL PANEL: SUNUCU ---
            GroupBox grpSunucu = new GroupBox
            {
                Text = "Sunucu Yönetimi",
                Left = 15,
                Top = 15,
                Width = 310,
                Height = 450
            };

            Label lblSPort = new Label { Text = "Port:", Left = 15, Top = 30, Width = 45 };
            txtServerPort = new TextBox { Text = "5000", Left = 65, Top = 27, Width = 80 };
            btnSunucuBaslat = new Button { Text = "Sunucu Başlat", Left = 15, Top = 62, Width = 130, Height = 32 };
            btnSunucuDurdur = new Button { Text = "Durdur", Left = 155, Top = 62, Width = 135, Height = 32, Enabled = false };

            lblSunucuDurum = new Label
            {
                Text = "Durum: Kapalı",
                Left = 15,
                Top = 104,
                Width = 275,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            Label lblIstemcilerBaslik = new Label { Text = "Bağlı İstemciler:", Left = 15, Top = 134, Width = 200 };
            lstBagliIstemciler = new ListBox { Left = 15, Top = 158, Width = 275, Height = 270 };

            btnSunucuBaslat.Click += BtnSunucuBaslat_Click;
            btnSunucuDurdur.Click += BtnSunucuDurdur_Click;

            grpSunucu.Controls.AddRange(new Control[]
            {
                lblSPort, txtServerPort, btnSunucuBaslat, btnSunucuDurdur,
                lblSunucuDurum, lblIstemcilerBaslik, lstBagliIstemciler
            });

            // --- SAĞ PANEL: İSTEMCİ ---
            GroupBox grpIstemci = new GroupBox
            {
                Text = "İstemci Sohbet Ekranı",
                Left = 340,
                Top = 15,
                Width = 490,
                Height = 450
            };

            Label lblIp = new Label { Text = "IP:", Left = 15, Top = 30, Width = 25 };
            txtIp = new TextBox { Text = "127.0.0.1", Left = 42, Top = 27, Width = 100 };

            Label lblCPort = new Label { Text = "Port:", Left = 150, Top = 30, Width = 35 };
            txtClientPort = new TextBox { Text = "5000", Left = 188, Top = 27, Width = 55 };

            Label lblAd = new Label { Text = "Kullanıcı:", Left = 250, Top = 30, Width = 60 };
            txtKullaniciAdi = new TextBox { Text = "Sarya", Left = 312, Top = 27, Width = 160 };

            btnBaglan = new Button { Text = "Bağlan", Left = 15, Top = 62, Width = 130, Height = 32 };
            btnBaglantiyiKes = new Button { Text = "Bağlantıyı Kes", Left = 155, Top = 62, Width = 140, Height = 32, Enabled = false };

            lblIstemciDurum = new Label
            {
                Text = "Bağlantı: Bağlı Değil",
                Left = 305,
                Top = 70,
                Width = 175,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            lstMesajlar = new ListBox { Left = 15, Top = 105, Width = 457, Height = 285 };

            txtMesaj = new TextBox { Left = 15, Top = 402, Width = 355, Enabled = false };
            btnGonder = new Button { Text = "Mesaj Gönder", Left = 378, Top = 400, Width = 94, Height = 27, Enabled = false };

            btnBaglan.Click += BtnBaglan_Click;
            btnBaglantiyiKes.Click += BtnBaglantiyiKes_Click;
            btnGonder.Click += BtnGonder_Click;
            txtMesaj.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnGonder_Click(s, e);
                }
            };

            grpIstemci.Controls.AddRange(new Control[]
            {
                lblIp, txtIp, lblCPort, txtClientPort, lblAd, txtKullaniciAdi,
                btnBaglan, btnBaglantiyiKes, lblIstemciDurum, lstMesajlar, txtMesaj, btnGonder
            });

            Controls.AddRange(new Control[] { grpSunucu, grpIstemci });
        }

        private void BtnSunucuBaslat_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtServerPort.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Geçerli bir port numarası girin (1-65535).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _server = new ChatServer(port);
                _server.IstemciListesiGuncellendi += list =>
                {
                    if (IsDisposed) return;
                    BeginInvoke(new Action(() =>
                    {
                        lstBagliIstemciler.Items.Clear();
                        foreach (string item in list)
                            lstBagliIstemciler.Items.Add(item);
                    }));
                };

                _server.Baslat();
                btnSunucuBaslat.Enabled = false;
                btnSunucuDurdur.Enabled = true;
                txtServerPort.Enabled = false;
                lblSunucuDurum.Text = $"Durum: Aktif (Port {port})";
                lblSunucuDurum.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sunucu başlatılamadı:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSunucuDurdur_Click(object sender, EventArgs e)
        {
            _server?.Durdur();
            _server = null;
            lstBagliIstemciler.Items.Clear();
            btnSunucuBaslat.Enabled = true;
            btnSunucuDurdur.Enabled = false;
            txtServerPort.Enabled = true;
            lblSunucuDurum.Text = "Durum: Kapalı";
            lblSunucuDurum.ForeColor = Color.DarkRed;
        }

        private async void BtnBaglan_Click(object sender, EventArgs e)
        {
            string kullanici = txtKullaniciAdi.Text.Trim();
            if (string.IsNullOrWhiteSpace(kullanici))
            {
                MessageBox.Show("Lütfen bir kullanıcı adı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtClientPort.Text, out int port))
            {
                MessageBox.Show("Geçerli bir port numarası girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _client = new ChatClient();
                _client.MesajAlindi += mesaj =>
                {
                    if (IsDisposed) return;
                    BeginInvoke(new Action(() =>
                    {
                        lstMesajlar.Items.Add(mesaj);
                        lstMesajlar.TopIndex = Math.Max(0, lstMesajlar.Items.Count - 1);
                    }));
                };
                _client.BaglantiKoptu += () =>
                {
                    if (IsDisposed) return;
                    BeginInvoke(new Action(IstemciArayuzSifirla));
                };

                await _client.BaglanAsync(txtIp.Text.Trim(), port, kullanici);

                btnBaglan.Enabled = false;
                btnBaglantiyiKes.Enabled = true;
                txtIp.Enabled = false;
                txtClientPort.Enabled = false;
                txtKullaniciAdi.Enabled = false;
                txtMesaj.Enabled = true;
                btnGonder.Enabled = true;
                lblIstemciDurum.Text = "Bağlantı: Bağlı";
                lblIstemciDurum.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sunucuya bağlanılamadı:\n{ex.Message}", "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                IstemciArayuzSifirla();
            }
        }

        private void BtnBaglantiyiKes_Click(object sender, EventArgs e)
        {
            _client?.BaglantiyiKes();
            IstemciArayuzSifirla();
        }

        private async void BtnGonder_Click(object sender, EventArgs e)
        {
            string metin = txtMesaj.Text.Trim();
            if (string.IsNullOrEmpty(metin) || _client == null) return;

            try
            {
                await _client.MesajGonderAsync(metin);
                txtMesaj.Clear();
                txtMesaj.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesaj gönderilemedi:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void IstemciArayuzSifirla()
        {
            btnBaglan.Enabled = true;
            btnBaglantiyiKes.Enabled = false;
            txtIp.Enabled = true;
            txtClientPort.Enabled = true;
            txtKullaniciAdi.Enabled = true;
            txtMesaj.Enabled = false;
            btnGonder.Enabled = false;
            lblIstemciDurum.Text = "Bağlantı: Bağlı Değil";
            lblIstemciDurum.ForeColor = Color.DarkRed;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _client?.BaglantiyiKes();
            _server?.Durdur();
        }
    }

    public class BagliIstemciBilgisi
    {
        public string KullaniciAdi { get; set; }
        public string EndPoint { get; set; }
        public DateTime BaglantiZamani { get; set; }
        public TcpClient Client { get; set; }
        public StreamWriter Writer { get; set; }

        public override string ToString()
        {
            return $"{KullaniciAdi} ({EndPoint}) - {BaglantiZamani:HH:mm:ss}";
        }
    }

    public class ChatServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private readonly List<BagliIstemciBilgisi> _istemciler = new List<BagliIstemciBilgisi>();
        private readonly object _kilit = new object();

        public event Action<List<string>> IstemciListesiGuncellendi;

        public ChatServer(int port)
        {
            _port = port;
        }

        public void Baslat()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _ = IstemciKabulEtAsync(_cts.Token);
        }

        private async Task IstemciKabulEtAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                    _ = IstemciDinleAsync(tcpClient, token);
                }
            }
            catch { }
        }

        private async Task IstemciDinleAsync(TcpClient tcpClient, CancellationToken token)
        {
            BagliIstemciBilgisi bilgi = null;
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                string kullaniciAdi = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(kullaniciAdi))
                {
                    tcpClient.Close();
                    return;
                }

                bilgi = new BagliIstemciBilgisi
                {
                    KullaniciAdi = kullaniciAdi,
                    EndPoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "Bilinmiyor",
                    BaglantiZamani = DateTime.Now,
                    Client = tcpClient,
                    Writer = writer
                };

                lock (_kilit) { _istemciler.Add(bilgi); }
                ListeyiBildir();
                await HerkeseGonderAsync($"[{DateTime.Now:HH:mm:ss}] * {kullaniciAdi} sohbete katıldı.");

                while (!token.IsCancellationRequested)
                {
                    string satir = await reader.ReadLineAsync();
                    if (satir == null) break;

                    string formatli = $"[{DateTime.Now:HH:mm:ss}] {kullaniciAdi}: {satir}";
                    await HerkeseGonderAsync(formatli);
                }
            }
            catch { }
            finally
            {
                if (bilgi != null)
                {
                    lock (_kilit) { _istemciler.Remove(bilgi); }
                    ListeyiBildir();
                    await HerkeseGonderAsync($"[{DateTime.Now:HH:mm:ss}] * {bilgi.KullaniciAdi} sohbetten ayrıldı.");
                }
                try { tcpClient.Close(); } catch { }
            }
        }

        private async Task HerkeseGonderAsync(string mesaj)
        {
            List<BagliIstemciBilgisi> kopya;
            lock (_kilit) { kopya = new List<BagliIstemciBilgisi>(_istemciler); }

            foreach (var istemci in kopya)
            {
                try { await istemci.Writer.WriteLineAsync(mesaj); } catch { }
            }
        }

        private void ListeyiBildir()
        {
            List<string> liste = new List<string>();
            lock (_kilit)
            {
                foreach (var item in _istemciler)
                    liste.Add(item.ToString());
            }
            IstemciListesiGuncellendi?.Invoke(liste);
        }

        public void Durdur()
        {
            try { _cts?.Cancel(); } catch { }
            try { _listener?.Stop(); } catch { }
            lock (_kilit)
            {
                foreach (var c in _istemciler)
                {
                    try { c.Client.Close(); } catch { }
                }
                _istemciler.Clear();
            }
        }
    }

    public class ChatClient
    {
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cts;

        public event Action<string> MesajAlindi;
        public event Action BaglantiKoptu;

        public async Task BaglanAsync(string ip, int port, string kullaniciAdi)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(ip, port);

            NetworkStream stream = _client.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            await _writer.WriteLineAsync(kullaniciAdi);

            _cts = new CancellationTokenSource();
            _ = MesajlariDinleAsync(_cts.Token);
        }

        public async Task MesajGonderAsync(string mesaj)
        {
            if (_writer != null)
                await _writer.WriteLineAsync(mesaj);
        }

        private async Task MesajlariDinleAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    string satir = await _reader.ReadLineAsync();
                    if (satir == null) break;
                    MesajAlindi?.Invoke(satir);
                }
            }
            catch { }
            finally
            {
                BaglantiKoptu?.Invoke();
            }
        }

        public void BaglantiyiKes()
        {
            try { _cts?.Cancel(); } catch { }
            try { _client?.Close(); } catch { }
        }
    }
}