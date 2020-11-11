[Reflection.Assembly]::LoadWithPartialName("System.Messaging")


$Source = '.\Private$\test'

$Destination = '.\Private$\idglog'

$SourceQueue = new-object System.Messaging.MessageQueue $Source

$DestinationQueue = new-object System.Messaging.MessageQueue $Destination

$TotalCount = $SourceQueue.GetAllMessages().Length

if ($TotalCount -gt 0)
{
$i = 0

do
 {         
   $DestinationQueue.send($SourceQueue.Receive())        
    $i++
 }
until ($i -eq $TotalCount)
}