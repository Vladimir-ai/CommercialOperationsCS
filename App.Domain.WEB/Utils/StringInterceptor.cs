
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;

namespace App.Domain.WEB.Utils
{
    [PSerializable]
    public class StringInterceptorAttribute : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            if (args.Value == null)
                args.Value = "";

            args.Value = ((string)args.Value).Trim();
            args.ProceedSetValue();
        }

        public override void OnGetValue(LocationInterceptionArgs args)
        {
            base.OnGetValue(args);
        }
    }
}