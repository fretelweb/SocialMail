/**
 * Author  Ronny Fretel <ronny@fretelweb.com>
 * Version 1.0
 */
using System;
using System.Windows.Forms;

namespace Socials
{
  /// <summary>
  /// Class with program entry point.
  /// </summary>
  internal sealed class Program
  {
    /// <summary>
    /// Program entry point.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new MainForm());
    }
    
  }
}
