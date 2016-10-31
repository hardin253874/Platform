param ($xml, $xsl)

if (-not $xml -or -not $xsl)
{
	Write-Host "& .\renderUnitTestResults.ps1 [-xml] xml-input [-xsl] xsl-input"
	exit;
}

trap [Exception]
{
	Write-Host $_.Exception;
    exit;
}

if (-!(Test-Path $xml)) 
{
	Write-Host "File $xml not found"
	exit
}

$xml = Resolve-Path $xml;
$xsl = Resolve-Path $xsl;
$output = $xml.ToString() + ".html";

$xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
$xslt.Load($xsl);
$xslt.Transform($xml, $output);

Write-Host "Generated " $output