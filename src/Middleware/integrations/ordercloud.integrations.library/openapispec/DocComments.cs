using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    /// <summary>
    /// Use to decorate controllers, actions, action parameters, models, and model properties.
    /// Supports markdown. Pass multiple strings for multiple paragraphs.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DocCommentsAttribute : Attribute
    {
        public DocCommentsAttribute(params string[] comments)
        {
            Comments = comments;
        }

        public string[] Comments { get; private set; }
    }
}
