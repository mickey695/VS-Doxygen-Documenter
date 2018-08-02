using System;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using Task = System.Threading.Tasks.Task;

namespace DoxygenDocumenter
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class ContextDoxygen
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("fa5b1616-08ab-4d8d-a52b-15fd41c187fc");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly AsyncPackage package;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ContextDoxygen"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">Command service to add command to, not null.</param>
		private ContextDoxygen(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static ContextDoxygen Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
		{
			get
			{
				return this.package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static async Task InitializeAsync(AsyncPackage package)
		{
			// Verify the current thread is the UI thread - the call to AddCommand in ContextDoxygen's constructor requires
			// the UI thread.
			ThreadHelper.ThrowIfNotOnUIThread();

			OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
			Instance = new ContextDoxygen(package, commandService);
		}

		/// <summary>
		/// This function is the callback used to execute the command when the menu item is clicked.
		/// See the constructor to see how the menu item is associated with this function using
		/// OleMenuCommandService service and MenuCommand class.
		/// </summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">Event args.</param>
		private async void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			DTE2 dte = (DTE2)await this.ServiceProvider.GetServiceAsync(typeof(DTE));
			if (dte == null)
			{
				ErrorMessage("Failed to get DTE instance");
				return;
			}

			TextDocument textDocument = (TextDocument)dte.ActiveDocument.Object("TextDocument");
			if (textDocument == null)
			{
				ErrorMessage("Failed to get a text document instance");
				return;
			}

			CodeFunction function = (CodeFunction)textDocument.Selection.ActivePoint.CodeElement[vsCMElement.vsCMElementFunction];
			if (function == null)
			{
				ErrorMessage("Documentation can only be created for functions. Not the global scope.");
				return;
			}

			generateDocumentation(textDocument, function);
		}

		void generateDocumentation(TextDocument textDocument, CodeFunction function)
		{
			EditPoint editPoint = function.StartPoint.CreateEditPoint();

			string documentation = "";

			documentation += documentationStart();
			documentation += makeBrief();
			documentation += makeParameters(function);
			documentation += makeReturn(function);
			documentation += documentationFinish();
			
			editPoint.Insert(documentation);	
		}

		#region Documentation Functions

		private string documentationStart()
		{
			return "/**";
		}

		private string makeLine(int indentation = 2, int lines=1)
		{
			return String.Concat(Enumerable.Repeat(String.Format("\n{0}* ", new String(' ', indentation)), lines));
		}

		private string makeBrief()
		{
			return makeLine() + "@brief";
		}

		private string makeParameters(CodeFunction function)
		{
			string result = "";
			foreach (CodeElement ce in function.Parameters)
			{
				result += makeLine(lines:2) + "@param " + ce.Name + " - ";
			}
			return result;
		}

		private string makeReturn(CodeFunction function)
		{
			string returnType = function.Prototype[(int)(vsCMPrototype.vsCMPrototypeType | vsCMPrototype.vsCMPrototypeNoName)].Split(' ')[0];

			return returnType != "void" ? makeLine(lines:2) + "@return " : "";
		}

		private string documentationFinish()
		{
			return makeLine() + "**/\n";
		}

		#endregion

		private void ErrorMessage(string message, string title="")
		{
			VsShellUtilities.ShowMessageBox(
				this.package,
				message,
				title,
				OLEMSGICON.OLEMSGICON_INFO,
				OLEMSGBUTTON.OLEMSGBUTTON_OK,
				OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}
	}
}
