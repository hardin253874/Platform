// Copyright 2011-2016 Global Software Innovation Pty Ltd
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Html;

namespace EDC.ReadiNow.Test.Html
{
    [TestFixture]
    public class HtmlSanitizerTests
    {

        [Test]
        public void TestHtmlSanitize()
        {
            var html = @"
<html>
<head>
    this element will be removed
</head>
<body onload='hack_my_website()'>
    <script src='Blahblahblah'></script>
    <link rel='stylesheet' href=Blahblahblah' type='text/css' />
    <div src='javascript:script()'>
        <input onclick='stuff()'></input>
        <img src='src path'>
    </div>
    <div>
        some text
        script
        some text
    </div>
    <div class='expression'>
        <span>
            <p>some text</p>
            <textarea>some text</textarea>
        </span>
    </div>
    <table class='table'>
        <thead>
            <th></th>
        </thead>
        <tbody>
            <tr>
                <td></td>
            </tr>
        </tbody>
        <tfoot></tfoot>
    </table>
    <p class=MsoNormal><a name='_MailEndCompose'><o:p>&nbsp;</o:p></a></p>
    <span style='mso-bookmark:_MailEndCompose'></span>
    <div style ='border:none;border-top:solid #E1E1E1 1.0pt;padding:3.0pt 0cm 0cm'></div>
</body>
</html>";

            var expectedResult = @"
<html>
<body>
    <div>
        <input>
        <img src='src path'>
    </div>
    <div>
    </div>
    <div>
        <span>
            <p>some text</p>
            <textarea>some text</textarea>
        </span>
    </div>
    <table class='table'>
        <thead>
            <th></th>
        </thead>
        <tbody>
            <tr>
                <td></td>
            </tr>
        </tbody>
        <tfoot></tfoot>
    </table>
    <p class='MsoNormal'><a name='_MailEndCompose'></a></p>
    <span style='mso-bookmark:_MailEndCompose'></span>
    <div style ='border:none;border-top:solid #E1E1E1 1.0pt;padding:3.0pt 0cm 0cm'></div>
</body>
</html>";

            var sanitize = new HtmlSanitizer();
            var result = sanitize.Sanitize(html);

            result = result.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("\"", "'").Replace(" ", "");
            expectedResult = expectedResult.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("\"", "'").Replace(" ", "");
            Assert.True(result == expectedResult);

        }


    }
}
