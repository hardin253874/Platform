// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using ReadiNow.Expressions.Evaluation;

namespace ReadiNow.Expressions.Test.Runner
{
    [TestFixture]
	[RunWithTransaction]
    class ScriptInvalidTests
    {
        [TestCase("script: +        ;error: Invalid script syntax. (pos 1)")]
        [TestCase("script: 1+       ;error: Invalid script syntax. (pos 3)")]
        [TestCase("script: abs(     ;error: Invalid script syntax. (pos 5)")]
        [TestCase("script: all()    ;error: Wrong number of values passed to 'all' function. (pos 1)")]
        [TestCase("script: convert();error: Wrong number of values passed to 'convert' function. (pos 1)")]
        [TestCase("script: blah()   ;error: 'blah' is not a recognised function. (pos 1)")]
        [TestCase("script: dateadd(day2,1,datefromparts(2012,1,25))         ;error: 'day2' was not a recognised date/time part. (pos 9)")]
        [TestCase("script: dateadd(1+1,1,datefromparts(2012,1,25))          ;error: Expected a date/time part. (pos 9)")]
        [TestCase("script: datediff(day2,null,datefromparts(2012,1,25))     ;error: 'day2' was not a recognised date/time part. (pos 10)")]
        [TestCase("script: datediff(1+1,null,datefromparts(2012,1,25))      ;error: Expected a date/time part. (pos 10)")]
        [TestCase("script: convert(1+2,2+3)                                 ;error: Expected a type or definition name. (pos 9)")]
        [TestCase("script: all(1+2)                                         ;error: Expected a definition name. (pos 5)")]
        [TestCase("script: Name.asdf                   ;context:AA_Employee:Peter Aylett  ;error:Fields and relationships can only be accessed on resources. (pos 6)")]
        [TestCase("script: 'hello'.asdf                ;error:Fields and relationships can only be accessed on resources. (pos 9)")]
        [RunAsDefaultTenant]
        public void Calculations_SyntaxErrors(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: ''+Name   ;error: Attempted to access a field or relationship, when no starting entity-type has been provided. (pos 4)")]
        [TestCase("script: ''+@p     ;error: Parameter @p was not recognised. (pos 4)")]
        [TestCase("script: ''+Blah   ;context:Person   ;error: The name 'Blah' could not be matched on 'Person'. (pos 4)")]
        [TestCase("script: let x=1 let x=1 select x  ;error: Variable name x has already been set. (pos 13)")]
        [TestCase("script: all(Person2) ;error: Could not find a definition called 'Person2'. (pos 5)")]
        [TestCase("script: resource(Blah,Blah) ;error: Could not find a definition called 'Blah'. (pos 10)")]
        [TestCase("script: resource(Person,Blah) ;error: Could not find a 'Person' called 'Blah'. (pos 17)")]
        [TestCase("script: AA_Herb   ;context:AA_Herb   ;error: The name 'AA_Herb' could not be matched on 'AA_Herb'. (pos 1)")]  // ensure we don't fall back to relationship name if we have 'from-name' and 'to-name'
        [TestCase("script: [Schedule for Trigger]   ;context:Scheduled Item   ;error: The name 'Schedule for Trigger' could not be matched on 'Scheduled Item'. (pos 1)")]  // ensure we don't fall back to relationship name if we have 'from-name' and 'to-name'
        [TestCase("script: [Schedule for Trigger]   ;context:Schedule   ;error: The name 'Schedule for Trigger' could not be matched on 'Schedule'. (pos 1)")]  // ensure we don't fall back to relationship name if we have 'from-name' and 'to-name'
        
        [RunAsDefaultTenant]
        public void Calculations_IdentifierErrors(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: 'hello'   ;convertto:bool   ;error: Result was of type String (const) but needed to be Bool.")]  // hmm ... need nicer names
        [TestCase("script: convert(date,1)             ;error: Cannot convert from Int32 to Date. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(time,1)             ;error: Cannot convert from Int32 to Time. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(datetime,1)         ;error: Cannot convert from Int32 to DateTime. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(date,1.1)           ;error: Cannot convert from Decimal to Date. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(time,1.1)           ;error: Cannot convert from Decimal to Time. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(datetime,1.1)       ;error: Cannot convert from Decimal to DateTime. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(date,true)          ;error: Cannot convert from Bool to Date. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(time,true)          ;error: Cannot convert from Bool to Time. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(datetime,true)      ;error: Cannot convert from Bool to DateTime. (pos 9)")]  // hmm ... need nicer names
        [TestCase("script: convert(Blah,e)             ;param:e=Person:null;  error: Cannot convert to Blah. Unrecognised type or definition name. (pos 9)")]
        [TestCase("script: abs('123')                  ;error: Could not find a match of the correct type for 'abs'. (pos 1)")]
        [TestCase("script: abs()                       ;error: Wrong number of values passed to 'abs' function. (pos 1)")]
        [TestCase("script: true is null                ;error: Could not find a match of the correct type for 'is'. (pos 6)")]
        [TestCase("script: dateadd(year,1,timefromparts(0,0,0))         ;error: Cannot use 'Year' with time data. (pos 9)")]
        [TestCase("script: dateadd(quarter,1,timefromparts(0,0,0))      ;error: Cannot use 'Quarter' with time data. (pos 9)")]
        [TestCase("script: dateadd(month,1,timefromparts(0,0,0))        ;error: Cannot use 'Month' with time data. (pos 9)")]
        [TestCase("script: dateadd(week,1,timefromparts(0,0,0))         ;error: Cannot use 'Week' with time data. (pos 9)")]
        [TestCase("script: dateadd(day,1,timefromparts(0,0,0))          ;error: Cannot use 'Day' with time data. (pos 9)")]
        [TestCase("script: dateadd(hour,1,datefromparts(2012,1,1))      ;error: Cannot use 'Hour' with date data. (pos 9)")]
        [TestCase("script: dateadd(minute,1,datefromparts(2012,1,1))    ;error: Cannot use 'Minute' with date data. (pos 9)")]
        [TestCase("script: dateadd(second,1,datefromparts(2012,1,1))    ;error: Cannot use 'Second' with date data. (pos 9)")]
        [TestCase("script: convert(aa_manager,context()).[Direct Reports].Age order by Age desc  ;context:AA_Employee:Jude Jacobs  ;error:'order by' can only be applied to resource lists. (pos 52)")]
        [TestCase("script: [Direct Reports].Name     ;context:AA_Manager:Jude Jacobs  ;convertto: string disallowlist ;error:Result was of type List of String but needed to be String.")]
        [RunAsDefaultTenant]
        public void Calculations_TypeErrors(string test)
        {
            TestHelper.Test(test);
        }

        [TestCase("script: any([Direct Reports])                            ;context:AA_Manager:Peter Aylett   ;host:Report	;error:'any' is unavailable in reports. (pos 1)")]
        [TestCase("script: every([Direct Reports].Name <> 'blah')           ;context:AA_Manager:Peter Aylett   ;host:Report	;error:'every' is unavailable in reports. (pos 1)")]
        [TestCase("script: [Direct Reports] order by Name   ;host:Report    ;context:AA_Manager:Peter Aylett   ;error:'order by' is not available in reports. (pos 18)")]
        [TestCase("script: resource(AA_Employee,'Peter' + ' Aylett').[First Name]       ;host:Report                  ;error:The 'resource' function cannot have dynamic parameters in reports. (pos 1)" )]
        [TestCase("script: resource(AA_Employee,'Peter' + ' Aylett')                    ;host:Report                  ;error:The 'resource' function cannot have dynamic parameters in reports. (pos 1)" )]
        [TestCase("script: id(Manager) > 0;      context:AA_Employee:David Quint   ;hostapi:false                ;error:The 'id' function is internal. (pos 1)")]
        [RunAsDefaultTenant]
        public void Calculations_HostSpecificErrors(string test)
        {
            TestHelper.Test(test);
        }
    }
}
