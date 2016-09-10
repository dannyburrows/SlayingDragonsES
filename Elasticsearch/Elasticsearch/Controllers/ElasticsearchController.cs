using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;

namespace Elasticsearch.Controllers
{
    public class ElasticsearchController : ApiController
    {
        /// <summary>
        /// Searches the elasticsearch index by a query string
        /// </summary>
        /// <param name="query">String containing the search parameters</param>
        /// <returns></returns>
        [HttpGet]
        public async Task< IHttpActionResult > SearchString( string query )
        {
            return Ok( );
        }

        /// <summary>
        /// Searches the index by latitude and longitude
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task< IHttpActionResult > SearchGeoDistance( long latitude, long longitude )
        {
            return Ok( );
        }
    }
}