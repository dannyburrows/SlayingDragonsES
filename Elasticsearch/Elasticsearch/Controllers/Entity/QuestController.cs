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
    [RoutePrefix("v1/Quest")]
    public class QuestController : ApiController
    {
        #region Search
        /// <summary>
        /// Performs a search on quests via string
        /// </summary>
        /// <param name="query">Search parameter</param>
        /// <returns>Result with name or description that match / partially match the query</returns>
        [Route("Search")]
        [HttpGet]
        [ResponseType(typeof(List<Quest> ))]
        public IHttpActionResult Search( string query )
        {
            List< Quest > quests = new List< Quest >();
            using ( var context = new Context( ) )
            {
                // really bad partial matching, here we go...
                var matches = context.Quests.Where( t => t.Name == query )
                    .Union( context.Quests.Where( t => t.Description == query ) );
                if ( matches.Any( ) )
                {
                    quests = matches.ToList( );
                }
                else
                {
                    // split the query by whitespace
                    var queryParts = query.Split( ' ' );
                    // test each part on each property that we want to test
                    foreach ( var part in queryParts )
                    {
                        quests.AddRange( context.Quests.Where( t => t.Name.Contains( part ) )
                            .Union( context.Quests.Where( t => t.Description.Contains( part ) ) ) );
                    }
                    // remove duplicates
                    quests = quests.GroupBy( t => t.Id ).Select( t => t.First( ) ).ToList( );
                }
            }

            return Ok( quests );
        }

        /// <summary>
        /// Searches the quests than contain certain treasure types
        /// </summary>
        /// <param name="query">Search text</param>
        /// <returns></returns>
        [Route("Treasure")]
        [HttpGet]
        [ResponseType(typeof(List<Quest> ))]
        public IHttpActionResult SearchViaTreasure( string query )
        {
            List<Quest> quests = new List< Quest >();
            using ( var context = new Context( ) )
            {
                // start with children records, find exact matching treasures, then select quests that have a reference
                var matches = context.Treasures // to the treaure
                    .Include( t => t.Quests )
                    .Where( t => t.Name == query )
                    .SelectMany( t => t.Quests )
                    .Union( context.Treasures
                        .Include( t => t.Quests )
                        .Where( t => t.Description == query )
                        .SelectMany( t => t.Quests ) ); // select all quests that have this treasure
                if ( matches.Any( ) )
                {
                    quests = matches.ToList( );
                }
                else
                {
                    // split the query by white space
                    var queryParts = query.ToLower().Split( ' ' ); // remove casing
                    foreach ( var part in queryParts )
                    {
                        quests.AddRange( context.Treasures
                            .Include( t => t.Quests )
                            .Where( t => t.Name.ToLower().Contains(part) )
                            .SelectMany( t => t.Quests )
                            .Union( context.Treasures
                                .Include( t => t.Quests )
                                .Where( t => t.Description.ToLower().Contains(part) )
                                .SelectMany( t => t.Quests ) ) );
                    }
                    quests = quests.GroupBy(t => t.Id).Select(t => t.First()).ToList();
                }
            }

            return Ok( quests );
        }
        
        #endregion

        #region CRUD

        /// <summary>
        /// Gets a specific instance of a quest
        /// </summary>
        /// <param name="id">ID for quest</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{Id}")]
        [ResponseType(typeof(Quest))]
        public async Task< IHttpActionResult > Get( Guid id )
        {
            Quest quest = null;
            using ( var context = new Context( ) )
            {
                quest = await context.Quests.FindAsync( id );
            }

            return Ok( quest );
        }

        /// <summary>
        /// Gets all quests
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(List<Quest>))]
        [Route]
        public async Task< IHttpActionResult > Query( )
        {
            List< Quest > quests;
            using ( var context = new Context( ) )
            {
                quests = await context.Quests.ToListAsync();
            }
            return Ok( quests );
        }

        /// <summary>
        /// Adds a new quest
        /// </summary>
        /// <param name="quest">Quest object</param>
        /// <returns></returns>
        [HttpPost]
        [Route]
        [ResponseType(typeof(Quest))]
        public async Task< IHttpActionResult > Add( Quest quest )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( );
            }

            using ( var context = new Context( ) )
            {
                context.Quests.Add( quest );
                await context.SaveChangesAsync( );
            }

            return Ok( quest );
        }

        /// <summary>
        /// Updates quest
        /// </summary>
        /// <param name="quest">Quest object</param>
        /// <returns></returns>
        [HttpPut]
        [Route]
        [ResponseType(typeof(Quest))]
        public async Task< IHttpActionResult > Update( Quest quest )
        {
            if ( !ModelState.IsValid )
            {
                return BadRequest( );
            }

            using ( var context = new Context( ) )
            {
                context.Entry(quest).State = EntityState.Modified;

                await context.SaveChangesAsync( );
            }

            return Ok( quest );
        }

        /// <summary>
        /// Deletes a quest object
        /// </summary>
        /// <param name="id">Guid for object</param>
        /// <returns></returns>        
        [HttpDelete]
        [Route("{Id}")]
        public async Task< IHttpActionResult > Delete( Guid id )
        {
            using ( var context = new Context( ) )
            {
                var deleteMe = await context.Quests.FindAsync( id );
                context.Quests.Remove( deleteMe );
                await context.SaveChangesAsync( );
            }

            return Ok( );
        }

        #endregion 
    }
}
