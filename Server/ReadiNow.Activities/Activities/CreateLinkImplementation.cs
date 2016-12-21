// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Console;
using EDC.Xml;

namespace EDC.SoftwarePlatform.Activities
{
    // Create a URL to point to a selected resource
    public sealed class CreateLinkImplementation : ActivityImplementationBase, IRunNowActivity
    {

        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            var resource = GetArgumentEntity<Resource>(inputs, "core:createLinkResourceArgument");

            if (resource == null)
            {
                throw new WorkflowRunException("The 'Resource' argument is empty. It must have a value.");
            }

            string Url = NavigationHelper.GetResourceViewUrl(resource.Id).ToString();

            string anchor = string.Format("<a href='{0}'>{1}</a>", Url, XmlHelper.EscapeXmlText(resource.Name ?? "[Unnamed]"));

            context.SetArgValue(ActivityInstance, GetArgumentKey("core:createLinkUrlOutput"), Url);
            context.SetArgValue(ActivityInstance, GetArgumentKey("core:createLinkAnchorOutput"), anchor);
        }
    }
}