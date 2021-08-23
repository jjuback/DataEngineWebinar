using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using C1.DataEngine;

namespace NFLStats.Data
{
    public class QueryService
    {
        private List<object> GetQueryResult(string name, int limit = 0)
        {
            dynamic result = Program.Workspace.GetQueryData(name, limit);
            return ClassFactory.CreateFromDataList(result, name);
        }

        private List<object> GetQueryResultSorted(string name, string sort, bool ascending = true, int limit = 0)
        {
            dynamic result = Program.Workspace.GetQueryData(name, limit);
            DataList.Sort(result, sort, ascending);
            return ClassFactory.CreateFromDataList(result, name);
        }

        public Task<List<object>> GetFieldGoalsAsync()
        {
            const string name = "FieldGoals";
            if (!Program.Workspace.QueryExists(name))
            {
                dynamic plays = Program.Workspace.table("Plays");
                dynamic query = Program.Workspace.query(name, new
                {
                    _range = plays.field_goal_attempt.Eq(1),
                    attempt = plays.field_goal_attempt,
                    result = plays.field_goal_result,
                    count = Op.Count(plays.field_goal_result)
                });
                query.Query.Execute();
            }
            return Task.FromResult<List<object>>(GetQueryResult(name));
        }

        public Task<List<object>> GetTouchdownsAsync(string category = null, string team = null)
        {
            // get appropriate query columns
            string group, value;
            switch (category)
            {
                case "passing":
                default:
                    group = "Plays.passer_player_name";
                    value = "Plays.pass_touchdown";
                    break;
                case "rushing":
                    group = "Plays.rusher_player_name";
                    value = "Plays.rush_touchdown";
                    break;
                case "receiving":
                    group = "Plays.receiver_player_name";
                    value = "Plays.pass_touchdown";
                    break;
            }

            // create runtime query and empty collections
            RuntimeQuery query = new RuntimeQuery();
            List<string> tables = new List<string>();
            List<RuntimeColumn> columns = new List<RuntimeColumn>();
            List<RuntimeRangeCondition> range = new List<RuntimeRangeCondition>();
            List<RuntimeRangeExpression> expr = new List<RuntimeRangeExpression>();

            // specify tables, columns
            tables.Add("Plays");
            columns.Add(new RuntimeColumn(group, null, "player"));
            columns.Add(new RuntimeColumn(value, "Count", "count"));
            
            // add range condition for value column
            expr.Add(new RuntimeRangeExpression("Bw", "1"));
            range.Add(new RuntimeRangeCondition(value, expr.ToArray()));

            // add range condition for team, if specified
            if (!string.IsNullOrEmpty(team))
            {
                List<RuntimeRangeExpression> expr2 = new List<RuntimeRangeExpression>();
                expr2.Add(new RuntimeRangeExpression("Bw", team));
                range.Add(new RuntimeRangeCondition("Plays.posteam", expr2.ToArray()));
            }
   
            // generate query name, populate runtime query arrays
            query.name = Guid.NewGuid().ToString();
            query.tables = tables.ToArray();
            query.columns = columns.ToArray();
            query.range = range.ToArray();
            
            // execute query and return top ten in descending order
            dynamic result = QueryFactory.CreateQueryFromRuntimeQuery(Program.Workspace, query);
            result.Query.Execute();
            return Task.FromResult<List<object>>(GetQueryResultSorted(query.name, "count", false, 10));
        }
    }
}
