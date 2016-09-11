using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Elasticsearch.Entity;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebGrease.Css.Extensions;

namespace Elasticsearch.Controllers
{
    [RoutePrefix(("v1/ES"))]
    public class ESQuestController : ApiController
    {

        #region Search
        /// <summary>
        /// Searches the elasticsearch index by a query string and matches via fuzziness
        /// </summary>
        /// <param name="query">Search text</param>
        /// <param name="fuzzy">Fuzziness coefficient</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Fuzzy")]
        public async Task< IHttpActionResult > SearchFuzzy( string query, int fuzzy )
        {
            var esClient = new ESClient.Client();

            var results = await esClient.SearchQueryViaQueryFuzzy( query, fuzzy );
            if (results == null)
            {
                return NotFound();
            }

            return Ok( new {count = results.Count( ), data = results} );
        }

        /// <summary>
        /// Searches the elasticsearch index by the query string
        /// </summary>
        /// <param name="query">Search text</param>
        /// <returns></returns>
        [HttpGet]
        [Route("NonFuzzy")]
        public async Task< IHttpActionResult > Search( string query )
        {
            var esClient = new ESClient.Client();

            var results = await esClient.SearchQueryViaQuery(query);
            if (results == null)
            {
                return NotFound( );
            }
            return Ok(new { count = results.Count(), data = results });
        }

        /// <summary>
        /// Searches the index by latitude and longitude
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GeoDistance")]
        public async Task< IHttpActionResult > SearchGeoDistance( long latitude, long longitude )
        {
            return Ok( );
        }
        #endregion

        #region CRUD

        [Route( "Migrate" )]
        [HttpPost]
        public async Task< IHttpActionResult > Migrate( )
        {
            var esClient = new ESClient.Client();
            bool success = true;

            using ( var context = new Context( ) )
            {
                var toIndex = context.Quests.Include( q => q.Treasures ).Select( q =>
                    new Models.Quest
                    {
                        BeginDate = q.BeginDate,
                        CoordEnd = new Models.Geo
                        {
                            Latitude = q.EndLat,
                            Longitude = q.EndLong
                        },
                        CoordStart = new Models.Geo
                        {
                            Latitude = q.StartLat,
                            Longitude = q.StartLong
                        },
                        Description = q.Description,
                        Difficulty = q.Difficulty,
                        EndDate = q.EndDate,
                        Id = q.Id,
                        Name = q.Name,
                        Treasures = q.Treasures.Select( t => new Models.Treasure
                        {
                            Id = t.Id,
                            Description = t.Description,
                            Name = t.Name,
                            Value = t.Value
                        } ).ToList( )
                    } );
                foreach ( var i in toIndex )
                {
                    success &= await esClient.Add( i );
                }
            }
            return Ok( success );
        }
        #endregion
    }
}