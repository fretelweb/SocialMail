/**
 * Author  Ronny Fretel <ronny@fretelweb.com>
 * Version 1.0
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
           
    TrayIcon _t;
    
    Configuracion config;
    
    public MainForm()
    {

      InitializeComponent();
      
      var bo = new BrowserOptions();
      bo.UserStyleSheet = @"#side.pane-list{width: 80px;}";
      Runtime.SetDefaultOptions(bo);
      
      //Cargar Configuracion 
      config = new Configuracion() {
        correo = ConfigurationSettings.AppSettings["correo"],
        pwd = ConfigurationSettings.AppSettings["pwd"],
        imap = ConfigurationSettings.AppSettings["imap"],
        puerto = Convert.ToInt32(ConfigurationSettings.AppSettings["puerto"]),
      };
      
      _t = new TrayIcon(this);
      
      var wc = new WebControl { Dock = DockStyle.Fill, };
      wc.WebView = new WebView{ Url = "m.facebook.com",  };
      var tp = new TabPage("Facebook");
      tp.Controls.Add(wc);
      
      tabControl1.TabPages.Add(tp);
      var wc2 = new WebControl { Dock = DockStyle.Fill, };
      wc2.WebView = new WebView{ Url = "mobile.twitter.com" };
      var tp2 = new TabPage("Twitter");
      tp2.Controls.Add(wc2);
      tabControl1.TabPages.Add(tp2);
      
      var wc3 = new WebControl { Dock = DockStyle.Fill, };
      wc3.WebView = new WebView{ Url = "web.whatsapp.com" };
      var tp3 = new TabPage("Whatsapp");
      tp3.Controls.Add(wc3);
      tabControl1.TabPages.Add(tp3);
      
      wc.WebView.NewWindow += onNewWindow;
      wc2.WebView.NewWindow += onNewWindow;
      wc3.WebView.NewWindow += onNewWindow;
      
      wc.WebView.StatusMessageChanged += onStatusMessageChanged;
      wc2.WebView.StatusMessageChanged += onStatusMessageChanged;
      wc3.WebView.StatusMessageChanged += onStatusMessageChanged;
      
      wc.WebView.TitleChanged += onTitleChanged;
      
    }

    void onTitleChanged(object sender, EventArgs e)
    {
      Text = ((WebView)sender).Title;
    }
    
    void onStatusMessageChanged(object sender, EventArgs e)
    {
      Text = ((WebView)sender).StatusMessage;
    }
    
    void onNewWindow(object sender, NewWindowEventArgs e)
    {
//      e.Accepted = true;
//      Process.Start(e.TargetUrl);

      webBrowser1.Navigate(new Uri(e.TargetUrl));
      webBrowser1.Visible = true;
      richTextBox1.Visible = false;
    }
    
    void MainForm_Load(object sender, EventArgs e)
    {
      Visible = true;
      Application.DoEvents();
      
      if (!string.IsNullOrEmpty(config.correo) && !string.IsNullOrEmpty(config.imap)) {
        CargarTodos();
      }
    }
    
    private void CargarNoLeidos()
    {
      var imap = new ImapClient(
                   config.imap, config.puerto, config.correo, config.pwd, AuthMethod.Auto, true);
      IEnumerable<uint> uids = imap.Search(SearchCondition.Unseen());
      IEnumerable<MailMessage> mensajes = imap.GetMessages(uids);
      foreach (var m in mensajes) {
        DateTime d = Convert.ToDateTime(m.Headers.Get("Date"));
        int indice = dataGridView1.Rows.Add(m.From, m.Subject, d);
        dataGridView1.Rows[indice].Tag = m;
        dataGridView1.Sort(dataGridView1.Columns[2], System.ComponentModel.ListSortDirection.Descending);
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
          MailMessage m = imap.GetMessage(uid,false);
          DateTime d = Convert.ToDateTime(m.Headers.Get("Date"));
          int indice = dataGridView1.Rows.Add(m.From, m.Subject, d);
          dataGridView1.Rows[indice].Tag = m;
          dataGridView1.Sort(dataGridView1.Columns[2], System.ComponentModel.ListSortDirection.Descending);
        } else {
          break;
        }
      }
      
      if (imap.Supports("IDLE")) {
        imap.NewMessage += OnNewMessage;
      }
      
    }
    
    private void OnNewMessage(object sender, IdleMessageEventArgs e)
    {
      
      var m = e.Client.GetMessage(e.MessageUID,false);
      var d = Convert.ToDateTime(m.Headers.Get("Date"));
      var indice = dataGridView1.Rows.Add(m.From, m.Subject, d);
      dataGridView1.Rows[indice].Tag = m;
      dataGridView1.Sort(dataGridView1.Columns[2], System.ComponentModel.ListSortDirection.Descending);
      dataGridView1.Refresh();
    }

    
    
    private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
    {
      if (dataGridView1.Rows[e.RowIndex].Tag != null) {
        var mail = dataGridView1.Rows[e.RowIndex].Tag as MailMessage;
        webBrowser1.Visible = mail.IsBodyHtml;
        richTextBox1.Visible = !mail.IsBodyHtml;
        if (mail.IsBodyHtml) {
          webBrowser1.DocumentText = mail.Body;
        } else {
          richTextBox1.Text = mail.Body;
        }
      }
    }
    

  }
}
