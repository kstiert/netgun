using System.Windows.Forms;
using ScintillaNET;

namespace Netgun.Controls
{
    public class Terminal : Scintilla 
    {
        public Terminal()
        {
            Lexer = Lexer.Cpp; // Close enough
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return false; // Stops the terminal from eating all key input
        }
    }
}
