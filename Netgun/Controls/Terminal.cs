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
            Styles[Style.Cpp.Number].ForeColor = Color.DarkOrange;
            Styles[Style.Cpp.String].ForeColor = Color.Brown;
            Styles[Style.Cpp.Regex].ForeColor = Color.DarkOliveGreen;
            UpdateUI += HandleUpdateUI;
            CharAdded += HandleCharAdded;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // Stops the terminal from eating all key input (except arrows/tab, it can have those)
            return keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Tab;
        }

        private void HandleCharAdded(object sender, CharAddedEventArgs e)
        {
            // Find the word start
            var currentPos = CurrentPosition;
            var wordStartPos = WordStartPosition(currentPos, false);

            int lenEntered = 0;

            // WordStartPosition doesn't cound $
            if(GetCharAt(wordStartPos - 1) == '$')
            {
                lenEntered = currentPos - wordStartPos + 1;
            }

            // Start completion if you type a $
            if(e.Char == '$')
            {
                lenEntered = 1;
            }

            if (lenEntered > 0)
            {
                AutoCShow(lenEntered, "$ $add $addToSet $all $allElementsTrue $and $anyElementTrue $avg $bit $cmp $comment $concat $cond $currentDate $dateToString $dayOfMonth $dayOfWeek $dayOfYear $divide $each $elemMatch $eq $exists $explain $first $geoIntersects $geoNear $geoWithin $group $gt $gte $hint $hour $ifNull $in $inc $isolated $last $let $limit $literal $lt $lte $map $match $max $maxScan $maxTimeMS $meta $millisecond $min $minute $mod $month $mul $multiply $natural $ne $near $nearSphere $nin $nor $not $or $orderby $out $pop $position $project $pull $pullAll $push $pushAll $query $redact $regex $rename $returnKey $second $set $setDifference $setEquals $setIntersection $setIsSubset $setOnInsert $setUnion $showDiskLoc $size $skip $slice $snapshot $sort $strcasecmp $substr $subtract $sum $text $toLower $toUpper $type $unset $unwind $week $where $year");
            }   
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
                        BraceBadLight(bracePos1);
                        HighlightGuide = 0;
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
