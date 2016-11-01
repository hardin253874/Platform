$hostName = [System.Net.Dns]::GetHostEntry("localhost").HostName;
write-host $hostName