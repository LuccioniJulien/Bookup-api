using System.Linq;
using BaseApi.Models;

namespace BaseApi.Dao {
    public static class CategoryQuery {
        public static IQueryable<string> GetNamesCategory (this IQueryable<Category> query, string predicat) {
            return query.Where (x => x.Name.ToUpper ().Contains (predicat))
                .Select (x => x.Name);
        }
        public static IQueryable<string> GetNamesCategory (this IQueryable<Category> query) => query.Select (x => x.Name);

    }
}