
$queueName = '.\private$\myqueue'

$queue = new-object System.Messaging.MessageQueue $queueName
$utf8  = new-object System.Text.UTF8Encoding

$msgs = $queue.GetAllMessages()
 
#write-host "Number of messages=$($msgs.Length)" 
$PrvMSMqReport = @()
foreach ($msg in $msgs)
  {
      $Messagebody =$utf8.GetString($msg.BodyStream.ToArray())
      $Id = $msg.Id
      $LookupId = $msg.LookupId
      $Destination = $msg.DestinationQueue
      $Rqueue = new-object System.Messaging.MessageQueue $msg.ResponseQueue
      $response = $Rqueue.Path
      $Lable = $msg.Label

     $row = new-object System.Object
     $row | Add-Member -type NoteProperty -Name Lable -Value $Lable
     $row | Add-Member -type NoteProperty -Name Id -Value $Id
     $row | Add-Member -type NoteProperty -Name LookUpId -Value $LookupId
     $row | Add-Member -type NoteProperty -Name MsgBody -Value $Messagebody.ToString()
     $row | Add-Member -type NoteProperty -Name Destination -Value $Destination
     $row | Add-Member -type NoteProperty -Name Responce -Value $responce
    $PrvMSMqReport +=$row
      
  }
 $PrvMSMqReport 

