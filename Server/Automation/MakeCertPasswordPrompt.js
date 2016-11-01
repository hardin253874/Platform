// Copyright 2011-2016 Global Software Innovation Pty Ltd
var programArgs = WScript.Arguments;
var defaultPassword = "Passw0rd!";
var maxItirations = 500;
if ( programArgs.Count() > 0 ) {
     defaultPassword = programArgs(0);
}
var shell = WScript.CreateObject("WScript.Shell");
var passIsEntered = false;
 
var timeOut = 0;
 
while (!passIsEntered && timeOut < maxItirations)
{
	var isActivated = shell.AppActivate("Enter Private Key Password");
	
	WScript.Sleep( 100 );
	if (isActivated)
	{
		shell.SendKeys( defaultPassword );
		WScript.Sleep( 200 );
		shell.SendKeys( "{TAB}" );
		WScript.Sleep( 200 );
		shell.SendKeys( "{ENTER}" );
		passIsEntered = true;
	}
	timeOut ++;
}
