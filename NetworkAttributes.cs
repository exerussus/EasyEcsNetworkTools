using System;

namespace Exerussus.EasyEcsNetworkTools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ClientMethodAttribute : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ServerMethodAttribute : Attribute
    {
        
    }
}