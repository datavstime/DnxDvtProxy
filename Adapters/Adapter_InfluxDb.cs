using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Configuration;
using LightweightInfluxDb;
using System.Threading.Tasks;


namespace DataVsTime
{
	public class Adapter_InfluxDb : IAdapter
	{
		private InfluxDb _idb;
		private int _maxSamplePeriod;

		public Adapter_InfluxDb()
		{
            var conf = new ConfigurationBuilder(".", new[] { new JsonConfigurationSource("config.json") }).Build();

            _idb = new InfluxDb(
				conf["Adapter:Configuration:Host"],
				conf["Adapter:Configuration:Database"],
				conf["Adapter:Configuration:Username"],
				conf["Adapter:Configuration:Password"]);

            _idb.Timeout = TimeSpan.FromSeconds(int.Parse(conf["Adapter:Configuration:Timeout_ms"]));

			_maxSamplePeriod = int.Parse(conf["Adapter:Configuration:DataFillHack_ms"]);
		}

		public async Task<List<string>> GetSeriesNamesAsync()
		{
			try
			{
                var series = await _idb.QuerySingleSeriesAsync("show measurements");
                return series.Select(a => (string)a[0]).ToList();
            }
			catch (Exception e)
			{
				Console.WriteLine("Query failed: " + e);
				return new List<string>();
			}
		}

		public async Task<List<double?>> GetValuesAsync(string series, long start, long stop, int step)
		{
			try
			{
				// the InfluxDB fill(previous) directove evidently does not look before startDate
				//   => this hack required.
				var numPreSteps = _maxSamplePeriod / step;
				var prePeriod = numPreSteps * step;

				var q = "select mean(v) from " + series + " where time > "
            	            + (start - prePeriod) + "ms and time < " + stop
                	        + "ms group by time(" + step + "ms) fill(previous)";

                var vs = await _idb.QuerySingleSeriesAsync(q);
                return vs.Select(a => a[1] == null ? null : (double?)Convert.ToDouble(a[1])).Skip(numPreSteps).ToList();
			}
			catch (Exception e)
			{
				Console.WriteLine("Query failed: " + e);
				return new List<double?>();
			}
		}
	}
}
