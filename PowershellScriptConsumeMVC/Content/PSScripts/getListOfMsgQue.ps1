
$PrvMSMqReport = @()
$prvque = Get-MsmqQueue -QueueType Private
    foreach ($pq in $prvque) {
         
        $hostname = $pq.MachineName
        $Qname = $pq.QueueName  
        $MessageCount = $pq.MessageCount
        $time = get-date -format yyy-MM-dd/HH:mm:ss
 
        $row = new-object System.Object
        $row | Add-Member -type NoteProperty -Name ServerName -Value $hostname
        $row | Add-Member -type NoteProperty -Name QueueName -Value $Qname
        $row | Add-Member -type NoteProperty -Name MessageCount -Value $MessageCount
        $row | Add-Member -type NoteProperty -Name Time -Value $time.ToString()
        $PrvMSMqReport += $row
        }
 
 
$PrvMSMqReport