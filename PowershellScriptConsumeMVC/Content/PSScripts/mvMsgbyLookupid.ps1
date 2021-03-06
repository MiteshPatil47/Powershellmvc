[CmdletBinding()]
param (
    [string] $Destination ,
    [string] $Source,
    [long]$lookupId ,
    [string] $msgbody 
)

$SourceQueue = new-object System.Messaging.MessageQueue $Source

$DestinationQueue = new-object System.Messaging.MessageQueue $Destination

$TotalCount = $SourceQueue.GetAllMessages().Length

$msgs = $SourceQueue.GetAllMessages()

$msg =  $msgs | Where-Object{ $_.LookupId -eq $lookupId}

foreach ($message in $msg){

$msgtosend = New-Object System.Messaging.Message
$msgtosend.Priority = [System.Messaging.MessagePriority]::high
$msgtosend.Label = $message.Label
$msgtosend.Body = $msgbody
$msgtosend.ResponseQueue = $SourceQueue
$result = $DestinationQueue.send($msgtosend)  
}

