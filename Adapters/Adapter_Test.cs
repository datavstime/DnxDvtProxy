using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DataVsTime
{
	public class Adapter_Test : IAdapter
	{
		private List<int> _periodsSeconds = new List<int> {1, 2, 5, 10, 20, 30, 60, 120};

		public Task<List<string>> GetSeriesNamesAsync()
		{
			return Task.FromResult(_periodsSeconds.Select(a => "SIN" + a).ToList());
		}

		public Task<List<double?>> GetValuesAsync(string series, long start, long stop, int step)
		{
			var period_seconds = int.Parse(series.Substring(3));

			var result = new List<double?>();
			for (long i=start; i<stop; i += step)
			{
				result.Add(Math.Sin( (i/1000.0) * 2.0 * Math.PI / period_seconds));
			}

			return Task.FromResult(result);
		}
	}
}
