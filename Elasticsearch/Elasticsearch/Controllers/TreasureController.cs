using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Elasticsearch.Entity;

namespace Elasticsearch.Controllers
{
    [RoutePrefix("v1/Treasures")]
    public class TreasureController : ApiController
    {
        #region Search
        /// <summary>
        /// Performs a search on treasures via string
        /// </summary>
        /// <param name="query">Search parameter</param>
        /// <returns>Result with name or description that match / partially match the query</returns>
        [Route("Search")]
        [HttpGet]
        public async Task< IHttpActionResult > Search( string query )
        {
            return Ok( );
        }

        /// <summary>
        /// Performs a search on treasures via latitude / longitude
        /// </summary>
        /// <param name="latitude">Latitude of point to search</param>
        /// <param name="longitude">Longitude of point to search</param>
        /// <param name="radius">Radius to search</param>
        /// <returns></returns>
        [Route("Geo")]
        [HttpGet]
        public async Task< IHttpActionResult > SearchGeoDistance( long latitude, long longitude, int radius )
        {
            return Ok();
        }
        #endregion

        #region CRUD

        /// <summary>
        /// Gets a specific instance of a treasure
        /// </summary>
        /// <param name="id">ID for treasure</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Id}")]
        [ResponseType(typeof(Treasure))]
        public async Task< IHttpActionResult > Get( Guid id )
        {
            Treasure treasure = null;
            using ( var context = new Entity.Context( ) )
            {
                treasure = await context.Treasures.FindAsync( id );
            }

            return Ok( treasure );
        }

        /// <summary>
        /// Gets all treasures
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<Treasure>))]
        [Route]
        public async Task< IHttpActionResult > Query( )
        {
            List< Treasure > treasures;
            using ( var context = new Entity.Context( ) )
            {
                treasures = context.Treasures.ToList();
            }

            return Ok( treasures );
        }

        /// <summary>
        /// Adds a new treasure
        /// </summary>
        /// <param name="treasure">Treasure object</param>
        /// <returns></returns>
        [HttpPost]
        [Route]
        [ResponseType(typeof(Treasure))]
        public async Task< IHttpActionResult > Add( Entity.Treasure treasure )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( );
            }

            using ( var context = new Entity.Context( ) )
            {
                context.Treasures.Add( treasure );
                await context.SaveChangesAsync( );
            }

            return Ok( treasure );
        }

        /// <summary>
        /// Updates treasure
        /// </summary>
        /// <param name="treasure">Treasure object</param>
        /// <returns></returns>
        [HttpPut]
        [Route]
        [ResponseType(typeof(Treasure))]
        public async Task< IHttpActionResult > Update( Entity.Treasure treasure )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( );
            }

            using ( var context = new Entity.Context( ) )
            {
                context.Entry(treasure).State = EntityState.Modified;

                await context.SaveChangesAsync( );
            }

            return Ok( treasure );
        }

        /// <summary>
        /// Deletes a treasure object
        /// </summary>
        /// <param name="id">Guid for object</param>
        /// <returns></returns>        
        [HttpDelete]
        [Route("{Id}")]
        public async Task< IHttpActionResult > Delete( Guid id )
        {
            using ( var context = new Entity.Context( ) )
            {
                var deleteMe = await context.Treasures.FindAsync( id );
                context.Treasures.Remove( deleteMe );
                await context.SaveChangesAsync( );
            }

            return Ok( );
        }

        #endregion 
    }
}
