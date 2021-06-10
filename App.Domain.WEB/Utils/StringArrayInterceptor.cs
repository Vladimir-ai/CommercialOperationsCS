using System;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace App.Domain.WEB.Utils
{
    [PSerializable]
    public class StringArrayInterceptorAttribute : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            string[] currValue = (string[])args.Value;

            if (currValue is null)
                currValue = Array.Empty<string>();

            if (currValue.Length == 1)
                currValue = currValue[0].Split(",",
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            args.Value = currValue;
            args.ProceedSetValue();
        }
    }
}