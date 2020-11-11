[reflection.assembly]::LoadWithPartialName("System.Messaging")

$queueName = ".\private$\myqueue"
$queue = new-object System.Messaging.MessageQueue($queueName)

$queue.MessageReadPropertyFilter.SetAll()

$messages = $queue.GetAllMessages()

foreach ($msg in $messages)
  {
     #$msg.MessageReadPropertyFilter.SetAll()
    $msg

  }