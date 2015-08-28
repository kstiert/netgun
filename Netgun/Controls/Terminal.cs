using System.Drawing;
using System.Windows.Forms;
using ScintillaNET;

namespace Netgun.Controls
{
    public class Terminal : Scintilla 
    {
        private int _lastCaretPos = 0;

        public Terminal()
        {
            Lexer = Lexer.Cpp; // Close enough
            Styles[Style.BraceLight].BackColor = Color.LightGray;
            Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            Styles[Style.BraceBad].ForeColor = Color.Red;
            //UpdateUI += HandleUpdateUI;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return false; // Stops the terminal from eating all key input
        }

        private void HandleUpdateUI(object sender, UpdateUIEventArgs e)
        {
            // Has the caret changed position?
            var caretPos = CurrentPosition;
            if (_lastCaretPos != caretPos)
            {
                _lastCaretPos = caretPos;
                var bracePos1 = -1;
                var bracePos2 = -1;

                // Is there a brace to the left or right?
                if (caretPos > 0 && IsBrace(GetCharAt(caretPos - 1)))
                    bracePos1 = (caretPos - 1);
                else if (IsBrace(GetCharAt(caretPos)))
                    bracePos1 = caretPos;

                if (bracePos1 >= 0)
                {
                    // Find the matching brace
                    bracePos2 = BraceMatch(bracePos1);
                    if (bracePos2 == Scintilla.InvalidPosition)
                    {
                        if (GetCharAt(bracePos1) == '{')
                        {
                            InsertText(bracePos1 + 1, ":}");
                            BraceHighlight(bracePos1, bracePos1 + 2);
                            HighlightGuide = GetColumn(bracePos1);
                        }
                        else
                        {
                            BraceBadLight(bracePos1);
                            HighlightGuide = 0;
                        }
                    }
                    else
                    {
                        BraceHighlight(bracePos1, bracePos2);
                        HighlightGuide = GetColumn(bracePos1);
                    }
                }
                else
                {
                    // Turn off brace matching
                    BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
                    HighlightGuide = 0;
                }
            }
        }

        private static bool IsBrace(int c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                    return true;
            }

            return false;
        }
    }
}
