
set CN=4
set CNLESS=2
set SERVER=http://spdevrt.sp.local:3000
rem set SERVER=http://syd1dev24.entdata.local:3000

pushd %SCRIPTDIR%

start runRemote.cmd -remote syd1dev04.entdata.local -sessions %CNLESS% -server %SERVER%
start runRemote.cmd -remote syd1dev24.entdata.local -sessions %CNLESS% -server %SERVER%
start runRemote.cmd -remote syd1dev32.entdata.local -sessions %CN% -server %SERVER%

start runRemote.cmd -remote sptc02.sp.local -sessions %CN% -server %SERVER% -test grc/tests/bcp-manager-1
start runRemote.cmd -remote sptc03.sp.local -sessions %CN% -server %SERVER% -test grc/tests/bcp-manager-1
start runRemote.cmd -remote sptc04.sp.local -sessions %CN% -server %SERVER% -test grc/tests/bcp-manager-1
start runRemote.cmd -remote sptc05.sp.local -sessions %CN% -server %SERVER% -test grc/tests/bcp-manager-1
start runRemote.cmd -remote sptc06.sp.local -sessions %CN% -server %SERVER% -test grc/tests/risk-manager-1
start runRemote.cmd -remote sptc07.sp.local -sessions %CN% -server %SERVER% -test grc/tests/risk-manager-1

start runRemote.cmd -remote sptc08.sp.local -sessions %CN% -server %SERVER% -test crm/tests/sales-manager-1
start runRemote.cmd -remote sptc09.sp.local -sessions %CN% -server %SERVER% -test crm/tests/lead-generator-1
start runRemote.cmd -remote sptc10.sp.local -sessions %CN% -server %SERVER% -test crm/tests/lead-generator-1
start runRemote.cmd -remote sptc11.sp.local -sessions %CN% -server %SERVER% -test crm/tests/sales-1

goto skip

:skip

popd

exit /b 0
--------------------------------------------------------------------------------

notes......

SYD1DEV01	Computer	Z210 Abida
SYD1DEV02	Computer	Z210 Tina
SYD1DEV03	Computer	Z220 Nino
SYD1DEV04	Computer	Z210 Jamal Mavadat
SYD1DEV12	Computer	Nino
SYD1DEV13	Computer	Z210 Shaofen Ning
SYD1DEV14	Computer	Z210 SPARE disabled 4/4/14
SYD1DEV16	Computer	Z210 Sri Korada
SYD1DEV17	Computer	Z210 Anurag Sharma
SYD1DEV18	Computer	Z210 Kun Dai
SYD1DEV19	Computer	Z210 Peter Aylett
SYD1DEV20	Computer	Z230 Con Christou
SYD1DEV21	Computer	Z210 disabled 4/4/14
SYD1DEV22	Computer	Z230 David Quint
SYD1DEV24	Computer	Z210 Steve Gibbon
SYD1DEV26	Computer	Z210 Scott Hopwood
SYD1DEV28	Computer	Z220 Brad Stevens
SYD1DEV32	Computer	Z220 Karen
SYD1DEV33	Computer	Z220 Anthony langsworth
SYD1DEV34	Computer	Z220 Alex Engelhardt
SYD1DEV35	Computer	Z220 Spare (QA)
SYD1DEV36	Computer	Z220 Ganesh Kumar
SYD1WKS03	Computer	HP Pro 3000 Summit Room
SYD1WKS45	Computer	Z220 Ganesh Kumar
SYD1WKS49	Computer	Z210 Peter Choi
SYD1WKS51	Computer	HP Pro 3000 Diana Walker
SYD1WKS52	Computer	Diana Walker

