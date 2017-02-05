# ================
# global variables
# ================

$NirvanaSourceDir="D:\Projects\NirvanaNewGlobalCache"

$XunitConsole="$NirvanaSourceDir\Packages\xunit.runner.console.2.1.0\tools\xunit.console.exe"
$UnitTestDll="$NirvanaSourceDir\x64\Release\UnitTests.dll"
$XunitOptions="-nologo"

# =========
# main loop
# =========

$loopCount = 1

do {
	Write-Host "*** current loop: $loopCount ***"
	$loopCount++
	iex "$XunitConsole $UnitTestDll $XunitOptions"
	Write-Host "last exit code: $lastExitCode or $?"
} while ($LastExitCode -eq 0)
