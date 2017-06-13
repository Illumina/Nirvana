# ================
# global variables
# ================

# =========
# main loop
# =========

cd D:\Projects\NirvanaDevelopment\UnitTests
dotnet build
$loopCount = 1

do {
	Write-Host
	Write-Host "********************************"
	Write-Host "*** current loop: $loopCount"
	Write-Host "********************************"
	Write-Host

	$loopCount++
	iex "dotnet test --no-build"
	Write-Host "last exit code: $lastExitCode or $?"
} while ($LastExitCode -eq 0)
