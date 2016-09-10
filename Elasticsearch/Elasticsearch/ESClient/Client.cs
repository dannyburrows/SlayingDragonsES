using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Nest;
using Newtonsoft.Json;

namespace Elasticsearch.ESClient
{
    internal class AWSCredentials
    {
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Region { get; set; }
    }

    public class Client
    {
        private ElasticClient _client;

        /// <summary>
        /// Instantiate client
        /// </summary>
        public Client( )
        {
            AWSCredentials credentials;
            string json = File.ReadAllText( string.Format( @"{0}aws.config.json", AppDomain.CurrentDomain.BaseDirectory ) );

            credentials = JsonConvert.DeserializeObject< AWSCredentials >( json );
                
            if (credentials == null) 
                throw new Exception("Whoops...");

            var httpConnection = new AwsHttpConnection(new AwsSettings
            {
                AccessKey = credentials.Key,
                SecretKey = credentials.Secret,
                Region = credentials.Region
            });


            // create uri and connect to the service
            var pool = new SingleNodeConnectionPool( new Uri( "http://search-slayingdragons-rjkkghkxvgnzkzzcm6b4savkme.us-west-2.es.amazonaws.com" ) );
            var config = new ConnectionSettings( pool, httpConnection );
            _client = new ElasticClient( config );
            // build indices and mappings
            CreateQuestIndices( );
            CreateTreasureIndices( );
        }

        /// <summary>
        /// Creates the quest indices and creates the model mappings
        /// </summary>
        /// <returns></returns>
        public bool CreateQuestIndices()
        {
            var exists = _client.IndexExists( "Quest" );
            if ( !exists.Exists )
            {
                var indexResponse = _client.CreateIndex( "Quest" );
                if ( indexResponse.Acknowledged )
                {
                    var mappingResponse = _client.Map< Models.Quest >( m => m.AutoMap( ).Index( "Quest" ) );
                    if ( mappingResponse.Acknowledged )
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates the teasure indices and model mappings
        /// </summary>
        /// <returns></returns>
        public bool CreateTreasureIndices( )
        {
            var exists = _client.IndexExists( "Treasure" );
            if ( !exists.Exists )
            {
                var indexResponse = _client.CreateIndex( "Treasure" );
                if ( indexResponse.Acknowledged )
                {
                    var mappingResponse = _client.Map< Models.Treasure >( m => m.AutoMap( ).Index( "Treasure" ) );
                    if ( mappingResponse.Acknowledged )
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Makes a raw json query against the indices
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public async Task<object> Raw(string json)
        {
            var response = await _client.SearchAsync<object>(s => s
             .AllIndices()
             .AllTypes()
             .Query(q => q.Raw(json)));

            return response;
        }

        #region Quest

        /// <summary>
        /// Searches the quest index via standard query
        /// </summary>
        /// <param name="query">Search text</param>
        /// <returns></returns>
        public async Task< IEnumerable< Models.Quest > > SearchQueryViaQuery( string query )
        {
            IEnumerable< Models.Quest > result = null;

            var response = await _client.SearchAsync< Models.Quest >( s => s.Index( "Quest" )
                .Type("QuestSearchModel")
                .Query( q =>
                    q.MultiMatch( mm =>
                        mm.Query( query )
                            .Type( TextQueryType.BestFields )
                            .Fields( f =>
                                f.Fields( f2 => f2.Name, f2 => f2.Description )
                            ) ) )
                .Sort( s2 => s2.Descending( SortSpecialField.Score ) ) );

            if ( response.Hits.Any( ) )
            {
                result = response.Hits.Select( h => h.Source );
            }

            return result;
        }

        /// <summary>
        /// Searchs the quest index via a fuzzy query
        /// </summary>
        /// <param name="query">Search text</param>
        /// <param name="fuzzy">Fuzziness index</param>
        /// <returns></returns>
        public async Task<IEnumerable<Models.Quest>> SearchQueryViaQueryFuzzy(string query, int fuzzy)
        {
            IEnumerable<Models.Quest> result = null;

            var response = await _client.SearchAsync< Models.Quest >( s => s.Index( "Quest" )
                .Type("QuestSearchModel")
                .Query( q =>
                    q.FunctionScore( fs => // define our own specific function to score
                        fs.Query( q2 =>
                            q2.MultiMatch( mm => // search on multiple fields
                                mm.Query( query )
                                    .Type( TextQueryType.BestFields )
                                    .Fields( f =>
                                        f.Field( f2 => f2.Name, 10.00 ) // Set the field and the boost
                                            .Field( f2 => f2.Description, 2.00 ) // higher boost means higher score
                                    ).Fuzziness( Fuzziness.EditDistance( fuzzy ) ) // controls amount of fuzzy
                                ) ) ) )
                .Sort( s2 => s2.Descending( SortSpecialField.Score ) ) );

            if (response.Hits.Any())
            {
                result = response.Hits.Select(h => h.Source);
            }

            return result;
        }

        /// <summary>
        /// Searchs quests that start within the passed in time frames
        /// </summary>
        /// <param name="queryStart">Starting date</param>
        /// <param name="queryEnd">Ending date</param>
        /// <returns></returns>
        public async Task< IEnumerable< Models.Quest > > FilterQuestViaDateTime( DateTime queryStart, DateTime? queryEnd = null )
        {
            if ( queryEnd == null )
                queryEnd = queryStart.AddDays( 7 );

            IEnumerable< Models.Quest > result = null;
            var response = await _client.SearchAsync< Models.Quest >( s => s.Index( "Quest" )
                .Type("QuestSearchModel")
                .Query( q =>
                    q.DateRange( dr =>
                        dr.Field( f => f.BeginDate ) // the field being search
                            .GreaterThanOrEquals( queryStart )
                            .LessThanOrEquals( queryEnd ) ) )
                .Sort( so => so.Ascending( a => a.BeginDate ) ) ); // sort by soonest
            if ( response.Hits.Any( ) )
            {
                result = response.Hits.Select( h => h.Source );
            }
            return result;
        }

        /// <summary>
        /// Filters quests by geo distance
        /// </summary>
        /// <param name="radius">Filter radius</param>
        /// <param name="latitude">Latitude to base search from</param>
        /// <param name="longitude">Longitude to base search from</param>
        /// <returns></returns>
        public async Task< IEnumerable< Models.Quest > > FilterQuestViaGeoDistance( double radius, double latitude, double longitude )
        {
            IEnumerable< Models.Quest > result = null;

            var response = await _client.SearchAsync< Models.Quest >( s => s.Index( "Quest" )
                .Type("QuestSearchModel")
                .Query( q =>
                    q.GeoDistance( gd =>
                        gd.Field( f => f.CoordStart )
                            .DistanceType( GeoDistanceType.Arc ) // 
                            .Location( latitude, longitude )
                            .Distance( radius, DistanceUnit.Miles ) )
                ).Sort( so => so.GeoDistance( gd =>
                    gd.Order( SortOrder.Ascending )
                        .PinTo( new GeoLocation( latitude, longitude ) ) ) ) ); // pin the sort from the original search point

            if ( response.Hits.Any( ) )
            {
                result = response.Hits.Select( h => h.Source );
            }

            return result;
        }

        /// <summary>
        /// Searches quests that have treasures that contain the teasure query
        /// </summary>
        /// <param name="treasureQuery">Search text</param>
        /// <returns></returns>
        public async Task< IEnumerable< Models.Quest > > SearchQuestViaTreasure( string treasureQuery )
        {
            IEnumerable< Models.Quest > result = null;

            var response = await _client.SearchAsync< Models.Quest >( s => s.Index( "Quest" )
                .Type("QuestSearchModel")
                .Query( q =>
                    q.Nested( n => // nested, we are going to be search on a nested proeprty field
                        n.Path( p => p.Treasures ) // search through treasures, these are indexed separately internally
                            .Query( q2 => // standard query from here
                                q2.MultiMatch( mm =>
                                    mm.Query( treasureQuery )
                                        .Type( TextQueryType.BestFields )
                                        .Fields( f2 =>
                                            f2.Field( f3 => f3.Name, 10.00 ).Field( f3 => f3.Description, 2.00 ) )
                                    ) ) ) ).Sort( so => so.Ascending( SortSpecialField.Score ) ) );
            if ( response.Hits.Any( ) )
            {
                result = response.Hits.Select( h => h.Source );
            }

            return result;
        }

        #region CRUD

        /// <summary>
        /// Adds a document to the index
        /// </summary>
        /// <param name="quest">Quest object</param>
        /// <returns></returns>
        public async Task< bool> Add( Models.Quest quest )
        {
            var response = await _client.IndexAsync( quest, idx => idx.Index( "Quest" ) );
            
            return response.Created;
        }

        /// <summary>
        /// Updates existing document
        /// </summary>
        /// <param name="quest">Quest object</param>
        /// <returns></returns>
        public async Task< bool > Update( Models.Quest quest )
        {
            var response = await _client.UpdateAsync( new DocumentPath< Models.Quest >( quest ), u => u.Index( "Quest" ).Doc( quest ) );
            if ( response.IsValid )
                return true;

            return false;
        }

        /// <summary>
        /// Deletes existing document
        /// </summary>
        /// <param name="id">Guid for document</param>
        /// <returns></returns>
        public async Task< bool > Delete( Guid id )
        {
            var response = await _client.DeleteAsync< Models.Quest >( id );
            if ( response.Found )
                return true;

            return false;
        }

        #endregion

        #endregion

        #region Treasure
        /// <summary>
        /// Searches the treasures via standard query
        /// </summary>
        /// <param name="query">Search text</param>
        /// <returns></returns>
        public async Task< IEnumerable< Models.Treasure > > SearchTreasureViaQuery( string query )
        {
            IEnumerable<Models.Treasure> result = null;

            var response = await _client.SearchAsync< Models.Treasure >( s => s.Index( "Treasure" )
                .Type( "TreasureSearchModel" )
                .Query( q =>
                    q.MultiMatch( mm => // search multiple fields
                        mm.Query( query )
                            .Type( TextQueryType.BestFields )
                            .Fields( f =>
                                f.Fields( f2 => f2.Name, f2 => f2.Description, f2 => f2.Value ) // each field we want to search
                            ) ) )
                .Sort( s2 => s2.Descending( SortSpecialField.Score ) ) ); // ensure best scores are bubbled to the top

            if ( response.Hits.Any( ) )
            {
                result = response.Hits.Select( h => h.Source );
            }

            return result;
        }

        /// <summary>
        /// Searches the treasure index via fuzzy search, with an emphasis on name matching
        /// </summary>
        /// <param name="query">Search text</param>
        /// <param name="fuzzy">Fuzziness index</param>
        /// <returns></returns>
        public async Task<IEnumerable<Models.Treasure>> SearchTreasureViaQueryFuzzy( string query, int fuzzy = 3 )
        {
            IEnumerable< Models.Treasure > result = null;

            var response = await _client.SearchAsync< Models.Treasure >( s => s.Index( "Treasure" )
                .Type( "TreasureSearchModel" )
                .Query( q =>
                    q.FunctionScore( fs => // define our own specific function to score
                        fs.Query( q2 =>
                            q2.MultiMatch( mm => // search on multiple fields
                                mm.Query( query )
                                    .Type( TextQueryType.BestFields )
                                    .Fields( f =>
                                        f.Field( f2 => f2.Name, 5.00 ) // Set the field and the boost
                                            .Field( f2 => f2.Description, 2.00 ) // higher boost means higher score
                                            .Field( f2 => f2.Value, 1.00 )
                                    ).Fuzziness( Fuzziness.EditDistance( fuzzy ) ) // controls amount of fuzzy
                                ) ) ) )
                .Sort( s2 => s2.Descending( SortSpecialField.Score ) ) );
            if ( response.Hits.Any( ) )
            {
                result = response.Hits.Select( h => h.Source );
            }
            return result;
        }
        #endregion
    }
}