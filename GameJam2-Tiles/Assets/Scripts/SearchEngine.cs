using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchQuery
{
    public enum SearchBias { MinAmountSingle, MaxAmountSingle, MinAmountTotal, MaxAmountTotal, MinVariaty, MaxVariaty}

    private static Dictionary<SearchBias, Comparer<Result>> searchSettingMap = new Dictionary<SearchBias, Comparer<Result>>()
    {
        { SearchBias.MinAmountSingle,   new ResultMinAmountSingle()   },
        { SearchBias.MaxAmountSingle,   new ResultMaxAmountSingle()   },
        { SearchBias.MinAmountTotal,    new ResultMinAmountTotal()    },
        { SearchBias.MaxAmountTotal,    new ResultMaxAmountTotal()    },
        { SearchBias.MinVariaty,        new ResultMinVariaty()        },
        { SearchBias.MaxVariaty,        new ResultMaxVariaty()        }
    };


    public List<Result> results;
    public int[] supply;

    public SearchQuery(List<List<Transform>> tiles)
    {
        supply = new int[tiles.Count];
        for (int i = 0; i < supply.Length; i++)
        {
            supply[i] = tiles[i].Count;
        }
    }

    public void SortBy(SearchBias bias)
    {
        results.Sort(searchSettingMap[bias]);
    }

    public class Result
    {
    }


    public class ResultMinAmountSingle : Comparer<Result>
    {
        public override int Compare(Result lhs, Result rhs)
        {
            return 0;
        }
    }

    public class ResultMaxAmountSingle  : Comparer<Result>
    {
        public override int Compare(Result lhs, Result rhs)
        {
            return 0;
        }
    }

    public class ResultMinAmountTotal : Comparer<Result>
    {
        public override int Compare(Result lhs, Result rhs)
        {
            return 0;
        }
    }

    public class ResultMaxAmountTotal  : Comparer<Result>
    {
        public override int Compare(Result lhs, Result rhs)
        {
            return 0;
        }
    }

    public class ResultMinVariaty  : Comparer<Result>
    {
        public override int Compare(Result lhs, Result rhs)
        {
            return 0;
        }
    }

    public class ResultMaxVariaty : Comparer<Result>
    {
        public override int Compare(Result lhs, Result rhs)
        {
            return 0;
        }
    }

}
