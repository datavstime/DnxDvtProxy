using System;

namespace DataVsTime
{
    public static class AdapterFactory
    {
        public static IAdapter GetAdapter(string adapterName)
        {
            switch (adapterName)
            {
                case "InfluxDb":
                    return new Adapter_InfluxDb();
                case "Test":
                    return new Adapter_Test();
                default:
                    throw new Exception("unknown adapter type: " + adapterName);
            }
        }
    }
}
