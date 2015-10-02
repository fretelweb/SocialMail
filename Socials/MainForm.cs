/**
 * Author  Ronny Fretel <ronny@fretelweb.com>
 * Version 1.0
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Windows.Forms;
using S22.Imap;
using CefSharp;
using System.Linq;
using CefSharp.WinForms;

namespace Socials
{
  
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    ChromiumWebBrowser bwWhatsapp;
    ChromiumWebBrowser bwFacebook;
    ChromiumWebBrowser bwTwitter;
    
    TrayIcon _t;
    
    
    public MainForm()
    {

      InitializeComponent();
      
      _t = new TrayIcon(this);
      
      var settings = new CefSettings();
      settings.CachePath = Environment.SpecialFolder.UserProfile.ToString();
      settings.PersistSessionCookies = true;
      Cef.Initialize(settings);
      
      
      bwFacebook = new ChromiumWebBrowser("m.facebook.com") { Dock = DockStyle.Fill };
      var tp = new TabPage("Facebook"){ Dock = DockStyle.Fill, };
      tabControl1.TabPages.Add(tp);
      tp.Controls.Add(bwFacebook);
      
      bwWhatsapp = new ChromiumWebBrowser("web.whatsapp.com") { Dock = DockStyle.Fill };
      var tp2 = new TabPage("Whatsapp"){ Dock = DockStyle.Fill, };
      tabControl1.TabPages.Add(tp2);
      tp2.Controls.Add(bwWhatsapp);
      
      bwTwitter = new ChromiumWebBrowser("mobile.twitter.com") { Dock = DockStyle.Fill };
      var tp3 = new TabPage("Twitter"){ Dock = DockStyle.Fill, };
      tabControl1.TabPages.Add(tp3);
      tp3.Controls.Add(bwTwitter);
      
      bwFacebook.LoadingStateChanged += bw_LoadingStateChanged;
      bwTwitter.LoadingStateChanged += bw_LoadingStateChanged;
      bwWhatsapp.LoadingStateChanged += bw_LoadingStateChanged;
      
      bwWhatsapp.ConsoleMessage += bw_ConsoleMessage;
      bwFacebook.ConsoleMessage += bw_ConsoleMessage;
      bwTwitter.ConsoleMessage += bw_ConsoleMessage;
      
      bwWhatsapp.TitleChanged += bw_TitleChanged;
      bwFacebook.TitleChanged += bw_TitleChanged;
      bwTwitter.TitleChanged += bw_TitleChanged;
    }

    void bw_TitleChanged(object sender, TitleChangedEventArgs e)
    {
      Debug.WriteLine("Enviado de : " + ((ChromiumWebBrowser)sender).Name);
      Debug.WriteLine("Titulo cambio a: " + e.Title);
    }
    
    void bw_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
    {
      Debug.WriteLine("Enviado de : " + ((ChromiumWebBrowser)sender).Name);
      Debug.WriteLine("Console mensaje: " + e.Message);
    }
    
    void bw_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
    {
      
    }
    
    void MainForm_Load(object sender, EventArgs e)
    {
      Visible = true;
      Application.DoEvents();
      
      CargarNoLeidos();
      
      if (dataGridView1.Rows.Count == 0) {
        CargarTodos();
      }

    }
    
    private void CargarNoLeidos()
    {
      var imap = new ImapClient("imap.gmail.com", 993, "", "", AuthMethod.Auto, true);
      IEnumerable<uint> uids = imap.Search(SearchCondition.Unseen());
      IEnumerable<MailMessage> mensajes = imap.GetMessages(uids);
      foreach (var m in mensajes) {
        int indice = dataGridView1.Rows.Add(m.From, m.Subject, m.Headers.Get("Date"));
        dataGridView1.Rows[indice].Tag = m;
      }
      
      if (imap.Supports("IDLE")) {
        imap.NewMessage += OnNewMessage;
      }
    }
    
    private void CargarTodos()
    {
      var imap = new ImapClient("imap.gmail.com", 993, "", "", AuthMethod.Auto, true);
      IEnumerable<uint> uids = imap.Search(SearchCondition.All(), "inbox");
      foreach (uint uid in uids) {
        Application.DoEvents();
        Text = uid + "" + uids.Count();
        if (!dataGridView1.IsDisposed) {
          MailMessage m = imap.GetMessage(uid);
          int indice = dataGridView1.Rows.Add(m.From, m.Subject,m.Headers.Get("Date"));
          dataGridView1.Rows[indice].Tag = m;
        } else {
          break;
        }
      }
      
    }
    
    private void OnNewMessage(object sender, IdleMessageEventArgs e)
    {
      
      MailMessage m = e.Client.GetMessage(e.MessageUID);
      int indice = dataGridView1.Rows.Add(m.From, m.Subject, m.Headers.Get("Date"));
      dataGridView1.Rows[indice].Tag = m;
      dataGridView1.Refresh();
    }

    
    
    private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
    {
      if (dataGridView1.Rows[e.RowIndex].Tag != null) {
        var mail = dataGridView1.Rows[e.RowIndex].Tag as MailMessage;
        if (mail.IsBodyHtml) {
          webBrowser1.DocumentText = mail.Body;
        } else {
          richTextBox1.Text = mail.Body;
        }
      }
    }
    

  }
}
