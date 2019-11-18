namespace UnityTools {
    public static class StringExtensions
    {
        const char lineBreak = '\n';
        const string lineBreakAdd = "-\n";
        
        public static string AdjustToMaxLineLength (this string input, int maxCharacters, out int lines) {
            lines = 1;

            int length = input.Length;
            string adjusted = "";
            int l = 0;
            for (int i = 0; i < length; i++) {                
                adjusted += input[i];
                l++;
                if (input[i] == lineBreak) {
                    l = 0;
                    lines++;
                }
                else {
                    if ( l == maxCharacters ) {
                        adjusted += lineBreakAdd;
                        l = 0;
                        lines++;
                    }
                }
            }
            return adjusted;
        }
        static int LineCount (this string input) {
            int lines = 1;
            int length = input.Length;
            for (int i = 0; i < length; i++) {                
                if (input[i] == lineBreak) lines++;
            }
            return lines;
        }
    }
}
