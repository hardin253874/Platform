// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Core.Cache;
using ReadiNow.Expressions.Evaluation;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace ReadiNow.Expressions.Test.Runner
{
    [TestFixture]
	[RunWithTransaction]
    class ScriptResourceTests
    {
        [TestCase("script: let x = [Resource Type].Name select x                        ;context:All Fields:AF 1        ;expect: string list:All Fields")]   // #27894
        [TestCase("script: let x = [Resource Type].[Resource Type] select x.Name        ;context:All Fields:AF 1        ;expect: string list:Definition")]
        [TestCase("script: Name + ' ' + Number / 2                                      ;context:All Fields:AF 1        ;expect: string:AF 1 50.000     ;host:Evaluate")]  // TODO: Fix SQL
        [TestCase("script: (Decimal + Currency) / 2 * Number                            ;context:All Fields:AF 1        ;expect: currency:10010.5")]
        [TestCase("script: Name + '.' + AA_Employee + '.' + [Weekday]                   ;context:AA_All Fields:Test 01        ;expect: string:Test 01.Tina Adlakha.Sunday")]
        [TestCase("script: Manager.Name + Manager.Name                                  ;context:AA_Manager:Scott Hopwood  ;expect: string:Jude JacobsJude Jacobs")]
        [TestCase("script: let m=Manager select m.Name + m.Name                         ;context:AA_Manager:Scott Hopwood  ;expect: string:Jude JacobsJude Jacobs")]
        [TestCase("script: Manager.Name + Manager.Manager.Name                          ;context:AA_Employee:David Quint   ;expect: string:Peter AylettJude Jacobs")]
        [TestCase("script: len(Name)                                                    ;context:AA_Employee:David Quint   ;expect: int:11")]
        [TestCase("script: (all(AA_Employee) where Name='Peter Aylett').Age             ;host:Evaluate; expect: int list:9999")]
        [TestCase("script: (all(AA_Employee) where Status='On Leave' order by Age).Age  ;host:Evaluate; expect: int list:21,22,25,32,37,40,45")]
        [TestCase("script: any(all(AA_Employee).Age>20)                                 ;host:Evaluate; expect: bool:true")]
        [TestCase("script: join(all([All Fields])) like '%, AF%'                        ;host:Evaluate; expect: bool:true")]
        //[TestCase("script: join(all([AA_Department]) order by Name)                     ;host:Evaluate; expect: string:Corporate,Development,Finance,Legal,Operations")]   // Fix me
        [TestCase("script: convert(aa_manager,context()).[Direct Reports]               ;context:AA_Employee:Peter Aylett  ;expect: AA_Employee list:Kun Dai,Anurag Sharma,Peter Choi,David Quint,Con Christou,Sri Korada   ;unsorted")]
        [TestCase( "script: convert(aa_manager,context()).[Direct Reports] order by Name         ;host:Evaluate;context:AA_Employee:Peter Aylett  ;expect: AA_Employee list:Anurag Sharma,Con Christou,David Quint,Kun Dai,Peter Choi,Sri Korada" )]
        [TestCase("script: count(convert(aa_manager,context()).[Direct Reports])        ;context:AA_Employee:Peter Aylett  ;expect: int:6")]
        [TestCase( "script: let level= len([AA_Herb].[Name]) select iif([AA_Herb] is null, level, 'x') ;context:AA_All Fields:Test 30 ;expect: string:null" )]
        [TestCase( "script: let a = max([AA_All Fields].[DateTime]) select ([AA_All Fields] where [DateTime] = a).[Name] ;context:AA_Herb:Coriander ;expect: string list:Test 28" )] // #28406
        // TODO: Fix text .. data no longer valid //[TestCase("script: all ([AF_All Fields]) where [Priority - Choice]='low'   ;host:Evaluate ;context:AF_All Fields:Test 01  ;expect: All Fields list:AF 1,AF 2 ;unsorted")]
        // TODO: Fix text .. data no longer valid //[TestCase("script: all ([AF_All Fields]) where [Priority - Choice]='low'   ;host:Evaluate ;expect: AF_All Fields list:AF 1,AF 2 ;unsorted")]
        // TODO: Fix text .. data no longer valid //[TestCase("script: all ([AF_All Fields]) where [Priority - Choice]='lowx'  ;host:Evaluate ;context:AF_All Fields:AF 1  ;expect: All Fields list:empty ;unsorted")]
        // TODO: Fix text .. data no longer valid //[TestCase("script: all ([AF_All Fields]) where [Priority - Choice]='lowx'  ;host:Evaluate ;expect: AF_All Fields list:empty ;unsorted")]
        [RunAsDefaultTenant]
        public void Calculations_MiscIntegration(string test)
        {
            TestHelper.Test(test);
        }

       
        [TestCase("script: resource(AA_Drink,Tab)                      ;expect: AA_Drink:Tab")]         // not this version can not have a space in the name
        [TestCase("script: resource(AA_Employee, 'Peter Aylett')       ;expect: AA_Employee:Peter Aylett")]
        [TestCase("script: resource(AA_Employee, 'Peter' + ' Aylett')  ;host:Evaluate; expect: AA_Employee:Peter Aylett")]
        [TestCase("script: resource(AA_Employee, 'doesntexist')        ;error:Could not find a 'AA_Employee' called 'doesntexist'. (pos 23)")]
        [TestCase("script: resource(AA_Employee, 'doesnt' + 'exist')   ;host:Evaluate; expect: AA_Employee:null")]
        [RunAsDefaultTenant]
        public void Calculations_TheResourceFunction(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: id(Manager) > 0;      context:AA_Employee:David Quint   ;hostapi:true ;expect: bool:true")]
        [TestCase("script: id(context()) > 0;    context:AA_Employee:David Quint   ;hostapi:true ;expect: bool:true")]
        [TestCase( "script: (all(AA_Drink) where Name like 'S%').Name;    context:AA_Employee:David Quint   ;expect: string list:Sunkist Orange,Sarsaparilla,Soda Water,Solo Lemon Lime,Solo,Sprite Lemonade" )]
        [TestCase( "script: count(all(AA_Drink));    context:AA_Employee:David Quint   ;expect: int:30" )]
        [RunAsDefaultTenant]
        public void Calculations_ResourceFunctions(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: context().Name       ;context:All Fields:AF 1        ;expect: string:AF 1")]
        [TestCase("script: Name                 ;context:All Fields:AF 1        ;expect: string:AF 1")]
        [TestCase("script: [Multi Text]         ;context:All Fields:AF 1        ;expect: string")]
        [TestCase("script: Number               ;context:All Fields:AF 1        ;expect: int:100")]
        [TestCase("script: Decimal              ;context:All Fields:AF 1        ;expect: decimal:100.11")]
        [TestCase("script: Currency             ;context:All Fields:AF 1        ;expect: currency:100.1")]
        [TestCase("script: [Yes No]             ;context:All Fields:AF 1        ;expect: bool:false")]
        [TestCase("script: [Date]               ;context:All Fields:AF 1        ;expect: date:2012-12-01")]
        [TestCase("script: [Time]               ;context:All Fields:AF 1        ;expect: time:13:00:00")]
        [TestCase("script: [Date Time]          ;context:All Fields:AF 1        ;expect: datetime:2012-12-01 1:00 PM")]
        [TestCase("script: [Legacy Guid]        ;context:Application:Test Solution ;expect: guid:89B98F72-72F2-43C2-A18A-DC8571765637")]
        [RunAsDefaultTenant]
        public void Calculations_AccessFields(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: context()            ;context:All Fields:AF 1        ;expect: All Fields:AF 1")]
        [TestCase("script: all([All Fields])    ;host:Evaluate                  ;expect: All Fields list:AF 1,AF 2,AF 3,AF 4,AF 5   ;unsorted")]
        //[TestCase("script: all(Note)            ;host:Evaluate                  ;expect: Note list:   ;unsorted")]            // TODO : Fix me
        [TestCase("script: AA_Employee          ;context:AA_All Fields:Test 01  ;expect: AA_Employee:Tina Adlakha")]
        [TestCase("script: AA_Employee          ;context:AA_All Fields:Test 01  ;convertto:string       ;expect: string:Tina Adlakha")]
        [TestCase("script: Person.Age           ;context:All Fields:AF 1        ;expect: int:5")]
        [TestCase("script: [Direct Reports]     ;context:AA_Manager:Scott Hopwood  ;expect: AA_Employee list:Martin Kalitis,Steve Gibbon  ;unsorted")]
        [TestCase("script: convert(AA_Manager, [Direct Reports]) ;context:AA_Manager:Jude Jacobs  ;expect: AA_Manager list")]
        [TestCase("script: resource(AA_Employee,[Peter Aylett]).[First Name]       ;host:Evaluate                  ;expect: string:Peter")]
        [TestCase("script: resource(AA_Employee,[Peter Aylett])                    ;host:Evaluate                  ;expect: AA_Employee:Peter Aylett")]
        [TestCase("script: Manager.Manager.Name ;context:AA_Employee:Jude Jacobs   ;expect: string:null")]    // #22454
        [TestCase( "script: convert(AA_Manager, context()) ;context:AA_Employee:Jude Jacobs  ;expect: AA_Manager:Jude Jacobs" )] // #25915
        [TestCase( "script: convert(AA_Manager, context()) ;context:AA_Employee:David Quint  ;host:Evaluate;expect: AA_Manager:null" )] // #25915 (todo: remove host:evaluate for #25916)
        [TestCase( "script: iif(convert(AA_Manager, context()) is null,'Emp','Mgr')        ;host:Evaluate;context:AA_Employee:David Quint  ;expect: string:Emp" )] // #25915 (todo: remove host:evaluate for #25916)
        [RunAsDefaultTenant]
        public void Calculations_AccessResource(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: [Direct Reports] where Name='David Quint'                        ;context:AA_Manager:Peter Aylett; expect: AA_Employee list:David Quint    ;unsorted")]
        [TestCase("script: let dr = [Direct Reports] select dr where Name='David Quint'     ;context:AA_Manager:Peter Aylett; expect: AA_Employee list:David Quint    ;unsorted")]
        [TestCase("script: count(AA_Employee where Status.Name='Available')                 ;context:AA_All Fields:Test 02; expect: int:1")]
        [TestCase("script: count(AA_Employee.Status where Name='Available')                 ;context:AA_All Fields:Test 02; expect: int:1")]
        [TestCase("script: count(context() where AA_Employee='Peter Aylett')                ;context:AA_All Fields:Test 02; expect: int:1")]
        [TestCase("script: count(context() where AA_Employee='Something else')              ;context:AA_All Fields:Test 02; expect: int:0")]
        [TestCase("script: count(context() where [AA_All Fields].[number] = 1000)           ;context:AA_Herb:Basil; expect: int:1")]
        [RunAsDefaultTenant]
        public void Calculations_Where(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: [Direct Reports] order by Name                   ;context:AA_Manager:Peter Aylett   ;host:Evaluate  ;expect: AA_Employee list:Anurag Sharma,Con Christou,David Quint,Kun Dai,Peter Choi,Sri Korada")]
		[TestCase( "script: [Direct Reports] order by Name asc               ;context:AA_Manager:Peter Aylett   ;host:Evaluate  ;expect: AA_Employee list:Anurag Sharma,Con Christou,David Quint,Kun Dai,Peter Choi,Sri Korada" )]
		[TestCase( "script: [Direct Reports] order by Name desc              ;context:AA_Manager:Peter Aylett   ;host:Evaluate  ;expect: AA_Employee list:Sri Korada,Peter Choi,Kun Dai,David Quint,Con Christou,Anurag Sharma" )]
		[TestCase( "script: [Direct Reports] order by Age, Name desc         ;context:AA_Manager:Peter Aylett   ;host:Evaluate  ;expect: AA_Employee list:David Quint,Peter Choi,Anurag Sharma,Con Christou,Sri Korada,Kun Dai" )]
        [RunAsDefaultTenant]
        public void Calculations_OrderBy(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: any([Direct Reports])                            ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:true")]
        [TestCase("script: any([Direct Reports] where true)                 ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:true")]
        [TestCase("script: any([Direct Reports] where false)                ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:false")]
        [TestCase("script: any([Direct Reports] where Name='David Quint')   ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:true")]
        //[TestCase("script: any(null where false)                            ;context:Manager:Peter Aylett   ;host:Evaluate	;expect: bool:false")] // fix me
        [TestCase("script: any([Direct Reports] where null)                 ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:false")]
        [TestCase("script: not any([Direct Reports])                        ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:false")]
        [TestCase("script: any([AA_Truck])                                  ;context:AA_All Fields:Test 30     ;host:Evaluate	;expect: bool:false")]
        [TestCase("script: not any([AA_Truck])                              ;context:AA_All Fields:Test 30     ;host:Evaluate	;expect: bool:true")]
        [TestCase("script: any([AA_Truck])                                  ;context:AA_All Fields:Test 01     ;host:Evaluate	;expect: bool:true")]
        [TestCase("script: not any([AA_Truck])                              ;context:AA_All Fields:Test 01     ;host:Evaluate	;expect: bool:false")]
        //[TestCase("script: any([AA_Truck].Description='Blah')               ;context:AA_All Fields:Test 01     ;host:Evaluate	;expect: bool:false")]  // investigate 
        //[TestCase("script: any([AA_Truck].Driver='Blah')                    ;context:AA_All Fields:Test 01     ;host:Evaluate	;expect: bool:false")]  // investigate 
        [TestCase("script: any([AA_Truck] where Description='Blah')               ;context:AA_All Fields:Test 01     ;host:Evaluate	;expect: bool:false")]
        [TestCase("script: any([AA_Truck] where Driver='Blah')                    ;context:AA_All Fields:Test 01     ;host:Evaluate	;expect: bool:false")]
        [TestCase("script: any([Direct Reports].Age>20)                     ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:true")]
        [TestCase("script: every([Direct Reports].Name <> 'blah')           ;context:AA_Manager:Peter Aylett   ;host:Evaluate	;expect: bool:true")]
        [TestCase("script: count([Direct Reports])                          ;context:AA_Manager:Peter Aylett   ;expect: int:6")]
        [TestCase("script: count([Direct Reports] where Name='David Quint') ;context:AA_Manager:Peter Aylett   ;expect: int:1")]
        [TestCase("script: count(all([All Fields]))                                                         ;host:Evaluate	;expect: int:5")]
        [TestCase("script: count(p)                                         ;param:p=AA_Person list:Scott Hopwood,Peter Aylett         ;expect: int:2")]
        [TestCase("script:let d = convert(AA_Manager, [Direct Reports]) select count(d)    ;context:AA_Manager:Peter Aylett            ;expect: int:1")]    // This does NOT count the direct reports. They used to, but it was a bad language choice as it prevented variables from being used in aggregate conditions.
        [TestCase("script: sum(all([All Fields]).Number)                                                    ;host:Evaluate	;expect: int:1500")]
        [TestCase("script: max(all([All Fields]).Number)                                                    ;host:Evaluate	;expect: int:500")]
        [TestCase("script: min(all([All Fields]).Number)                                                    ;host:Evaluate	;expect: int:100")]
        [TestCase("script: sum(all([All Fields]).Decimal)                                                   ;host:Evaluate	;expect: decimal:1501.66")]
        [TestCase("script: max(all([All Fields]).Decimal)                                                   ;host:Evaluate	;expect: decimal:500.56")]
        [TestCase("script: min(all([All Fields]).Decimal)                                                   ;host:Evaluate	;expect: decimal:100.11")]
        [TestCase("script: round(stdev(all([All Fields]).Number),4)                                         ;host:Evaluate	;expect: decimal:158.1139")]
        [TestCase("script: avg(all([All Fields]).Number)                                                    ;host:Evaluate	;expect: decimal:300")]
        [TestCase("script: max(all([All Fields]).Name)                                                      ;host:Evaluate	;expect: string:AF 5")]
        [TestCase("script: min(all([All Fields]).Name)                                                      ;host:Evaluate	;expect: string:AF 1")]
        [TestCase("script: sum(convert([All Fields], Instances).Number)                                                    ;context:Type:All Fields	;expect: int:1500")]
        [TestCase("script: max(convert([All Fields], Instances).Number)                                                    ;context:Type:All Fields	;expect: int:500")]
        [TestCase("script: min(convert([All Fields], Instances).Number)                                                    ;context:Type:All Fields	;expect: int:100")]
        [TestCase("script: sum(convert([All Fields], Instances).Decimal)                                                   ;context:Type:All Fields	;expect: decimal:1501.66")]
        [TestCase("script: max(convert([All Fields], Instances).Decimal)                                                   ;context:Type:All Fields	;expect: decimal:500.56")]
        [TestCase("script: min(convert([All Fields], Instances).Decimal)                                                   ;context:Type:All Fields	;expect: decimal:100.11")]
        [TestCase("script: round(stdev(convert([All Fields], Instances).Number),4)                                         ;context:Type:All Fields	;expect: decimal:158.1139")]
        [TestCase("script: avg(convert([All Fields], Instances).Number)                                                    ;context:Type:All Fields	;expect: decimal:300")]
        [TestCase("script: max(convert([All Fields], Instances).Name)                                                      ;context:Type:All Fields	;expect: string:AF 5")]
        [TestCase("script: min(convert([All Fields], Instances).Name)                                                      ;context:Type:All Fields	;expect: string:AF 1")]
        [TestCase("script: count(convert([All Fields], Instances))                                                         ;context:Type:All Fields	;expect: int:5")]
        [TestCase( "script: max(Building.Rooms.[Room Type])                                                                ;context:Campuses:Broadway Campus    ;expect: Room Type:Computer Lab" )]
        [TestCase( "script: max(Building.Rooms.[Room Type])                                                                ;context:Campuses:Blackfriars Campus ;expect: Room Type:Theater room" )]
        [TestCase( "script: min(Building.Rooms.[Room Type])                                                                ;context:Campuses:Broadway Campus    ;expect: Room Type:Lecture room" )]
        [TestCase( "script: min(Building.Rooms.[Room Type])                                                                ;context:Campuses:Blackfriars Campus ;expect: Room Type:Lecture room" )]
        [TestCase( "script: iif(min(Building.Rooms.[Room Type])='Lecture room',1,2)                                        ;context:Campuses:Blackfriars Campus ;expect: int:1" )]
        [TestCase( "script: iif(min(Building.Rooms.[Room Type]) is null,1,2)                                               ;context:Campuses:Blackfriars Campus ;expect: int:2" )]
        [TestCase( "script: iif(count(Building.Rooms)=0,1,2)                                                               ;context:Campuses:Blackfriars Campus ;expect: int:2" )]
        [TestCase("script: count([Direct Reports])			;context:AA_Manager:Peter Aylett			;expect:int:6")]
        [RunAsDefaultTenant]
        public void Calculations_Aggregate(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: 'Hello' + AA_Truck   		    ;context:AA_All Fields:Test 30			    ;expect:string:Hello")]
        [TestCase("script: 'Hello' + AA_Employee   		    ;context:AA_All Fields:Test 30			    ;expect:string:Hello")]
        [TestCase("script: 'Hello' + AA_Drinks   		    ;context:AA_All Fields:Test 30			    ;expect:string:Hello")]
        [TestCase("script: 'Hello' + [AA_ChocBars (Rev)]    ;context:AA_All Fields:Test 30			    ;expect:string:Hello")]
        [TestCase("script: count(AA_Truck)				    ;context:AA_All Fields:Test 30			    ;expect:int:0")]
        [TestCase("script: count(AA_Employee)			    ;context:AA_All Fields:Test 30			    ;expect:int:0")]
        [TestCase("script: count(AA_Drinks)				    ;context:AA_All Fields:Test 30			    ;expect:int:0")]
        [TestCase("script: count([AA_ChocBars (Rev)])		;context:AA_All Fields:Test 30			    ;expect:int:0")]
        [TestCase("script: any(AA_Truck)				    ;context:AA_All Fields:Test 30			    ;host:Evaluate  ;expect:bool:false")]
        [TestCase("script: any(AA_Employee)			        ;context:AA_All Fields:Test 30			    ;host:Evaluate  ;expect:bool:false")]
        [TestCase("script: any(AA_Drinks)				    ;context:AA_All Fields:Test 30			    ;host:Evaluate  ;expect:bool:false")]
        [TestCase("script: any([AA_ChocBars (Rev)])		    ;context:AA_All Fields:Test 30			    ;host:Evaluate  ;expect:bool:false")]
        [TestCase("script: count(resource(AA_Herb, Cilantro).AA_Stores)									;host:Evaluate	;expect: int:0")]   // Bug 25780, Expression engine returns a list with a null rather than an empty list.
        [RunAsDefaultTenant]
        public void Calculations_EmptyRelationships(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script:  Age				;context:AA_Employee:Peter Aylett	;expect: int:9999")]
        [TestCase("script:  Manager.Age 		;context:AA_Employee:Peter Aylett	;expect: int:45")]
        [TestCase("script:  Age-Manager.Age		;context:AA_Employee:Peter Aylett	;expect: int:9954")]
        [TestCase("script:  Manager.Age-Age		;context:AA_Employee:Peter Aylett	;expect: int:-9954")]
        [TestCase("script:  AA_Truck.Name		;context:AA_All Fields:Test 01	;expect: string list")]
        [TestCase("script:  convert(AA_Manager,context()).[Direct Reports]		;context:AA_Employee:Peter Aylett	;expect: AA_Employee list:Kun Dai, Anurag Sharma, Peter Choi, David Quint, Con Christou, Sri Korada;unsorted")] //Bug 17437
        //[TestCase("script:  convert(AA_Manager,context()).[Direct Reports] order by Name	;context:AA_Employee:Peter Aylett	;expect: AA_Employee list:Anurag Sharma, Con Christou, David Quint, Kun Dai, Peter Choi,Sri Korada")] //Bug 17437
        //[TestCase("convert(AA_manager,context()).[Direct Reports] order by Age 			;context:AA_Employee:Peter Aylett	;expect: AA_Employee list:David Quint,Peter Choi, Anurag Sharma, Con Christou, Sri Korada, Kun Dai")] //Bug 17437
        //[TestCase("script:	convert(AA_Manager,context()).[Direct Reports] where Name='Karen Jones' ;context:AA_Employee:Peter Aylett	;expect: AA_Employee list:empty")]  //Bug 17437
        //[TestCase("script:	convert(AA_Manager,context()).[Direct Reports] where Name='Kun Dai' ;context:AA_Employee:Peter Aylett	;expect: AA_Employee list:Kun Dai")]  //Bug 17437
        [TestCase("script: iif(Age<30,'Below 30',iif(Age <60,'Below 60',iif(Age <90,'Below 90','Greater 90')))		;context:AA_Employee:Peter Aylett	;expect:string:Greater 90")]
        [TestCase("script: IsNull([Manager],'No Manager') 		;context:AA_Employee:Peter Aylett			;expect:string:Jude Jacobs")]
        [TestCase("script: Name = 'Peter Aylett'			;context:AA_Employee:Peter Aylett			;expect:bool:true")]
        [TestCase("script: Name = 'Peter'				;context:AA_Employee:Peter Aylett			;expect:bool:false")]
        [TestCase("script: Name like 'Pe%'				;context:AA_Employee:Peter Aylett			;expect:bool:true")]
        [TestCase("script: Name not like '%Fred%'			;context:AA_Employee:Peter Aylett			;expect:bool:true")]
        [TestCase("script: Name not like '%Aylett'			;context:AA_Employee:Peter Aylett			;expect:bool:false")]
        [TestCase("script: Name like '%ay%'				;context:AA_Employee:Peter Aylett			;expect:bool:true")]
        [TestCase("script: iif(Name not like '%Aylett',Name,'Other')	;context:AA_Employee:Peter Aylett			;expect:string:Other")]
        [TestCase("script: Boolean				;context:AA_All Fields:Test 30			;expect:bool:false")]
        [TestCase("script: iif(not Boolean,'Yes','No')		;context:AA_All Fields:Test 30			;expect:string:Yes")]
        [TestCase("script: iif(Not Boolean,'Yes','No')		;context:AA_All Fields:Test 30			;expect:string:Yes")]
        [TestCase("script: len(Name)					;context:AA_Employee:Peter Aylett			;expect:int:12")]
        [TestCase("script: replace(Name, 'Pe', 'La')			;context:AA_Employee:Peter Aylett			;expect:string:Later Aylett")]
        [TestCase("script: charindex('et', Name)			;context:AA_Employee:Peter Aylett			;expect:int:2")]
        [TestCase("script: charindex('aa', Name)			;context:AA_Employee:Peter Aylett			;expect:int:0")]
        [TestCase("script: left(Name,3)					;context:AA_Employee:Peter Aylett			;expect:string:Pet")]
        [TestCase("script: right(Name,4)				;context:AA_Employee:Peter Aylett			;expect:string:lett")]
        [TestCase("script: Name						;context:AA_Employee:Peter Aylett			;expect:string:Peter Aylett")]
        [TestCase("script: Manager					;context:AA_Employee:Peter Aylett			;expect:AA_Manager:Jude Jacobs")]
        [TestCase("script: Name + Manager				;context:AA_Employee:Peter Aylett			;expect:string:Peter AylettJude Jacobs")]
        [TestCase("script: Name + ', Manager is ' + Manager		;context:AA_Employee:Peter Aylett			;expect:string:Peter Aylett, Manager is Jude Jacobs")]
        [TestCase("script: tolower(Name)				    ;context:AA_Employee:Peter Aylett			;expect:string:peter aylett")]
        [TestCase("script: toupper(Name)				    ;context:AA_Employee:Peter Aylett			;expect:string:PETER AYLETT")]
        [TestCase("script: Substring(Name,7,3)				;context:AA_Employee:Peter Aylett			;expect:string:Ayl")]
        [TestCase("script: any([Direct Reports].Age>20)			;context:AA_Manager:Peter Aylett			;expect:bool:true; host:evaluate")]
        [TestCase("script: every([Direct Reports].Age>5)		;context:AA_Manager:Peter Aylett			;expect:bool:true; host:evaluate")]
        [TestCase("script: Sum([Direct Reports].Age)			;context:AA_Manager:Peter Aylett			;expect:int:197")]
        [TestCase("script: avg([Direct Reports].Age)			;context:AA_Manager:Peter Aylett			;expect:decimal(3):32.833")]
        [TestCase("script: stdev([Direct Reports].Age)			;context:AA_Manager:Peter Aylett			;expect:decimal(3):19.031")]
        [TestCase("script: any([Scientist (Rev)].Discipline='Mathematics')		;context:AA_All Fields:Test 30			;expect:bool:false; host:evaluate")]
        [TestCase("script: every([Scientist (Rev)].Discipline='Mathematics')	;context:AA_All Fields:Test 30			;expect:bool:true; host:evaluate")]
        [RunAsDefaultTenant]
        public void Calculations_NinosTestsUsingPeterAylett(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: Currency * 2  									;context:All Fields:AF 4	;expect: currency:800.80")]
        [TestCase("script: Currency * Number								;context:All Fields:AF 4	;expect: currency:160160")]
        [TestCase("script: Currency * Decimal								;context:All Fields:AF 4	;expect: currency:160,336.176")]
        [TestCase("script: 'Currency = '+ currency							;context:All Fields:AF 4	;expect: string:Currency = 400.400")]
        [TestCase("script: 'Number = '+ Number								;context:All Fields:AF 4	;expect: string:Number = 400")]
        [TestCase("script: Currency/Number									;context:All Fields:AF 4	;expect: currency:1.001")]
        [TestCase("script: Currency < 1000 and Number < 10 					;context:All Fields:AF 4	;expect: bool:false")]
        [TestCase("script: ([Currency] < 1000) and ([Number] > 10) 			;context:All Fields:AF 4	;expect: bool:true")]
        [TestCase("script: Currency >= 1000 or Number >= 10		 			;context:All Fields:AF 4	;expect: bool:true")]
        [TestCase("script: (Currency >= 1000) or (Number<= 10)	 			;context:All Fields:AF 4	;expect: bool:false")]
        [TestCase("script: Currency <400						 			;context:All Fields:AF 4	;expect: bool:false")]
        [TestCase("script: Currency <=400						 			;context:All Fields:AF 4	;expect: bool:false")]
        [TestCase("script: Currency >400						 			;context:All Fields:AF 4	;expect: bool:true")]
        [TestCase("script: Currency >1000						 			;context:All Fields:AF 4	;expect: bool:false")]
        [TestCase("script: Currency = 400						 			;context:All Fields:AF 4	;expect: bool:false")]
        [TestCase("script: Currency = 400.40					 			;context:All Fields:AF 4	;expect: bool:true")]
        [TestCase("script: Number = 400							 			;context:All Fields:AF 4	;expect: bool:true")]
        [TestCase("script: Number >= 400						 			;context:All Fields:AF 4	;expect: bool:true")]
        [TestCase("script: iif([Yes No],'Yes','No')							;context:All Fields:AF 4	;expect: string:Yes")]
        [TestCase("script: iif(not([Yes No]),'Yes','No')					;context:All Fields:AF 4	;expect: string:No")]
        [TestCase("script: iif([Yes No] and (Currency=400.4),'Yes','No')	;context:All Fields:AF 4	;expect: string:Yes")]
        [TestCase("script: iif([Yes No] or (Currency=100),'Yes','No')		;context:All Fields:AF 4	;expect: string:Yes")]
        [TestCase("script: iif(not([Yes No]) or(Currency=400.40),'Yes','No')	;context:All Fields:AF 4	;expect: string:Yes")]
        [TestCase("script: Name												;context:All Fields:AF 4	;expect: string:AF 4")]
        [TestCase("script: Description										;context:All Fields:AF 4	;expect: string untrimmed:\n      Description\n      of\n      AF 4\n    ")]
        [TestCase("script: Currency											;context:All Fields:AF 4	;expect: currency:400.40")]
        [TestCase("script: Decimal											;context:All Fields:AF 4	;expect: decimal:400.440")]
        [TestCase("script: Number											;context:All Fields:AF 4	;expect: int32:400")]
        [TestCase("script: [Single Text]									;context:All Fields:AF 4	;expect: string:single even")]
        [TestCase("script: [Multi Text]										;context:All Fields:AF 4	;expect: string untrimmed:\n      multi\n      text\n      here\n    ")]
        [TestCase("script: Date												;context:All Fields:AF 4	;expect: date:4/12/2012")]
        [TestCase("script: [Date Time]										;context:All Fields:AF 4	;expect: datetime:4/12/2012 4:00 am")]
        [TestCase("script: Time												;context:All Fields:AF 4	;expect: time:04:00:00")]
        [TestCase("script: AA_Employee										;context:AA_All Fields:Test 04	;expect: AA_Employee:Nino Carabella")]
        //[TestCase("script: Person.Age										;context:All Fields:AF 4	;expect: int32:29")]  // broken by shared data changes
        //[TestCase("script: [Priority - Choice]							;context:All Fields:AF 4	;expect: Priority:High")]   // "Priority" is ambiguous. Two relationships match. Bad test data.
        //[TestCase("script: [Priority - Choice].Order						;context:All Fields:AF 4	;expect: int32:3")] // broken by shared data changes
        [TestCase("script: let x=number select 4*x							;context:All Fields:AF 4	;expect: int32:1600")]
        [TestCase("script: all ([All Fields]) where number > 200 order by Name							;context:All Fields:AF 4	;host:Evaluate	;expect: All Fields list: AF 3, AF 4, AF 5")]
        //[TestCase("script: all ([All Fields]) where [Priority - Choice]>'Low' order by Name			;context:All Fields			;host:Evaluate	;expect: All Fields list: AF 3, AF 4, AF 5")]   // TODO : Enum order is not respected. (But note this is a lexical comparison)
        [TestCase("script: all ([AA_All Fields]) where [AA_Employee]='Peter Choi' order by Name				;context:AA_All Fields:Test 05	;host:Evaluate	;expect: AA_All Fields list: Test 05")]
        //[TestCase("script: all ([All Fields]) where [Manager]<>'Jude Jacobs' order by Name				;context:All Fields:AF 4	;host:Evaluate	;expect: All Fields list: AF 2, AF 4")] // broken by shared data changes
        //[TestCase("script: all ([All Fields]) where [Manager].[Direct Reports] like 'Scot%' order by Name				;context:All Fields:AF 4	;host:Evaluate	;expect: All Fields list: AF 1, AF 3, AF 5")] // broken by shared data changes
        //[TestCase("script: all ([All Fields]) where [Manager].[Works for Organisation] like 'EDC' order by Name	desc			;context:All Fields:AF 4	;host:Evaluate	;expect: All Fields list: AF 5, AF 4, AF 3, AF 2, AF 1")] // broken by shared data changes
        //[TestCase("script: all ([All Fields]) where [Manager].[Age] > 500 order by Name				;context:All Fields:AF 4	;host:Evaluate	;expect: All Fields list: AF 2, AF 4")] // broken by shared data changes
        [TestCase("script: count(all([All Fields]))													;context:All Fields			;host:Evaluate	;expect: int32:5")]
        //[TestCase("script: join(all([All Fields]) order by Name)									;context:All Fields			;host:Evaluate	;expect: All Fields list: AF 1, AF 3, AF 5")]   // can't use order-by in functions yet. Need to fix this.
        [RunAsDefaultTenant]
        public void Calculations_NinosTestsUsingAF4(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: iif(true, p, q)	;param:p=All Fields:AF 4;param:q=All Fields:AF 5	;expect: All Fields:AF 4")] // Bug 26598
        [TestCase("script: iif(false, p, q)	;param:p=All Fields:AF 4;param:q=All Fields:AF 5	;expect: All Fields:AF 5")] // Bug 26598
        [TestCase("script: iif(true, p, iif(true, p, p))	;param:p=All Fields:AF 4	;expect: All Fields:AF 4")]   // Bug 26598
        [RunAsDefaultTenant]
        public void Calculations_Bug26598(string test)
        {
            TestHelper.Test(test);
        }


        [Test]
        [TestCase( true, "script: [Single Text]										;context:All Fields:AF 4	;expect: string:null" )]
        [TestCase( false, "script: [Multi Text]										;context:All Fields:AF 4	;expect: string untrimmed:\n      multi\n      text\n      here\n    " )]
        [RunAsDefaultTenant]
        [RunWithTransaction]
        public void TestWriteOnlyFields( bool isFieldWriteOnly, string test )
        {
            try
            {
                CacheManager.ClearCaches();

                // Note: Rolling back the transaction does not invalidate caches.
                if ( isFieldWriteOnly )
                {
                    var field = Entity.Get<Field>( "test:singleTextStringFieldTestField", true );
                    field.IsFieldWriteOnly = true;
                    field.Save( );
                }

                TestHelper.Test( test );
            }
            finally
            {
                CacheManager.ClearCaches( );
            }
        }
    }
}
