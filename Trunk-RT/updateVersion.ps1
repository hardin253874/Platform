Param ([string]$ver)
echo "version arg '$ver'"
(Get-Content .\project.clj) | Foreach-Object {$_ -replace 'defproject rt "0.1.0', ('defproject rt "' + $ver)} | Set-Content .\project.clj
