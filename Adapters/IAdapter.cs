using System.Collections.Generic;
using System.Threading.Tasks;


namespace MojoProxy
{
	public interface IAdapter
	{
		Task<List<string>> GetSeriesNamesAsync();

	    Task<List<double?>> GetValuesAsync(string seriesName, long startDate, long endDate, int interval);
	}
}
