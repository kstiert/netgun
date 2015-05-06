using ScintillaNET;
using System.Drawing;

namespace Netgun.Controls
{
    public class Terminal : Scintilla 
    {
        public Terminal()
        {
            Lexer = Lexer.Cpp; // Close enough
        }
    }
}
