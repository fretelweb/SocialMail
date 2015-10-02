/**
 * Author  Ronny Fretel <ronny@fretelweb.com>
 * Version 1.0
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Socials
{
  /// <summary>
  /// Description of TrayIcon.
  /// </summary>
  public class TrayIcon
  {
    
    NotifyIcon _tray;
    
    Form _ventana;
    

    
    public TrayIcon(Form ventana)
    {
      _ventana = ventana;
//      _ventana.ShowInTaskbar = false;
      _ventana.FormClosing += onClosing;
      _ventana.SizeChanged += onSizeChanged;
      
      _tray = new NotifyIcon();
      _tray.ContextMenu = new ContextMenu();
      _tray.ContextMenu.MenuItems.Add("Abrir", onAbrir);
      _tray.ContextMenu.MenuItems.Add("Salir", onSalir);
      _tray.ContextMenu.MenuItems[0].DefaultItem = true;
      _tray.DoubleClick += onDoubleClick;
      
      _tray.Icon = _ventana.Icon;
      _tray.Text = _ventana.Text;
      
      _tray.Visible = true;
    }

    void onDoubleClick(object sender, EventArgs e)
    {
      onAbrir(null, EventArgs.Empty);
    }

    void onAbrir(object sender, EventArgs e)
    {
      _ventana.ShowInTaskbar = true;
      _ventana.Show();
      if (_ventana.WindowState == FormWindowState.Minimized) {
        _ventana.WindowState = FormWindowState.Normal;
      }
    }
    void onSalir(object sender, EventArgs e)
    {
      _tray.Dispose();
      Application.Exit();
    }

    void onSizeChanged(object sender, EventArgs e)
    {
      if (_ventana.WindowState == FormWindowState.Minimized) {
        _ventana.ShowInTaskbar = false;
        _ventana.Hide();
      }
    }
    void onClosing(object sender, FormClosingEventArgs e)
    {
      if (e.CloseReason == CloseReason.UserClosing) {
        e.Cancel = true;
        _ventana.ShowInTaskbar = false;
        _ventana.Hide();
      }
    }
        
  }
}
