using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Elasticsearch.Entity;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http.Description;
using Nest;
using WebGrease.Css.Extensions;

namespace Elasticsearch.Controllers
{
    [RoutePrefix(("v1/ES/Quest"))]
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
        [ResponseType(typeof(IEnumerable<Models.Quest>))]
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
        [ResponseType(typeof(IEnumerable<Models.Quest> ))]
        public async Task< IHttpActionResult > Search( string query )
        {
            var esClient = new ESClient.Client();

            var results = await esClient.SearchQueryViaQuery(query);
            if (results == null)
                return NotFound( );

            return Ok(new { count = results.Count(), data = results });
        }

        /// <summary>
        /// Searches the index by latitude and longitude
        /// </summary>
        /// <param name="latitude">Latitude to pin query to</param>
        /// <param name="longitude">Longitude to pin query to</param>
        /// <param name="radius">Radius to search</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GeoDistance")]
        [ResponseType(typeof(IEnumerable<Models.Quest>))]
        public async Task< IHttpActionResult > SearchGeoDistance( long latitude, long longitude, int radius )
        {
            var esClient = new ESClient.Client();

            var results = await esClient.FilterQuestViaGeoDistance( radius, latitude, longitude );
            if ( results == null )
                return NotFound( );

            return Ok( new {count = results.Count( ), data = results} );
        }

        /// <summary>
        /// Seearches the index by date range, returning quests that start within the date range
        /// </summary>
        /// <param name="start">Earliest date it can start</param>
        /// <param name="end">Latest date it can start</param>
        /// <returns></returns>
        [HttpGet]
        [Route("DateRange")]
        [ResponseType(typeof(IEnumerable<Models.Quest> ))]
        public async Task< IHttpActionResult > SearchDateRange( DateTime start, DateTime end )
        {
            var esClient = new ESClient.Client();

            var results = await esClient.FilterQuestViaDateTime( start, end );
            if ( results == null )
                return NotFound( );

            return Ok( new {count = results.Count( ), data = results} );
        }

        /// <summary>
        /// Searches quests that have a treasure matching the search text
        /// </summary>
        /// <param name="query">Search text</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Teasure")]
        [ResponseType(typeof(IEnumerable<Models.Quest>))]
        public async Task< IHttpActionResult > SearchViaTreasure( string query )
        {
            var esClient = new ESClient.Client();

            var results = await esClient.SearchQuestViaTreasure( query );
            if ( results == null )
                return NotFound( );

            return Ok(new { count = results.Count(), data = results });
        }
        #endregion

        #region CRUD

        /// <summary>
        /// Runs the migration from our local database to the elasticsearch index. Builds index and mappings if they do not exist
        /// </summary>
        /// <returns></returns>
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
                            Lat = q.EndLat,
                            Lon = q.EndLong
                        },
                        CoordStart = new Models.Geo
                        {
                            Lat = q.StartLat,
                            Lon = q.StartLong
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

        [HttpPost]
        [Route( )]
        [ResponseType(typeof(bool))]
        public async Task< IHttpActionResult > Post( Models.Quest quest )
        {
            if ( !ModelState.IsValid )
                return BadRequest( );

            if ( quest.Id == Guid.Empty )
                quest.Id = Guid.NewGuid( );

            var esClient = new ESClient.Client();
            var result = await esClient.Add( quest );

            return Ok( result );
        }

        [HttpPut]
        [Route()]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> Put(Models.Quest quest)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var esClient = new ESClient.Client( );
            var result = await esClient.Update( quest );

            return Ok( result );
        }

        [HttpDelete]
        [Route()]
        [ResponseType(typeof(bool))]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            var esClient = new ESClient.Client( );
            var result = await esClient.Delete( id );

            return Ok( result );
        }

        #endregion
    }
}