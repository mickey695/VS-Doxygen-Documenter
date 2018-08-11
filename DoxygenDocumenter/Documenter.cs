using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using System.Diagnostics;//debug

namespace DoxygenDocumenter
{
    class Documenter
    {
        private int indentation {get; set;}
        private int tabSize { get; set; }

        private void decideStyle(string sampleData, int tabSize)
        {
            this.tabSize = sampleData.Count(c => c == '\t') == 0 ? 0 : tabSize;
        }

        private int calculateIndentation(string indentationData)
        {
            int tabs = indentationData.Count(c => c == '\t');
            int spaces = indentationData.Length - tabs;
            
            
            return tabs * tabSize + spaces;
        }

        private string indentationString(int size)
        {
            if (tabSize != 0)
            {
                int spaces = size % tabSize;
                int tabs = size / tabSize;
                return new String('\t', tabs) + new String(' ', spaces);
            }
            else
            {
                return new String(' ', size);
            }
        }

        public void generateDocumentation(TextDocument textDocument, CodeFunction function)
        {

            EditPoint leftClickEditPoint = textDocument.Selection.ActivePoint.CreateEditPoint();

            string lineData = leftClickEditPoint.GetText(-textDocument.Selection.ActivePoint.LineCharOffset + 1);
            int indentationStart = lineData.TakeWhile(c => char.IsWhiteSpace(c)).Count();
            string indentationSample = lineData.Substring(0, indentationStart);
            decideStyle(indentationSample, textDocument.TabSize);
            this.indentation = calculateIndentation(indentationSample);
            
            string documentation = "";

            documentation += documentationStart();
            documentation += makeBrief();
            documentation += makeParameters(function);
            documentation += makeReturn(function);
            documentation += documentationFinish();

            leftClickEditPoint.StartOfLine();
            leftClickEditPoint.Insert(documentation);
        }

        #region Documentation Functions

        // Stars the documentation block
        private string documentationStart()
        {
            return indentationString(this.indentation) + "/**";
        }

        // Creates a blank line for the documentation
        private string makeLine(int lines = 1)
        {
            return String.Concat(Enumerable.Repeat(String.Format("\n{0}* ", indentationString(this.indentation + 2)), lines));
        }

        // Creates the brief switch which appears on everything
        private string makeBrief()
        {
            return makeLine() + "@brief";
        }

        // Create switches for each parameter of the function
        private string makeParameters(CodeFunction function)
        {
            string result = "";
            foreach (CodeElement ce in function.Parameters)
            {
                result += makeLine(lines: 2) + "@param " + ce.Name + " - ";
            }
            return result;
        }

        // Get the return type and use it to decide if there should be a @return switch for the documentation
        private string makeReturn(CodeFunction function)
        {
            //This also works for constructors/destructors which is cool
            if (function.Type.TypeKind == vsCMTypeRef.vsCMTypeRefVoid)
            {
                return "";
            }
            else
            { 
                return makeLine(lines: 2) + "@return ";
            }
        }

        // Finish the documentation
        // fix so the function declration would be properly indented
        private string documentationFinish()
        {
            return makeLine() + "**/\n";
        }

        #endregion


    }
}
