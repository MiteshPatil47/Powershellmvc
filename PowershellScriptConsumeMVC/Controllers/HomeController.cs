using PowershellScriptConsumeMVC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PowershellScriptConsumeMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            MsgQueModels model = new MsgQueModels();
            string path = @"D:\PowershelScript\getListOfMsgQue.ps1";
            //var result = RunPsScript(path);
            List<MsgQueueModel> objList = new List<MsgQueueModel>();
            List<MsgModel> rsltList = new List<MsgModel>();
            objList = GetmsgQueueList();
         //   NoError();

            
            model.msgList = objList;
            var distinctCategories = objList
                        .Select(m => new { m.QueueName, m.ServerName })
                        .Distinct()
                        .ToList();
            model.MsgQueList = new SelectList(distinctCategories, "QueueName", "QueueName");

            rsltList = GetAllMessages();

            model.msgBodyList = rsltList;

            return View(model);
            
        }

        // Display Partial View
        public ActionResult PartialView(string countrylist, MsgQueModels model)
        {
            string path = @"D:\PowershelScript\GetMessagedInTableFormat.ps1";

            if (countrylist == null)
            {
                model.msgQuueName = ".\\private$\\myqueue";
            }
            else
            {
                model.msgQuueName = countrylist;
            }

            List<MsgQueueModel> objList = new List<MsgQueueModel>();
            List<MsgModel> rstList = new List<MsgModel>();
            model.msgList = objList;
            objList = GetmsgQueueList();
            
            var distinctCategories = objList
                   .Select(m => new { m.QueueName, m.ServerName })
                   .Distinct()
                   .ToList();
            model.MsgQueList = new SelectList(distinctCategories, "QueueName", "QueueName");
            rstList = GetAllMessages();

            model.msgBodyList = rstList;
            //return PartialView("~/Views/Home/_PartialStakes.cshtml", stakesDetails.StacksList(clsStakes.Country));
            return PartialView("~/Views/Home/_PartialStakes.cshtml", rstList);

        }
        public ActionResult Powershell()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult About()
        {
            MsgQueModels model = new MsgQueModels();
            string path = @"D:\PowershelScript\GetMessagedInTableFormat.ps1";
            //var result = RunPsScript(path);
            List<MsgQueueModel> objList = new List<MsgQueueModel>();

            objList = GetmsgQueueList();
            //   NoError();

            model.msgList = objList;
            var distinctCategories = objList
                        .Select(m => new { m.QueueName, m.ServerName })
                        .Distinct()
                        .ToList();
            model.MsgQueList = new SelectList(distinctCategories, "QueueName", "QueueName");


            return View(model);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private static void RunPowershellScript(string scriptFile, string scriptParameters)
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            Pipeline pipeline = runspace.CreatePipeline();
            Command scriptCommand = new Command(scriptFile);
            Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();
            foreach (string scriptParameter in scriptParameters.Split(' '))
            {
                CommandParameter commandParm = new CommandParameter(null, scriptParameter);
                commandParameters.Add(commandParm);
                scriptCommand.Parameters.Add(commandParm);
            }
            pipeline.Commands.Add(scriptCommand);
            Collection<PSObject> psObjects;
            psObjects = pipeline.Invoke();
        }

        private Collection<PSObject> RunPsScript(string psScriptPath)
        {
            string psScript = string.Empty;
            if (System.IO.File.Exists(psScriptPath))
                psScript = System.IO.File.ReadAllText(psScriptPath);
            else
                throw new FileNotFoundException("Wrong path for the script file");
            RunspaceConfiguration config = RunspaceConfiguration.Create();
            PSSnapInException psEx;
            //add Microsoft SharePoint PowerShell SnapIn
            PSSnapInInfo pssnap = config.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out psEx);
            //create powershell runspace
            Runspace cmdlet = RunspaceFactory.CreateRunspace(config);
            cmdlet.Open();
            RunspaceInvoke scriptInvoker = new RunspaceInvoke(cmdlet);
            // set powershell execution policy to unrestricted
            scriptInvoker.Invoke("Set-ExecutionPolicy Unrestricted");
            // create a pipeline and load it with command object
            Pipeline pipeline = cmdlet.CreatePipeline();
            try
            {
                // Using Get-SPFarm powershell command 
                pipeline.Commands.AddScript(psScript);
                pipeline.Commands.AddScript("Out-String");
                // this will format the output
                Collection<PSObject> output = pipeline.Invoke();
                pipeline.Stop();
                cmdlet.Close();
                // process each object in the output and append to stringbuilder  
                StringBuilder results = new StringBuilder();
                foreach (PSObject obj in output)
                {
                    results.AppendLine(obj.ToString());
                }
                return output;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
       private  List<MsgQueueModel> GetmsgQueueList()
        {
            List<MsgQueueModel> objList = new List<MsgQueueModel>();
            //Execute PS1(PowerShell script) file
            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                //string path = System.IO.Path.GetDirectoryName(@"C:\Temp\") + "\\Get-EventLog.ps1";
                string path = @"D:\PowershelScript\getListOfMsgQue.ps1";
                if (!string.IsNullOrEmpty(path))
                    PowerShellInst.AddScript(System.IO.File.ReadAllText(path));

                Collection<PSObject> PSOutput = PowerShellInst.Invoke();
                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {
                        MsgQueueModel model = new MsgQueueModel();
                        model.ServerName = obj.Properties["ServerName"].Value.ToString() ;
                        model.QueueName = obj.Properties["QueueName"].Value.ToString();
                        model.MessageCount = obj.Properties["MessageCount"].Value.ToString() ;
                        model.CreationTime =   obj.Properties["Time"].Value.ToString();
                        objList.Add(model);
                    }
                }
               
            }
            return objList;
        }

        private List<MsgModel> GetAllMessages()
        {
            List<MsgModel> objList = new List<MsgModel>();
            //Execute PS1(PowerShell script) file
            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                //string path = System.IO.Path.GetDirectoryName(@"C:\Temp\") + "\\Get-EventLog.ps1";
                string path = @"D:\PowershelScript\GetMessagedInTableFormat.ps1";
                if (!string.IsNullOrEmpty(path))
                    PowerShellInst.AddScript(System.IO.File.ReadAllText(path));

                Collection<PSObject> PSOutput = PowerShellInst.Invoke();
                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {
                        MsgModel model = new MsgModel();
                        model.Lable = obj.Properties["Lable"].Value.ToString();
                        model.Id = obj.Properties["Id"].Value.ToString();
                        model.LookupId = obj.Properties["LookupId"].Value.ToString();
                        model.MessageBody = obj.Properties["MsgBody"].Value.ToString();
                        //model.Destination = obj.Properties["Destination"].Value.ToString();
                        //model.Response = obj.Properties["responce"].Value.ToString();
                        objList.Add(model);
                    }
                }

            }
            return objList;
        }


        void NoError()
        {
            //execute powershell cmdlets or scripts using command arguments as process
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = @"powershell.exe";
            //execute powershell script using script file
            //processInfo.Arguments = @"& {c:\temp\Get-EventLog.ps1}";
            //execute powershell command
            processInfo.Arguments = @"& {Get-EventLog -LogName Application -Newest 10 -EntryType Information | Select EntryType, Message}";
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;

            //start powershell process using process start info
            Process process = new Process();
            process.StartInfo = processInfo;
            process.Start();

            Console.WriteLine("Output - {0}", process.StandardOutput.ReadToEnd());
            Console.WriteLine("Errors - {0}", process.StandardError.ReadToEnd());
            Console.Read();
        }

        private string getData()
        {
            string Result = string.Empty;
            InitialSessionState iss = InitialSessionState.CreateDefault2();

            //In our specific case we don't need to import any module, but I'm adding these two lines below
            //to show where we would import a Module from Path.
            //iss.ImportPSModulesFromPath("C:\\inetpub\\LocalScriptsAndModules\\MyModuleFolder1");
            //iss.ImportPSModulesFromPath("C:\\inetpub\\LocalScriptsAndModules\\MyOtherModule2");
            string text = System.IO.File.ReadAllText(@"D:\PowershelScript\getListOfMsgQue.ps1");

            // Initialize PowerShell Engine
            var shell = PowerShell.Create(iss);

            // Add the command to the Powershell Object, then add the parameter from the text box with ID Input
            //The first one is the command we want to run with the input, so Get-ChildItem is all we need
            //shell.Commands.AddCommand("Get-ChildItem");
            shell.Commands.AddCommand(text);
            //Now we're adding the variable (so the directory) chosen by the user of the web application
            //Note that "Path" below comes from Get-ChildItem -Directory and Input.Text it's what the user typed
         //   shell.Commands.AddParameter("Path", Input.Text);

            // Execute the script 
            try
            {
                var results = shell.Invoke();

                // display results, with BaseObject converted to string
                // Note : use |out-string for console-like output
                if (results.Count > 0)
                {
                    // We use a string builder ton create our result text
                    var builder = new StringBuilder();

                    foreach (var psObject in results)
                    {
                        // Convert the Base Object to a string and append it to the string builder.
                        // Add \r\n for line breaks
                        builder.Append(psObject.BaseObject.ToString() + "\r\n");
                    }

                    // Encode the string in HTML (prevent security issue with 'dangerous' caracters like < >
                    Result = Server.HtmlEncode(builder.ToString());
                }
            }
            catch (ActionPreferenceStopException Error) { Result = Error.Message; }
            catch (RuntimeException Error) { Result = Error.Message; };
            return Result;


        }
       
    }
}