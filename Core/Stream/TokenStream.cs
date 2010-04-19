using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MUd {
    public class TokenStream {

        Queue<string> fTokens = new Queue<string>();

        char[] fTokenDelimers = new char[] { ' ', '\t' };
        string[] fLineComments = new string[] { "//", "#", ";" };
        string[] fBlockComment = new string[] { "/*", "*/" };

        public TokenStream(string filename) {
            StreamReader r = new StreamReader(filename);
            IReadTokens(r);
            r.Close();
        }

        public TokenStream(Stream parent) {
            StreamReader r = new StreamReader(parent);
            IReadTokens(r);
            r.Close();
        }

        private void IReadTokens(StreamReader r) {
            bool comment_block = false;
            while (!r.EndOfStream) {
                //Split the current line by tokens
                string line = r.ReadLine();
                string[] tok = line.Split(fTokenDelimers, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < tok.Length; i++) {
                    //First, check to see if this is a comment
                    bool line_comment = false;
                    foreach (string comment in fLineComments) {
                        if (tok[i].StartsWith(comment)) {
                            line_comment = true;
                            break;
                        }
                    }

                    //If it is a line comment, break the line parse and go to the next line
                    if (line_comment)
                        break;

                    //Now, if we are in a comment block, let's see if this is an ending
                    //If not, make sure we aren't beginning a comment block
                    //        If not, save the token :)
                    if (comment_block) {
                        if (tok[i] == fBlockComment[1])
                            comment_block = false; //Yayayay! We can save the ***next*** token :)
                    } else {
                        if (tok[i].StartsWith(fBlockComment[0]))
                            comment_block = true; //Start ignoring :(
                        else
                            fTokens.Enqueue(tok[i]); //Wooo! We have a token :D
                    }
                }
            }
        }

        public string NextToken() {
            if (fTokens.Count > 0)
                return fTokens.Dequeue();
            else
                return null;
        }
    }
}
