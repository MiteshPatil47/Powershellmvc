using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using PowershellScriptConsumeMVC.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Messaging;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PowershellScriptConsumeMVC.Controllers
{
    public class HomeController : Controller
    {
        public static Int32 j = 0;
        public System.Messaging.MessageQueue mq;
        [System.Messaging.MessagingDescription("MsgResponseQueue")]
        public System.Messaging.MessageQueue ResponseQueue { get; set; }
        public ActionResult Index()
        {
            MsgQueModels model = new MsgQueModels();
           List<MsgQueueModel> objList = new List<MsgQueueModel>();
            List<MsgModel> rsltList = new List<MsgModel>();
            CreateMsg();
          //  SendMessage("test", "288230376151732226", @".\Private$\myqueue", @".\Private$\test");
             objList = GetmsgQueueList();
              
            model.msgList = objList;
            var distinctCategories = objList
                        .Select(m => new { m.QueueName, m.ServerName })
                        .Distinct()
                        .ToList();
            model.MsgQueList = new SelectList(distinctCategories, "QueueName", "QueueName");
           // rsltList = GetAllMessages("private$\\myqueue");
            model.msgBodyList = rsltList;
            return View(model);
            
        }

        private void CreateMsg()
        {
            MessageQueue qu1 = new MessageQueue(".\\private$\\test");

            Message msg = new Message();

            msg.Body = "test message";
            msg.Label = "msg" + j.ToString();
            ResponseQueue = new MessageQueue(".\\private$\\myqueue");
            msg.ResponseQueue = ResponseQueue;
            j++;

            qu1.Send(msg);
        }
        // Display Partial View
        public ActionResult PartialView(string queueName, MsgQueModels model)
        {
  
            if (queueName == null)
            {
                model.msgQuueName = ".\\private$\\myqueue";
            }
            else
            {
                model.msgQuueName = ".\\" + queueName;
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


            rstList = GetAllMessagesBYQueueName(model.msgQuueName);
         //     rstList = GetAllMessages(model.msgQuueName);

            model.msgBodyList = rstList;
            //return PartialView("~/Views/Home/_PartialStakes.cshtml", stakesDetails.StacksList(clsStakes.Country));
            return PartialView("~/Views/Home/_PartialStakes.cshtml", rstList);

        }
     
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Ajax call method on submit 
        /// </summary>
        /// <param name="LookupId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MoveQueue(string LookupId, string message,string msgQueueName, string destination)
        {
            bool result = false;
            long lookid = Convert.ToInt64(LookupId);
            //var msg = GetMessageBody(message);
             // C# call 
            result =SendMessage(message, LookupId, ".\\"+destination, ".\\" + msgQueueName);
             //Power shell script call 
            // result = MoveQueueMsg(msg, lookid, msgQueueName, destination);
            return Json (JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }


        #region "Functionality methods"

        /// <summary>
        /// Get all Private message queue list
        /// </summary>
        /// <returns></returns>
        private List<MsgQueueModel> GetmsgQueueList()
        {
            List<MsgQueueModel> objList = new List<MsgQueueModel>();
            //Execute PS1(PowerShell script) file
            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                //string path = System.IO.Path.GetDirectoryName(@"C:\Temp\") + "\\Get-EventLog.ps1";
                string path = Server.MapPath( @"~\Content\PSScripts\getListOfMsgQue.ps1");
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
        /// <summary>
        /// Get all Gessages by message queue name 
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private List<MsgModel> GetAllMessages(string queueName)
        {
            List<MsgModel> objList = new List<MsgModel>();
            
          //Execute PS1(PowerShell script) file
            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                 // string path = @"D:\PowershelScript\GetMessagedInTableFormat.ps1";
                string path = Server.MapPath(@"~\Content\PSScripts\GetMessagedInTableFormat.ps1");
                if (!string.IsNullOrEmpty(path))
                    PowerShellInst.AddScript(System.IO.File.ReadAllText(path)).AddParameter("queueName", queueName);

                Collection<PSObject> PSOutput = PowerShellInst.Invoke();
                foreach (PSObject obj in PSOutput)
                {
                    if (obj != null)
                    {
                        MsgModel model = new MsgModel();
                        model.Lable = obj.Properties["Lable"].Value.ToString();
                        model.Id = obj.Properties["Id"].Value.ToString();
                        model.LookupId = obj.Properties["LookupId"].Value.ToString();
                        model.SendTime = obj.Properties["SendTime"].Value.ToString();
                        model.MessageBody = obj.Properties["MsgBody"].Value.ToString();
                       
                        //model.Destination = obj.Properties["Destination"].Value.ToString();
                        //model.Response = obj.Properties["responce"].Value.ToString();
                        objList.Add(model);
                    }
                }

            }
            return objList;
        }
        /// <summary>
        /// on submit button call to move queue message from destination to response queue
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lookupid"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private bool MoveQueueMsg(string msg,long lookupid, string queueName, string destination)
        {
            bool strResult = false;
            Dictionary<string, string> paramsdict = new Dictionary<string, string>();
            try
            {
                string path = Server.MapPath(@"~\Content\PSScripts\mvMsgbyLookupid.ps1");

                string scriptName = "mvMsgbyLookupid.ps1";
                //Execute PS1(PowerShell script) file
                using (PowerShell PowerShellInst = PowerShell.Create())
                {
                   // parameter dictionary with values
                    paramsdict.Add("Destination", ".\\" + queueName);
                    paramsdict.Add("Source", ".\\" + destination);
                    paramsdict.Add("lookupId", lookupid.ToString());
                    paramsdict.Add("msgbody", msg);

                    Collection<PSObject> PSOutput = RunPowershellScriptWithParameters(scriptName, paramsdict);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (PSObject obj in PSOutput)
                    {
                        stringBuilder.AppendLine(obj.ToString());
                    }

                }
                strResult = true;
            }
            catch(Exception ex)
            {
                strResult = false;
            }
            return strResult;
        }
        /// <summary>
        /// Get all Mesages for Message Queue using c# 
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private List<MsgModel>  GetAllMessagesBYQueueName(string queueName)
        {
            List<MsgModel> rstList = new List<MsgModel>();
            // Connect to a queue on the local computer.
            MessageQueue queue = new MessageQueue(queueName);

            // Set the queue to read the priority. By default, it
            // is not read.
            queue.MessageReadPropertyFilter.Priority = true;

           // Populate an array with copies of all the messages in the queue.
            Message[] msgs = queue.GetAllMessages();
         
            // Loop through the messages.
            foreach (Message msg in msgs)
            {
                MsgModel model = new MsgModel();
              //  StringBuilder ms = new StringBuilder();
                model.Lable = msg.Label;
                model.LookupId = msg.LookupId.ToString();
                msg.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                StreamReader sr = new StreamReader(msg.BodyStream);
                var msgBody = sr.ReadToEnd();
                model.MessageBody = msgBody;
               //model.SendTime = Convert.ToString( msg.ArrivedTime);
                model.Id = msg.Id;
                if(msg.ResponseQueue != null)
                model.Response = msg.ResponseQueue.QueueName;
                rstList.Add(model);
            }
            return rstList;
        }
        private string GetMessageBody(string xmlString)
        {
            string strResult = string.Empty;
            xmlString = xmlString.Replace("<?xml version=\"1.0\"?>", "");
            xmlString = xmlString.Replace("<string>","").Replace("</string>","").Replace("\n","");
            strResult = xmlString;
            return strResult;
        }

        /// <summary>
        /// Move message from recipent queue to response queue 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lid"></param>
        /// <param name="recipentQueue"></param>
        /// <param name="responseQueue"></param>
        /// <returns></returns>
        private bool SendMessage(string msg, string lid, string recipentQueue, string responseQueue)
        {
            bool bResult = false;
            long lookid = Convert.ToInt64(lid);
            try
            {
                MessageQueue queue = new MessageQueue(responseQueue, QueueAccessMode.SendAndReceive);
                //MessageQueue queue = new MessageQueue(responseQueue);
                // Connect to a queue on the local computer.
                MessageQueue queue1 = new MessageQueue(recipentQueue);

                // Populate an array with copies of all the messages in the queue.
                Message msgs = queue1.PeekByLookupId(lookid);
                msgs.Body = msg;
                //  mm.Label = "Msg" + j.ToString();
                //to add response queue name 
                ResponseQueue = new System.Messaging.MessageQueue(recipentQueue);
                msgs.ResponseQueue = ResponseQueue;
                j++;
                //queue.Path = responseQueue;
                
                //queue.Send(msgs);
                
                queue.Send(msgs);
                queue.Close();
                queue1.ReceiveByLookupId(msgs.LookupId);
                bResult = true;
            }
            catch (MessageQueueException ex)
            {
                bResult = false;
            }
            return bResult;
        }

        #endregion

        #region "Power sehll script calling "

        /// <summary>
        /// To Execute Powershell script by multiple parameters 
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="scriptParameters"></param>
        /// <returns></returns>
        private Collection<PSObject> RunPowershellScriptWithParameters(string scriptName, Dictionary<string, string> scriptParameters)
        {
            try
            {
                string path = Server.MapPath(@"~\Content\PSScripts\"+ scriptName);
                RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
                Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
                runspace.Open();
                RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
                Pipeline pipeline = runspace.CreatePipeline();
              
                Command scriptCommand = new Command(System.IO.File.ReadAllText(path), true);
                Collection<CommandParameter> commandParameters = new Collection<CommandParameter>();
                foreach (var scriptParameter in scriptParameters)
                {
                    CommandParameter commandParm = new CommandParameter(scriptParameter.Key, scriptParameter.Value);
                    commandParameters.Add(commandParm);
                    scriptCommand.Parameters.Add(commandParm);
                }
                pipeline.Commands.Add(scriptCommand);
                Collection<PSObject> psObjects;
                psObjects = pipeline.Invoke();

                return psObjects;
            }
            catch(Exception ex)
            {
                return null;
            }

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
        #endregion
    }
}