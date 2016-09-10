﻿using System;
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
        public async Task< IEnumerable< Models.Quest > > FilterViaDateTime( DateTime queryStart, DateTime? queryEnd = null )
        {
            if ( queryEnd == null )
                queryEnd = queryStart.AddDays( 7 );

            IEnumerable< Models.Quest > result = null;
            var response = await _client.SearchAsync< Models.Quest >( s => s.Index( "Quest" )
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