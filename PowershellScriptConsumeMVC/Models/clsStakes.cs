using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PowershellScriptConsumeMVC.Models
{
    public class clsStakes
    {
        public string Country { get; set; }
    }

    public class ClsPartialStackes
    {
        public string Date { get; set; }
        public string Race { get; set; }
    }

    public class MsgQueModels
    {
        public SelectList MsgQueList { get; set; }
        public string msgQuueName { get; set; }

        public List<MsgQueueModel> msgList { get; set; }
        public List<MsgModel> msgBodyList { get; set; }
    }

    public class MsgQueueModel
    {
        public string ServerName { get; set; }
        public string QueueName { get; set; }
        public string MessageCount { get; set; }
        public string CreationTime { get; set; }
    }

    public class MsgModel
    {
        public string Lable { get; set; }
        public string Id { get; set; }
        public string LookupId { get; set; }
        public string MessageBody { get; set; }
        public string Destination { get; set; }
        public string Response { get; set; }
    }
}