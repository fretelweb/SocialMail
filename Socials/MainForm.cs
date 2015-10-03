/**
 * Author  Ronny Fretel <ronny@fretelweb.com>
 * Version 1.0
 */
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Windows.Forms;
using System.Configuration;

using EO.WebBrowser;
using EO.WebBrowser.WinForm;
using S22.Imap;
using System.Linq;

namespace Socials
{
  
  struct Configuracion
  {
    public string correo;
    public string pwd;
    public string imap;
    public int puerto;
  }
  
  
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    WebView bwWhatsapp;
    WebView bwFacebook;
    WebView bwTwitter;
    
    WebControl wc;
        
    TrayIcon _t;
    
    Configuracion config;
    
    public MainForm()
    {

      InitializeComponent();
      
      BrowserOptions bo = new BrowserOptions();
      bo.UserStyleSheet = @"#side.pane-list{width: 90px !important;}";
      Runtime.SetDefaultOptions(bo);
      
      //Cargar Configuracion 
      config = new Configuracion() {
        correo = ConfigurationSettings.AppSettings["correo"],
        pwd = ConfigurationSettings.AppSettings["pwd"],
        imap = ConfigurationSettings.AppSettings["imap"],
        puerto = Convert.ToInt32(ConfigurationSettings.AppSettings["puerto"]),
      };
      
      _t = new TrayIcon(this);
      
      var wc = new WebControl() { Dock = DockStyle.Fill, };
      wc.WebView = new WebView(){ Url = "m.facebook.com" };
      var tp = new TabPage("Facebook");
      tp.Controls.Add(wc);
      tabControl1.TabPages.Add(tp);
      
      var wc2 = new WebControl() { Dock = DockStyle.Fill, };
      wc2.WebView = new WebView(){ Url = "mobile.twitter.com" };
      var tp2 = new TabPage("Twitter");
      tp2.Controls.Add(wc2);
      tabControl1.TabPages.Add(tp2);
      
      var wc3 = new WebControl() { Dock = DockStyle.Fill, };
      wc3.WebView = new WebView(){ Url = "web.whatsapp.com" };
      var tp3 = new TabPage("Whatsapp");
      tp3.Controls.Add(wc3);
      tabControl1.TabPages.Add(tp3);
      
      
    }

    
    void MainForm_Load(object sender, EventArgs e)
    {
      Visible = true;
      Application.DoEvents();
      
      CargarNoLeidos();

      CargarTodos();


    }
    
    private void CargarNoLeidos()
    {
      var imap = new ImapClient(
                   config.imap, config.puerto, config.correo, config.pwd, AuthMethod.Auto, true);
      IEnumerable<uint> uids = imap.Search(SearchCondition.Unseen());
      IEnumerable<MailMessage> mensajes = imap.GetMessages(uids);
      foreach (var m in mensajes) {
        DateTime d = Convert.ToDateTime(m.Headers.Get("Date"));
        int indice = dataGridView1.Rows.Add(m.From, m.Subject,d );
        dataGridView1.Rows[indice].Tag = m;
      }
      
      if (imap.Supports("IDLE")) {
        imap.NewMessage += OnNewMessage;
      }
    }
    
    private void CargarTodos()
    {
      var imap = new ImapClient(
                   config.imap, config.puerto, config.correo, config.pwd, AuthMethod.Auto, true);
      IEnumerable<uint> uids = imap.Search(
                                 SearchCondition.SentSince(DateTime.Now.AddDays(-7)), "inbox"
                               );
      foreach (uint uid in uids) {
        Application.DoEvents();
        Text = uid + "" + uids.Count();
        if (!dataGridView1.IsDisposed) {
          MailMessage m = imap.GetMessage(uid);
          DateTime d = Convert.ToDateTime(m.Headers.Get("Date"));
          int indice = dataGridView1.Rows.Add(m.From, m.Subject, d);
          dataGridView1.Rows[indice].Tag = m;
        } else {
          break;
        }
      }
      
    }
    
    private void OnNewMessage(object sender, IdleMessageEventArgs e)
    {
      
      MailMessage m = e.Client.GetMessage(e.MessageUID);
      DateTime d = Convert.ToDateTime(m.Headers.Get("Date"));
      int indice = dataGridView1.Rows.Add(m.From, m.Subject, d);
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
