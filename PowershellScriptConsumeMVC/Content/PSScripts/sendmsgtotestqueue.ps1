
[Reflection.Assembly]::LoadWithPartialName("System.Messaging")
$path = ".\Private$\test"
$queueName = '.\private$\myqueue'
$queue = $null
$exists = [System.Messaging.MessageQueue]::Exists($path)
if($exists -eq $false)
{
	$queue = [System.Messaging.MessageQueue]::Create($path)
} else
{
	$queue = New-Object System.Messaging.MessageQueue -ArgumentList $path
}
[console]::WriteLine("queue {0} exists (or created)", $path)

#
# Send message
#
$msg = New-Object System.Messaging.Message
$msg.Priority = [System.Messaging.MessagePriority]::high
$msg.Label = "Test Message "
$msg.Body = "Test Body (powershell)"
$msg.ResponseQueue = $queueName
$queue.Send($msg)
[console]::WriteLine("sent '{0}' message with body '{1}'", $msg.Label, $msg.Body)
