using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Elasticsearch.Entity;

namespace Elasticsearch.Controllers
{
    [ControllerName("Entity")]
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
            List< Treasure > treasures = new List< Treasure >();
            using ( var context = new Context( ) )
            {
                // really bad partial matching, here we go...
                var matches = context.Treasures.Where( t => t.Name == query )
                    .Union( context.Treasures.Where( t => t.Description == query ) );
                if ( matches.Any( ) )
                {
                    treasures = matches.ToList( );
                }
                else
                {
                    // split the query by whitespace
                    var queryParts = query.Split( ' ' );
                    // test each part on each property that we want to test
                    foreach ( var part in queryParts )
                    {
                        treasures.AddRange( context.Treasures.Where( t => t.Name.Contains( part ) )
                            .Union( context.Treasures.Where( t => t.Description.Contains( part ) ) ) );
                    }
                    // remove duplicates
                    treasures = treasures.GroupBy( t => t.Id ).Select( t => t.First( ) ).ToList( );
                }
            }

            return Ok( treasures );
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
            using ( var context = new Context( ) )
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
            using ( var context = new Context( ) )
            {
                treasures = await context.Treasures.ToListAsync();
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
        public async Task< IHttpActionResult > Add( Treasure treasure )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( );
            }

            using ( var context = new Context( ) )
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
        public async Task< IHttpActionResult > Update( Treasure treasure )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( );
            }

            using ( var context = new Context( ) )
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
            using ( var context = new Context( ) )
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
